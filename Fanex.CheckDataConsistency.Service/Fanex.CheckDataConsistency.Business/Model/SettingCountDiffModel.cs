using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Model;

public class SettingCountDiffModel
{
    public string SpName { get; set; }

    public int RuleNumber { get; set; }

    public string DBMain { get; set; }

    public List<SettingCountModel> MainDiffData { get; set; } = [];

    public string DBRep { get; set; }

    public List<SettingCountModel> RepDiffData { get; set; } = [];
}