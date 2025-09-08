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

namespace NewArm
{



    public partial class Form1 : Form
    {
        private bool getMousePositionRun = true;
        private bool run = false;
        delegate void sendStringDelegate(string str);
        delegate void sendVoidDelegate();
        int num = 0;
        KeyboardHook k_hook;

        string filename = "tasks.json";
        static string fileConfig = "config.json";
        Config config;

        private List<Task> tasks;
        private Task nowTask;


        public Dictionary<string, TimerTask> timerTasks = new Dictionary<string, TimerTask>();


        public Form1()
        {
            InitializeComponent();
            this.Focus();
            comboBox1.SelectedIndex = 0;
            loadTasks();
            updateTasksView();

            initHook();

            //开启捕捉鼠标位置的常驻线程
            new Thread(workGetMousePosition).Start();


        }


        private void Form1_Shown(object sender, EventArgs e)
        {
            // 读取配置项
            ReadConfig();
            updateConfigToUI();

            Init(typeof(LeftClicks), new TaskConfig { triggerCodes = [WinApi.VK_F2], interval = config.cdTimeMs });
            Init(typeof(Penren), new TaskConfig { triggerCodes = [WinApi.VK_F9], interval = 500, param = ["words.txt"] });
            //timerTasks["LeftClicks"] = new LeftClicks(dealLog);
            //timerTasks["Penren"] = new Penren(dealLog);

        }

        /// <summary>
        /// 处理来自task的日志信息，包括模块启动、结束ui界面的相关更新
        /// </summary>
        /// <param name="log"></param>
        public void dealLog(LogInfo log)
        {
            string res = log.text;
            Invoke(new Action(() =>
            {
                if (log.type == LogType.Start) { res = $"启动 {res}"; dealStart(log.taskName); }
                else if (log.type == LogType.Stop) { res = $"停止 {res}"; dealStop(log.taskName); }
                res = $"[{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}]{log.taskName} {res}\r\n";
            }));
            printLog(res);
        }


        public void printLog(string text)
        {
            Invoke(new Action(() =>
            {
                if (LogTextbox.Text.Length > 30000) LogTextbox.Text = LogTextbox.Text.Substring(-30000);
                LogTextbox.AppendText(text);
            }));
        }

        public void dealStart(string name)
        {
            if(name == "LeftClicks")
            {
                button6.BackColor = Color.Red;
                try
                {
                    Icon = Resources.Icon_act;
                } catch { }
            }
        }

        public void dealStop(string name)
        {
            if(name == "LeftClicks")
            {
                button6.BackColor = Color.Green;
                try
                {
                    Icon = Resources.Icon_nav;
                }catch { }
            }
        }

        /// <summary>
        /// 直接激活特定进程
        /// </summary>
        /// <param name="taskName"></param>
        public void Trigger(string taskName)
        {
            if (timerTasks.TryGetValue(taskName,out var task))
            {
                if (task.isRunning) task.Stop();
                else task.Start();
            }
            else
            {
                dealLog(LogInfo.Info($"{taskName}不存在"));
            }

        }

        public void Init(Type task, TaskConfig config = null)
        {
            if (!timerTasks.ContainsKey(task.Name))
            {
                timerTasks[task.Name] = (TimerTask)(Activator.CreateInstance(task, args: [(Log)dealLog]));

            }
            else
            {
                timerTasks[task.Name].Stop();
            }
            if(config!=null) timerTasks[task.Name].Init(config);
        }

        public void Print(string str)
        {
            if (textBox1.InvokeRequired)
            {
                sendStringDelegate mevent = new sendStringDelegate(Print);
                Invoke(mevent, (object)str);
            }
            else
            {
                textBox1.AppendText(str + "\r\n");
            }
        }


        public void PrintLabel(string str)
        {
            if (label5.InvokeRequired)
            {
                sendStringDelegate mevent = new sendStringDelegate(PrintLabel);
                Invoke(mevent, (object)str);
            }
            else
            {
                label5.Text = str;
            }
        }

        #region old_tasks


        private void loadTasks()
        {
            try
            {
                tasks = IOController.getDataFromJson(filename);



                
            }
            catch
            {
                tasks = new List<Task>();
            }

        }





        
        private void saveTasks()
        {
            try
            {
                IOController.saveDataAsJson(filename, tasks);
            }
            catch
            {

            }

        }

