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
    public partial class Form1 : Form
    {
        public string  FileUrl { get; set; }
        public Form1(string fileurl)
        {
            InitializeComponent();
            FileUrl = fileurl;
            /*
            string dir = @"d:\";
            OfficeFile file = new OfficeFile();
            file.Download(FileUrl, dir);
            file.Open();
            */
        }

        private void button1_Click(object sender, EventArgs e)
        {/*
            //string url = @"http://www.ganedu.net/images/mugshots/2012/10/fullsize/20121027123249_.doc";
            string dir = @"d:\";
            OfficeFile file = new OfficeFile();
            file.Download(FileUrl, dir);
            file.Open();*/
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
