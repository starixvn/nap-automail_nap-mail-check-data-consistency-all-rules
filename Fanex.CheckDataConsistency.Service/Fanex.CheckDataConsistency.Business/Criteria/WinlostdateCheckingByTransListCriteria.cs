using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class WinlostdateCheckingByTransListCriteria : CriteriaBase
{
    public string TransIdList { get; set; }

    public override string GetSettingKey() => "DBTP_DBA_Consistency_WinlostdateChecking_ByTransIdList";

    public override bool IsValid() => true;
}