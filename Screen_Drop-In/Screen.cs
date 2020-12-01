using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using static Screen_Drop_In.Native;

namespace Screen_Drop_In
{
    public sealed class Screen : IEquatable<Screen>
    {
        private static readonly int PRIMARYMON = 0x00000001;
        private static int i = 0;
        private readonly IntPtr _monitorHandle;
        private readonly IntPtr _monitorHdc;

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

        public static Screen[] AllScreens { get; }

        public static Screen PrimaryScreen { get; }

        public int BitsPerPixel { get; }

        public Rectangle Bounds { get; }

        public string DeviceName { get; }

        public bool Primary { get; }

        public Rectangle WorkingArea { get; }

        public static Screen? FromHandle(IntPtr handle)
        {
            if (handle == IntPtr.Zero) return null;

            RECT _rect;
            bool result = GetWindowRect(handle, out _rect);
            if (!result) return null;

            Point handlePt = new Point(_rect.Left, _rect.Top);
            return FromPoint(handlePt);
        }

        public static Screen? FromPoint(Point pt, bool nearest = false)
        {
            POINT PT = new POINT() { X = pt.X, Y = pt.Y };
            IntPtr monitorHandle = MonitorFromPoint(PT, nearest ? 2 : 0);

            if (monitorHandle == IntPtr.Zero) return null;

            for (int i = 0; i < AllScreens.Length; i++)
            {
                if (AllScreens[i]._monitorHandle == monitorHandle) return AllScreens[i];
            }

            return null;
        }

        public static Screen? FromRectangle(Rectangle Rct)
        {
            return ScreenOfLargestPortion(Rct);
        }

        public static Rectangle? GetBounds(Point pt)
        {
            return FromPoint(pt)?.Bounds;
        }

        public static Rectangle? GetBounds(Rectangle Rct)
        {
            return ScreenOfLargestPortion(Rct)?.Bounds;
        }

        public static Rectangle? GetWorkingArea(Point pt)
        {
            return FromPoint(pt)?.WorkingArea;
        }

        public static Rectangle? GetWorkingArea(Rectangle Rct)
        {
            return ScreenOfLargestPortion(Rct)?.WorkingArea;
        }
        public bool Equals(Screen? other)
        {
            if (other is not null && other is Screen)
            {
                return other._monitorHandle == _monitorHandle && other._monitorHdc == _monitorHdc;
            }
            return false;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not null && obj is Screen scr)
            {
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

        private static bool MonEnumProc(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lParam)
        {
            AllScreens[i] = new Screen(monitor, hdc);
            i++;
            return true;
        }

        private static Screen? ScreenOfLargestPortion(Rectangle R)
        {
            Rectangle absoluteRect(Rectangle r)
            {
                if (r.Width < 0)
                    r = new Rectangle(r.X + r.Width, r.Y, Math.Abs(r.Width), r.Height);
                if(r.Height < 0)
                    r = new Rectangle(r.X, r.Y + r.Height, r.Width, Math.Abs(r.Height));
                return r;
            }

            int areaFromSize(Size s) => s.Width * s.Height;

            Screen? scr = null;
            Rectangle? rct = null;
            foreach (var screen in AllScreens)
            {
                Rectangle _rct = Rectangle.Intersect(screen.Bounds, absoluteRect(R));
                if (_rct.IsEmpty) continue;
                if (rct is null || (areaFromSize(_rct.Size) > areaFromSize(rct.Value.Size)))
                {
                    rct = _rct;
                    scr = screen;
                }
            }

            return scr;
        }
    }
}