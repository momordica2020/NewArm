using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewArm.Core
{
    /// <summary>
    /// 响应键鼠操作的 Windows API
    /// </summary>
    public class WinApi
    {
        
        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public INPUTUNION u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUTUNION
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll")]
        private static extern uint GetSystemMetrics(int nIndex);



        // 键盘钩子相关
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardOrMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, nint wParam, nint lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("gdi32.dll")]
        private static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);


        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }


        /// <summary>
        /// 鼠标事件结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt; // 鼠标坐标
            public uint mouseData; // 额外数据（如滚轮方向）
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }



        [StructLayout(LayoutKind.Sequential)]
        struct MONITORINFO
        {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // 系统 DPI 设置
        private const int SM_CXSCREEN = 0; // 屏幕宽度
        private const int SM_CYSCREEN = 1; // 屏幕高度
        private const int MONITOR_DEFAULTTOPRIMARY = 1;

        public delegate IntPtr LowLevelKeyboardOrMouseProc(int nCode, nint wParam, nint lParam);


        // 输入类型和标志
        private const uint INPUT_MOUSE = 0;
        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_UNICODE = 0x0004;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

        #region 键盘虚拟键码

        // 字母键 (A-Z)
        public const ushort VK_A = 0x41;
        public const ushort VK_B = 0x42;
        public const ushort VK_C = 0x43;
        public const ushort VK_D = 0x44;
        public const ushort VK_E = 0x45;
        public const ushort VK_F = 0x46;
        public const ushort VK_G = 0x47;
        public const ushort VK_H = 0x48;
        public const ushort VK_I = 0x49;
        public const ushort VK_J = 0x4A;
        public const ushort VK_K = 0x4B;
        public const ushort VK_L = 0x4C;
        public const ushort VK_M = 0x4D;
        public const ushort VK_N = 0x4E;
        public const ushort VK_O = 0x4F;
        public const ushort VK_P = 0x50;
        public const ushort VK_Q = 0x51;
        public const ushort VK_R = 0x52;
        public const ushort VK_S = 0x53;
        public const ushort VK_T = 0x54;
        public const ushort VK_U = 0x55;
        public const ushort VK_V = 0x56;
        public const ushort VK_W = 0x57;
        public const ushort VK_X = 0x58;
        public const ushort VK_Y = 0x59;
        public const ushort VK_Z = 0x5A;

        // 数字键 (0-9)
        public const ushort VK_0 = 0x30;
        public const ushort VK_1 = 0x31;
        public const ushort VK_2 = 0x32;
        public const ushort VK_3 = 0x33;
        public const ushort VK_4 = 0x34;
        public const ushort VK_5 = 0x35;
        public const ushort VK_6 = 0x36;
        public const ushort VK_7 = 0x37;
        public const ushort VK_8 = 0x38;
        public const ushort VK_9 = 0x39;

        // 小键盘数字键
        public const ushort VK_NUMPAD0 = 0x60;
        public const ushort VK_NUMPAD1 = 0x61;
        public const ushort VK_NUMPAD2 = 0x62;
        public const ushort VK_NUMPAD3 = 0x63;
        public const ushort VK_NUMPAD4 = 0x64;
        public const ushort VK_NUMPAD5 = 0x65;
        public const ushort VK_NUMPAD6 = 0x66;
        public const ushort VK_NUMPAD7 = 0x67;
        public const ushort VK_NUMPAD8 = 0x68;
        public const ushort VK_NUMPAD9 = 0x69;

        // 功能键 (F1-F24)
        public const ushort VK_F1 = 0x70;
        public const ushort VK_F2 = 0x71;
        public const ushort VK_F3 = 0x72;
        public const ushort VK_F4 = 0x73;
        public const ushort VK_F5 = 0x74;
        public const ushort VK_F6 = 0x75;
        public const ushort VK_F7 = 0x76;
        public const ushort VK_F8 = 0x77;
        public const ushort VK_F9 = 0x78;
        public const ushort VK_F10 = 0x79;
        public const ushort VK_F11 = 0x7A;
        public const ushort VK_F12 = 0x7B;
        public const ushort VK_F13 = 0x7C;
        public const ushort VK_F14 = 0x7D;
        public const ushort VK_F15 = 0x7E;
        public const ushort VK_F16 = 0x7F;
        public const ushort VK_F17 = 0x80;
        public const ushort VK_F18 = 0x81;
        public const ushort VK_F19 = 0x82;
        public const ushort VK_F20 = 0x83;
        public const ushort VK_F21 = 0x84;
        public const ushort VK_F22 = 0x85;
        public const ushort VK_F23 = 0x86;
        public const ushort VK_F24 = 0x87;

        // 控制键
        public const ushort VK_LCONTROL = 0xA2;
        public const ushort VK_RCONTROL = 0xA3;
        public const ushort VK_CONTROL = 0x11;
        public const ushort VK_LSHIFT = 0xA0;
        public const ushort VK_RSHIFT = 0xA1;
        public const ushort VK_SHIFT = 0x10;
        public const ushort VK_LMENU = 0xA4;
        public const ushort VK_RMENU = 0xA5;
        public const ushort VK_MENU = 0x12;
        public const ushort VK_LWIN = 0x5B;
        public const ushort VK_RWIN = 0x5C;

        // 常用符号键
        public const ushort VK_OEM_1 = 0xBA;      // 分号 ; 或 :
        public const ushort VK_OEM_PLUS = 0xBB;   // 加号 + 或 =
        public const ushort VK_OEM_COMMA = 0xBC;  // 逗号 , 或 <
        public const ushort VK_OEM_MINUS = 0xBD;  // 减号 - 或 _
        public const ushort VK_OEM_PERIOD = 0xBE; // 句号 . 或 >
        public const ushort VK_OEM_2 = 0xBF;      // 斜杠 / 或 ?
        public const ushort VK_OEM_3 = 0xC0;      // 反引号 ` 或 ~
        public const ushort VK_OEM_4 = 0xDB;      // 左方括号 [ 或 {
        public const ushort VK_OEM_5 = 0xDC;      // 反斜杠 \ 或 |
        public const ushort VK_OEM_6 = 0xDD;      // 右方括号 ] 或 }
        public const ushort VK_OEM_7 = 0xDE;      // 单引号 ' 或 "
        public const ushort VK_OEM_8 = 0xDF;      // 通常未定义

        // 导航和编辑键
        public const ushort VK_RETURN = 0x0D;     // Enter
        public const ushort VK_ESCAPE = 0x1B;     // Esc
        public const ushort VK_BACK = 0x08;       // Backspace
        public const ushort VK_TAB = 0x09;        // Tab
        public const ushort VK_SPACE = 0x20;      // 空格
        public const ushort VK_DELETE = 0x2E;     // Delete
        public const ushort VK_INSERT = 0x2D;     // Insert
        public const ushort VK_HOME = 0x24;       // Home
        public const ushort VK_END = 0x23;        // End
        public const ushort VK_PRIOR = 0x21;      // Page Up
        public const ushort VK_NEXT = 0x22;       // Page Down

        // 方向键
        public const ushort VK_LEFT = 0x25;       // 左箭头
        public const ushort VK_UP = 0x26;         // 上箭头
        public const ushort VK_RIGHT = 0x27;      // 右箭头
        public const ushort VK_DOWN = 0x28;       // 下箭头

        // 小键盘操作键
        public const ushort VK_ADD = 0x6B;        // 小键盘 +
        public const ushort VK_SUBTRACT = 0x6D;   // 小键盘 -
        public const ushort VK_MULTIPLY = 0x6A;   // 小键盘 *
        public const ushort VK_DIVIDE = 0x6F;     // 小键盘 /
        public const ushort VK_DECIMAL = 0x6E;    // 小键盘 .
        public const ushort VK_NUMLOCK = 0x90;    // Num Lock
        public const ushort VK_SEPARATOR = 0x6C;  // 小键盘 Enter

        // 其他功能键
        public const ushort VK_CAPITAL = 0x14;    // Caps Lock
        public const ushort VK_SCROLL = 0x91;     // Scroll Lock
        public const ushort VK_PAUSE = 0x13;      // Pause/Break
        public const ushort VK_SNAPSHOT = 0x2C;   // Print Screen
        public const ushort VK_APPS = 0x5D;       // 菜单键

        // 多媒体和特殊键
        public const ushort VK_VOLUME_MUTE = 0xAD;    // 静音
        public const ushort VK_VOLUME_DOWN = 0xAE;    // 音量减
        public const ushort VK_VOLUME_UP = 0xAF;      // 音量加
        public const ushort VK_MEDIA_NEXT_TRACK = 0xB0; // 下一曲
        public const ushort VK_MEDIA_PREV_TRACK = 0xB1; // 上一曲
        public const ushort VK_MEDIA_STOP = 0xB2;      // 停止播放
        public const ushort VK_MEDIA_PLAY_PAUSE = 0xB3; // 播放/暂停
        public const ushort VK_BROWSER_BACK = 0xA6;    // 浏览器后退
        public const ushort VK_BROWSER_FORWARD = 0xA7; // 浏览器前进

        #endregion

        #region 钩子类型 (WH_*)
        public const int WH_CALLWNDPROC = 4;        // 捕获窗口消息（处理前）
        public const int WH_CALLWNDPROCRET = 12;    // 捕获窗口消息处理后的返回值
        public const int WH_CBT = 5;                // 捕获计算机训练事件（如窗口激活、移动）
        public const int WH_DEBUG = 9;              // 调试钩子，监控其他钩子
        public const int WH_FOREGROUNDIDLE = 11;    // 捕获前台线程空闲事件
        public const int WH_GETMESSAGE = 3;         // 捕获 GetMessage/PeekMessage 消息
        public const int WH_JOURNALPLAYBACK = 1;    // 回放记录的输入事件
        public const int WH_JOURNALRECORD = 0;      // 记录输入事件
        public const int WH_KEYBOARD = 2;           // 应用程序级键盘钩子
        public const int WH_KEYBOARD_LL = 13;       // 低级键盘钩子（全局）
        public const int WH_MOUSE = 7;              // 应用程序级鼠标钩子
        public const int WH_MOUSE_LL = 14;          // 低级鼠标钩子（全局）
        public const int WH_MSGFILTER = -1;         // 捕获线程内消息（如对话框、菜单）
        public const int WH_SHELL = 10;             // 捕获 Shell 事件（如窗口激活）
        public const int WH_SYSMSGFILTER = 6;       // 捕获系统消息（如对话框、菜单）
        #endregion

        #region 常见消息类型 (WM_*)
        // 键盘相关消息
        public const int WM_KEYDOWN = 0x0100;       // 按键按下
        public const int WM_KEYUP = 0x0101;         // 按键释放
        public const int WM_SYSKEYDOWN = 0x0104;    // 系统按键按下（如 Alt+键）
        public const int WM_SYSKEYUP = 0x0105;      // 系统按键释放

        // 鼠标相关消息
        public const int WM_LBUTTONDOWN = 0x0201;   // 左键按下
        public const int WM_LBUTTONUP = 0x0202;     // 左键释放
        public const int WM_LBUTTONDBLCLK = 0x0203; // 左键双击
        public const int WM_RBUTTONDOWN = 0x0204;   // 右键按下
        public const int WM_RBUTTONUP = 0x0205;     // 右键释放
        public const int WM_RBUTTONDBLCLK = 0x0206; // 右键双击
        public const int WM_MBUTTONDOWN = 0x0207;   // 中键按下
        public const int WM_MBUTTONUP = 0x0208;     // 中键释放
        public const int WM_MBUTTONDBLCLK = 0x0209; // 中键双击
        public const int WM_MOUSEMOVE = 0x0200;     // 鼠标移动
        public const int WM_MOUSEWHEEL = 0x020A;    // 鼠标滚轮
        public const int WM_MOUSEHWHEEL = 0x020E;   // 水平滚轮

        // 其他相关消息
        public const int WM_HOTKEY = 0x0312;        // 全局热键消息
        public const int WM_ACTIVATE = 0x0006;      // 窗口激活
        public const int WM_CREATE = 0x0001;        // 窗口创建
        public const int WM_DESTROY = 0x0002;       // 窗口销毁
        public const int WM_CLOSE = 0x0010;         // 窗口关闭
        public const int WM_QUIT = 0x0012;          // 应用程序退出
        #endregion  


        /// <summary>
        /// 键盘按下和释放指定虚拟键码
        /// </summary>
        public static void KeyPress(ushort virtualKey)
        {
            INPUT[] inputs = new INPUT[2];
            // 按下
            inputs[0] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = virtualKey,
                        wScan = 0,
                        dwFlags = 0,
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };
            // 释放
            inputs[1] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = virtualKey,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(10); // 确保按键生效
        }


        /// <summary>
        /// 键盘按下指定虚拟键码
        /// </summary>
        public static void KeyDown(ushort virtualKey)
        {
            INPUT[] inputs = new INPUT[1];
            // 按下
            inputs[0] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = virtualKey,
                        wScan = 0,
                        dwFlags = 0,
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }


        /// <summary>
        /// 键盘释放指定虚拟键码
        /// </summary>
        public static void KeyUp(ushort virtualKey)
        {
            INPUT[] inputs = new INPUT[1];
           
            // 释放
            inputs[0] = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = virtualKey,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }


        /// <summary>
        /// 模拟输入 Unicode 字符串，绕过输入法
        /// </summary>
        public static void TextInput(string text)
        {
            foreach (char c in text)
            {
                INPUT[] inputs = new INPUT[2];
                // 按下
                inputs[0] = new INPUT
                {
                    type = INPUT_KEYBOARD,
                    u = new INPUTUNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = c,
                            dwFlags = KEYEVENTF_UNICODE,
                            time = 0,
                            dwExtraInfo = GetMessageExtraInfo()
                        }
                    }
                };
                // 释放
                inputs[1] = new INPUT
                {
                    type = INPUT_KEYBOARD,
                    u = new INPUTUNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = c,
                            dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP,
                            time = 0,
                            dwExtraInfo = GetMessageExtraInfo()
                        }
                    }
                };
                SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
                Thread.Sleep(10); // 确保字符顺序
            }
        }


        /// <summary>
        /// 模拟鼠标点击
        /// </summary>
        /// <param name="button">left, right, middle</param>
        public static void Click(string button = "left", int interval = 10)
        {
            uint downFlag, upFlag;
            switch (button.ToLower())
            {
                case "right":
                    downFlag = MOUSEEVENTF_RIGHTDOWN;
                    upFlag = MOUSEEVENTF_RIGHTUP;
                    break;
                case "middle":
                    downFlag = MOUSEEVENTF_MIDDLEDOWN;
                    upFlag = MOUSEEVENTF_MIDDLEUP;
                    break;
                default: // left
                    downFlag = MOUSEEVENTF_LEFTDOWN;
                    upFlag = MOUSEEVENTF_LEFTUP;
                    break;
            }

            INPUT[] inputs = new INPUT[2];
            // 按下
            inputs[0] = new INPUT
            {
                type = INPUT_MOUSE,
                u = new INPUTUNION
                {
                    mi = new MOUSEINPUT
                    {
                        dx = 0,
                        dy = 0,
                        mouseData = 0,
                        dwFlags = downFlag,
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };
            // 释放
            inputs[1] = new INPUT
            {
                type = INPUT_MOUSE,
                u = new INPUTUNION
                {
                    mi = new MOUSEINPUT
                    {
                        dx = 0,
                        dy = 0,
                        mouseData = 0,
                        dwFlags = upFlag,
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(interval); // 确保点击生效
        }

        /// <summary>
        /// 模拟鼠标按下
        /// </summary>
        /// <param name="button">left, right, middle</param>
        public static void MouseDown(string button = "left")
        {
            uint downFlag;
            switch (button.ToLower())
            {
                case "right":
                    downFlag = MOUSEEVENTF_RIGHTDOWN;
                    break;
                case "middle":
                    downFlag = MOUSEEVENTF_MIDDLEDOWN;
                    break;
                default: // left
                    downFlag = MOUSEEVENTF_LEFTDOWN;
                    break;
            }

            INPUT[] inputs = new INPUT[1];
            // 按下
            inputs[0] = new INPUT
            {
                type = INPUT_MOUSE,
                u = new INPUTUNION
                {
                    mi = new MOUSEINPUT
                    {
                        dx = 0,
                        dy = 0,
                        mouseData = 0,
                        dwFlags = downFlag,
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }


        /// <summary>
        /// 模拟鼠标释放
        /// </summary>
        /// <param name="button">left, right, middle</param>
        public static void MouseUp(string button = "left")
        {
            uint upFlag;
            switch (button.ToLower())
            {
                case "right":
                    upFlag = MOUSEEVENTF_RIGHTUP;
                    break;
                case "middle":
                    upFlag = MOUSEEVENTF_MIDDLEUP;
                    break;
                default: // left
                    upFlag = MOUSEEVENTF_LEFTUP;
                    break;
            }

            INPUT[] inputs = new INPUT[1];
            // 释放
            inputs[0] = new INPUT
            {
                type = INPUT_MOUSE,
                u = new INPUTUNION
                {
                    mi = new MOUSEINPUT
                    {
                        dx = 0,
                        dy = 0,
                        mouseData = 0,
                        dwFlags = upFlag,
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        /// <summary>
        /// 模拟鼠标移动到指定相对位置
        /// </summary>
        public static void MouseMove(int dx, int dy)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0] = new INPUT
            {
                type = INPUT_MOUSE,
                u = new INPUTUNION
                {
                    mi = new MOUSEINPUT
                    {
                        dx = dx,
                        dy = dy,
                        mouseData = 0,
                        dwFlags = MOUSEEVENTF_MOVE,
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(10); // 确保移动生效
        }


        /// <summary>
        /// 模拟鼠标移动到屏幕上的绝对位置（像素坐标）
        /// </summary>
        /// <param name="x">目标 X 坐标（像素）</param>
        /// <param name="y">目标 Y 坐标（像素）</param>
        public static void MouseMoveAbsolute(int x, int y)
        {
            // 获取屏幕分辨率
            int screenWidth = (int)GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = (int)GetSystemMetrics(SM_CYSCREEN);

            // 将像素坐标映射到 0-65535 范围
            int absoluteX = (x * 65535) / screenWidth;
            int absoluteY = (y * 65535) / screenHeight;

            INPUT[] inputs = new INPUT[1];
            inputs[0] = new INPUT
            {
                type = INPUT_MOUSE,
                u = new INPUTUNION
                {
                    mi = new MOUSEINPUT
                    {
                        dx = absoluteX,
                        dy = absoluteY,
                        mouseData = 0,
                        dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE,
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(10);
        }


















        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);


        private const int DRIVERVERSION = 0;//设备驱动程序版本
        private const int TECHNOLOGY = 2;
        private const int HORZSIZE = 4; // 物理屏幕的宽度（毫米）
        private const int VERTSIZE = 6; // 物理屏幕的高度（毫米）
        private const int HORZRES = 8; // 水平分辨率
        private const int VERTRES = 10; // 垂直分辨率
        private const int BITSPIXEL = 12; // 像素相连颜色位数
        private const int PLANES = 14;
        private const int NUMBRUSHES = 16;
        private const int NUMPENS = 18;
        private const int NUMMARKERS = 20;
        private const int NUMFONTS = 22;
        private const int NUMCOLORS = 24;
        private const int PDEVICESIZE = 26;
        private const int CURVECAPS = 28;
        private const int LINECAPS = 30;
        private const int POLYGONALCAPS = 32;
        private const int TEXTCAPS = 34;
        private const int CLIPCAPS = 36;
        private const int RASTERCAPS = 38;
        private const int ASPECTX = 40;
        private const int ASPECTY = 42;
        private const int ASPECTXY = 44;
        private const int SHADEBLENDCAPS = 45;
        private const int LOGPIXELSX = 88;
        private const int LOGPIXELSY = 90;
        private const int SIZEPALETTE = 104;
        private const int NUMRESERVED = 106;
        private const int COLORRES = 108;
        private const int PHYSICALWIDTH = 110;
        private const int PHYSICALHEIGHT = 111;
        private const int PHYSICALOFFSETX = 112;
        private const int PHYSICALOFFSETY = 113;
        private const int SCALINGFACTORX = 114; // 打印机x轴的比例系数
        private const int SCALINGFACTORY = 115; // 打印机y轴的比例系数
        private const int VREFRESH = 116;
        private const int DESKTOPHORZRES = 118; // 真实水平分辨率
        private const int DESKTOPVERTRES = 117; // 真实垂直分辨率
        private const int BLTALIGNMENT = 119;


        
        

        public static (int Width, int Height) GetTrueScreenResolution()
        {
            IntPtr hdc = GetDC(IntPtr.Zero); // 获取主屏幕的设备上下文
            int realWidth = GetDeviceCaps(hdc, DESKTOPHORZRES);
            int realHeight = GetDeviceCaps(hdc, DESKTOPVERTRES);
            ReleaseDC(IntPtr.Zero, hdc);
            return (realWidth, realHeight);
        }



        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

        private const uint MONITOR_DEFAULTTONEAREST = 2;
        private const int MDT_EFFECTIVE_DPI = 0;

        
        /// <summary>
        /// 获取显示器的 DPI 缩放比例
        /// 本机150%无法正确取得，故而在此直接返回1.5f
        /// </summary>
        /// <param name="hMonitor"></param>
        /// <returns></returns>
        public static float GetDpiScale(IntPtr hMonitor = (IntPtr)0)
        {
            return 1.5f;

            const int CCHDEVICENAME = 32;
            const int LOGPIXELSX = 88;



            IntPtr hdc = GetDC(IntPtr.Zero);
            int dpi = GetDeviceCaps(hdc, LOGPIXELSX);
            ReleaseDC(IntPtr.Zero, hdc);
            return dpi / 96.0f; // 96 是标准 DPI
        }
        //public static (uint DpiX, uint DpiY) GetDpiForCurrentMonitor()
        //{
        //    var hwnd = IntPtr.Zero; // 使用默认屏幕
        //    var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
        //    GetDpiForMonitor(monitor, MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY);
        //    return (dpiX, dpiY);
        //}



        public static void drawScreen()
        {
            try
            {
                // 获取鼠标位置
                if (!GetCursorPos(out POINT mousePoint))
                {
                    Console.WriteLine("无法获取鼠标位置");
                    return;
                }
                // 获取 DPI 缩放比例
                float dpiScale = GetDpiScale();
                // 获取物理显示屏长度宽度
                (int realWidth, int realHeight) = GetTrueScreenResolution();
                // 计算鼠标在物理尺寸下的真实坐标
                var realMousePoint = new Point((int)(mousePoint.X * dpiScale),(int)( mousePoint.Y * dpiScale));
                

                //// 计算显示器尺寸（物理像素）
                //int width = monitorInfo.rcMonitor.Right - monitorInfo.rcMonitor.Left;
                //int height = monitorInfo.rcMonitor.Bottom - monitorInfo.rcMonitor.Top;

                // 创建位图捕获屏幕
                using (Bitmap screenshot = new Bitmap(realWidth, realHeight, PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(screenshot))
                    {
                        // 捕获整个显示器屏幕
                        g.CopyFromScreen(
                            0,
                            0,
                            0,
                            0,
                            new Size(realWidth, realHeight),
                            CopyPixelOperation.SourceCopy
                        );
                    }

                    // 标记鼠标位置（红色圆点）
                    using (Graphics g = Graphics.FromImage(screenshot))
                    {
                        // 绘制红色圆点（半径 5 像素）
                        using (Brush brush = new SolidBrush(Color.Red))
                        {
                            int radius = 5;
                            g.FillEllipse(brush, realMousePoint.X - radius, realMousePoint.Y - radius, radius * 2, radius * 2);
                        }
                    }

                    // 保存截图到本地
                    string outputPath = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    screenshot.Save(outputPath, ImageFormat.Png);
                   // Console.WriteLine($"截图已保存到: {outputPath}");
                   // Console.WriteLine($"鼠标位置 (物理): ({mousePoint.X}, {mousePoint.Y})");
                   // Console.WriteLine($"鼠标位置 (相对于显示器): ({relativeX}, {relativeY})");
                   //Console.WriteLine($"DPI 缩放比例: {dpiScale * 100}%");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
            }
        }

        //private static float GetDpiScale()
        //{
        //    using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
        //    {
        //        return g.DpiX / 96.0f; // 96 是标准 DPI
        //    }
        //}

        public static Color GetColor()
        {
            try
            {
                // 获取鼠标当前位置
                // 获取鼠标位置
                if (GetCursorPos(out POINT mousePoint))
                {
                    float dpiScale = GetDpiScale();
                    (int realWidth, int realHeight) = GetTrueScreenResolution();
                    var realMousePoint = new Point((int)(mousePoint.X * dpiScale), (int)(mousePoint.Y * dpiScale));

                    IntPtr hdc = GetDC(IntPtr.Zero);


                    // 获取像素颜色
                    uint pixel = GetPixel(hdc, realMousePoint.X, realMousePoint.Y); // 使用原始坐标
                    ReleaseDC(IntPtr.Zero, hdc);

                    // 提取 RGB 值
                    byte r = (byte)(pixel & 0xFF);
                    byte g = (byte)((pixel >> 8) & 0xFF);
                    byte b = (byte)((pixel >> 16) & 0xFF);
                    return Color.FromArgb(r, g, b);

                    //Console.WriteLine($"鼠标位置 (物理): ({point.X}, {point.Y})");
                    //Console.WriteLine($"鼠标位置 (逻辑): ({logicalX}, {logicalY})");
                    //Console.WriteLine($"RGB 颜色值: ({r}, {g}, {b})");
                    //Console.WriteLine($"DPI 缩放比例: {dpiScale * 100}%");
                }
                else
                {
                    Console.WriteLine("无法获取鼠标位置");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
            }
            return Color.White;
        }






        #region mouse functions
        //[System.Runtime.InteropServices.DllImport("user32")]
        //private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        ////标示是否采用绝对坐标 
        //const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        //private void mouseMoveTo(int x, int y)
        //{
        //    mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, (int)(x * tx), (int)(y * ty), 0, 0);
        //    Thread.Sleep(50);
        //}

        //private void mouseMoveToD(int dx, int dy)
        //{
        //    mouse_event(MOUSEEVENTF_MOVE, (int)(1 * dx), (int)(1 * dy), 0, 0);
        //    Thread.Sleep(50);
        //}

        //private void mouseLeftClick()
        //{
        //    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        //}

        //private void mouseClickAt(int x, int y)
        //{
        //    mouseMoveTo(x, y);
        //    mouseLeftClick();
        //}

        //private void mouseClickAtD(int dx, int dy)
        //{
        //    mouseMoveToD(dx, dy);
        //    mouseLeftClick();
        //}

        //private void mouseLeftDown()
        //{
        //    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        //}

        //private void mouseLeftUp()
        //{
        //    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        //}
        #endregion
    }
}
