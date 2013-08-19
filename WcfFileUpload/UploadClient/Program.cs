using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;


namespace UploadClient
{
    class Program
    {
        static void Main(string[] args)
        {
            uploadByConfig();
            uploadByCode();

        }
        
        // 使用配置文件中的信息，连接wcf后上传文件
        static void uploadByConfig()
        {
            UploadServiceClient client = new UploadServiceClient();
            FileUploadMessage file = new FileUploadMessage();
            file.SavePath = "ppp";
            file.FileName = "safs.xlsx";
            file.FileData = new FileStream("d:\\jeasyui ie7 测试.xlsx", FileMode.Open);

            client.UploadFile(file.FileName, file.SavePath, file.FileData);
            file.FileData.Close();

            client.Close();

        }

        // 不适用配置文件，完全用代码生成与wcf的连接，之后上传文件
        static void uploadByCode()
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.TransferMode = TransferMode.Streamed;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            binding.SendTimeout = new TimeSpan(0, 10, 0);

            IUploadService channel = ChannelFactory<IUploadService>.CreateChannel(binding,
              new EndpointAddress("http://localhost:36014/UploadService.svc"));

            using (channel as IDisposable)
            {
                FileUploadMessage file = new FileUploadMessage();
                file.SavePath = "ppp";
                file.FileName = "safs2.xlsx";
                file.FileData = new FileStream("d:\\jeasyui ie7 测试.xlsx", FileMode.Open);

                channel.UploadFile(file);

                file.FileData.Close();
            }

        }
    }
}
