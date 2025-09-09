using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewArm.TaskFunctions
{
    public class TaskConfig
    {
        public string TaskId;
        public string[] Params;
        public ushort[] HotKey;
        public int Cd;
        public bool StopWhenKeyUp = false;
    }
}
