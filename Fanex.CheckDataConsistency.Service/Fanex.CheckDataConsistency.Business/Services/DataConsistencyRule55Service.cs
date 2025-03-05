using Fanex.CheckDataConsistency.Business.Model;
using System.Linq;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface DataConsistencyRule55Service
{
    MessageRule55Model QueryData();
}

public class DataConsistencyRule55ServiceImp(DataServiceRule55 dataService) : DataConsistencyRule55Service
{
    public MessageRule55Model QueryData()
    {
        var data = dataService.QueryRule55();
        var message = new MessageRule55Model
        {
            Rule = "#55",
            Message = "Check if bet tickets are inserted into the correct SiteDBGroupID by reverting the transID",
            SPName = "DBA_Consistency_CheckWrongSiteDBGroupID_Job",
            DB = DB.DBCONSISTENCY,
            Data = data.Item1.Skip(0).Take(100).ToList(),
            SeqIds = data.Item2
        };

        return message;
    }
}