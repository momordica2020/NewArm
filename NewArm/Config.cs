using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewArm
{
    public class Config
    {
        public bool actMouseLeft { get; set; }
        public bool actMouseRight { get; set; }

        public int cdTimeMs { get; set; }
        public Keys actKey { get; set; }
    }
}
