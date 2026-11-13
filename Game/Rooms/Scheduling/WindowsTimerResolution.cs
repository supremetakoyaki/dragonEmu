using System.Runtime.InteropServices;

namespace DragonEmu.Game.Rooms;

internal static class WindowsTimerResolution
{
    private const uint Milliseconds = 1;

    public static void Begin()
    {
        if (OperatingSystem.IsWindows())
        {
            TimeBeginPeriod(Milliseconds);
        }
    }

    public static void End()
    {
        if (OperatingSystem.IsWindows())
        {
            TimeEndPeriod(Milliseconds);
        }
    }

    [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
    private static extern uint TimeBeginPeriod(uint period);

    [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
    private static extern uint TimeEndPeriod(uint period);
}
