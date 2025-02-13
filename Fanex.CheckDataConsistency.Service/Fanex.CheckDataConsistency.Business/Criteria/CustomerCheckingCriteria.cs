using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class CustomerCheckingCriteria(string server, string spName) : CriteriaBase
{
    public string TableName { get; set; }

    public string VValue { get; set; }

    public int? MaxCust_New { get; set; }

    public override string GetSettingKey() => $"{server}_{spName}";

    public override bool IsValid() => true;
}