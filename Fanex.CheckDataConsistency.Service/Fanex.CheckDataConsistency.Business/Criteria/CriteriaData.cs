using Fanex.Data.Repository;
using System;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class CriteriaData(string spName) : CriteriaBase
{
    public int CustId { get; set; }

    public DateTime? WinlostDate { get; set; }

    public int Retry { get; set; }

    public override string GetSettingKey() => spName;

    public override bool IsValid() => true;
}