        private void updateTasks()
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                this.tasks[i].isRun = checkedListBox1.GetItemChecked(i);
            }
        }

        private void updateTasksView()
        {
            if (checkedListBox1.InvokeRequired)
            {
                sendVoidDelegate mevent = new sendVoidDelegate(updateTasksView);
                Invoke(mevent);
            }
            else
            {
                checkedListBox1.Items.Clear();
                foreach (var i in tasks)
                {
                    string name = i.name;
                    bool isRun = i.isRun;
                    checkedListBox1.Items.Add(name, isRun);
                }
            }
        }

        private void updateTaskItemView()
        {
            if (checkedListBox1.InvokeRequired)
            {
                sendVoidDelegate mevent = new sendVoidDelegate(updateTaskItemView);
                Invoke(mevent);
            }
            else
            {
                if (nowTask == null) return;
                textBox2.Text = nowTask.name;
                comboBox2.SelectedIndex = Util.key2int(nowTask.hotKey);
                listView1.Items.Clear();
                foreach (var i in nowTask.items)
                {
                    ListViewItem item = new ListViewItem(i.command.ToString());
                    item.SubItems.Add(i.args);
                    listView1.Items.Add(item);
                }
            }
        }

        private void addTask()
        {
            tasks.Add(new Task());
            updateTasksView();
            checkedListBox1.SelectedIndex = checkedListBox1.SelectedItems.Count - 1;
            updateTaskItemView();
        }

        private void deleteTask(int index)
        {
            tasks.RemoveAt(index);
            updateTasksView();
            if (tasks.Count > 0) checkedListBox1.SelectedIndex = index;
            updateTaskItemView();
        }

        private void addTaskItem(Command comm, string args)
        {
            TaskItem item = new TaskItem();
            item.command = comm;
            item.args = args;
            nowTask.items.Add(item);
            updateTaskItemView();
        }

        private void changeTaskItemPosition(int ds)
        {
            int beforep = listView1.SelectedIndices[0];
            int afterp = beforep + ds;
            if (afterp < 0 || afterp >= nowTask.items.Count) return;
            TaskItem i1 = nowTask.items[beforep];
            TaskItem i2 = nowTask.items[afterp];
            Command tmpc = i2.command;
            string tmpa = i2.args;
            i2.args = i1.args;
            i2.command = i1.command;
            i1.args = tmpa;
            i1.command = tmpc;

            updateTaskItemView();
        }

        private void deleteTaskItem(int index)
        {
            nowTask.items.RemoveAt(index);

            updateTaskItemView();
        }

        private void updateTaskItem()
        {
            nowTask.name = textBox2.Text;
            nowTask.hotKey = Util.str2key(comboBox2.SelectedItem.ToString());
        }


        private void dealTaskItem(TaskItem item)
        {
            switch (item.command)
            {
                case Command.mouseMovStatic:
                    int x = 0;
                    int y = 0;
                    int.TryParse(item.args.Split(' ')[0], out x);
                    int.TryParse(item.args.Split(' ')[1], out y);
                    WinApi.MouseMoveAbsolute(x, y);
                    break;
                case Command.mouseMov:
                    int dx = 0;
                    int dy = 0;
                    int.TryParse(item.args.Split(' ')[0], out dx);
                    int.TryParse(item.args.Split(' ')[1], out dy);
                    WinApi.MouseMove(dx, dy);
                    break;
                case Command.mouseLC:
                    WinApi.Click("left");
                    break;
                case Command.mouseLD:
                    WinApi.MouseDown("left");
                    break;
                case Command.mouseLU:
                    WinApi.MouseUp("left");
                    break;
                case Command.wait:
                    int time = 0;
                    int.TryParse(item.args, out time);
                    Thread.Sleep(time);
                    break;
                default:
                    break;
            }
        }

        private void dealTask(Keys key)
        {
            foreach (var task in tasks)
            {
                if (task.isRun && task.hotKey == key)
                {
                    int beginx = MousePosition.X;
                    int beginy = MousePosition.Y;

                    foreach (var item in task.items)
                    {

                        dealTaskItem(item);
                    }

                    WinApi.MouseMove(beginx, beginy);
                }
            }
        }


        #endregion




        /// <summary>
        /// 安装键盘钩子
        /// </summary>
        private void initHook()
        {
            k_hook = new KeyboardHook();
            k_hook.KeyDownEvent += new KeyEventHandler(hook_KeyDown);//钩住键按下 
            k_hook.Start();//安装键盘钩子
        }

        /// <summary>
        /// 取消钩子
        /// </summary>
        private void stopHook()
        {
            try
            {
                k_hook.Stop();
            }
            catch
            {

            }
        }

        private void hook_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyValue == (int)Keys.A && (int)Control.ModifierKeys == (int)Keys.Alt)
            if (run)
            {
                dealTask(e.KeyCode);
            }


            //if (e.KeyCode == config.actKey)
            //{
            //    if (leftloop) stopMouseClickLoop();
            //    else startMouseClickLoop();
            //}

        }

        



        public void workGetMousePosition()
        {
            while (getMousePositionRun)
            {
                PrintLabel(string.Format("x:{0},y:{1}", MousePosition.X, MousePosition.Y));
                Thread.Sleep(500);
            }
        }

        //double tx = 48.1927710843;
        //double ty = 85.4700854701;

      

        //private bool ctrlPressed;
        //private bool altPressed;
        //private bool shiftPressed;
        private Keys currentHotkey;

       































        private void button1_Click(object sender, EventArgs e)
        {
            if (run)
            {
                run = false;
                Print("已中止热键响应.");
                button1.Text = "激活";
            }
            else
            {
                run = true;
                Print("开始响应热键.");
                button1.Text = "停止";
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                stopHook();
                getMousePositionRun = false;
                saveTasks();
                WriteConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Environment.Exit(0);
        }

        private void checkedListBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(MousePosition);
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (checkedListBox1.SelectedItem != null)
                {
                    int index = checkedListBox1.SelectedIndex;
                    nowTask = tasks[index];
                    updateTaskItemView();
                }
                updateTasks();
            }
        }

        private void 新建任务ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addTask();
        }

        private void 删除任务ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (checkedListBox1.SelectedItems.Count > 0)
            {
                deleteTask(checkedListBox1.SelectedIndex);
            }

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
            if (listView1.SelectedItems.Count > 0)
                changeTaskItemPosition(-1);
        }

        private void 下移ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                changeTaskItemPosition(1);
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                deleteTaskItem(listView1.SelectedIndices[0]);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Command comm = (Command)comboBox1.SelectedIndex;
            if (comm == Command.mouseMov || comm == Command.mouseMovStatic)
                addTaskItem(comm, string.Format("{0} {1}", textBox3.Text, textBox4.Text));
            else
                addTaskItem(comm, string.Empty);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            addTaskItem(Command.key, textBox5.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            addTaskItem(Command.wait, numericUpDown1.Value.ToString());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            updateTaskItem();
            updateTasksView();
        }



        private void button6_Click(object sender, EventArgs e)
        {
            Init(typeof(LeftClicks), new TaskConfig { triggerCodes = [(ushort)config.actKey], interval = config.cdTimeMs });
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
        public void ReadConfig()
        {
            try
            {
                // 从文件中读取 JSON 字符串
                if (!File.Exists(fileConfig))
                {
                    throw new FileNotFoundException("Configuration file not found.");
                }
                string jsonString = File.ReadAllText(fileConfig);

                // 使用 Newtonsoft.Json 反序列化 JSON 字符串到 Config 对象
                config = JsonConvert.DeserializeObject<Config>(jsonString);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取配置文件时出错: {ex.Message}");
            }

            // 要是没有，就初始化
            if (config == null)
            {
                config = new Config
                {
                    actMouseLeft = true,
                    actMouseRight = false,
                    actKey = Keys.F1,
                    cdTimeMs = 50,
                };
            }
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void WriteConfig()
        {
            try
            {
                updateConfig();
                // 将对象序列化为 JSON 字符串
                string jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);

                // 将 JSON 字符串写入文件
                File.WriteAllText(fileConfig, jsonString);

                //Console.WriteLine("配置文件已成功写入！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入配置文件时出错: {ex.Message}");
            }
        }


        private void textBox6_KeyDown(object sender, KeyEventArgs e)
        {
            //// 检查修饰键 (Ctrl, Alt, Shift)
            //ctrlPressed = e.Control;
            //altPressed = e.Alt;
            //shiftPressed = e.Shift;

            // 将组合键保存到 currentHotkey
            currentHotkey = e.KeyCode;

            // 显示热键
            UpdateHotkeyTextBox();
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
            config.actKey = currentHotkey;
        }

        private void textBox6_KeyUp(object sender, KeyEventArgs e)
        {
            //if (!e.Control) ctrlPressed = false;
            //if (!e.Alt) altPressed = false;
            //if (!e.Shift) shiftPressed = false;

            // 显示热键
            UpdateHotkeyTextBox();
            updateConfigToUI();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Icon = Resources.Icon_blue;
        }









    }
}
