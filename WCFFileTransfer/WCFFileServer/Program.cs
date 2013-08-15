using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WCFFileServer
{
    static class Program
    {
        static TransferForm trForm ;
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            trForm = new TransferForm();
            Application.Run(trForm);
        }
        /// <summary>
        /// 应用程序日志
        /// </summary>
        /// <returns></returns>
        public static ILog Get_ILog()
        {
            return trForm;
        }
    }
}
