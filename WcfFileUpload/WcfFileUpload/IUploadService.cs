using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WcfFileUpload
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IUploadService”。

    [ServiceContract]
    public interface IUploadService
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
