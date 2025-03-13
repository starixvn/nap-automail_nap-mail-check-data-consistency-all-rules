using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class ConsistencyDetectSiteDBGroupIDJobCriteria(string spName) : CriteriaBase
{
    public override string GetSettingKey() => spName;

    public override bool IsValid() => true;
}