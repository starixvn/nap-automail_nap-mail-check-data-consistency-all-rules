using Fanex.CheckDataConsistency.Business.Model;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface DataConsistencyRule50ProviderService
{
    DataConsistencyRule50Service Execute(string rule);
}

public class DataConsistencyRule50ProviderServiceImp(DataService dataService) : DataConsistencyRule50ProviderService
{
    public DataConsistencyRule50Service Execute(string rule)
    {
        var dict = new Dictionary<string, DataConsistencyRule50Service>
        {
            { Rule.Rule50BalanceUp, new  DataConsistencyServiceRule50BUImp(dataService) },
            { Rule.Rule50MoveBet, new  DataConsistencyServiceRule50MBImp(dataService) }
        };

        return dict[rule];
    }
}