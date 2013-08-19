using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UploadConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            UpLoadServiceClient client = new UpLoadServiceClient();
            FileUploadMessage file = new FileUploadMessage();
            file.SavePath = "ppp";
            file.FileName = "safs.doc";
            file.FileData = new FileStream("d:\\20121027123249_.doc", FileMode.Open);

            client.UploadFile(file.FileName, file.SavePath, file.FileData);
            file.FileData.Close();

            client.Close();
        }
    }
}
