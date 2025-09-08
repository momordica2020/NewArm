using NewArm.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewArm.TaskFunctions.tasks
{
    public class Penren : TimerTask
    {
        public Penren(Log _log) : base(_log)
        {
        }

        public string _filePath;
        string[] _lines;
        Random _random = new Random();

        int _count = 0;



        protected override void _init()
        {
            _count = 0;
            _filePath = Config.param[0];
            // 读取文件
            _lines = File.ReadAllLines(_filePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            if (_lines.Length == 0)
            {
                log(LogInfo.Info("文件为空或没有有效行"));
                return;
            }
        }

        protected override void _work()
        {
            if (_count >= 10)
            {
                Stop();
                return;
            }
            string randomLine = _lines[_random.Next(_lines.Length)];
            WinApi.TextInput(randomLine);
            WinApi.KeyPress(WinApi.VK_RETURN);
        }
    }
}
