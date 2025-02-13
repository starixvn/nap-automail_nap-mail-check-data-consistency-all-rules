using Fanex.CheckDataConsistency.Business.Model;
using System;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface RefineDataService
{
    List<MessageRule4849Model> SelectDifferentDataRule4849(DateTime currentTime);

    List<MessageRule50Model> SelectDataRule50(string rule);

    List<MessageRule53Model> SelectDataRule53();

    List<MessageRule46Model> SelectDifferentDataRule46();

    void UpdateSiteDBSystemSettingRule46();

    List<MessageRule47Model> SelectDifferentDataRule47();

    List<MessageRule51Model> SelectDifferentDataRule51();

    List<MessageRule52Model> SelectDifferentDataRule52();
}

public class RefineDataServiceImp(
    DataConsistencyRule4849ProviderService dataServiceRule4849,
    DataConsistencyRule50ProviderService dataServiceRule50,
    DataConsistencyRule53Service dataServiceRule53,
    DataConsistencyRule46Service dataServiceRule46,
    DataConsistencyRule47Service dataServiceRule47,
    DataConsistencyRule51Service dataServiceRule51,
    DataConsistencyRule52Service dataServiceRule52)
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

    public List<MessageRule46Model> SelectDifferentDataRule46()
    {
        var resultRule46 = dataServiceRule46.QueryData();

        return resultRule46;
    }

    public void UpdateSiteDBSystemSettingRule46()
    {
        var dataSiteDBs = AppConfigService.DataSiteDBs;
        dataServiceRule46.UpdateSiteDBSystemSettingRule46(dataSiteDBs);
    }

    public List<MessageRule47Model> SelectDifferentDataRule47()
    {
        var resultRule47 = dataServiceRule47.QueryData();

        return resultRule47;
    }

    public List<MessageRule51Model> SelectDifferentDataRule51()
    {
        var resultRule51 = dataServiceRule51.QueryData();

        return resultRule51;
    }

    public List<MessageRule52Model> SelectDifferentDataRule52()
    {
        var resultRule52 = dataServiceRule52.QueryData();

        return resultRule52;
    }
}