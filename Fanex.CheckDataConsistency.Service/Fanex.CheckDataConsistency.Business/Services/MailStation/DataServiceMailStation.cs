using Fanex.CheckDataConsistency.Business.Criterias.MailStation;
using Fanex.Data.Repository;
using Fanex.MailDelivery.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using MailTaskModel = Fanex.CheckDataConsistency.Business.Model.MailStation.MailTaskModel;

namespace Fanex.CheckDataConsistency.Business.Services.MailStation;

public interface DataServiceMailStation
{
    IEnumerable<MailTaskModel> QueryMailTask();

    void UpdateMailStatus(JobStatus jobStatus);
}

public class DataServiceMailStationImp(IDynamicRepository repository, Log log) : DataServiceMailStation
{
    public IEnumerable<MailTaskModel> QueryMailTask()
    {
        var criteria = new GetMailTaskListCriteria
        {
            MailName = AppConfigService.Config.MailNameRule4849,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today
        };

        return repository.Fetch<MailTaskModel>(criteria);
    }

    public void UpdateMailStatus(JobStatus jobStatus)
    {
        var task = QueryMailTask().FirstOrDefault(s => s.Status == 1);
        log.Log($"The Rule 48-49 - UpdateMailStatus - {JsonConvert.SerializeObject(task, Formatting.Indented)}");
        var taskId = task != null ? task.MailTaskID : 0;
        var command = new UpdateMailStatusCommand
        {
            TaskID = taskId,
            Status = (int)jobStatus,
            LastSent = DateTime.Now,
        };

        repository.Execute(command);
    }
}