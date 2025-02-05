using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criterias;

public class SiteDBConfigCriteria : CriteriaBase
{
    public override string GetSettingKey() => "Acc_SiteDBConfigSel";

    public override bool IsValid() => true;
}