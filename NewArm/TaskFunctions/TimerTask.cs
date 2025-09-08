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

        //public string[] trigger_Codes;
        private Dictionary<ushort, bool> trigger_state;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        public delegate void Log(LogInfo msg);

        private Log _log;
        protected void log(LogInfo msg)
        {
            msg.taskName = this.GetType().Name;
            _log(msg);
        }

        public TimerTask(Log __log)
        {
            
            isRunning = false;
            //_cts = new CancellationTokenSource();
            _timer = new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
            _log = __log;
        }

        public void Start()
        {
            if (isRunning)
            {
                log(LogInfo.Info("任务已在运行"));
                return;
            }

            try
            {



                // 启动定时器
                isRunning = true;
                _timer.Change(Config.interval, Timeout.Infinite);
                log(LogInfo.Start);
            }
            catch (Exception ex)
            {
                log(LogInfo.Error(ex));
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
            log(LogInfo.Info($"初始化{this.GetType().Name}脚本,热键{string.Join("+", config.triggerCodes.Select(k => ((System.Windows.Forms.Keys)k).ToString()))},间隔{config.interval}ms {string.Join("\r\n", config.param ?? [])}"));

            Config = config;
            //trigger_Codes = trigger_codes;
            if (Config.triggerCodes.Length > 0)
            {
                trigger_state = new Dictionary<ushort, bool>();
                foreach (ushort code in Config.triggerCodes)
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
                _hookId = WinApi.SetWindowsHookEx(WinApi.WH_KEYBOARD_LL, KeyboardProc, KeyboardHook.GetModuleHandle(curModule.ModuleName), 0);
            }

            _init();
        }
        protected abstract void _init();


        protected abstract void _work();

        public void Stop()
        {
            if (!isRunning)
            {
                log(LogInfo.Stop);
                return;
            }

            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            isRunning = false;
            
            log(LogInfo.Stop);
        }

        private void OnTimerElapsed(object state)
        {
            try
            {
                _work();

                // 重新设置定时器
                _timer.Change(Config.interval, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                log(LogInfo.Error(ex));
                Stop();
            }
        }

        private nint KeyboardProc(int nCode, nint wParam, nint lParam)
        {
            if (nCode >= 0 && wParam == (uint)WinApi.WM_KEYDOWN)
            {
                ushort vkCode = (ushort)Marshal.ReadInt32(lParam);
                if (trigger_state.ContainsKey(vkCode))
                {
                    trigger_state[vkCode] = true;
                }
            }
            else if(nCode >= 0 && wParam == (ushort)WinApi.WM_KEYUP)
            {
                ushort vkCode = (ushort)Marshal.ReadInt32(lParam);
                if(trigger_state.ContainsKey(vkCode))
                {
                    trigger_state[vkCode] = false;
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
                if (isRunning)
                    Stop();
                else
                    Start();
            }
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
