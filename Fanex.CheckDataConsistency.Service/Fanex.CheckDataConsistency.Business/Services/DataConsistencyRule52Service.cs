using Fanex.CheckDataConsistency.Business.Model;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface DataConsistencyRule52Service
{
    List<MessageRule52Model> QueryData();
}

public class DataConsistencyRule52ServiceImp(DataService dataService) : DataConsistencyRule52Service
{
    public List<MessageRule52Model> QueryData()
    {
        var data = dataService.QueryRule52();

        if (data.Count <= 0)
        {
            return [];
        }

        var message = new MessageRule52Model
        {
            Rule = "#52",
            Message = "Automate transfer from Super Balance to Licensee Code: Verify number of transfer tickets match with number of Supers, who make the transfer.",
            SPName = "SiteDB_DBA_Consistency_AutoTransfer_Check_TotalSuperVSTicket",
            Data = data
        };

        return [message];
    }
}