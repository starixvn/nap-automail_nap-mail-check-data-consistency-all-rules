using Fanex.CheckDataConsistency.Business.Criteria;
using Fanex.CheckDataConsistency.Business.Model;
using Fanex.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface DataServiceRule55
{
    Tuple<List<ConsistencyRule55Model>, List<FromTOSeqID>> QueryRule55();
}

public class DataServiceRule55Imp(IDynamicRepository repository) : DataServiceRule55
{
    public Tuple<List<ConsistencyRule55Model>, List<FromTOSeqID>> QueryRule55()
    {
        var criteria = new CriteriaDataBase("DBA_Consistency_CheckWrongSiteDBGroupID_Job");
        var result = repository.FetchMultiple<ConsistencyRule55Model, FromTOSeqID>(criteria);

        return new Tuple<List<ConsistencyRule55Model>, List<FromTOSeqID>>
        (
            result.Item1.ToList(),
            result.Item2.ToList()
        );
    }
}