using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices; 
using Microsoft.Win32;
using Newtonsoft.Json;
using System.IO;
using NewArm.Properties;

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
                comboBox2.SelectedIndex = key2int(nowTask.hotKey);
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
            if (tasks.Count > 0) checkedListBox1.SelectedIndex = index ;
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
            nowTask.hotKey = str2key(comboBox2.SelectedItem.ToString());
        }

        private static Keys str2key(string str)
        {
            if (str == "left") return Keys.Left;
            else if (str == "right") return Keys.Right;
            else if (str == "down") return Keys.Down;
            else if (str == "up") return Keys.Up;
            else if (str == "0") return Keys.D0;
            else if (str == "1") return Keys.D1;
            else if (str == "2") return Keys.D2;
            else if (str == "3") return Keys.D3;
            else if (str == "4") return Keys.D4;
            else if (str == "5") return Keys.D5;
            else if (str == "6") return Keys.D6;
            else if (str == "7") return Keys.D7;
            else if (str == "8") return Keys.D8;
            else if (str == "9") return Keys.D9;
            else return Keys.Space;
        }

        private static int key2int(Keys key)
        {
            if (key == Keys.Left) return 0;
            else if (key == Keys.Right) return 1;
            else if (key == Keys.Up) return 2;
            else if (key == Keys.Down) return 3;
            else if (key == Keys.D0) return 4;
            else if (key == Keys.D1) return 5;
            else if (key == Keys.D2) return 6;
            else if (key == Keys.D3) return 7;
            else if (key == Keys.D4) return 8;
            else if (key == Keys.D5) return 9;
            else if (key == Keys.D6) return 10;
            else if (key == Keys.D7) return 11;
            else if (key == Keys.D8) return 12;
            else if (key == Keys.D9) return 13;
            else return 0;
        }





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

        private void dealTaskItem(TaskItem item)
        {
            switch (item.command)
            {
                case Command.mouseMovStatic:
                    int x = 0;
                    int y = 0;
                    int.TryParse(item.args.Split(' ')[0], out x);
                    int.TryParse(item.args.Split(' ')[1], out y);
                    mouseMoveTo(x, y);
                    break;
                case Command.mouseMov:
                    int dx = 0;
                    int dy = 0;
                    int.TryParse(item.args.Split(' ')[0], out dx);
                    int.TryParse(item.args.Split(' ')[1], out dy);
                    mouseMoveToD(dx, dy);
                    break;
                case Command.mouseLC:
                    mouseLeftClick();
                    break;
                case Command.mouseLD:
                    mouseLeftDown();
                    break;
                case Command.mouseLU:
                    mouseLeftUp();
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

                    mouseMoveTo(beginx, beginy);
                }
            }
        }

        private void hook_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyValue == (int)Keys.A && (int)Control.ModifierKeys == (int)Keys.Alt)
            if (run)
            {
                dealTask(e.KeyCode);
            }


            if (e.KeyCode == config.actKey)
            {
                if (leftloop) stopMouseClickLoop();
                else startMouseClickLoop();
            }
        }

        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        //移动鼠标 
        const int MOUSEEVENTF_MOVE = 0x0001;
        //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //模拟鼠标左键抬起 
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        //模拟鼠标右键按下 
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        //模拟鼠标右键抬起 
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        //模拟鼠标中键按下 
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        //模拟鼠标中键抬起 
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        //标示是否采用绝对坐标 
        const int MOUSEEVENTF_ABSOLUTE = 0x8000; 
        
        public void print(string str)
        {
            if (textBox1.InvokeRequired)
            {
                sendStringDelegate mevent = new sendStringDelegate(print);
                Invoke(mevent, (object)str);
            }
            else
            {
                textBox1.AppendText(str + "\r\n");
            }
        }

        public void printLabel(string str)
        {
            if (label5.InvokeRequired)
            {
                sendStringDelegate mevent = new sendStringDelegate(printLabel);
                Invoke(mevent, (object)str);
            }
            else
            {
                label5.Text = str;
            }
        }

        public void workGetMousePosition()
        {
            while (getMousePositionRun)
            {
                printLabel(string.Format("x:{0},y:{1}", MousePosition.X, MousePosition.Y));
                Thread.Sleep(500);
            }
        }

        double tx = 48.1927710843;
        double ty = 85.4700854701;

        private void mouseMoveTo(int x, int y)
        {
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, (int)(x * tx), (int)(y * ty), 0, 0);
            Thread.Sleep(50);
        }

        private void mouseMoveToD(int dx, int dy)
        {
            mouse_event(MOUSEEVENTF_MOVE, (int)(1* dx), (int)(1* dy), 0, 0);
            Thread.Sleep(50);
        }

        private void mouseLeftClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        private void mouseClickAt(int x, int y)
        {
            mouseMoveTo(x, y);
            mouseLeftClick();
        }

        private void mouseClickAtD(int dx, int dy)
        {
            mouseMoveToD(dx, dy);
            mouseLeftClick();
        }

        private void mouseLeftDown()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        }

        private void mouseLeftUp()
        {
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        private bool leftloop = false;
        //private bool ctrlPressed;
        //private bool altPressed;
        //private bool shiftPressed;
        private Keys currentHotkey;

        private void workMouseClickLoop()
        {
            while (leftloop)
            {
                mouseLeftClick();
                //mouseLeftDown();
                Thread.Sleep(config.cdTimeMs);
                //mouseLeftUp();
                //Thread.Sleep(config.cdTimeMs / 2);
            }
        }

        private void startMouseClickLoop()
        {
            leftloop = true;
            button6.BackColor = Color.Red;
            try
            {
                Icon = Resources.Icon_act;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            new Thread(workMouseClickLoop).Start();
        }

        private void stopMouseClickLoop()
        {
            button6.BackColor = Color.Green;
            try
            {
                Icon = Resources.Icon_nav;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            leftloop = false;
        }

































        private void button1_Click(object sender, EventArgs e)
        {
            if (run)
            {
                run = false;
                print("已中止热键响应.");
                button1.Text = "激活";
            }
            else
            {
                run = true;
                print("开始响应热键.");
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
            catch(Exception ex)
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
            if(listView1.SelectedItems.Count>0)
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
            Command comm=(Command)comboBox1.SelectedIndex;
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
            if (leftloop) stopMouseClickLoop();
            else startMouseClickLoop();
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
            }catch(Exception ex)
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
        private void Form1_Shown(object sender, EventArgs e)
        {
            // 读取配置项
            ReadConfig();
            updateConfigToUI();
            

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
    }
}
