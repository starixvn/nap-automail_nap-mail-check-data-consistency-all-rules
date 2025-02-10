using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class SiteDBConfigCriteria : CriteriaBase
{
    public override string GetSettingKey() => "Acc_SiteDBConfigSel";

    public override bool IsValid() => true;
}