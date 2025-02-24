using Fanex.CheckDataConsistency.Business.Model;
using Fanex.CheckDataConsistency.Business.Services;
using Fanex.CheckDataConsistency.Business.Services.MailStation;
using Fanex.CheckDataConsistency.Service.Services;
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
        services.AddSingleton<DataServiceRule206, DataServiceRule206Imp>();
        services.AddSingleton<DataServiceRule207, DataServiceRule207Imp>();
        services.AddSingleton<DataServiceRule200201202203, DataServiceRule200201202203Imp>();
        services.AddSingleton<DataServiceRule55, DataServiceRule55Imp>();
        services.AddSingleton<IDynamicRepository, DynamicRepository>();

        services.AddSingleton<MailService, MailServiceImp>();
        services.AddSingleton<RefineDataService, RefineDataServiceImp>();
        services.AddSingleton<DataConsistencyRule4849ProviderService, DataConsistencyRule4849ProviderServiceImp>();
        services.AddSingleton<DataConsistencyRule50ProviderService, DataConsistencyRule50ProviderServiceImp>();
        services.AddSingleton<DataConsistencyRule53Service, DataConsistencyRule53ServiceImp>();
        services.AddSingleton<DataServiceMailStation, DataServiceMailStationImp>();
        services.AddSingleton<DataConsistencyRule46Service, DataConsistencyRule46ServiceImp>();

        services.AddSingleton<MainRule4849Service, MainRule4849ServiceImp>();
        services.AddSingleton<MainRule50Service, MainRule50ServiceImp>();
        services.AddSingleton<MainRule53Service, MainRule53ServiceImp>();
        services.AddSingleton<MainRule46Service, MainRule46ServiceImp>();
        services.AddSingleton<MainRule47Service, MainRule47ServiceImp>();
        services.AddSingleton<MainRule51Service, MainRule51ServiceImp>();
        services.AddSingleton<MainRule52Service, MainRule52ServiceImp>();
        services.AddSingleton<MainRule204Service, MainRule204ServiceImp>();
        services.AddSingleton<MainRule205Service, MainRule205ServiceImp>();
        services.AddSingleton<MainRule206Service, MainRule206ServiceImp>();
        services.AddSingleton<MainRule207Service, MainRule207ServiceImp>();
        services.AddSingleton<MainRule200201202203Service, MainRule200201202203ServiceImp>();
        services.AddSingleton<MainRule55Service, MainRule55ServiceImp>();

        services.AddSingleton<DataConsistencyRule47Service, DataConsistencyRule47ServiceImp>();
        services.AddSingleton<DataConsistencyRule51Service, DataConsistencyRule51ServiceImp>();
        services.AddSingleton<DataConsistencyRule52Service, DataConsistencyRule52ServiceImp>();
        services.AddSingleton<DataConsistencyRule200201202203Service, DataConsistencyRule200201202203ServiceImp>();
        services.AddSingleton<DataConsistencyRule55Service, DataConsistencyRule55ServiceImp>();

        services.AddSingleton<CompareRule204Service, CompareRule204ServiceImp>();
        services.AddSingleton<CompareRule205Service, CompareRule205ServiceImp>();
        services.AddSingleton<CompareRule206Service, CompareRule206ServiceImp>();
        services.AddSingleton<CompareRule207Service, CompareRule207ServiceImp>();

        services.AddSingleton<MainBackgroundRule46Service>();
        services.AddSingleton<MainBackgroundRule47Service>();
        services.AddSingleton<MainBackgroundRule53Service>();
        services.AddSingleton<MainBackgroundRule55Service>();
        services.AddSingleton<MainBackgroundRule204Service>();
        services.AddSingleton<MainBackgroundRule205Service>();
        services.AddSingleton<MainBackgroundRule206Service>();
        services.AddSingleton<MainBackgroundRule207Service>();

        Services = services;
        MailClientHandler.Start();
    }
}