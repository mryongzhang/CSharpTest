using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.ServiceModel;
using MSWord = Microsoft.Office.Interop.Word;
using MSExcel = Microsoft.Office.Interop.Excel;
using ZyUtility;
using System.ComponentModel;

namespace UpDownFile2
{
    /// <summary>
    /// 自动从URL下载Office文件，之后根据文件的扩展名判断是Word文件还是Excel文件，
    /// 用Word或Excel打开
    /// </summary>
    public class OfficeFile
    {
        public string URL { get; set; }         //被下载的文件地址，绝对路径
        //public string Dir { get; set; }
        public string LocalFile { get; set; }
        public string FileName { get; set; }

        private string Dir;                     //本地暂存目录
        private StatusForm statusForm;

        public OfficeFile(string arg, StatusForm form)
        {
            // 从arg解码加密过的信息
            statusForm = form;
            URL = Cryptography.Decrypt(arg);
            Dir = @"c:\temp\";
            FileName = URL.Substring(URL.LastIndexOf("/") + 1);  //被下载的文件名
            LocalFile = Dir + FileName;   //另存为的绝对路径＋文件名





        }
        
        /// <summary>
        /// 下载服务器文件至客户端
        public void Download()
        {
            WebClient client = new WebClient();

            try
            {
                WebRequest myrequest = WebRequest.Create(URL);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //MessageBox.Show(e.Message,"Error");  
            }

            try
            {
                Uri uri = new Uri(URL);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCallback);
                // Specify a progress notification handler.
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
                client.DownloadFileAsync(uri, "serverdata.txt");

                //client.DownloadFile(URL, LocalFile);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //MessageBox.Show(exp.Message,"Error");
            }
        }

        private void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            // Displays the operation identifier, and the transfer progress.
            statusForm.UpdateDownloadProgress(FileName, e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
        }

        private void DownloadFileCallback(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //todo: 错误处理
                return;
            }

            // 隐藏状态显示窗体
            statusForm.Hide();
            // 打开文件
            Open();
            
        }



        public void Open()
        {
            MSWord.Application m_word = new MSWord.Application();

            Object filename = FileName;
            Object filefullname = LocalFile;
            Object confirmConversions = Type.Missing;
            Object readOnly = Type.Missing;
            Object addToRecentFiles = Type.Missing;
            Object passwordDocument = Type.Missing;
            Object passwordTemplate = Type.Missing;
            Object revert = Type.Missing;
            Object writePasswordDocument = Type.Missing;
            Object writePasswordTemplate = Type.Missing;
            Object format = Type.Missing;
            Object encoding = Type.Missing;
            Object visible = Type.Missing;
            Object openConflictDocument = Type.Missing;
            Object openAndRepair = Type.Missing;
            Object documentDirection = Type.Missing;
            Object noEncodingDialog = Type.Missing;

            try
            {
                // 打开word文档
                MSWord.Document doc = m_word.Documents.Open(ref filefullname,
                        ref confirmConversions, ref readOnly, ref addToRecentFiles,
                        ref passwordDocument, ref passwordTemplate, ref revert,
                        ref writePasswordDocument, ref writePasswordTemplate,
                        ref format, ref encoding, ref visible, ref openConflictDocument,
                        ref openAndRepair, ref documentDirection, ref noEncodingDialog
                        );

                // 启用修改履历
                doc.TrackRevisions = true;
                doc.ShowRevisions = false;

                // 设定word的显示模式是阅读模式
                // doc.ActiveWindow.View.Type = MSWord.WdViewType.wdReadingView;
                m_word.Visible = true;


                // 隐藏工具栏
                //doc.ActiveWindow.ToggleRibbon();

                // 设定修订者的名称
                //m_word.ActiveDocument.Application.UserName = "张勇";
                m_word.UserName = "张勇";

                // 设置文档保护，只允许读
                //doc.Protect(MSWord.WdProtectionType.wdAllowOnlyReading);

                //捕获文档关闭的事件，关键！
                m_word.DocumentBeforeClose += new MSWord.ApplicationEvents4_DocumentBeforeCloseEventHandler(wordApp_DocumentBeforeClose);
                //m_word = null;

                //MessageBox.Show(m_word.Documents.Count.ToString());  
                //MessageBox.Show(m_word.Documents[1].FullName.ToString());  
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("打开Word文档出错");
                //MessageBox.Show("打开Word文档出错");
            }

            return;
        }

        private void wordApp_DocumentBeforeClose(MSWord.Document Doc, ref bool Cancel)
        {
            //当关闭程序打开的word文档的时候
            if (string.Compare(Doc.FullName, LocalFile, true) == 0)
            {
                // 保存文档
                Doc.Save();
                // 关闭word
                Doc.Application.Quit();

                // 异步方式启动上传文件函数
                AsyncDelegate dlgt = new AsyncDelegate(Upload);
                IAsyncResult ar = dlgt.BeginInvoke(LocalFile, new AsyncCallback(CallbackMethod), dlgt);
                
            }
            //Doc.Application.Quit();
        }

        // 用于上传文件的操作
        public void Upload(string filename)
        {
            Thread.Sleep(200);

            //上传文件
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.TransferMode = TransferMode.Streamed;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            binding.SendTimeout = new TimeSpan(0, 10, 0);

            //wcf地址的取得
            string strWCF = URL.Substring(0, URL.IndexOf("upload")) + @"wcf/UploadService.svc";
            IUploadService channel = ChannelFactory<IUploadService>.CreateChannel(binding,
                new EndpointAddress(strWCF));

            /*
            IUploadService channel = ChannelFactory<IUploadService>.CreateChannel(binding,
              new EndpointAddress("http://localhost:36014/UploadService.svc"));
            */

            using (channel as IDisposable)
            {
                FileUploadMessage file = new FileUploadMessage();
                file.SavePath = "ppp";
                file.FileName = FileName;
                file.FileData = new FileStream(filename, FileMode.Open);

                channel.UploadFile(file);

                file.FileData.Close();
            }
            
        }

        // 上传函数执行完成后的回调函数
        public void CallbackMethod(IAsyncResult ar)
        {
            AsyncDelegate dlgt = (AsyncDelegate)ar.AsyncState;
            dlgt.EndInvoke(ar);
            MessageBox.Show("修改后的文件已上传！");
        }

        public delegate void AsyncDelegate(string filename);

    }

}
