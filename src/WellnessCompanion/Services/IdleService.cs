using System.Runtime.InteropServices;

namespace WellnessCompanion.Services;

public static class IdleService
{
    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    public static TimeSpan GetIdleTime()
    {
        var info = new LASTINPUTINFO
        {
            cbSize = (uint)Marshal.SizeOf<LASTINPUTINFO>()
        };

        if (!GetLastInputInfo(ref info))
            return TimeSpan.Zero;

        uint tick = unchecked((uint)Environment.TickCount);
        uint idleMilliseconds = unchecked(tick - info.dwTime);
        return TimeSpan.FromMilliseconds(idleMilliseconds);
    }
}
