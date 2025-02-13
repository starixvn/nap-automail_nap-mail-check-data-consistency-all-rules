using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class DataConsistencyConfigSelectCriteria : CriteriaBase
{
    public override string GetSettingKey() => "CustomerSetting_DataConsistency_Config_SELECT";

    public override bool IsValid() => true;
}