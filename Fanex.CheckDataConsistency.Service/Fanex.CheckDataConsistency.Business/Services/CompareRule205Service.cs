using Fanex.CheckDataConsistency.Business.Criteria;
using Fanex.CheckDataConsistency.Business.Model;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface CompareRule205Service
{
    void CheckRule205Customer(DataConsistencyConfigModel config, ResultModel resultModel);

    void CheckRule205CustSub(DataConsistencyConfigModel config, ResultModel resultModel);
}

public class CompareRule205ServiceImp(DataService dataService) : CompareRule205Service
{
    public void CheckRule205Customer(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        QueryRule205Customer(config, resultModel);
        var command = new DataConsistencyConfigUpdateCommand
        {
            RuleNumber = 205,
            TableName = config.TableName,
            VValue = resultModel.MaxCust_New.ToString()
        };

        dataService.UpdateConfig(command);

        resultModel.RulesList.Add("205.1 - Normal Account - Check exists Customer, not exist CustInfoMini");
        resultModel.RulesList.Add($"CustID From {config.VValue} To {resultModel.MaxCust_New}");
        resultModel.RulesList.Add("DBTP.bodb02 - DBA_Consistency_Customer_Checking_DBTP(table Customer)");
        resultModel.RulesList.Add("DBGLOB.GlobCustDB - DBA_Consistency_Customer_Checking(table CustInfoMini)");
    }

    private void QueryRule205Customer(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        const int rowCount = 1;
        var dictResult = new Dictionary<string, CustomerModel>();
        var customerFromDBTP = dataService.QueryCustomerFromDBTP(config.TableName, config.SpName + "_DBTP", config.VValue);
        var customerFromDBGLOB = dataService.QueryCustomerFromDBGLOB(config.TableName, config.SpName, config.VValue, customerFromDBTP.MaxCust_New);

        if (customerFromDBTP.RowCount != customerFromDBGLOB.RowCount || customerFromDBTP.MaxCust_New != customerFromDBGLOB.MaxCust_New)
        {
            dictResult.Add($"{rowCount}_{DB.DBTP}", customerFromDBTP);
            dictResult.Add($"{rowCount}_{DB.DBGLOB}", customerFromDBGLOB);
        }

        resultModel.DictResultCustomer = dictResult;
        resultModel.MaxCust_New = customerFromDBTP.MaxCust_New;
    }

    public void CheckRule205CustSub(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        QueryRule205CustSub(config, resultModel);

        var command = new DataConsistencyConfigUpdateCommand
        {
            RuleNumber = 205,
            TableName = config.TableName,
            VValue = resultModel.MaxCust_New.ToString()
        };

        dataService.UpdateConfig(command);

        resultModel.RulesList.Add("205.2 - Sub Account - Check exists CustSub, not exist CustSubMini");
        resultModel.RulesList.Add($"SubaccID From {config.VValue} To {resultModel.MaxCust_New}");
        resultModel.RulesList.Add("DBTP.bodb02 - DBA_Consistency_CustSub_Checking_DBTP(table CustSub)");
        resultModel.RulesList.Add("DBGLOB.GlobCustDB - DBA_Consistency_CustSub_Checking(table CustSubMini)");
    }

    private void QueryRule205CustSub(DataConsistencyConfigModel config, ResultModel resultModel)
    {
        const int rowCount = 1;
        var dictResult = new Dictionary<string, CustomerModel>();
        var custSubFromDBTP = dataService.QueryCustomerFromDBTP(config.TableName, config.SpName + "_DBTP", config.VValue);
        var custSubFromDBGLOB = dataService.QueryCustomerFromDBGLOB(config.TableName, config.SpName, config.VValue, custSubFromDBTP.MaxCust_New);

        if (custSubFromDBTP.RowCount != custSubFromDBGLOB.RowCount || custSubFromDBTP.MaxCust_New != custSubFromDBGLOB.MaxCust_New)
        {
            dictResult.Add($"{rowCount}_{DB.DBTP}", custSubFromDBTP);
            dictResult.Add($"{rowCount}_{DB.DBGLOB}", custSubFromDBGLOB);
        }

        resultModel.DictResultCustomer = dictResult;
        resultModel.MaxCust_New = custSubFromDBTP.MaxCust_New;
    }
}