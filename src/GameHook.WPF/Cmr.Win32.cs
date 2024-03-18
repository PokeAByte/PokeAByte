using System.Runtime.InteropServices;

namespace GameHook.WPF
{
    public static class Cmr
    {
        public static partial class Win32
        {
            [DllImport("user32")]
            public static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

            [DllImport("User32")]
            public static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public class MONITORINFO
            {
                public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                public RECT rcMonitor = new RECT { };
                public RECT rcWork = new RECT { };
                public int dwFlags = 0;
            }

            public struct RECT
            {
                public int left, top, right, bottom;

                public RECT(int Left, int Top, int Right, int Bottom)
                {
                    left = Left;
                    top = Top;
                    right = Right;
                    bottom = Bottom;
                }

                public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

                public bool Equals(RECT r)
                {
                    return r.left == left && r.top == top && r.right == right && r.bottom == bottom;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int x;
                public int y;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct MINMAXINFO
            {
                public POINT ptReserved;
                public POINT ptMaxSize;
                public POINT ptMaxPosition;
                public POINT ptMinTrackSize;
                public POINT ptMaxTrackSize;
            };
        }
    }
}