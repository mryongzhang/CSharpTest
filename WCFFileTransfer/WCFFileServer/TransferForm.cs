using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel;
using System.Threading;

namespace WCFFileServer
{
    public partial class TransferForm : Form, ILog
    {
        public ServiceHost _myServiceHost;//使用 ServiceHost 对象加载服务、配置终结点、应用安全设置并启用侦听程序来处理传入的请求。
        public delegate bool FormAddLog(string info);
        FormAddLog _formAddLog;
        AppParam _appParam;

        public TransferForm()
        {
            InitializeComponent();
            _formAddLog = new FormAddLog(this.Log);

            _appParam = AppValue.GetParam();
            AppParam.Load(ref _appParam);

            txtSaveFilePath.Text = _appParam._saveDir;
        }

        private void TransferForm_Load(object sender, EventArgs e)
        {
            InitLog();
        }
        void InitLog()
        {
            this.CloseServer.Enabled = false;
            Rectangle rc = lvLog.ClientRectangle;
            lvLog.Clear();

            int n = (rc.Width - 17) / 5;
            lvLog.Columns.Add("序号", n, System.Windows.Forms.HorizontalAlignment.Left);
            lvLog.Columns.Add("消息", n * 3, System.Windows.Forms.HorizontalAlignment.Left);
            lvLog.Columns.Add("时间", n, System.Windows.Forms.HorizontalAlignment.Left);

        }
        /// <summary>
        /// 记录上传文件信息，将文件信息显示在ListBox
        /// </summary>
        /// <param name="info">文件路径</param>
        /// <returns></returns>
        public bool Log(string info)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(_formAddLog, info);
                return true;
            }
            string[] lvData = new string[3];
            lvData[0] = (lvLog.Items.Count + 1).ToString();
            lvData[1] = info;
            lvData[2] = DateTime.Now.ToString();
            ListViewItem lvItem = new ListViewItem(lvData);
            lvLog.Items.Add(lvItem);
            return true;
        }
        /// <summary>
        /// 修改保存文件路径
        /// </summary>
        private void btnChoosePath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dir = new FolderBrowserDialog();
            if (dir.ShowDialog() == DialogResult.OK)
            {
                txtSaveFilePath.Text = dir.SelectedPath;
                _appParam._saveDir = txtSaveFilePath.Text;
                AppParam.Save(_appParam);//保存修改路径
            }
            AppParam _appParam3 = AppValue.GetParam();
        }
        /// <summary>
        /// 启动服务
        /// </summary>
        private void OpenServer_Click(object sender, EventArgs e)
        {
            if (_myServiceHost == null)
            {
                Thread threadRead = new Thread(new ThreadStart(ServerStart));
                threadRead.Start();
                this.OpenServer.Enabled = false;
                this.CloseServer.Enabled = true;
                lblMsg.Text = "服务已经开启";
            }
            else
            {
                _myServiceHost.Close();
                _myServiceHost = null;
                this.OpenServer.Enabled = true;
                Program.Get_ILog().Log("停止服务");
                lblMsg.Text = "服务已经停止";
            }
        }

        void ServerStart()
        {
            try
            {
                _myServiceHost = new ServiceHost(typeof(WCFFileServer.Transfer));//实例化WCF服务对象
                _myServiceHost.Open();
                lblMsg.Text = "服务已经开启";
            }
            catch (Exception ex)
            {
                Program.Get_ILog().Log(ex.Message);
                return;
            }
            Program.Get_ILog().Log("启动成功");
        }

        private void CloseServer_Click(object sender, EventArgs e)
        {
            _myServiceHost.Close();
            _myServiceHost = null;
            this.OpenServer.Enabled = true;
            this.CloseServer.Enabled = false;
            lblMsg.Text = "服务已经停止";
            Program.Get_ILog().Log("停止服务");
        }
    }
}
