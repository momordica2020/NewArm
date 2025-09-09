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


        protected override void _init()
        {
            
        }

        protected override void _work()
        {
            WinApi.Click("left", Config.Cd);
        }
    }
}
