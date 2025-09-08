using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewArm
{
    public class Task
    {
        public string name = string.Empty;
        public List<TaskItem> items = new List<TaskItem>();
        public bool isRun;
        public Keys hotKey;

        public Task()
        {
            name= string.Empty;
            items=new List<TaskItem>();
            isRun=false;
        }
    }
}
