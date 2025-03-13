using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class CustInfoInHouseCheckingDBPRORule207Criteria(string server, string spName) : CriteriaBase
{
    public byte[] CustListJson { get; set; }

    public override string GetSettingKey() => $"{server}_{spName}";

    public override bool IsValid() => true;
}