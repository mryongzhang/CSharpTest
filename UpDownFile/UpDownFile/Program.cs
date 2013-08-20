using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using MSWord = Microsoft.Office.Interop.Word;
using MSExcel = Microsoft.Office.Interop.Excel;

namespace UpDownFile
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            string url = @"http://www.ganedu.net/images/mugshots/2012/10/fullsize/20121027123249_.doc";
            string dir = @"d:\";
            OfficeFile file = new OfficeFile();
            file.Download(url, dir);
            file.Open();
            */

            OpenExcelFile();
 

            Console.ReadKey();
            
        }

        static void OpenExcelFile()
        {
            string FileName = "1.xlsx";
            string LocalFile = @"d:\1.xlsx";
            MSExcel.Application m_excel = new MSExcel.Application();
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

            object MissingValue=Type.Missing;

            try
            {
                // 打开Excel文档
                MSExcel.Workbook workbook = m_excel.Workbooks.Open(LocalFile, MissingValue,
                        MissingValue, MissingValue, MissingValue,
                        MissingValue, MissingValue, MissingValue,
                        MissingValue, MissingValue, MissingValue,
                        MissingValue, MissingValue, MissingValue,
                        MissingValue);

                m_excel.UserName = "张勇";

                m_excel.Visible = true;

                //m_excel.WorkbookBeforeClose 
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("打开Excel文档出错");

            }

            return;


        }

    }

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

    }
}
