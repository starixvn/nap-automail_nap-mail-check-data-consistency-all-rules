using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class WinlostdateCheckingCriteria : CriteriaBase
{
    public override string GetSettingKey() => "DB05_DBA_Consistency_WinlostdateChecking";

    public override bool IsValid() => true;
}