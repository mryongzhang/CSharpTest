using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Threading;
using System.IO;

namespace FileInterface
{
    [ServiceContract]
    public interface IUpload
    {
        [OperationContract(Action = "UploadFile")]
        void UploadFile(FileUploadMessage request);//文件传输
    }


    [MessageContract]
    public class FileUploadMessage
    {
        [MessageHeader(MustUnderstand = true)]
        public string SavePath;//文件保存路径

        [MessageHeader(MustUnderstand = true)]
        public string FileName;//文件名称

        [MessageBodyMember(Order = 1)]
        public Stream FileData;//文件传输流
    }
}
