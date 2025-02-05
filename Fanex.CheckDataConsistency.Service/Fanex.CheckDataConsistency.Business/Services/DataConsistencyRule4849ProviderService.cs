using Fanex.CheckDataConsistency.Business.Model;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface DataConsistencyRule4849ProviderService
{
    DataConsistencyRule4849Service Execute(string rule);
}

public class DataConsistencyRule4849ProviderServiceImp(
    DataService dataService,
    Log log) : DataConsistencyRule4849ProviderService
{
    public DataConsistencyRule4849Service Execute(string rule)
    {
        var dict = new Dictionary<string, DataConsistencyRule4849Service>
        {
            { Rule.Rule48 , new  DataConsistencyServiceRule48Imp(dataService, log) },
            { Rule.Rule49, new  DataConsistencyServiceRule49Imp(dataService, log) },
        };

        return dict[rule];
    }
}