using Microsoft.Win32;
using NewArm.Core;
using NewArm.Properties;
using NewArm.TaskFunctions;
using NewArm.TaskFunctions.tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static NewArm.TimerTask;
using static System.Net.Mime.MediaTypeNames;

namespace NewArm
{



    public partial class Form1 : Form
    {
        //private bool getMousePositionRun = true;
        // private bool run = false;
        // delegate void sendStringDelegate(string str);
        // delegate void sendVoidDelegate();
        // int num = 0;
        // KeyboardHook k_hook;

        ConfigManager configManager;
        Config config;

        public Dictionary<string, TimerTask> timerTasks = new Dictionary<string, TimerTask>();
        KeysMonitor monitor;
        bool windowExist = false;


        public Form1()
        {

            configManager = new ConfigManager();
            config = configManager.Load();

            monitor = new KeysMonitor();
            monitor.LogReportAction = dealLog;
            monitor.keyboardDPS = PrintKeyboardDPS;
            monitor.mouseDPS = PrintMouseDPS;
            monitor.mouseStateEvent = PrintMouseState;
            monitor.keyboardStateEvent = PrintHotkeyState;

            InitializeComponent();
            this.Focus();
            comboBox1.SelectedIndex = 0;
            // loadTasks();
            // updateTasksView();

            //initHook();

            //开启捕捉鼠标位置的常驻线程
            //new Thread(workGetMousePosition).Start();


        }


        private void Form1_Shown(object sender, EventArgs e)
        {
            windowExist = true;
            updateConfigToUI();

            monitor.Start();

            

            //Init(typeof(LeftClicks), new TaskConfig { HotKey = [WinApi.VK_F2], Cd = config.cdTimeMs });
            Init(typeof(Penren), new TaskConfig { HotKey = [WinApi.VK_F9], Cd = 500, Params = ["words.txt"] });
            Init(typeof(AutoPaint), new TaskConfig { HotKey = [WinApi.VK_F4], Cd = 100 });
            Init(typeof(DragMouse),
                new TaskConfig
                {
                    HotKey = [WinApi.VK_SPACE, WinApi.VK_1],
                    Cd = 1,
                    Params = ["0"],
                    StopWhenKeyUp=true,
                },
                "0");
            Init(typeof(DragMouse),
                new TaskConfig
                {
                    HotKey = [WinApi.VK_SPACE, WinApi.VK_2],
                    Cd = 1,
                    Params = ["1"],
                    StopWhenKeyUp = true,
                },
                "1");
            Init(typeof(DragMouse),
                new TaskConfig
                {
                    HotKey = [WinApi.VK_SPACE, WinApi.VK_3],
                    Cd = 1,
                    Params = ["2"],
                    StopWhenKeyUp = true,
                },
                "2");
            Init(typeof(DragMouse),
                new TaskConfig
                {
                    HotKey = [WinApi.VK_SPACE, WinApi.VK_4],
                    Cd = 1,
                    Params = ["3"],
                    StopWhenKeyUp = true,
                },
                "3");




            //timerTasks["LeftClicks"] = new LeftClicks(dealLog);
            //timerTasks["Penren"] = new Penren(dealLog);

        }

        /// <summary>
        /// 处理来自task的日志信息，包括模块启动、结束ui界面的相关更新
        /// </summary>
        /// <param name="log"></param>
        public void dealLog(Log log)
        {
            string res = log.Content;
            Invoke(new Action(() =>
            {
                if (log.Type == LogType.Start) { res = $"启动 {res}"; dealStart(log.TaskId); }
                else if (log.Type == LogType.Stop) { res = $"停止 {res}"; dealStop(log.TaskId); }
                res = $"[{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}][{log.TaskId}]{res}\r\n";
            }));
            printLog(res);
        }


        public void printLog(string text)
        {
            if (!windowExist) return;
            Invoke(new Action(() =>
            {
                int maxlen = 30000;
                LogTextbox.Text = LogTextbox.Text.Length <= maxlen ? LogTextbox.Text : LogTextbox.Text.Substring(LogTextbox.Text.Length - maxlen, maxlen);
                LogTextbox.AppendText(text);
            }));
        }

        public void dealStart(string name)
        {
            if (!windowExist) return;
            if (name == "LeftClicks")
            {
                button6.BackColor = Color.Red;
                try
                {
                    Icon = Resources.Icon_act;
                }
                catch { }
            }
        }

        public void dealStop(string name)
        {
            if (!windowExist) return;
            if (name == "LeftClicks")
            {
                button6.BackColor = Color.Green;
                try
                {
                    Icon = Resources.Icon_nav;
                }
                catch { }
            }
        }

        /// <summary>
        /// 直接激活特定进程
        /// </summary>
        /// <param name="taskName"></param>
        public void Trigger(string taskName)
        {
            if (timerTasks.TryGetValue(taskName, out var task))
            {
                if (task.isRunning) task.Stop();
                else task.Start();
            }
            else
            {
                dealLog(Log.Text($"{taskName}不存在"));
            }

        }

