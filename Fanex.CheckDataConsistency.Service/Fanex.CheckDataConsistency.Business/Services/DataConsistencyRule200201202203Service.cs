using Fanex.CheckDataConsistency.Business.Model;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface DataConsistencyRule200201202203Service
{
    List<SettingDiffDto> CheckSettingCount(List<DataConsistencyConfigModel> configs);
}

public class DataConsistencyRule200201202203ServiceImp(DataServiceRule200201202203 dataService) : DataConsistencyRule200201202203Service
{
    public List<SettingDiffDto> CheckSettingCount(List<DataConsistencyConfigModel> configs)
    {
        var dataDto = new List<SettingDiffDto>();
        configs.ForEach(config =>
        {
            var data = new List<SettingCountDiffModel>();
            DB.MainDB.ForEach(mainDb =>
            {
                CheckingDataBetweenMainAndRep(mainDb, config, data);
            });
            if (data.Count > 0)
            {
                dataDto.Add(new SettingDiffDto
                {
                    Config = config,
                    Data = data,
                    TimeModel = AppConfigService.TimeModel
                });
            }
        });

        return dataDto;
    }

    private void CheckingDataBetweenMainAndRep(string mainDb, DataConsistencyConfigModel config, ICollection<SettingCountDiffModel> data)
    {
        var spName = $"{mainDb}_{config.SpName}";
        var isUseTimeModel = AppConfigService.RetryCountRule200201202203 > 0;
        var mainData = dataService.CheckingCount(spName, config.TableName, isUseTimeModel);

        DB.RepDB.ForEach(repDb =>
        {
            spName = repDb.Equals("DBTP") ? $"{repDb}_{config.SpName}_DBTP" : $"{repDb}_{config.SpName}";

            var repData = dataService.CheckingCount(spName, config.TableName, true);
            var diffMainData = new List<SettingCountModel>();
            var diffRepData = new List<SettingCountModel>();
            mainData.ForEach(mainRow =>
            {
                var repRow = repData.Find(x => x.TableName == mainRow.TableName && x.Site == mainRow.Site);
                if (repRow == null)
                {
                    diffMainData.Add(mainRow);
                }
                else
                {
                    if (mainRow.RowCount == repRow.RowCount && mainRow.CHECKSUM_Value == repRow.CHECKSUM_Value)
                    {
                        return;
                    }

                    diffMainData.Add(mainRow);
                    diffRepData.Add(repRow);
                }
            });

            var diffModel = new SettingCountDiffModel
            {
                DBMain = mainDb,
                MainDiffData = diffMainData,
                DBRep = repDb,
                RepDiffData = diffRepData,
                SpName = config.SpName,
                RuleNumber = config.RuleNumber
            };

            if (diffModel.MainDiffData.Count > 0 || diffModel.RepDiffData.Count > 0)
            {
                data.Add(diffModel);
            }
        });
    }
}