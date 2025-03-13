using Fanex.Data.Repository;
using System;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class Rule4849QueryDataCriteria(string spName) : CriteriaBase
{
    public DateTime CurrentTime { get; set; }

    public string SMAListJson { get; set; }

    public string TableName { get; set; }

    public override string GetSettingKey() => spName;

    public override bool IsValid() => true;
}