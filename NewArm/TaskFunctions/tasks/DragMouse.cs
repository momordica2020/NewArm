using Emgu.CV.XPhoto;
using NewArm.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NewArm.TaskFunctions.tasks
{
    internal class DragMouse : TimerTask
    {


        //   0
        // 3   1
        //   2
        int direction = 0;

        protected override void _init()
        {
            if (Config.Params!=null && !string.IsNullOrWhiteSpace(Config.Params[0]))
            direction = int.Parse(Config.Params[0]);
           // log(LogInfo.Info($"{direction}"));
            
        }

        protected override void _work()
        {

            int len = 1000;
            int dx = 0;
            int dy = 0;
            if (direction == 0) dy = -1000;
            else if (direction == 1) dx = 1000;
            else if (direction == 2) dy = 1000;
            else if (direction == 3) dx = -1000;
            int step = 25;
            //WinApi.KeyDown(WinApi.VK_SPACE);
            //Thread.Sleep(100);
            for(int i = 0; i * step < dx; i++)
            {
                if (!isRunning) break;
                WinApi.MouseMove(step, 0);
                Thread.Sleep(1);
            }
            //WinApi.KeyUp(WinApi.VK_SPACE);
            Stop();
            
        }
    }
}
