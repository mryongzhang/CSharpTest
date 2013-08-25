using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileOnlineEdit
{
    public partial class StatusForm : Form
    {
        public enum StatusType { Downloading, FileOpening, FileOpened, Uploading, Uploaded };

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
            lblFileSize.Text = ((float)BytesReceived/1024).ToString() + "KB/" + ((float)TotalBytesToReceive/1024).ToString() + "KB";
            progressBar1.Maximum = (int)(TotalBytesToReceive/1024);
            progressBar1.Value = (int)(BytesReceived/1024);
        }

        public void ShowStatus(StatusType status)
        {
            switch (status)
            {
                case StatusType.Downloading:
                    this.Text = "文件下载中";
                    panel1.Visible = true;
                    panel2.Visible = false;
                    panel3.Visible = false;
                    this.Show();
                    break;
                case StatusType.FileOpening:
                    this.Text = "文件打开中";
                    panel1.Visible = false;
                    panel2.Visible = true;
                    panel3.Visible = false;
                    this.Show();
                    break;

                case StatusType.FileOpened:
                    this.Hide();
                    break;
                case StatusType.Uploading:
                    this.Text = "文件上传中";
                    panel1.Visible = false;
                    panel2.Visible = false;
                    panel3.Visible = true;
                    this.Show();
                    break;
                case StatusType.Uploaded:
                    this.Hide();
                    break;

                default:
                    break;
            }

        }

    }
}
