using Microsoft.Win32;
using RepairTracker.Database;

namespace RepairTracker.Helpers;

public enum ThemePreference { System, Dark, Light }

public static class ThemeManager
{
    public static ThemePreference Preference { get; private set; } = ThemePreference.System;
    public static event Action? ThemeChanged;

    public static bool IsDark => Preference switch
    {
        ThemePreference.Dark  => true,
        ThemePreference.Light => false,
        _                     => IsSystemDark()
    };

    public static void LoadSaved()
    {
        string? saved = DbContext.GetAppState("theme");
        Preference = saved != null && Enum.TryParse<ThemePreference>(saved, out var p)
            ? p : ThemePreference.System;
        AppColors.Apply(IsDark);
    }

    public static void SetPreference(ThemePreference pref)
    {
        Preference = pref;
        DbContext.SetAppState("theme", pref.ToString());
        AppColors.Apply(IsDark);
        ThemeChanged?.Invoke();
    }

    private static bool IsSystemDark()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is int v && v == 0;
        }
        catch { return true; }
    }
}
