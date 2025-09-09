using NewArm.Core;
using NewArm.TaskFunctions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NewArm
{
    /// <summary>
    /// 任务模板，包括定时执行，并响应键盘按键。
    /// </summary>
    public abstract class TimerTask
    {
        public bool isRunning;
        private IntPtr _hookId = IntPtr.Zero;

        ///// <summary>
        ///// 执行间隔
        ///// </summary>
        //public int intervalMs;
        public TaskConfig Config;
        private Timer _timer;

        private int _cd_interval = 300;
        private DateTime _last_trigger_time = DateTime.Now;

        //public string[] trigger_Codes;
        private Dictionary<ushort, bool> trigger_state;

        //private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        

        private readonly WinApi.LowLevelKeyboardOrMouseProc _keyboardProcDelegate;

        public LogEvent LogReportAction = null;
        protected void log(Log msg)
        {
            if(Config!=null)
            msg.TaskId = Config.TaskId;
            if (LogReportAction != null)
            {
                LogReportAction(msg);
            }
        }

        public TimerTask()
        {
            isRunning = false;
            //_cts = new CancellationTokenSource();
            _timer = new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
            _keyboardProcDelegate = KeyboardProc; // 保存委托引用
        }

        public void Start()
        {
            if (isRunning)
            {
                log(Log.Text("任务已在运行"));
                return;
            }

            try
            {



                // 启动定时器
                isRunning = true;
                _timer.Change(Config.Cd, Timeout.Infinite);
                log(Log.Start);
            }
            catch (Exception ex)
            {
                log(Log.Error(ex));
            }
        }

        /// <summary>
        /// 初始化该任务模块。传入激活按键、初始化参数、任务执行间隔
        /// </summary>
        /// <param name="trigger_codes"></param>
        /// <param name="interval"></param>
        /// <param name="param"></param>
        public void Init(TaskConfig config)
        {
            log(Log.Text($"初始化{this.GetType().Name}脚本,热键{string.Join("+", config.HotKey.Select(k => ((System.Windows.Forms.Keys)k).ToString()))},间隔{config.Cd}ms {string.Join("\r\n", config.Params ?? [])}"));

            Config = config;
            //trigger_Codes = trigger_codes;
            if (Config.HotKey.Length > 0)
            {
                trigger_state = new Dictionary<ushort, bool>();
                foreach (ushort code in Config.HotKey)
                {
                    trigger_state[code] = false;
                }
            }

            // 设置键盘钩子
            if (_hookId != IntPtr.Zero)
            {
                WinApi.UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                _hookId = WinApi.SetWindowsHookEx(WinApi.WH_KEYBOARD_LL, _keyboardProcDelegate, KeyboardHook.GetModuleHandle(curModule.ModuleName), 0);
            }

            _init();
        }
        protected abstract void _init();


        protected abstract void _work();

        public void Stop()
        {
            if (!isRunning)
            {
                log(Log.Stop);
                return;
            }

            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            isRunning = false;

            log(Log.Stop);
        }

        private void OnTimerElapsed(object state)
        {
            try
            {
                _work();

                if (isRunning)
                {
                    // 重新设置定时器
                    _timer.Change(Config.Cd, Timeout.Infinite);
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
            if (nCode >= 0)
            {
                ushort vkCode = (ushort)Marshal.ReadInt32(lParam);
                switch ((int)wParam)
                {
                    case WinApi.WM_KEYDOWN:
                    case WinApi.WM_SYSKEYDOWN:

                        //log(LogInfo.Info($"{((System.Windows.Forms.Keys)vkCode).ToString()} Down"));
                        if (trigger_state.ContainsKey(vkCode))
                        {
                            trigger_state[vkCode] = true;

                        }
                        break;
                    case WinApi.WM_KEYUP:
                    case WinApi.WM_SYSKEYUP:
                        //log(LogInfo.Info($"{((System.Windows.Forms.Keys)vkCode).ToString()} Up"));
                        if (trigger_state.ContainsKey(vkCode))
                        {
                            trigger_state[vkCode] = false;

                        }
                        break;
                }
            }

            bool trigger = true;
            foreach (var val in trigger_state.Values)
            {
                if(val == false)
                {
                    trigger = false;
                    break;
                }
            }
            if (trigger)
            {
                if ((DateTime.Now - _last_trigger_time).TotalMilliseconds > _cd_interval)
                {
                    _last_trigger_time = DateTime.Now;
                    if (isRunning)
                        Stop();
                    else
                        Start();
                }
            }
            //return (IntPtr)0;
            return WinApi.CallNextHookEx((int)_hookId, nCode, (int)wParam, lParam);
        }
    }









    //[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    //public class TaskNameAttribute : Attribute
    //{
    //    public string Description { get; }

    //    public TaskNameAttribute(string description)
    //    {
    //        Description = description;
    //    }
    //}
}
