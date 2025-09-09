using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewArm.Core
{
    public enum LogType
    {
        Start,Stop,Text,Error,State
    }


    public class Log
    {
        public string Content = "";
        public LogType Type = LogType.Text;
        public string TaskId = "";

        public static Log Start{ get{ return new Log { Content = "", Type = LogType.Start };  } }
        public static Log Stop { get { return new Log { Content = "", Type = LogType.Stop }; } }
        public static Log Text(string text) { return new Log { Content=text,Type = LogType.Text}; }
        public static Log Error(Exception ex) {return new Log { Content=$"{ex.Message}\r\n{ex.StackTrace}",Type=LogType.Error}; }
    }
}
