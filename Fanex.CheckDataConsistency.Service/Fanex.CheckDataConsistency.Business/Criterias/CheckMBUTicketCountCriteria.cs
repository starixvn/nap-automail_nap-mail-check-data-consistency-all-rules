using Fanex.Data.Repository;

namespace Fanex.CheckDataConsistency.Business.Criterias;

public class CheckMBUTicketCountCriteria(string spName) : CriteriaBase
{
    private string spName { get; set; } = spName;

    public int Bettype { get; set; }

    public override string GetSettingKey() => spName;

    public override bool IsValid() => true;
}