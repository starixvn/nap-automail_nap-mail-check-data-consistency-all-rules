using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class ConsistencyCheckTicketCountTransferCriteria(string db) : CriteriaBase
{
    public int SiteDBGroupID { get; set; }

    public long TransferLastTransId { get; set; }

    public long NewTransferLastTransId { get; set; }

    public override string GetSettingKey() => $"{db}DBA_Consistency_CheckTicketCountAndChecksum_Transfer";

    public override bool IsValid() => true;
}