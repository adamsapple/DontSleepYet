namespace DontSleepYet.Models;

public class LocalSettingsOptions
{
    public string? ApplicationDataFolder
    {
        get; set;
    }

    public string? LocalSettingsFile
    {
        get; set;
    }

    public int DontSleepWakeUpDurationSeconds
    {
        get; set;
    }

    public bool IsDontSleepActive
    {
        get; set;
    }
}
