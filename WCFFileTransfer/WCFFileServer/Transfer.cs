using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using FileInterface;

namespace WCFFileServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class Transfer : ITransfer
    {
        public void TransferFile(FileTransferMessage request)
        {
            string logInfo;

            Program.Get_ILog().Log(logInfo = string.Format("开始接收文件,name={0}", request.FileName));//填写日志
            //文件信息
            string uploadFolder = AppValue.GetParam()._saveDir;
            string savaPath = request.SavePath;
            string fileName = request.FileName;
            Stream sourceStream = request.FileData;
            FileStream targetStream = null;
            //判断文件是否可读
            if (!sourceStream.CanRead)
            {
                throw new Exception("数据流不可读!");
            }
            if (savaPath == null) savaPath = @"文件传输\";
            if (!savaPath.EndsWith("\\")) savaPath += "\\";
            if (!uploadFolder.EndsWith("\\")) uploadFolder += "\\";

            uploadFolder = uploadFolder + savaPath;
            //创建保存文件夹
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            int fileSize = 0;
            string filePath = Path.Combine(uploadFolder, fileName);//Combine合并两个路径
            try
            {
                //文件流传输
                using (targetStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    //定义文件缓冲区
                    const int bufferLen = 4096;
                    byte[] buffer = new byte[bufferLen];
                    int count = 0;

                    while ((count = sourceStream.Read(buffer, 0, bufferLen)) > 0)
                    {
                        targetStream.Write(buffer, 0, count);
                        fileSize += count;
                    }
                    targetStream.Close();
                    sourceStream.Close();
                }
            }
            catch (Exception ex)
            {
                Program.Get_ILog().Log(logInfo + ex.Message);
            }

            Program.Get_ILog().Log(string.Format("接收文件完毕 name={0},filesize={1}",
              request.FileName, fileSize));
        }
    }
}
