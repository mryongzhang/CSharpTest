﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MSWord = Microsoft.Office.Interop.Word;

namespace UpDownFile2
{
    public class OfficeFile
    {
        public string URL { get; set; }
        public string Dir { get; set; }
        public string LocalFile { get; set; }
        public string FileName { get; set; }


        /// <summary>
        /// 下载服务器文件至客户端

        /// </summary>
        /// <param name="URL">被下载的文件地址，绝对路径</param>
        /// <param name="Dir">另存放的目录</param>
        public void Download(string _URL, string _Dir)
        {
            URL = _URL;
            Dir = _Dir;
            WebClient client = new WebClient();
            FileName = URL.Substring(URL.LastIndexOf("/") + 1);  //被下载的文件名

            LocalFile = Dir + FileName;   //另存为的绝对路径＋文件名

            try
            {
                WebRequest myre = WebRequest.Create(URL);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //MessageBox.Show(e.Message,"Error");  
            }

            try
            {
                client.DownloadFile(URL, LocalFile);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //MessageBox.Show(exp.Message,"Error");
            }
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

                // 设定修订者的名称
                m_word.UserName = "zyzy";
                // 启用修改履历
                doc.TrackRevisions = true;
                m_word.Visible = true;

                // 设置文档保护，只允许读
                //doc.Protect(MSWord.WdProtectionType.wdAllowOnlyReading);

                //捕获文档关闭的事件，关键！
                m_word.DocumentBeforeClose += new MSWord.ApplicationEvents4_DocumentBeforeCloseEventHandler(wordApp_DocumentBeforeClose);
                m_word = null;


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
                MessageBox.Show("save");
                Doc.Save();
            }
            //Doc.Application.Quit();
        }

    }

}
