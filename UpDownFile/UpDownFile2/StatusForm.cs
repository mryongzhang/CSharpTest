using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UpDownFile2
{
    public partial class StatusForm : Form
    {
        string arg;
        public StatusForm(string _arg)
        {
            InitializeComponent();
            arg = _arg;

            OfficeFile officeFile = new OfficeFile(arg, this);
        }



        /// <summary>
        /// 更新下载状态
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="BytesReceived"></param>
        /// <param name="TotalBytesToReceive"></param>
        /// <param name="ProgressPercentage"></param>
        public void UpdateDownloadProgress(string FileName, long BytesReceived, long TotalBytesToReceive, int ProgressPercentage)
        {
            lblFileName.Text = FileName;
            lblPercent.Text = ProgressPercentage.ToString() + "%";
            lblFileSize.Text = BytesReceived.ToString() + "/" + TotalBytesToReceive.ToString();
            progressBar1.Maximum = (int)(TotalBytesToReceive/1024);
            progressBar1.Value = (int)(BytesReceived/1024);
        }

    }
}
