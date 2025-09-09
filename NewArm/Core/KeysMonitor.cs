using Emgu.CV.Ocl;
using Microsoft.VisualBasic.Logging;
using NewArm.TaskFunctions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NewArm.Core
{
    public class MouseState
    {
        public bool leftMouseDown;
        public bool rightMouseDown;
        public bool middleMouseDown;
        public int middleWheel;
        public Point mouseLogicLocation;
        public Point mouseRealLocation
        {
            get
            {
                if (mouseLogicLocation != Point.Empty)
                {
                    return new Point(
                        (int)(mouseLogicLocation.X * WinApi.GetDpiScale()),
                        (int)(mouseLogicLocation.Y * WinApi.GetDpiScale()));
                }
                else
                {
                    return Point.Empty;
                }
            }
        }
    }


    internal class KeysMonitor
    {
        public bool isRunning;
        private IntPtr _hookKeyId = IntPtr.Zero;
        private IntPtr _hookMouseId = IntPtr.Zero;

        ///// <summary>
        ///// 执行间隔
        ///// </summary>
        public int intervalMs = 500;
        private Timer _timer;

        private int _cd_interval = 300;
        private DateTime _last_trigger_time = DateTime.Now;

        private HashSet<ushort> trigger_keys = new HashSet<ushort>();
        MouseState mouseState = new MouseState();


        // 存储事件时间戳的队列
        private static readonly ConcurrentQueue<DateTime> keyboardEvents = new ConcurrentQueue<DateTime>();
        private static readonly ConcurrentQueue<DateTime> mouseEvents = new ConcurrentQueue<DateTime>();
        private static readonly object lockObject = new object();

        private readonly WinApi.LowLevelKeyboardOrMouseProc _keyboardProcDelegate;
        private readonly WinApi.LowLevelKeyboardOrMouseProc _mouseProcDelegate;

        public MouseStateEvent mouseStateEvent;
        public KeyboardStateEvent keyboardStateEvent;
        public KeyboardDPS keyboardDPS;
        public MouseDPS mouseDPS;
        private Queue<double> klastdps = new Queue<double>();
        private Queue<double> mlastdps = new Queue<double>();

        public LogEvent LogReportAction;
        protected void log(Log msg)
        {
            msg.TaskId = "Monitor";
            if(LogReportAction != null)  LogReportAction(msg);
        }



        public KeysMonitor()
        {
            isRunning = false;
            //_cts = new CancellationTokenSource();
            _timer = new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
            _keyboardProcDelegate = KeyboardProc; // 保存委托引用
            _mouseProcDelegate = MouseProc; // 保存委托引用

            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                _hookKeyId = WinApi.SetWindowsHookEx(WinApi.WH_KEYBOARD_LL, _keyboardProcDelegate, WinApi.GetModuleHandle(curModule.ModuleName), 0);
                _hookMouseId = WinApi.SetWindowsHookEx(WinApi.WH_MOUSE_LL, _mouseProcDelegate, WinApi.GetModuleHandle(curModule.ModuleName), 0);

            }
        }
        public void Start()
        {
            if (isRunning)
            {
                //log(Log.Text("任务已在运行"));
                return;
            }

            try
            {
                // 启动定时器
                isRunning = true;
                _timer.Change(intervalMs, Timeout.Infinite);
                log(Log.Start);
            }
            catch (Exception ex)
            {
                log(Log.Error(ex));
            }
        }
        public void Stop()
        {
            if (!isRunning)
            {
                log(Log.Stop);
                return;
            }

            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            isRunning = false;

            //log(Log.Stop);
        }



        private void OnTimerElapsed(object state)
        {
            try
            {
                DateTime now = DateTime.Now;
                DateTime oneMinuteAgo = now.AddSeconds(-1);

                // 移除旧事件
                while (keyboardEvents.TryPeek(out DateTime time) && time < oneMinuteAgo)
                    keyboardEvents.TryDequeue(out _);

                while (mouseEvents.TryPeek(out DateTime time) && time < oneMinuteAgo)
                    mouseEvents.TryDequeue(out _);

                // 获取计数
                int keyboardCount = keyboardEvents.Count; // 注意：Count 可能非原子
                int mouseCount = mouseEvents.Count;

                if (klastdps.Count > 6) klastdps.Dequeue();
                klastdps.Enqueue(keyboardCount);
                double kdps = klastdps.Sum() / klastdps.Count;

                if (mlastdps.Count > 6) mlastdps.Dequeue();
                mlastdps.Enqueue(mouseCount);
                double mdps = mlastdps.Sum() / mlastdps.Count;


                if (keyboardDPS!=null) keyboardDPS(kdps);
                if (mouseDPS != null) mouseDPS(mdps);

                if (isRunning)
                {
                    // 重置定时器
                    _timer.Change(intervalMs, Timeout.Infinite);
                }

            }
            catch (Exception ex)
            {
                log(Log.Error(ex));
                Stop();
            }

        }


        private IntPtr KeyboardProc(int nCode, nint wParam, nint lParam)
        {
            if(nCode >= 0)
            {
                ushort vkCode = (ushort)Marshal.ReadInt32(lParam);
                switch ((int)wParam)
                {
                    case WinApi.WM_KEYDOWN:
                    case WinApi.WM_SYSKEYDOWN:
                        if (!trigger_keys.Contains(vkCode))
                        {
                            keyboardEvents.Enqueue(DateTime.Now);
                            trigger_keys.Add(vkCode);
                        }
                        break;
                    case WinApi.WM_KEYUP:
                    case WinApi.WM_SYSKEYUP:
                        trigger_keys.Remove(vkCode);
                        break;
                }
                if (keyboardStateEvent != null) keyboardStateEvent(trigger_keys.Select(k=>(System.Windows.Forms.Keys)k).ToArray());
            }
            
            //return (IntPtr)0;
            return WinApi.CallNextHookEx((int)_hookKeyId, nCode, (int)wParam, lParam);
        }



        /// <summary>
        /// 鼠标钩子回调函数
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private IntPtr MouseProc(int nCode, nint wParam, nint lParam)
        {
            if (nCode >= 0)
            {
                WinApi.MSLLHOOKSTRUCT hookStruct = (WinApi.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(WinApi.MSLLHOOKSTRUCT));

                switch ((int)wParam)
                {
                    case WinApi.WM_LBUTTONDOWN:
                        if (!mouseState.leftMouseDown)
                        {
                            mouseEvents.Enqueue(DateTime.Now);
                        }
                        mouseState.leftMouseDown = true;
                        break;
                    case WinApi.WM_LBUTTONUP:
                        mouseState.leftMouseDown = false;
                        break;
                    case WinApi.WM_RBUTTONDOWN:
                        if (!mouseState.rightMouseDown)
                        {
                            mouseEvents.Enqueue(DateTime.Now);
                        }
                        mouseState.rightMouseDown = true;
                        break;
                    case WinApi.WM_RBUTTONUP:
                        mouseState.rightMouseDown = false;
                        break;
                    case WinApi.WM_MBUTTONDOWN:
                        if (!mouseState.middleMouseDown)
                        {
                            mouseEvents.Enqueue(DateTime.Now);
                        }
                        mouseState.middleMouseDown = true;
                        break;
                    case WinApi.WM_MBUTTONUP:
                        mouseState.middleMouseDown = false;
                        break;
                    case WinApi.WM_MOUSEMOVE:
                        mouseState.mouseLogicLocation = new Point(hookStruct.pt.X,hookStruct.pt.Y);
                        
                        break;
                    case WinApi.WM_MOUSEWHEEL:
                        int delta = (short)((hookStruct.mouseData >> 16) & 0xffff);
                        mouseState.middleWheel = delta;
                        break;
                }
            }
            if (mouseStateEvent != null) mouseStateEvent(mouseState);

            // 传递给下一个钩子
            return WinApi.CallNextHookEx(_hookMouseId, nCode, wParam, lParam);
        }


    }
}
