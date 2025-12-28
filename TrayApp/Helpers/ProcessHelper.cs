using System.Diagnostics;

namespace LolStatsTracker.TrayApp.Helpers;

public static class ProcessHelper
{
    public static bool IsProcessRunning(string processName)
    {
        try
        {
            var processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }
        catch
        {
            return false;
        }
    }
    
    public static Process? GetProcessByName(string processName)
    {
        try
        {
            var processes = Process.GetProcessesByName(processName);
            return processes.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }
    
    public static Process? GetProcessById(int processId)
    {
        try
        {
            return Process.GetProcessById(processId);
        }
        catch
        {
            return null;
        }
    }
}
