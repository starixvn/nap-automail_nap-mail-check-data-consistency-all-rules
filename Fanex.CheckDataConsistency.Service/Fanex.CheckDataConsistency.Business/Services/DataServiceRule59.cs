using Fanex.CheckDataConsistency.Business.Criteria;
using Fanex.Data.Repository;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface DataServiceRule59
{
    List<string> GetTransIdStep1();

    List<string> CheckingWithTransIdStep2(string transIds);
}

public class DataServiceRule59Imp(IDynamicRepository repository) : DataServiceRule59
{
    public List<string> GetTransIdStep1()
    {
        var criteria = new WinlostdateCheckingCriteria();
        var result = repository.Fetch<string>(criteria).ToList();

        return result;
    }

    public List<string> CheckingWithTransIdStep2(string transIds)
    {
        var criteria = new WinlostdateCheckingByTransListCriteria
        {
            TransIdList = transIds
        };

        var result = repository.Fetch<string>(criteria).ToList();
        return result;
    }
}