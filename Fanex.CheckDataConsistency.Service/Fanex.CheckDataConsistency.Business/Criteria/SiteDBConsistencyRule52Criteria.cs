using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class SiteDBConsistencyRule52Criteria(string siteDB) : CriteriaBase
{
    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public override string GetSettingKey() => $"{siteDB}_SiteDB_DBA_Consistency_AutoTransfer_Check_TotalSuperVSTicket";

    public override bool IsValid() => true;
}