        public void Init(Type task, TaskConfig config = null, string name_tail = "")
        {
            string id = $"{task.Name}{name_tail}";
            if (config == null) config = new TaskConfig();
            config.TaskId = id;
            if (!timerTasks.ContainsKey(id))
            {
                timerTasks[id] = (TimerTask)(Activator.CreateInstance(task));

            }
            else
            {
                timerTasks[id].Stop();
            }
            timerTasks[id].LogReportAction = dealLog;
            timerTasks[id].Init(config);
        }

        public void Print(string str)
        {
            if (!windowExist) return;
            Invoke(new Action(() =>
            {
                textBox1.AppendText(str + "\r\n");
            }));
        }


        public void PrintHotkeyState(Keys[] keys)
        {
            if (!windowExist) return;
            Invoke(new Action(() =>
            {
                LabelHotkey.Text = string.Join("  ", keys.Select(k => k.ToString()));
            }));
        }

        public void PrintMouseState(MouseState state)
        {
            if (!windowExist) return;
            Invoke(new Action(() =>
            {
                StringBuilder sb = new StringBuilder();
                if (state.leftMouseDown) sb.Append(" 鼠标左键");
                if (state.rightMouseDown) sb.Append(" 鼠标右键");
                if (state.middleMouseDown) sb.Append(" 鼠标中键");
                if (state.middleWheel != 0) sb.Append($" 滚轮滑动{state.middleWheel}");
                if (state.mouseLogicLocation != Point.Empty) sb.Append($" 逻辑坐标({state.mouseLogicLocation.X},{state.mouseLogicLocation.Y})");
                if (state.mouseRealLocation != Point.Empty) sb.Append($" 真实坐标({state.mouseRealLocation.X},{state.mouseRealLocation.Y})");
                LabelMouse.Text = sb.ToString();
            }));
        }

        public void PrintMouseDPS(double dps)
        {
            if (!windowExist) return;
            Invoke(new Action(() =>
            {
                labelMouseDPS.Text = $"鼠标DPS={dps:f2}";
            }));
        }
        public void PrintKeyboardDPS(double dps)
        {
            if (!windowExist) return;
            Invoke(new Action(() =>
            {
                labelKeyDPS.Text = $"键盘DPS={dps:f2}";
            }));
        }

        ///// <summary>
        ///// 安装键盘钩子
        ///// </summary>
        //private void initHook()
        //{
        //    k_hook = new KeyboardHook();
        //    k_hook.KeyDownEvent += new KeyEventHandler(hook_KeyDown);//钩住键按下 
        //    k_hook.Start();//安装键盘钩子
        //}

        ///// <summary>
        ///// 取消钩子
        ///// </summary>
        //private void stopHook()
        //{
        //    try
        //    {
        //        k_hook.Stop();
        //    }
        //    catch
        //    {

        //    }
        //}

        //private void hook_KeyDown(object sender, KeyEventArgs e)
        //{
        //    //if (e.KeyValue == (int)Keys.A && (int)Control.ModifierKeys == (int)Keys.Alt)
        //    if (run)
        //    {
        //       // dealTask(e.KeyCode);
        //    }


        //    //if (e.KeyCode == config.actKey)
        //    //{
        //    //    if (leftloop) stopMouseClickLoop();
        //    //    else startMouseClickLoop();
        //    //}

        //}





        //public void workGetMousePosition()
        //{
        //    while (getMousePositionRun)
        //    {
        //        PrintLabel(string.Format("x:{0},y:{1}", MousePosition.X, MousePosition.Y));
        //        Thread.Sleep(500);
        //    }
        //}

        //double tx = 48.1927710843;
        //double ty = 85.4700854701;



        //private bool ctrlPressed;
        //private bool altPressed;
        //private bool shiftPressed;
        //private Keys currentHotkey;

































        private void button1_Click(object sender, EventArgs e)
        {
            //if (run)
            //{
            //    run = false;
            //    Print("已中止热键响应.");
            //    button1.Text = "激活";
            //}
            //else
            //{
            //    run = true;
            //    Print("开始响应热键.");
            //    button1.Text = "停止";
            //}
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                windowExist = false;
                //stopHook();
                //getMousePositionRun = false;

                updateConfig();
                configManager.Save();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Environment.Exit(0);
        }

        private void checkedListBox1_MouseDown(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Right)
            //{
            //    contextMenuStrip1.Show(MousePosition);
            //}
            //else if (e.Button == MouseButtons.Left)
            //{
            //    if (checkedListBox1.SelectedItem != null)
            //    {
            //        int index = checkedListBox1.SelectedIndex;
            //        nowTask = tasks[index];
            //        updateTaskItemView();
            //    }
            //    updateTasks();
            //}
        }

