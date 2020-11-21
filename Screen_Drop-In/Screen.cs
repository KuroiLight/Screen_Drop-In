using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using static Screen_Drop_In.Native;

namespace Screen_Drop_In
{
    public sealed class Screen : IEquatable<Screen>
    {
        public static Screen[] AllScreens { get; }

        public static Screen PrimaryScreen { get; }

        public int BitsPerPixel { get; }
        public Rectangle Bounds { get; }
        public Rectangle WorkingArea { get; }
        public string DeviceName { get; }
        public bool Primary { get; }

        private readonly IntPtr _monitorHandle;
        private readonly IntPtr _monitorHdc;

        private static readonly int PRIMARYMON = 0x00000001;
        private static int i = 0;

        static Screen()
        {
            var numOfMonitors = GetSystemMetrics(SM_CMONITORS);
            AllScreens = new Screen[numOfMonitors];
            var dc = GetDC(new HandleRef(null, IntPtr.Zero));
            var href = new HandleRef(null, dc);
            MonitorEnumProc proc = MonEnumProc;
            bool res = EnumDisplayMonitors(href, IntPtr.Zero, proc, IntPtr.Zero);
            PrimaryScreen = AllScreens.First((S) => S.Primary);
        }

        private static bool MonEnumProc(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lParam)
        {
            AllScreens[i] = new Screen(monitor, hdc);
            i++;
            return true;
        }

        private Screen(IntPtr ScrHandle, IntPtr HDC)
        {
            _monitorHandle = ScrHandle;
            _monitorHdc = HDC;

            var monitorinfo = new MONITORINFOEX();

            GetMonitorInfo(new HandleRef(null, ScrHandle), monitorinfo);
            Bounds = monitorinfo.rcMonitor.ToDrawingRectangle();
            WorkingArea = monitorinfo.rcWork.ToDrawingRectangle();
            Primary = ((monitorinfo.dwFlags & PRIMARYMON) != 0);
            DeviceName = new string(monitorinfo.szDevice).TrimEnd((char)0);
            BitsPerPixel = GetDeviceCaps(HDC, 12);
        }

        public static Screen? FromPoint(Point pt)
        {
            for (int i = 0; i < AllScreens.Length; i++) {
                if (AllScreens[i].Bounds.Contains(pt)) return AllScreens[i];
            }
            return null;
        }

        public bool Equals(Screen? other)
        {
            if (other is not null && other is Screen) {
                return other._monitorHandle == _monitorHandle && other._monitorHdc == _monitorHdc;
            }
            return false;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not null && obj is Screen scr) {
                return scr._monitorHandle == _monitorHandle && scr._monitorHdc == _monitorHdc;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _monitorHandle.ToInt32() + _monitorHdc.ToInt32();
        }

        public override string ToString()
        {
            return $"{_monitorHandle} - {_monitorHdc} - {DeviceName} Bounds{Bounds} WorkingArea{WorkingArea} BitsPerPixel:{BitsPerPixel}";
        }
    }
}
