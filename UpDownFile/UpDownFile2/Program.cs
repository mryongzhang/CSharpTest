using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZyUtility;

namespace UpDownFile2
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length == 0)
            {
                return;
            }
            else
            {
                /*
                string fileurl = Cryptography.Decrypt(args[0].Substring(7));
                //MessageBox.Show(fileurl);
                Application.Run(new Form1(fileurl));
                */

                Application.Run(new StatusForm(args[0].Substring(7)));

                /*
                string fileurl = Cryptography.Decrypt(args[0].Substring(7));
                string dir = @"d:\";
                OfficeFile file = new OfficeFile();
                file.Download(fileurl, dir);
                file.Open();
                */
            }
        }
    }
}
