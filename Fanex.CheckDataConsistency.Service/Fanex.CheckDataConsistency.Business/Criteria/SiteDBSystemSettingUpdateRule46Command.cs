using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class SiteDBSystemSettingUpdateRule46Command(string siteDb) : NonQueryCommand
{
    public string CheckConsistencyKeyName { get; set; } = "SiteDB_CheckCons_Transfer_DiffSGId_LastTransId";

    public long NewTransferLastTransId { get; set; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public override string GetSettingKey() => $"{siteDb}_SiteDB_DBA_Consistency_SystemSetting_Update";

    public override bool IsValid() => true;
}