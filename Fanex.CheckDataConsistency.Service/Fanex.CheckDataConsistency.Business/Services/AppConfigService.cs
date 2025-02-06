using Fanex.CheckDataConsistency.Business.Model;
using System;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Services;

public static class AppConfigService
{
    public static ConfigSettingModel Config { get; set; }

    public static DateTime? CurrentTime { get; set; } = null;

    public static bool IsOnProcessing { get; set; }

    public static Dictionary<string, List<RetryInfo>> RetryInfos { get; set; } = new()
    {
        { Rule.Rule48, [new RetryInfo()] },
        { Rule.Rule49, [new RetryInfo()] }
    };

    public static int RetryCount { get; set; }
}