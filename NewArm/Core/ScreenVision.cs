using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NewArm.Core
{
    public class ScreenVision
    {


        ///// <summary>
        ///// 查找屏幕上满足特定颜色和尺寸条件的对象的中心坐标。
        ///// </summary>
        ///// <param name="region">要搜索的屏幕区域。</param>
        ///// <param name="targetColor">目标的RGB颜色。</param>
        ///// <param name="tolerance">颜色容差值，RGB各通道允许的偏差范围。</param>
        ///// <param name="min_width">目标的最小宽度。</param>
        ///// <param name="max_width">目标的最大宽度。</param>
        ///// <param name="min_height">目标的最小高度。</param>
        ///// <param name="max_height">目标的最大高度。</param>
        ///// <returns>返回所有满足条件的目标中心点坐标列表。</returns>
        //public static List<Point> FindTargetsOnScreen(Rectangle region, Color targetColor, int tolerance, int min_width, int max_width, int min_height, int max_height)
        //{
        //    Bitmap screenshot = null;
        //    Graphics graph = null;
        //    var targetsToClick = new List<Point>();

        //    try
        //    {
        //        // 1. 屏幕截图
        //        screenshot = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb);
        //        graph = Graphics.FromImage(screenshot);
        //        graph.CopyFromScreen(region.Left, region.Top, 0, 0, region.Size, CopyPixelOperation.SourceCopy);

        //        // 使用 LockBits 来高效访问像素数据
        //        BitmapData bmpData = screenshot.LockBits(new Rectangle(0, 0, screenshot.Width, screenshot.Height), ImageLockMode.ReadWrite, screenshot.PixelFormat);
        //        IntPtr ptr = bmpData.Scan0;
        //        int bytes = Math.Abs(bmpData.Stride) * screenshot.Height;
        //        byte[] rgbValues = new byte[bytes];
        //        Marshal.Copy(ptr, rgbValues, 0, bytes);

        //        // 用于标记已访问过的像素
        //        bool[,] visited = new bool[screenshot.Width, screenshot.Height];

        //        // 2. 遍历所有像素，寻找未被访问过的目标颜色点作为区域识别的起点
        //        for (int y = 0; y < screenshot.Height; y++)
        //        {
        //            for (int x = 0; x < screenshot.Width; x++)
        //            {
        //                if (visited[x, y])
        //                {
        //                    continue;
        //                }

        //                int index = y * bmpData.Stride + x * 4; // 每个像素占4个字节 (B, G, R, A)
        //                int b = rgbValues[index];
        //                int g = rgbValues[index + 1];
        //                int r = rgbValues[index + 2];

        //                // 检查颜色是否在容差范围内
        //                if (IsColorMatch(r, g, b, targetColor, tolerance))
        //                {
        //                    // 3. 从当前点开始进行洪水填充，识别整个连接区域
        //                    Rectangle boundingBox = FloodFill(x, y, bmpData, rgbValues, visited, targetColor, tolerance);

        //                    // 4. 尺寸筛选
        //                    if (boundingBox.Width >= min_width && boundingBox.Width <= max_width &&
        //                        boundingBox.Height >= min_height && boundingBox.Height <= max_height)
        //                    {
        //                        // 5. 计算中心点并添加到列表
        //                        int centerX = region.Left + boundingBox.Left + boundingBox.Width / 2;
        //                        int centerY = region.Top + boundingBox.Top + boundingBox.Height / 2;
        //                        targetsToClick.Add(new Point(centerX, centerY));
        //                    }
        //                }
        //            }
        //        }

        //        screenshot.UnlockBits(bmpData);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"发生错误: {ex.Message}");
        //    }
        //    finally
        //    {
        //        graph?.Dispose();
        //        screenshot?.Dispose();
        //    }

        //    if (targetsToClick.Count == 0)
        //    {
        //        Console.WriteLine("未找到任何同时满足颜色和尺寸条件的标记。");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"筛选完成，共找到 {targetsToClick.Count} 个有效目标。");
        //    }

        //    return targetsToClick;
        //}

        ///// <summary>
        ///// 检查颜色是否匹配。
        ///// </summary>
        //private static bool IsColorMatch(int r, int g, int b, Color targetColor, int tolerance)
        //{
        //    return Math.Abs(r - targetColor.R) <= tolerance &&
        //           Math.Abs(g - targetColor.G) <= tolerance &&
        //           Math.Abs(b - targetColor.B) <= tolerance;
        //}

        ///// <summary>
        ///// 使用基于队列的洪水填充算法来识别一个连通区域并计算其边界框。
        ///// </summary>
        //private static Rectangle FloodFill(int startX, int startY, BitmapData bmpData, byte[] rgbValues, bool[,] visited, Color targetColor, int tolerance)
        //{
        //    Queue<Point> queue = new Queue<Point>();
        //    queue.Enqueue(new Point(startX, startY));
        //    visited[startX, startY] = true;

        //    int minX = startX, maxX = startX;
        //    int minY = startY, maxY = startY;

        //    while (queue.Count > 0)
        //    {
        //        Point current = queue.Dequeue();

        //        // 更新边界
        //        if (current.X < minX) minX = current.X;
        //        if (current.X > maxX) maxX = current.X;
        //        if (current.Y < minY) minY = current.Y;
        //        if (current.Y > maxY) maxY = current.Y;

        //        // 检查周围的像素 (上, 下, 左, 右)
        //        for (int i = -1; i <= 1; i++)
        //        {
        //            for (int j = -1; j <= 1; j++)
        //            {
        //                // 只检查上下左右，不检查对角线 (如需检查对角线，移除此条件)
        //                if (Math.Abs(i) + Math.Abs(j) != 1) continue;

        //                int nextX = current.X + i;
        //                int nextY = current.Y + j;

        //                // 检查边界
        //                if (nextX >= 0 && nextX < bmpData.Width && nextY >= 0 && nextY < bmpData.Height && !visited[nextX, nextY])
        //                {
        //                    int index = nextY * bmpData.Stride + nextX * 4;
        //                    int b = rgbValues[index];
        //                    int g = rgbValues[index + 1];
        //                    int r = rgbValues[index + 2];

        //                    if (IsColorMatch(r, g, b, targetColor, tolerance))
        //                    {
        //                        visited[nextX, nextY] = true;
        //                        queue.Enqueue(new Point(nextX, nextY));
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        //}



        /// <summary>
        /// 截取屏幕指定区域并查找满足特定颜色和尺寸条件的坐标列表。
        /// </summary>
        /// <param name="region">要截取的屏幕区域。</param>
        /// <param name="targetColorRgb">目标的RGB颜色。</param>
        /// <param name="tolerance">颜色容差。</param>
        /// <param name="minSize">目标的最小尺寸 (宽度, 高度)。</param>
        /// <param name="maxSize">目标的最大尺寸 (宽度, 高度)。</param>
        /// <returns>返回所有满足条件的目标中心点坐标列表。</returns>
        public static List<Point> FindTargetsOnScreen(
            Rectangle region,
            Color targetColorRgb,
            int tolerance,
            int minWidth,
            int maxWidth,
            int minHeight,
            int maxHeight)
        {
            Size minSize = new Size(minWidth, minHeight);
            Size maxSize = new Size(maxWidth, maxHeight);

            var targetsToClick = new List<Point>();

            // 1. 屏幕截图
            using (Bitmap screenshot = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(region.Location, Point.Empty, region.Size);
                }

                // 2. 将 Bitmap 转换为 EMGU CV 的 Image<Bgr, byte>
                using (Image<Bgr, byte> screenshotCv = screenshot.ToImage<Bgr, byte>())
                {
                    // 3. 设置颜色范围
                    // 注意：EMGU CV 使用 BGR 颜色空间
                    Bgr targetColorBgr = new Bgr(targetColorRgb.B, targetColorRgb.G, targetColorRgb.R);

                    var lowerBound = new Bgr(
                        Math.Max(0, targetColorBgr.Blue - tolerance),
                        Math.Max(0, targetColorBgr.Green - tolerance),
                        Math.Max(0, targetColorBgr.Red - tolerance)
                    );

                    var upperBound = new Bgr(
                        Math.Min(255, targetColorBgr.Blue + tolerance),
                        Math.Min(255, targetColorBgr.Green + tolerance),
                        Math.Min(255, targetColorBgr.Red + tolerance)
                    );

                    // 4. 创建颜色蒙版
                    using (Mat mask = new Mat())
                    {
                        CvInvoke.InRange(screenshotCv,
                                         new ScalarArray(new MCvScalar(lowerBound.Blue, lowerBound.Green, lowerBound.Red)),
                                         new ScalarArray(new MCvScalar(upperBound.Blue, upperBound.Green, upperBound.Red)),
                                         mask);

                        // 5. 查找轮廓
                        using (var contours = new VectorOfVectorOfPoint())
                        {
                            CvInvoke.FindContours(mask, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                            if (contours.Size == 0)
                            {
                                Console.WriteLine("未找到任何指定颜色的区域。");
                                return targetsToClick;
                            }

                            // 6. 遍历并筛选轮廓
                            for (int i = 0; i < contours.Size; i++)
                            {
                                Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);

                                // 7. 判断尺寸是否符合条件
                                if (rect.Width > minSize.Width && rect.Width < maxSize.Width &&
                                    rect.Height > minSize.Height && rect.Height < maxSize.Height)
                                {
                                    // 8. 计算中心点并添加到列表
                                    int centerX = rect.X + rect.Width / 2;
                                    int centerY = rect.Y + rect.Height / 2;
                                    targetsToClick.Add(new Point(centerX, centerY));
                                }
                            }
                        }
                    }
                }
            }

            if (targetsToClick.Count == 0)
            {
                Console.WriteLine("任务完成：未找到任何同时满足颜色和尺寸条件的标记。");
            }
            else
            {
                Console.WriteLine($"筛选完成，共找到 {targetsToClick.Count} 个有效目标。");
            }

            return targetsToClick;
        }
    }
}
