using Fanex.Data.Repository;
using System;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class CheckSettingCountCriteria(string spName) : CriteriaBase
{
    public string TableName { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public override string GetSettingKey() => spName;

    public override bool IsValid() => true;
}