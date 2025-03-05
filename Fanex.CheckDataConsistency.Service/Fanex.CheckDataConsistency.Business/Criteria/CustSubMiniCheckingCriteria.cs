using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class CustSubMiniCheckingCriteria(string server, string spName) : CriteriaBase
{
    public string TableName { get; set; }

    public string VValue { get; set; }

    public string CustMiniList { get; set; }

    public byte[] CustListJson { get; set; }

    public override string GetSettingKey() => $"{server}_{spName}";

    public override bool IsValid() => true;
}