using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewArm.Core
{
    public delegate void LogEvent(Log msg);

    public delegate void MouseStateEvent(MouseState state);
    public delegate void KeyboardStateEvent(System.Windows.Forms.Keys[] keys);

    public delegate void MouseDPS(double dps);
    public delegate void KeyboardDPS(double dps);

}
