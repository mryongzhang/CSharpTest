using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSWord = Microsoft.Office.Interop.Word;
using MSExcel = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Text.RegularExpressions;

namespace SearchKeyWord
{
    class Program
    {
        static void Main(string[] args)
        {
            //SearchKeyWord s = new SearchKeyWord(@"K:\Documents and Settings\zhangy\My Documents\Downloads\经典Oracle教程.doc", "的");

            SearchKeyWord s = new SearchKeyWord(@"E:\My Work folder\開発\IRS\SVN\BI\BI-tools\成果物\Excel\Excel接続検証_ホワイトペーパー.docx", "の");
            Console.ReadKey();
        }
    }

    class SearchKeyWord
    {
        public string LocalFile { get; set; }   // 本地存储文件名（包含路径）
        public string KeyWord { get; set; }     //要检索的关键字
        enum FileType { Word, Excel };
        private FileType fileType;

        public SearchKeyWord(string filename, string keyword)
        {
            LocalFile = filename;
            KeyWord = keyword;
            string fileExtension = Path.GetExtension(LocalFile);
            if (string.Compare(fileExtension, ".doc", true) == 0 || string.Compare(fileExtension, ".docx", true) == 0)
            {
                fileType = FileType.Word;
            }
            else if (string.Compare(fileExtension, ".xls", true) == 0 || string.Compare(fileExtension, ".xlsx", true) == 0)
            {
                fileType = FileType.Excel;
            }

            Search();



        }

        private void Search()
        {
            if (fileType == FileType.Word)
            {
                SearchWord();
            }
            else
            {
                SerachExcel();
            }
        }

        private void SearchWord()
        {
            MSWord.Application m_word = new MSWord.Application();

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
            Object openAndRepair = Type.Missing;
            Object documentDirection = Type.Missing;
            Object noEncodingDialog = Type.Missing;
            Object xmlTransform = Type.Missing;

            try
            {
                // 打开word文档
                MSWord.Document doc = m_word.Documents.Open(ref filefullname,
                        ref confirmConversions, ref readOnly, ref addToRecentFiles,
                        ref passwordDocument, ref passwordTemplate, ref revert,
                        ref writePasswordDocument, ref writePasswordTemplate,
                        ref format, ref encoding, ref visible, ref openAndRepair,
                        ref documentDirection, ref noEncodingDialog, ref xmlTransform
                        );


                doc.ShowRevisions = false;  //不显示修改标记，默认显示最终版
                m_word.Visible = false;

                int i = 0;
                MSWord.Range range= doc.Content;
                string text = range.Text;

                int cnt = doc.ComputeStatistics(MSWord.WdStatistic.wdStatisticCharacters);
                int pages = doc.ComputeStatistics(MSWord.WdStatistic.wdStatisticPages);
                int words = doc.ComputeStatistics(MSWord.WdStatistic.wdStatisticWords);
                int lines = doc.ComputeStatistics(MSWord.WdStatistic.wdStatisticLines);
                int paras = doc.ComputeStatistics(MSWord.WdStatistic.wdStatisticParagraphs);


                Console.WriteLine("页数:{0},字数:{1},段落数:{2},行数:{3}", pages, words,paras,lines);


                m_word.Quit(SaveChanges: false);


                //for (int j = 0; j < 100; j++ )
                //{
                //    i = 0;
                //    range = doc.Content;
                //    while (range.Find.Execute(FindText: KeyWord, Forward: true) && range.Find.Found)
                //    {
                //        i++;
                //    }

                //    Console.WriteLine("{0}", i);
                //}
                for (int j = 0; j < 10; j++ )
                {
                    i = 0;
                    i = Regex.Matches(text, KeyWord).Count;
                    Console.WriteLine("{0}", i);
                }

                for (int j = 0; j < 10; j++)
                {
                    i = 0;
                    i = Regex.Matches(text, "oracle").Count;
                    Console.WriteLine("{0}", i);
                }

                Console.WriteLine("{0}", text.Count());
                i = 0;
                /*
                while (range.Find.Execute(FindText: "延长", Forward: true) && range.Find.Found)
                {
                    i++;
                }
                
                Console.WriteLine("{0}", i);
                */
            }
            catch (System.Exception e)
            {
                Console.WriteLine("打开Word文档出错");
                Environment.Exit(0);
                return;
            }

            return;
        }

        private void SerachExcel()
        {
            MSExcel.Application m_excel = new MSExcel.Application();

            object MissingValue = Type.Missing;
            object readOnly = Type.Missing;


                readOnly = true;

                //readOnly = Type.Missing;


            try
            {
                // 打开Excel文档
                MSExcel.Workbook workbook = m_excel.Workbooks.Open(LocalFile, MissingValue,
                        readOnly, MissingValue, MissingValue,
                        MissingValue, MissingValue, MissingValue,
                        MissingValue, MissingValue, MissingValue,
                        MissingValue, MissingValue, MissingValue,
                        MissingValue);

                m_excel.Visible = true;

            }
            catch (System.Exception e)
            {
                Console.WriteLine("打开Excel文档出错");
                // 程序退出
                Environment.Exit(0);
                return;
            }

            return;
        }
    }
}
