using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FileInterface;
using System.ServiceModel;
using System.IO;
using System.Threading;


namespace WCFFileTransfer
{
    public partial class TransferFile : Form
    {
        ITransfer _proxy;
        DataTable files = new DataTable();
        public TransferFile()
        {
            InitializeComponent();
            files.Columns.Add(new DataColumn("文件路径", typeof(string)));

            this.dataGridView1.DataSource = files;
            this.dataGridView1.Columns[0].Width = 500; 
            AddUrl();
        }
        /// <summary>
        /// WCF服务地址
        /// </summary>
        private void AddUrl()
        {
            CBSerURL.Items.Add("net.tcp://127.0.0.1/service");
            CBSerURL.SelectedIndex = 0;
        }
        /// <summary>
        /// 选择要发送的文件,添加到DataTable中
        /// </summary>
        private void btnChooseFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    for (int i = 0; i < dialog.FileNames.Length; i++)
                    {
                        DataRow type = files.NewRow();
                        type["文件路径"] = dialog.FileNames[i];
                        try
                        {
                            files.Rows.Add(type);
                        }
                        catch { continue; }
                    }
                }
            }
        }
        /// <summary>
        /// 连接文件传输服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (_proxy == null)
            {
                try
                {
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.TransferMode = TransferMode.Streamed;
                    binding.SendTimeout = new TimeSpan(0, 30, 0);
                    //利用通道创建客户端代理
                    _proxy = ChannelFactory<ITransfer>.CreateChannel(binding, new EndpointAddress(CBSerURL.Text));
                    IContextChannel obj = _proxy as IContextChannel;
                    //string s = obj.SessionId;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                btnConnect.Text = "停止";
            }
            else
            {
                _proxy = null;
                btnConnect.Text = "连接";
            }
        }
        /// <summary>
        /// 传输文件
        /// </summary>
        private void btnSendFile_Click(object sender, EventArgs e)
        {
            string filePath="";
            for (int i = 0; i < dataGridView1.Rows.Count-1; i++)
            {
                filePath = dataGridView1.Rows[i].Cells[0].Value.ToString();

                if (filePath == string.Empty)
                {
                    MessageBox.Show("请选择要传输的文件");
                    return;
                }
                if (_proxy == null)
                {
                    MessageBox.Show("服务已经断开");
                    return;
                }

                FileTransferMessage file = null;
                try
                {
                    file = new FileTransferMessage();

                    file.FileName = Path.GetFileName(filePath);
                    file.FileData = new FileStream(filePath, FileMode.Open);
                    IContextChannel obj = _proxy as IContextChannel;
 
                    FileSendThread sendThread = new FileSendThread();
                    sendThread._file = file;
                    sendThread._proxy = _proxy;
                    Thread threadRead = new Thread(new ThreadStart(sendThread.SendFile));
                    threadRead.Start();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            MessageBox.Show("文件传输成功");
        }


    }

    class FileSendThread
    {
        public FileTransferMessage _file;
        public ITransfer _proxy;
        public void SendFile()
        {
            try
            {
                _proxy.TransferFile(_file);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (_file.FileData != null)
                {
                    _file.FileData.Close();
                }
            }
        }
    }
}
