using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGameSystem.Models;
using AutoGameSystem.Utilities;
using System.Runtime.InteropServices;

namespace AutoGameSystem.Services
{
    public class InputService
    {
        private readonly AppConfig _config;
        private readonly Random _random;

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray)] INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion U;
            public static int Size => Marshal.SizeOf(typeof(INPUT));
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private const int INPUT_MOUSE = 0;
        private const int INPUT_KEYBOARD = 1;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_MOVE = 0x0001;

        public InputService(AppConfig config)
        {
            _config = config;
            _random = new Random();
        }

        private void AddJitterDelay()
        {
            int delay = _config.ClickDelayMs + _random.Next(-_config.JitterRangeMs, _config.JitterRangeMs);
            Thread.Sleep(Math.Max(0, delay));
        }

        public void Click(Point point)
        {
            try
            {
                SetCursorPos(point.X, point.Y);

                INPUT[] inputs = new INPUT[2];
                inputs[0] = CreateMouseInput(0, 0, MOUSEEVENTF_LEFTDOWN);
                inputs[1] = CreateMouseInput(0, 0, MOUSEEVENTF_LEFTUP);

                SendInput(2, inputs, INPUT.Size);
                AddJitterDelay();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error clicking at {point}: {ex.Message}");
            }
        }

        public void Click(IntPtr hWnd, Point clientPoint)
        {
            WindowManager windowManager = new WindowManager();
            Point screenPoint = windowManager.ClientToScreen(hWnd, clientPoint);
            Click(screenPoint);
        }

        public void SendKey(ushort keyCode)
        {
            try
            {
                INPUT[] inputs = new INPUT[2];
                inputs[0] = CreateKeyboardInput(keyCode, 0);
                inputs[1] = CreateKeyboardInput(keyCode, 2); // KEYEVENTF_KEYUP

                SendInput(2, inputs, INPUT.Size);
                AddJitterDelay();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error sending key {keyCode}: {ex.Message}");
            }
        }

        private INPUT CreateMouseInput(int dx, int dy, uint flags)
        {
            return new INPUT
            {
                type = INPUT_MOUSE,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dx = dx,
                        dy = dy,
                        mouseData = 0,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
        }

        private INPUT CreateKeyboardInput(ushort keyCode, uint flags)
        {
            return new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = keyCode,
                        wScan = 0,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
        }
    }
}
