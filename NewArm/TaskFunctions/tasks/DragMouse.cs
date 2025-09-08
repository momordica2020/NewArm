using Emgu.CV.XPhoto;
using NewArm.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NewArm.TaskFunctions.tasks
{
    internal class DragMouse : TimerTask
    {
        public DragMouse(Log __log) : base(__log)
        {
        }

        protected override void _init()
        {
            
        }

        protected override void _work()
        {
            int dx = 1100;
            int dy = 0;
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
