using ONEbook.UM.Configurations;

namespace Fanex.CheckDataConsistency.Business.Model;

public class ConfigSettingModel
{
    public string SentryClientKey { get; set; }

    public string CategorySentryLog { get; set; }

    public string CronSchedule { get; set; }

    public UMClientConfiguration UMConfig { get; set; }

    public int MailIDRule53 { get; set; }

    public string Path { get; set; }

    public string MailNameRule4849 { get; set; }

    public int MailIDRule4849Sub { get; set; }
}