using Fanex.Data.Repository;
using System;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class SiteDBConsistencySystemSettingSelectCommand(string siteDB) : NonQueryCommand
{
    public string CheckConsistencyKeyName { get; set; }

    public int SiteDBGroupID { get; set; }

    public long TransferLastTransId { get; set; }

    public long NewTransferLastTransId { get; set; }

    public DateTime CurrentTime { get; set; } = DateTime.Now;

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public override string GetSettingKey() => $"{siteDB}_SiteDB_DBA_Consistency_SystemSetting_Select";

    public override bool IsValid() => true;
}