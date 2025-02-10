using Fanex.Data.Repository;
using System;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class Rule4849QueryDataCriteria(string spName) : CriteriaBase
{
    private string spName { get; set; } = spName;

    public DateTime CurrentTime { get; set; }

    public string RecommendList { get; set; }

    public string TableName { get; set; }

    public override string GetSettingKey() => spName;

    public override bool IsValid() => true;
}