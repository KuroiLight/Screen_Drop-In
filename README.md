# Screen_Drop-In
Drop-In replacement for Screen when not using WindowsForms (Compiled with .Net 5.0, C# 9.0, Nullable=enable).

Based of WpfScreenHelper but slimmed down and returns null (C# 9.0 nullables) when no match is found

#### Instance members:
* int BitsPerPixel
* Rectangle Bounds
* Rectangle WorkingArea
* string DeviceName
* bool Primary

#### Static members:
* Screen[] AllScreens
* Screen PrimaryScreen
* Screen? FromPoint(Point)
* Rectangle? GetBounds(Point/Rectangle)
* Rectangle? GetWorkingArea(Point/Rectangle)
* Screen? FromHandle(IntPtr)