        private void 新建任务ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // addTask();
        }

        private void 删除任务ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (checkedListBox1.SelectedItems.Count > 0)
            //{
            //    deleteTask(checkedListBox1.SelectedIndex);
            //}

        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip2.Show(MousePosition);
            }
        }

        private void 上移ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (listView1.SelectedItems.Count > 0)
            //    changeTaskItemPosition(-1);
        }

        private void 下移ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (listView1.SelectedItems.Count > 0)
            //    changeTaskItemPosition(1);
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (listView1.SelectedItems.Count > 0)
            //{
            //    deleteTaskItem(listView1.SelectedIndices[0]);
            //}

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Command comm = (Command)comboBox1.SelectedIndex;
            //if (comm == Command.mouseMov || comm == Command.mouseMovStatic)
            //    addTaskItem(comm, string.Format("{0} {1}", textBox3.Text, textBox4.Text));
            //else
            //    addTaskItem(comm, string.Empty);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // addTaskItem(Command.key, textBox5.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //addTaskItem(Command.wait, numericUpDown1.Value.ToString());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //updateTaskItem();
            //updateTasksView();
        }



        private void button6_Click(object sender, EventArgs e)
        {
            updateConfigToUI();
            //Init(typeof(LeftClicks), new TaskConfig { HotKey = [(ushort)config.actKey], Cd = config.cdTimeMs });
            Trigger(typeof(LeftClicks).Name);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            config.actMouseLeft = checkBox1.Checked;
            updateConfigToUI();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            config.actMouseRight = checkBox2.Checked;
            updateConfigToUI();
        }



        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            config.cdTimeMs = (int)numericUpDown2.Value;
            updateConfigToUI();
        }


        /// <summary>
        /// 把界面的鼠标连点配置项更新到config里
        /// </summary>
        public void updateConfig()
        {
            try
            {
                string keyString = textBox6.Text;

                if (Enum.TryParse(keyString, true, out Keys key))
                {
                    config.actKey = key;
                }
                else
                {
                    MessageBox.Show($"Invalid key: '{keyString}'. Please enter a valid key name.");
                }

                config.actMouseLeft = checkBox1.Checked;
                config.actMouseRight = checkBox2.Checked;
                config.cdTimeMs = (int)numericUpDown2.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新配置文件时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 把config的鼠标连点配置项更新到UI里
        /// </summary>
        public void updateConfigToUI()
        {
            try
            {
                if (config != null)
                {
                    // 输出读取的配置项
                    checkBox1.Checked = config.actMouseLeft;
                    checkBox2.Checked = config.actMouseRight;
                    textBox6.Text = config.actKey.ToString();
                    numericUpDown2.Value = config.cdTimeMs;
                    label10.Text = $"（{(1000.0 / config.cdTimeMs).ToString("F2")}次/秒）";

                    textBox6.Text = config.actKey.ToString();
                    button6.Text = $"{(config.actMouseLeft ? "左" : "")}{(config.actMouseRight ? "右" : "")}连点（{config.actKey.ToString()}）";
                }



                Init(typeof(LeftClicks), new TaskConfig { HotKey = [(ushort)config.actKey], Cd = config.cdTimeMs, Params = [config.actMouseRight ? "right" : "left"] });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }



        private void textBox6_KeyDown(object sender, KeyEventArgs e)
        {
            //// 检查修饰键 (Ctrl, Alt, Shift)
            //ctrlPressed = e.Control;
            //altPressed = e.Alt;
            //shiftPressed = e.Shift;

            // 将组合键保存到 currentHotkey
            //currentHotkey = e.KeyCode;

            //// 显示热键
            //UpdateHotkeyTextBox();
        }

        private void UpdateHotkeyTextBox()
        {
            //string hotkeyDisplay = "";

            //if (ctrlPressed)
            //    hotkeyDisplay += "Ctrl + ";
            //if (altPressed)
            //    hotkeyDisplay += "Alt + ";
            //if (shiftPressed)
            //    hotkeyDisplay += "Shift + ";

            //hotkeyDisplay += currentHotkey != Keys.None ? currentHotkey.ToString() : "";
            //config.actKey = currentHotkey;
        }

        private void textBox6_KeyUp(object sender, KeyEventArgs e)
        {
            //if (!e.Control) ctrlPressed = false;
            //if (!e.Alt) altPressed = false;
            //if (!e.Shift) shiftPressed = false;

            //// 显示热键
            //UpdateHotkeyTextBox();
            //updateConfigToUI();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Icon = Resources.Icon_blue;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //var paintArea = new Rectangle(50, 150, 1600, 1000);
            //var targets = ScreenVision.FindTargetsOnScreen(paintArea, Util.GetMousColor(), 3, new Size(5, 5), new Size(35, 35));
        }


    }
}
