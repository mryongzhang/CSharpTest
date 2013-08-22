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
using System.Collections.Specialized;

namespace UpDownFile2
{
    /// <summary>
    /// 自动从URL下载Office文件，之后根据文件的扩展名判断是Word文件还是Excel文件，
    /// 用Word或Excel打开
    /// </summary>
    public class OfficeFile
    {
        public string LocalFile { get; set; }   // 本地存储文件名（包含路径）
        public string FileName { get; set; }    // 文件名

        //本地暂存路径，\My Documents\ycsytemp\
        private readonly string Dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"ycsytemp\");

        private StatusForm statusForm;   // 状态显示窗口

        enum FileType { Word, Excel };
        private FileType fileType;

        DateTime fileLastWriteTime;      // 文件刚下载完毕时的更新时间


        private struct ServerFileInfo
        {
            public string URL { get; set; } //被下载的internet文件地址（绝对路径）
            public string OpenMode { get; set; }
            public string UserID { get; set; }
            public string UserName { get; set; }
        }

        //服务器端传回的链接字符串中的信息
        ServerFileInfo serverFile = new ServerFileInfo();

        public OfficeFile(string arg, StatusForm form)
        {
            statusForm = form;

            // 从arg解密后的信息
            string DecryStr = Cryptography.Decrypt(arg);
            //要查询的字段分解
            StringDictionary sd = new StringDictionary();
            int index;
            string[] SplitttedData = DecryStr.Split(new char[] { '|' });
            foreach (string SingleData in SplitttedData)
            {
                index = SingleData.IndexOf('=');
                sd.Add(SingleData.Substring(0, index), SingleData.Substring(index + 1));
            }

            //解密后的各个字段值
            serverFile.URL = sd["url"];
            serverFile.OpenMode = sd["openmode"];
            serverFile.UserID = sd["userid"];
            serverFile.UserName = sd["username"];

            if (!Directory.Exists(Dir))
            {
                Directory.CreateDirectory(Dir);
            }

            FileName = serverFile.URL.Substring(serverFile.URL.LastIndexOf("/") + 1);  //被下载的文件名
            LocalFile = Dir + FileName;   //另存为的绝对路径＋文件名

            string fileExtension = Path.GetExtension(LocalFile);
            if (string.Compare(fileExtension, ".doc", true) == 0 || string.Compare(fileExtension, ".docx", true) == 0)
            {
                fileType = FileType.Word;
            }
            else if (string.Compare(fileExtension, ".xls", true) == 0 || string.Compare(fileExtension, ".xlsx", true) == 0)
            {
                fileType = FileType.Excel;
            } 
            else
            {
                MessageBox.Show("不支持打开本文件格式！\n请确认打开的是Word或Excel文件！", "文件格式错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Download();
        }
        
        /// <summary>
        /// 下载服务器文件至客户端
        public void Download()
        {
            WebClient client = new WebClient();

            try
            {
                WebRequest myrequest = WebRequest.Create(serverFile.URL);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //MessageBox.Show(e.Message,"Error");  
            }

            try
            {
                // 异步方式下载Internet上的文件
                Uri uri = new Uri(serverFile.URL);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCallback);
                // Specify a progress notification handler.
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
                client.DownloadFileAsync(uri, LocalFile);

                // 显示文件下载中的提示
                statusForm.ShowStatus(StatusForm.StatusType.Downloading);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //MessageBox.Show(exp.Message,"Error");
            }
        }

        private void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            // Displays the operation identifier, and the transfer progress. 更新下载进度
            statusForm.UpdateDownloadProgress(FileName, e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
        }

        private void DownloadFileCallback(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //todo: 错误处理
                MessageBox.Show(e.Error.Message);
                return;
            }

            // 隐藏状态显示窗体
            statusForm.Hide();

            // 先保存刚下载完状态的更新时间
            fileLastWriteTime = (new FileInfo(LocalFile)).LastWriteTime;

            // 打开文件
            Open();
            
        }

        private void Open()
        {
            if (fileType == FileType.Word)
            {
                OpenWord();
            } 
            else
            {
                OpenExcel();
            }
        }

        private void OpenWord()
        {
            // 显示文件打开中的提示
            statusForm.ShowStatus(StatusForm.StatusType.FileOpening);
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

                // 判断是否以只读模式打开
                if (serverFile.OpenMode == "read")
                {
                    // 设置文档保护，只允许读
                    doc.Protect(MSWord.WdProtectionType.wdAllowOnlyReading);
                    // 隐藏工具栏
                    //doc.ActiveWindow.ToggleRibbon();

                    /* 另外一种方式
                    // 设定word的显示模式是阅读模式
                    // doc.ActiveWindow.View.Type = MSWord.WdViewType.wdReadingView;
                    */
                }
                else
                {
                    // 启用修改履历
                    doc.TrackRevisions = true;
                    //doc.ShowRevisions = false;    //若设置此属性，则打开的word文档无论修改与否，word都认为进行了修改，每次都会保存。
                    
                    // 设定修订者的名称
                    //m_word.ActiveDocument.Application.UserName = serverFile.UserName;
                    m_word.UserName = serverFile.UserName;
                }

                m_word.Visible = true;

                //捕获文档关闭的事件，关键！
                m_word.DocumentBeforeClose += new MSWord.ApplicationEvents4_DocumentBeforeCloseEventHandler(wordApp_DocumentBeforeClose);
                
                // 显示文件已打开的提示
                statusForm.ShowStatus(StatusForm.StatusType.FileOpened);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("打开Word文档出错");
                //MessageBox.Show("打开Word文档出错");
            }

