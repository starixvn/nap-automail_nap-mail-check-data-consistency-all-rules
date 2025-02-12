using Fanex.CheckDataConsistency.Service;
using Fanex.CheckDataConsistency.Service.Services;
using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry.RavenClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddConfiguration();
services.AddFanexSql(configuration);
services.AddFanexLogSentry(configuration);
services.AddOneBookUmForService(configuration);
services.AddInternalService();

services.AddHostedService<MainBackgroundRule53Service>();
services.AddHostedService<MainBackgroundRule46Service>();
services.AddHostedService<MainBackgroundRule47Service>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

await app.RunAsync();