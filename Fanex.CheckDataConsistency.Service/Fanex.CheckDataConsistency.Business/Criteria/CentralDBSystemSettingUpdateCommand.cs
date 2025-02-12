using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class CentralDBSystemSettingUpdateCommand : NonQueryCommand
{
    public string CheckConsistencyKeyName { get; set; } = "DiffSGId_LastTransId";

    public long NewTransferLastTransId { get; set; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "DBA_Consistency_SystemSetting_Update";

    public override bool IsValid() => true;
}