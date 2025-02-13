using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class ConsistencySystemSettingSelectCommand : NonQueryCommand
{
    public string CheckConsistencyKeyName { get; set; }

    public long TransferLastTransId { get; set; }

    public long NewTransferLastTransId { get; set; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "DBA_Consistency_SystemSetting_Select";

    public override bool IsValid() => true;
}