using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ServiceModel;
using UploadWcfService;

namespace UpLoadConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            // 自行创建host
            AppDomain.CreateDomain("Server").DoCallBack(delegate
            {
                ServiceHost host = new ServiceHost(typeof(UpLoadService),
                  new Uri("http://localhost:1631/UpLoadService.svc"));

                BasicHttpBinding binding = new BasicHttpBinding();
                binding.TransferMode = TransferMode.Streamed;
                binding.MaxReceivedMessageSize = 67108864;

                host.AddServiceEndpoint(typeof(IUpLoadService), binding, "");

                host.Open();
            });

            BasicHttpBinding binding2 = new BasicHttpBinding();
            binding2.TransferMode = TransferMode.Streamed;
            binding2.SendTimeout = new TimeSpan(0, 10, 0);

            IUpLoadService channel = ChannelFactory<IUpLoadService>.CreateChannel(binding2,
              new EndpointAddress("http://localhost:1631/UpLoadService.svc"));

            using (channel as IDisposable)
            {
                FileUploadMessage file = new FileUploadMessage();
                file.SavePath = "ppp";
                file.FileName = "safs34.doc";
                file.FileData = new FileStream("d:\\20121027123249_.doc", FileMode.Open);

                channel.UploadFile(file);

                file.FileData.Close();
            }

        }
    }
}
