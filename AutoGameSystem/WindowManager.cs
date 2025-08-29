using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;

namespace AutoGameSystem
{
    public class WindowManager
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out Rectangle lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        public IntPtr FindEmulatorWindow(string windowTitle)
        {
            return FindWindow(null, windowTitle);
        }

        public bool BringWindowToFront(IntPtr hWnd)
        {
            return SetForegroundWindow(hWnd);
        }

        public Rectangle GetWindowClientRect(IntPtr hWnd)
        {
            GetClientRect(hWnd, out Rectangle rect);
            return rect;
        }

        public Rectangle GetWindowRect(IntPtr hWnd)
        {
            GetWindowRect(hWnd, out Rectangle rect);
            return rect;
        }

        public Point ClientToScreen(IntPtr hWnd, Point clientPoint)
        {
            Point screenPoint = clientPoint;
            ClientToScreen(hWnd, ref screenPoint);
            return screenPoint;
        }

        public Bitmap CaptureWindow(IntPtr hWnd)
        {
            Rectangle clientRect = GetWindowClientRect(hWnd);
            Rectangle windowRect = GetWindowRect(hWnd);

            int borderWidth = (windowRect.Width - clientRect.Width) / 2;
            int titleBarHeight = windowRect.Height - clientRect.Height - borderWidth * 2;

            Bitmap bmp = new Bitmap(clientRect.Width, clientRect.Height);
            Graphics g = Graphics.FromImage(bmp);

            IntPtr hdc = g.GetHdc();
            PrintWindow(hWnd, hdc, 0);
            g.ReleaseHdc(hdc);

            g.Dispose();

            // Crop out borders and title bar
            if (borderWidth > 0 && titleBarHeight > 0)
            {
                Rectangle cropArea = new Rectangle(
                    borderWidth,
                    titleBarHeight,
                    clientRect.Width,
                    clientRect.Height
                );

                Bitmap croppedBmp = new Bitmap(cropArea.Width, cropArea.Height);
                using (Graphics cropG = Graphics.FromImage(croppedBmp))
                {
                    cropG.DrawImage(bmp, new Rectangle(0, 0, croppedBmp.Width, croppedBmp.Height),
                                   cropArea, GraphicsUnit.Pixel);
                }
                bmp.Dispose();
                return croppedBmp;
            }

            return bmp;
        }
    }
}
