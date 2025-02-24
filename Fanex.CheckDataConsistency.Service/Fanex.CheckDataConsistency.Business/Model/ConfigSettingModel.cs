using System;

namespace Fanex.CheckDataConsistency.Business.Model;

public class ConfigSettingModel
{
    private static readonly DateTimeOffset StartTime = DateTimeOffset.Now;

    public int MailIDRule53 { get; set; }

    public string Path { get; set; }

    public string MailNameRule4849 { get; set; }

    public int MailIDRule4849Sub { get; set; }

    public int MailIDRule46 { get; set; }

    public int MailIDRule47 { get; set; }

    public int MailIDRule204 { get; set; }

    public int MailIDRule205 { get; set; }

    public int MailIDRule206 { get; set; }

    public int MailIDRule207 { get; set; }

    public int MailIDRule55 { get; set; }

    public int SleepIntervalInSecond = 5;

    public bool IsUpdateData { get; set; }

    public bool LifeCycleCompleted
    {
        get
        {
            var workedHours = (DateTimeOffset.Now - StartTime).TotalHours;
            return workedHours >= 12 && DateTimeOffset.Now.Hour >= 21 && DateTimeOffset.Now.Hour <= 22;
        }
    }

    public bool SafeToRestart => LifeCycleCompleted;
}