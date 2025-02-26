using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class ConsistencyBalanceTriggerCheckingCommand(string spName) : NonQueryCommand
{
    public int CheckingTime { get; set; }

    public long LastRowInserted { get; set; }

    public string TargetServer { get; set; }

    public string SampleData { get; set; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public override string GetSettingKey() => spName;

    public override bool IsValid() => true;
}