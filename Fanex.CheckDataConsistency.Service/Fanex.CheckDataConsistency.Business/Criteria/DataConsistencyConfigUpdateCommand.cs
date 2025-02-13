using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class DataConsistencyConfigUpdateCommand : NonQueryCommand
{
    public string TableName { get; set; }

    public string VValue { get; set; }

    public int RuleNumber { get; set; }

    public override string GetSettingKey() => "CustomerSetting_DataConsistency_Config_Update";

    public override bool IsValid() => true;
}