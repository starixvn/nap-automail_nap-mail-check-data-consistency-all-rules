using Fanex.CheckDataConsistency.Business.Model;
using System;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Services;

public static class AppConfigService
{
    public static ConfigSettingModel Config { get; set; }

    public static DateTime? CurrentTime { get; set; } = null;

    public static bool IsOnProcessingRule4849 { get; set; }

    public static bool IsOnProcessingRule46 { get; set; }

    public static bool IsOnProcessingRule47 { get; set; }

    public static bool IsOnProcessingRule204 { get; set; }

    public static bool IsOnProcessingRule205 { get; set; }

    public static bool IsOnProcessingRule206 { get; set; }

    public static bool IsOnProcessingRule207 { get; set; }

    public static bool IsOnProcessingRule61 { get; set; }

    public static Dictionary<string, List<RetryInfo>> RetryInfos { get; set; } = new()
    {
        { Rule.Rule48, [new RetryInfo()] },
        { Rule.Rule49, [new RetryInfo()] }
    };

    public static int RetryCountRule4849 { get; set; }

    public static int RetryCountRule200201202203 { get; set; }

    public static TimeModel TimeModel { get; set; } = new();

    public static Dictionary<string, Rule46DTO> DataSiteDBs { get; set; }
}