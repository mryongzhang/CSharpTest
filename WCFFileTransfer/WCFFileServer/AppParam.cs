using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace WCFFileServer
{
     class AppParam
    {
        [Serializable]
        public struct SaveParam
        {
          public string saveDir;
        } ;

        public SaveParam _saveParam;

        public AppParam()
        {
            _saveParam = new SaveParam();
            _saveDir = "d:\\FileSave";//默认保存文件路径
        }
        /// <summary>
        /// 文件保存路径
        /// </summary>
        public string _saveDir
        {
            get
            {
                return _saveParam.saveDir;
            }
            set
            {
                _saveParam.saveDir = value;
            }
        }
        static string GetCurDir()
        {
            string str = Process.GetCurrentProcess().MainModule.FileName;
            int n = str.LastIndexOf('\\');
            if (n >= 0)
            {
                str = str.Remove(n, str.Length - n);
            }
            return str;
        }
        /// <summary>
        /// 将上传文件路径保存到文件中
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        static public bool Save(AppParam param)
        {
            FileStream fs = null;
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();//使用BinaryFormatter进行序列化
                string strDir = GetCurDir();

                fs = new FileStream(strDir + @"\FileSevice.dat", FileMode.Create);
                formatter.Serialize(fs, param._saveParam);//将一个对象图按字节的顺序持久化到一个指定的流
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存参数失败！");
                return false;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
            return true;
        }

        static public bool Load(ref AppParam param)
        {
            FileStream fs = null;
            try
            {
                string strDir = GetCurDir();

                fs = new FileStream(strDir + @"\FileSevice.dat", FileMode.Open);

                BinaryFormatter formatter = new BinaryFormatter();//使用BinaryFormatter进行反序列化
                param._saveParam = (AppParam.SaveParam)formatter.Deserialize(fs);//反序列化文件路径
            }
            catch (Exception ex)
            {

                return false;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
            return false;
        }
    }
}
