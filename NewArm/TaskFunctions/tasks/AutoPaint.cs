using Emgu.CV.CvEnum;
using NewArm.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewArm.TaskFunctions.tasks
{
    public class AutoPaint : TimerTask
    {
        public AutoPaint(Log __log) : base(__log)
        {
        }

        public Rectangle paintArea;

        protected override void _init()
        {
            paintArea = new Rectangle(100, 225, 2400, 1200);
        }

        protected override void _work()
        {
            //WinApi.drawScreen();
            //WinApi.GetDpiScale();
            //WinApi.GetTrueScreenResolution();
            WinApi.Click("left", 10);
            Thread.Sleep(500);
            var targets = ScreenVision.FindTargetsOnScreen(paintArea,WinApi.GetColor(), 3, 5, 35, 5, 35);
            log(LogInfo.Info($"{targets.Count}个点,{WinApi.GetColor().ToString()}"));
            targets = targets.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            foreach (var t in targets)
            {
                if (!isRunning) break;
                var screenX = (int)((t.X + paintArea.X) / WinApi.GetDpiScale());
                var screenY = (int)((t.Y + paintArea.Y) / WinApi.GetDpiScale());
                //log(LogInfo.Info($"{screenX},{screenY}"));
                WinApi.MouseMoveAbsolute(screenX, screenY);
                WinApi.Click("left", 10);
                Thread.Sleep(10);


                if (!isRunning) break;
            }
            Stop();
        }
    }
}
