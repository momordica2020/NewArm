using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewArm.Core
{
    public class Util
    {
        public static Keys str2key(string str)
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

        public static int key2int(Keys key)
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
        /// 获取鼠标当前位置的RGB颜色值
        /// </summary>
        /// <returns></returns>
        public static Color GetMousColor()
        {
            // 获取当前鼠标位置
            Point mousePosition = Cursor.Position;

            // 截取1x1像素的屏幕区域（即鼠标所在点）
            using var bitmap = new Bitmap(1, 1);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(mousePosition.X, mousePosition.Y, 0, 0, new Size(1, 1));
            }

            // 获取像素的RGB值
            Color pixelColor = bitmap.GetPixel(0, 0);
            return pixelColor;
        }
    }
}
