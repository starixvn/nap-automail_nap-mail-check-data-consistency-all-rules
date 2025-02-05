using Fanex.CheckDataConsistency.Business.Model;
using System;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface RefineDataService
{
    List<MessageRule4849Model> SelectDifferentDataRule4849(DateTime currentTime);

    List<MessageRule50Model> SelectDataRule50(string rule);

    List<MessageRule53Model> SelectDataRule53();
}

public class RefineDataServiceImp(
    DataConsistencyRule4849ProviderService dataServiceRule4849,
    DataConsistencyRule50ProviderService dataServiceRule50,
    DataConsistencyRule53Service dataServiceRule53)
    : RefineDataService
{
    public List<MessageRule4849Model> SelectDifferentDataRule4849(DateTime currentTime)
    {
        var result = new List<MessageRule4849Model>();
        var resultRule48 = dataServiceRule4849.Execute(Rule.Rule48).QueryData(currentTime);
        var resultRule49 = dataServiceRule4849.Execute(Rule.Rule49).QueryData(currentTime);

        result.AddRange(resultRule48);
        result.AddRange(resultRule49);

        return result;
    }

    public List<MessageRule50Model> SelectDataRule50(string rule)
    {
        var resultRule50 = dataServiceRule50.Execute(rule).QueryData();

        return resultRule50;
    }

    public List<MessageRule53Model> SelectDataRule53()
    {
        var resultRule53 = dataServiceRule53.QueryData();

        return resultRule53;
    }
}