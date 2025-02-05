using Fanex.CheckDataConsistency.Business.Model;
using Fanex.CheckDataConsistency.Business.Services;
using Fanex.CheckDataConsistency.Business.Services.MailStation;
using Fanex.Data.Repository;
using Fanex.MailDelivery.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Fanex.CheckDataConsistency.Service;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Services { get; set; }

    public static void AddConfiguration(this IServiceCollection services)
    {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        var text = File.ReadAllText("appsettings.json");
        AppConfigService.Config = JsonConvert.DeserializeObject<ConfigSettingModel>(text);
    }

    public static void AddInternalService(this IServiceCollection services)
    {
        services.AddSingleton<Log, LogImp>();
        services.AddSingleton<DataService, DataServiceImp>();
        services.AddSingleton<IDynamicRepository, DynamicRepository>();
        services.AddSingleton<MainRule4849Service, MainRule4849ServiceImp>();
        services.AddSingleton<MainRule50Service, MainRule50ServiceImp>();
        services.AddSingleton<MainRule53Service, MainRule53ServiceImp>();
        services.AddSingleton<MailService, MailServiceImp>();
        services.AddSingleton<RefineDataService, RefineDataServiceImp>();
        services.AddSingleton<DataConsistencyRule4849ProviderService, DataConsistencyRule4849ProviderServiceImp>();
        services.AddSingleton<DataConsistencyRule50ProviderService, DataConsistencyRule50ProviderServiceImp>();
        services.AddSingleton<DataConsistencyRule53Service, DataConsistencyRule53ServiceImp>();
        services.AddSingleton<DataServiceMailStation, DataServiceMailStationImp>();

        Services = services;
        MailClientHandler.Start();
    }
}