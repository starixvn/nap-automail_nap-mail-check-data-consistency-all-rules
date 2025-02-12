using Fanex.CheckDataConsistency.Business.Model;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface DataConsistencyRule51Service
{
    List<MessageRule51Model> QueryData();
}

public class DataConsistencyRule51ServiceImp(DataService dataService) : DataConsistencyRule51Service
{
    public List<MessageRule51Model> QueryData()
    {
        var result = new List<MessageRule51Model>();
        var resultOutput = new Result();
        var data = dataService.QueryRule51(resultOutput);

        if (data.Count > 0 || resultOutput.ErrCode > 0)
        {
            var message = new MessageRule51Model
            {
                Rule = "#51",
                Message = "Automate transfer from Super Balance to Licensee Code: Compare estimated number of transfer tickets (from Super to Licensee Code) equal to actual number of transfer tickets",
                SPName = "DBA_Consistency_AutoTransfer_Check_TotalTicketCorrect",
                Data = data
            };

            result.Add(message);
        }

        return result;
    }
}