            return;
        }

        private void OpenExcel()
        {
            // 显示文件打开中的提示
            statusForm.ShowStatus(StatusForm.StatusType.FileOpening);

            MSExcel.Application m_excel = new MSExcel.Application();

            object MissingValue = Type.Missing;

            try
            {
                // 打开Excel文档
                MSExcel.Workbook workbook = m_excel.Workbooks.Open(LocalFile, MissingValue,
                        MissingValue, MissingValue, MissingValue,
                        MissingValue, MissingValue, MissingValue,
                        MissingValue, MissingValue, MissingValue,
                        MissingValue, MissingValue, MissingValue,
                        MissingValue);

                // 判断是否以只读模式打开
                if (serverFile.OpenMode == "read")
                {
                    workbook.Protect(MissingValue, true, true);
                }
                else
                {
                    m_excel.UserName = "UserName";
                }

                m_excel.Visible = true;

                //捕获文档关闭的事件，关键！
                m_excel.WorkbookBeforeClose += new MSExcel.AppEvents_WorkbookBeforeCloseEventHandler(excelApp_WorkbookBeforeClose);
                
                // 显示文件已打开的提示
                statusForm.ShowStatus(StatusForm.StatusType.FileOpened);

            }
            catch (System.Exception ex)
            {
                Console.WriteLine("打开Excel文档出错");
            }

            return;
        }

        private void wordApp_DocumentBeforeClose(MSWord.Document Doc, ref bool Cancel)
        {
            //当关闭程序打开的word文档的时候
            if (string.Compare(Doc.FullName, LocalFile, true) == 0)
            {
                // 保存文档, word好像无法忽略修改项保存退出，所以只能都保存一下。
                if (!Doc.Saved)
                {
                    Doc.Save();
                }

                // 关闭word
                Doc.Application.Quit();

                if (serverFile.OpenMode == "read")
                {
                    // 程序退出
                    Environment.Exit(0);
                    return;
                }

                // 异步方式启动上传文件函数
                AsyncDelegate dlgt = new AsyncDelegate(Upload);
                IAsyncResult ar = dlgt.BeginInvoke(LocalFile, new AsyncCallback(CallbackMethod), dlgt);               
            }
        }

        private void excelApp_WorkbookBeforeClose(MSExcel.Workbook Workbook, ref bool Cancel)
        {
            //当关闭程序打开的excel文档的时候
            if (string.Compare(Workbook.FullName, LocalFile, true) == 0)
            {
                // 保存文档
                if (serverFile.OpenMode == "edit" && !Workbook.Saved)
                {
                    Workbook.Save();
                }
                else
                {
                    Workbook.Close(false);
                }
                // 关闭excel
                Workbook.Application.Quit();

                // 异步方式启动上传文件函数
                AsyncDelegate dlgt = new AsyncDelegate(Upload);
                IAsyncResult ar = dlgt.BeginInvoke(LocalFile, new AsyncCallback(CallbackMethod), dlgt);
            }

        }

        // 用于上传文件的操作
        public void Upload(string filename)
        {
            Thread.Sleep(200);

            FileInfo fileInfo = new FileInfo(LocalFile);

            // 文件的最终更新时间与打开前相同，则文件未被修改，不需要上传回服务器
            if (fileLastWriteTime == fileInfo.LastWriteTime)
            {
                //MessageBox.Show("修改后的文件已成功上传，请刷新之前的页面确认！", "文件已上传", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // 删除本地临时文件
                File.Delete(LocalFile);

                // 程序退出
                Environment.Exit(0);
                return;
            }

            // 显示文件上传中的提示
            statusForm.ShowStatus(StatusForm.StatusType.Uploading);

            //建立与WCF的通道
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.TransferMode = TransferMode.Streamed;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            binding.SendTimeout = new TimeSpan(0, 10, 0);

            //wcf地址的取得
            string strWCF = serverFile.URL.Substring(0, serverFile.URL.IndexOf("upload")) + @"wcf/UploadService.svc";
            IUploadService channel = ChannelFactory<IUploadService>.CreateChannel(binding,
                new EndpointAddress(strWCF));

            using (channel as IDisposable)
            {
                // 调用WCF接口，上传文件
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

            // 删除本地临时文件
            File.Delete(LocalFile);

            // 显示上传完毕的提示
            statusForm.ShowStatus(StatusForm.StatusType.Uploaded);
            MessageBox.Show("修改后的文件已成功上传，请刷新之前的页面确认！", "文件已上传", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // 程序退出
            Environment.Exit(0);
        }

        public delegate void AsyncDelegate(string filename);

    }

}
