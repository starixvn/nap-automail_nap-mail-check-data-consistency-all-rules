using Fanex.CheckDataConsistency.Business.Criteria;
using Fanex.CheckDataConsistency.Business.Model;
using Fanex.Data.Repository;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface DataServiceRule200201202203
{
    List<SettingCountModel> CheckingCount(string spName, string tableName, bool isUseTimeModel);

    List<DataConsistencyConfigModel> GetConfigData();
}

public class DataServiceRule200201202203Imp(IDynamicRepository repository) : DataServiceRule200201202203
{
    public List<SettingCountModel> CheckingCount(string spName, string tableName, bool isUseTimeModel)
    {
        var criteria = new CheckSettingCountCriteria(spName)
        {
            TableName = tableName,
            StartTime = isUseTimeModel ? AppConfigService.TimeModel.StartTime : null,
            EndTime = isUseTimeModel ? AppConfigService.TimeModel.EndTime : null
        };
        var result = repository.FetchMultiple<SettingCountModel, TimeModel>(criteria);

        var timeModel = result.Item2.ToList().FirstOrDefault();
        if (AppConfigService.RetryCountRule200201202203 == 0 && !isUseTimeModel)
        {
            AppConfigService.TimeModel = timeModel;
        }

        return result.Item1.ToList();
    }

    public List<DataConsistencyConfigModel> GetConfigData()
    {
        var criteria = new DataConsistencyConfigSelectCriteria();
        return repository.Fetch<DataConsistencyConfigModel>(criteria).Where(s => s.GroupId == 1).ToList();
    }
}