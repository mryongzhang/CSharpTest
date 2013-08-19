using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WcfFileUpload
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“UploadService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 UploadService.svc 或 UploadService.svc.cs，然后开始调试。
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class UploadService : IUploadService
    {
        public void UploadFile(FileUploadMessage request)
        {
            string uploadFolder = @"D:\Upload\";
            string savaPath = request.SavePath;
            string dateString = DateTime.Now.ToShortDateString() + @"\";
            string fileName = request.FileName;
            Stream sourceStream = request.FileData;
            FileStream targetStream = null;

            if (!sourceStream.CanRead)
            {
                throw new Exception("数据流不可读!");
            }
            if (savaPath == null) savaPath = @"Photo\";
            if (!savaPath.EndsWith("\\")) savaPath += "\\";

            uploadFolder = uploadFolder + savaPath + dateString;
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            string filePath = Path.Combine(uploadFolder, fileName);
            try
            {

                using (targetStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    //read from the input stream in 4K chunks
                    //and save to output stream
                    const int bufferLen = 4096;
                    byte[] buffer = new byte[bufferLen];
                    int count = 0;
                    while ((count = sourceStream.Read(buffer, 0, bufferLen)) > 0)
                    {
                        targetStream.Write(buffer, 0, count);
                    }
                    targetStream.Close();
                    sourceStream.Close();
                }
            }
            catch (Exception ex)
            {
                //todo:write to log
            }

        }
    }
}
