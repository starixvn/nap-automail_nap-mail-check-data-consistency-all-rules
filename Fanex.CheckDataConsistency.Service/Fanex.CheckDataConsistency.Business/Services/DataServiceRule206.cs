using Fanex.CheckDataConsistency.Business.Criteria;
using Fanex.CheckDataConsistency.Business.Model;
using Fanex.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface DataServiceRule206
{
    Tuple<List<CustInfoTable1Model>, CustInfoTable2Model> QueryCustomerInHouseCheckingRule206(string server, string tableName, string spName, string vvalue, string smaListJson, byte[] custlistJson);
}

public class DataServiceRule206Imp(IDynamicRepository repository) : DataServiceRule206
{
    public Tuple<List<CustInfoTable1Model>, CustInfoTable2Model> QueryCustomerInHouseCheckingRule206(string server, string tableName, string spName, string vvalue, string smaListJson, byte[] custlistJson)
    {
        var criteria = new CustomerInHouseCheckingRule206Criteria(server, spName)
        {
            TableName = tableName,
            VValue = vvalue,
            SMAListJson = smaListJson,
            CustListJson = custlistJson
        };
        var result = repository.FetchMultiple<CustInfoTable1Model, CustInfoTable2Model>(criteria);

        var table1 = result.FirstResultSet.ToList();
        var table2 = result.Item2.ToList().FirstOrDefault();

        return new Tuple<List<CustInfoTable1Model>, CustInfoTable2Model>(table1, table2);
    }
}