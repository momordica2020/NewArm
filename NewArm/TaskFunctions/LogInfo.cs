using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewArm.TaskFunctions
{
    public enum LogType
    {
        Start,Stop,Info,Error
    }
    public class LogInfo
    {
        public string taskName = "";
        public string text = "";
        public LogType type = LogType.Info;

        public static LogInfo Start{ get{ return new LogInfo { text = "", type = LogType.Start };  } }
        public static LogInfo Stop { get { return new LogInfo { text = "", type = LogType.Stop }; } }
        public static LogInfo Info(string text) { return new LogInfo {text=text,type = LogType.Info}; }
        public static LogInfo Error(Exception ex) {return new LogInfo {text=$"{ex.Message}\r\n{ex.StackTrace}",type=LogType.Error}; }
    }
}
