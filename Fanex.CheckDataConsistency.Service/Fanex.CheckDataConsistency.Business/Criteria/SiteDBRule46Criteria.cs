using Fanex.Data.Repository;
using System;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class SiteDBRule46Criteria(string siteDB) : CriteriaBase
{
    public int SiteDBGroupID { get; set; }

    public long TransferLastTransId { get; set; }

    public long NewTransferLastTransId { get; set; }

    public DateTime CurrentTime { get; set; } = DateTime.Now;

    private string siteDB { get; set; } = siteDB;

    public override string GetSettingKey() => $"{siteDB}_SiteDB_DBA_Consistency_CheckTicketCountAndChecksum_Transfer";

    public override bool IsValid() => true;
}