using NewArm.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewArm.TaskFunctions.tasks
{
    //[TaskName("leftclick")]
    public class LeftClicks : TimerTask
    {
        string key = "left";

        protected override void _init()
        {
            if(Config!=null && Config.Params != null)
            {
                key = Config.Params[0];
            }
        }

        protected override void _work()
        {
            WinApi.Click(key);
        }
    }
}
