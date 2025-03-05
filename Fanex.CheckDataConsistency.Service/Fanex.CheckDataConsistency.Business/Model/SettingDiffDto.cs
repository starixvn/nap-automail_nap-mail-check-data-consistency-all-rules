using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Model;

public class SettingDiffDto
{
    public DataConsistencyConfigModel Config { get; set; }

    public List<SettingCountDiffModel> Data { get; set; } = [];

    public TimeModel TimeModel { get; set; }
}