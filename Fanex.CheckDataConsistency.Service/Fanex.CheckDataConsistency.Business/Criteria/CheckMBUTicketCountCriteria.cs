using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criteria;

public class CheckMBUTicketCountCriteria(string spName) : CriteriaBase
{
    public int Bettype { get; set; }

    public override string GetSettingKey() => spName;

    public override bool IsValid() => true;
}