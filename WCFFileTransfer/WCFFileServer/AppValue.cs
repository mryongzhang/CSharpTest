using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCFFileServer
{
    static class AppValue
    {
        public static AppParam _appParam = new AppParam();
        static public AppParam GetParam()
        {
            return _appParam;
        }
    }
}
