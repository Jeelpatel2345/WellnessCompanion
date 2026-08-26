using Microsoft.Win32;
using System.IO;
namespace WellnessCompanion.Services;

public static class StartupService
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "WellnessCompanion";

    public static void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKey);
        if (key == null)
            return;

        if (!enabled)
        {
            key.DeleteValue(AppName, false);
            return;
        }

        var executable = Path.Combine(
            AppContext.BaseDirectory,
            "WellnessCompanion.exe");

        if (!File.Exists(executable))
            return;

        var command = $"\"{executable}\" --startup";
        key.SetValue(AppName, command);
    }

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
        return key?.GetValue(AppName) is string value && !string.IsNullOrWhiteSpace(value);
    }
}
