#[使用WCF上传文件[转]](http://www.cnblogs.com/xiaozhuang/archive/2008/04/01/1133138.html "使用WCF上传文件")
http://www.cnblogs.com/xiaozhuang/archive/2008/04/01/1133138.html

在WCF没出现之前，我一直使用用WebService来上传文件，我不知道别人为什么要这么做，因为我们的文件服务器和网站后台和网站前台都不在同一个机器，操作人员觉得用FTP传文件太麻烦，我就做一个专门用来上传文件的WebService,把这个WebService部署在文件服务器上，然后在网站后台调用这个WebService，把网站后台页面上传上来的文件转化为字节流传给WebService，然后WebService把这个字节流保存文件到一个只允许静态页面的网站（静态网站可以防止一些脚本木马）。 
WebService来上传文件存在的问题是效率不高，而且不能传输大数据量的文件，当然你可以用Wse中的MTOM来传输大文件，有了WCF就好多了，通过使用WCF传递Stream对象来传递大数据文件，但有一些限制：

1、只有 BasicHttpBinding、NetTcpBinding 和 NetNamedPipeBinding 支持传送流数据。

2、 流数据类型必须是可序列化的 Stream 或 MemoryStream。

3、 传递时消息体(Message Body)中不能包含其他数据。

4、TransferMode的限制和MaxReceivedMessageSize的限制等。

下面具体实现：新建一个WCFService，接口文件的代码如下：


	[ServiceContract]
    public interface IUpLoadService
    {
        [OperationContract(Action = "UploadFile", IsOneWay = true)]
        void UploadFile(FileUploadMessage request);
    }


    [MessageContract]
    public class FileUploadMessage
    {
        [MessageHeader(MustUnderstand = true)]
        public string SavePath;

        [MessageHeader(MustUnderstand = true)]
        public string FileName;

        [MessageBodyMember(Order = 1)]
        public Stream FileData;

    }

定义FileUploadMessage类的目的是因为第三个限制，要不然文件名和存放路径就没办法传递给WCF了，根据第二个限制，文件数据是用System.IO.Stream来传递的

接口方法只有一个，就是上传文件，注意方法参数是FileUploadMessage

接口实现类文件的代码如下：


 	public class UpLoadService : IUpLoadService
    {
        public void UploadFile(FileUploadMessage request)
        {
            string uploadFolder = @"C:\kkk\";
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

    }

实现的功能是到指定目录下按照日期进行目录划分，然后以传过来的文件名保存文件。

这篇文章最主要的地方就是下面的Web.Config配置：


	<system.serviceModel>
	    <bindings>
	      <basicHttpBinding>
	        <binding name="FileTransferServicesBinding" maxReceivedMessageSize="9223372036854775807"
	          messageEncoding="Mtom" transferMode="Streamed" sendTimeout="00:10:00" />
	          </basicHttpBinding>
	    </bindings>
	    <services>
	      <service behaviorConfiguration="UploadWcfService.UpLoadServiceBehavior"
	        name="UploadWcfService.UpLoadService">
	        <endpoint address="" binding="basicHttpBinding" bindingConfiguration="FileTransferServicesBinding" contract="UploadWcfService.IUpLoadService">
	        </endpoint>
	        <endpoint address="mex" binding="mexHttpBinding" contract="IMetadataExchange" />
	      </service>
	    </services>
	    <behaviors>
	      <serviceBehaviors>
	        <behavior name="UploadWcfService.UpLoadServiceBehavior">
	          <serviceMetadata httpGetEnabled="true" />
	          <serviceDebug includeExceptionDetailInFaults="false" />
	        </behavior>
	      </serviceBehaviors>
	    </behaviors>
  	</system.serviceModel>

配置要遵循上面的第一条和第四条限制，因为默认.net只能传4M的文件，所以要在
<System.Web>配置节下面加上<httpRuntimemaxRequestLength="2097151" />

这样WCFService就完成了，新建一个Console项目或者Web项目测试一下。要注意的
是Client端的配置必须要和服务端一样，[实例程序在这里下载](http://files.cnblogs.com/xiaozhuang/HCUpLoad.rar "实例程序在这里下载")。
