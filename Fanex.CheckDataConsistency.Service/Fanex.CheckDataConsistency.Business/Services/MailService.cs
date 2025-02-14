using Fanex.CheckDataConsistency.Business.Model;
using Fanex.MailDelivery.Core.Models;
using RazorEngineCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface MailService
{
    Mail CreateMailRule4849(List<MessageRule4849Model> messageModels, DateTime currentTime);

    Mail CreateMailRule50(List<MessageRule50Model> messageModels);

    Mail CreateMailRule53(List<MessageRule53Model> messageModels);

    Mail CreateMailRule46(List<MessageRule46Model> messageModels);

    Mail CreateMailRule47(List<MessageRule47Model> messageModels);

    Mail CreateMailRule51(List<MessageRule51Model> messageModels);

    Mail CreateMailRule52(List<MessageRule52Model> messageModels);

    Mail CreateMailRule204(List<ResultModel> data);

    Mail CreateMailRule205(List<ResultModel> data);

    Mail CreateMailRule206(List<ResultModel> data);

    Mail CreateMailRule207(List<ResultModel> data);
}

public class MailServiceImp : MailService
{
    private static readonly ConcurrentDictionary<int, IRazorEngineCompiledTemplate> CompiledTemplateCache = new();

    public Mail CreateMailRule4849(List<MessageRule4849Model> messageModels, DateTime currentTime)
    {
        return new Mail
        {
            Title = "Data Consistency Checking service",
            Subject = $"Data Consistency Checking - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            Body = LoadBodyRule4849(messageModels, currentTime),
        };
    }

    private static string LoadBodyRule4849(List<MessageRule4849Model> messageModels, DateTime currentTime)
    {
        const string templatePath = "Templates/EmailTemplateRule4849.cshtml";
        var hashCode = templatePath.GetHashCode();
        var compiledTemplate = CompiledTemplateCache.GetOrAdd(hashCode, _ =>
        {
            var template = File.ReadAllText(templatePath);
            var razorEngine = new RazorEngine();
            return razorEngine.Compile(template, builder =>
            {
                builder.AddUsing("System");
            });
        });

        var tuple = new Tuple<List<MessageRule4849Model>, DateTime>(messageModels, currentTime);
        return compiledTemplate.Run(tuple);
    }

    public Mail CreateMailRule50(List<MessageRule50Model> messageModels)
    {
        return new Mail
        {
            Title = "Data Consistency Checking service",
            Subject = $"Data Consistency Checking - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            Body = LoadBodyRule50(messageModels),
        };
    }

    private static string LoadBodyRule50(List<MessageRule50Model> messageModels)
    {
        const string templatePath = "Templates/EmailTemplateRule50.cshtml";
        var hashCode = templatePath.GetHashCode();
        var compiledTemplate = CompiledTemplateCache.GetOrAdd(hashCode, _ =>
        {
            var template = File.ReadAllText(templatePath);
            var razorEngine = new RazorEngine();
            return razorEngine.Compile(template, builder =>
            {
                builder.AddUsing("System");
            });
        });

        var tuple = new Tuple<List<MessageRule50Model>>(messageModels);
        return compiledTemplate.Run(tuple);
    }

    public Mail CreateMailRule53(List<MessageRule53Model> messageModels)
    {
        return new Mail
        {
            Title = "Data Consistency Checking service",
            Subject = $"Data Consistency Checking - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            Body = LoadBodyRule53(messageModels),
            MailingListId = AppConfigService.Config.MailIDRule53
        };
    }

    private static string LoadBodyRule53(List<MessageRule53Model> messageModels)
    {
        const string templatePath = "Templates/EmailTemplateRule53.cshtml";
        var hashCode = templatePath.GetHashCode();
        var compiledTemplate = CompiledTemplateCache.GetOrAdd(hashCode, _ =>
        {
            var template = File.ReadAllText(templatePath);
            var razorEngine = new RazorEngine();
            return razorEngine.Compile(template, builder =>
            {
                builder.AddUsing("System");
            });
        });

        var tuple = new Tuple<List<MessageRule53Model>>(messageModels);
        return compiledTemplate.Run(tuple);
    }

    public Mail CreateMailRule46(List<MessageRule46Model> messageModels)
    {
        return new Mail
        {
            Title = "Data Consistency Checking service",
            Subject = $"Data Consistency Checking - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            Body = LoadBodyRule46(messageModels),
            MailingListId = AppConfigService.Config.MailIDRule46
        };
    }

    private static string LoadBodyRule46(List<MessageRule46Model> messageModels)
    {
        const string templatePath = "Templates/EmailTemplateRule46.cshtml";
        var hashCode = templatePath.GetHashCode();
        var compiledTemplate = CompiledTemplateCache.GetOrAdd(hashCode, _ =>
        {
            var template = File.ReadAllText(templatePath);
            var razorEngine = new RazorEngine();
            return razorEngine.Compile(template, builder =>
            {
                builder.AddUsing("System");
            });
        });

        var tuple = new Tuple<List<MessageRule46Model>>(messageModels);
        return compiledTemplate.Run(tuple);
    }

    public Mail CreateMailRule47(List<MessageRule47Model> messageModels)
    {
        return new Mail
        {
            Title = "Data Consistency Checking service",
            Subject = $"Data Consistency Checking - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            Body = LoadBodyRule47(messageModels),
            MailingListId = AppConfigService.Config.MailIDRule47
        };
    }

    private static string LoadBodyRule47(List<MessageRule47Model> messageModels)
    {
        const string templatePath = "Templates/EmailTemplateRule47.cshtml";
        var hashCode = templatePath.GetHashCode();
        var compiledTemplate = CompiledTemplateCache.GetOrAdd(hashCode, _ =>
        {
            var template = File.ReadAllText(templatePath);
            var razorEngine = new RazorEngine();
            return razorEngine.Compile(template, builder =>
            {
                builder.AddUsing("System");
            });
        });

        var tuple = new Tuple<List<MessageRule47Model>>(messageModels);
        return compiledTemplate.Run(tuple);
    }

    public Mail CreateMailRule51(List<MessageRule51Model> messageModels)
    {
        return new Mail
        {
            Title = "Data Consistency Checking service",
            Subject = $"Data Consistency Checking - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            Body = LoadBodyRule51(messageModels),
        };
    }

    private static string LoadBodyRule51(List<MessageRule51Model> messageModels)
    {
        const string templatePath = "Templates/EmailTemplateRule51.cshtml";
        var hashCode = templatePath.GetHashCode();
        var compiledTemplate = CompiledTemplateCache.GetOrAdd(hashCode, _ =>
        {
            var template = File.ReadAllText(templatePath);
            var razorEngine = new RazorEngine();
            return razorEngine.Compile(template, builder =>
            {
                builder.AddUsing("System");
            });
        });

        var tuple = new Tuple<List<MessageRule51Model>>(messageModels);
        return compiledTemplate.Run(tuple);
    }

    public Mail CreateMailRule52(List<MessageRule52Model> messageModels)
    {
        return new Mail
        {
            Title = "Data Consistency Checking service",
            Subject = $"Data Consistency Checking - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            Body = LoadBodyRule52(messageModels)
        };
    }

    private static string LoadBodyRule52(List<MessageRule52Model> messageModels)
    {
        const string templatePath = "Templates/EmailTemplateRule52.cshtml";
        var hashCode = templatePath.GetHashCode();
        var compiledTemplate = CompiledTemplateCache.GetOrAdd(hashCode, _ =>
        {
            var template = File.ReadAllText(templatePath);
            var razorEngine = new RazorEngine();
            return razorEngine.Compile(template, builder =>
            {
                builder.AddUsing("System");
            });
        });

        var tuple = new Tuple<List<MessageRule52Model>>(messageModels);
        return compiledTemplate.Run(tuple);
    }

    public Mail CreateMailRule204(List<ResultModel> data)
    {
        return new Mail
        {
            Title = "Data Consistency Customer Setting Checking service",
            Subject = $"Data Consistency Customer Setting Checking - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            Body = LoadBodyRule204(data)
        };
    }

    private static string LoadBodyRule204(List<ResultModel> data)
    {
        const string templatePath = "Templates/EmailTemplateRule204.cshtml";
        var hashCode = templatePath.GetHashCode();
        var compiledTemplate = CompiledTemplateCache.GetOrAdd(hashCode, _ =>
        {
            var template = File.ReadAllText(templatePath);
            var razorEngine = new RazorEngine();
            return razorEngine.Compile(template, builder =>
            {
                builder.AddUsing("System");
            });
        });

        var tuple = new Tuple<List<ResultModel>>(data);
        return compiledTemplate.Run(tuple);
    }

    public Mail CreateMailRule205(List<ResultModel> data)
    {
        return new Mail
        {
            Title = "Data Consistency Customer Setting Checking service",
            Subject = $"Data Consistency Customer Setting Checking - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            Body = LoadBodyRule205(data)
        };
    }

    private static string LoadBodyRule205(List<ResultModel> data)
    {
        const string templatePath = "Templates/EmailTemplateRule205.cshtml";
        var hashCode = templatePath.GetHashCode();
        var compiledTemplate = CompiledTemplateCache.GetOrAdd(hashCode, _ =>
        {
            var template = File.ReadAllText(templatePath);
            var razorEngine = new RazorEngine();
            return razorEngine.Compile(template, builder =>
            {
                builder.AddUsing("System");
            });
        });

        var tuple = new Tuple<List<ResultModel>>(data);
        return compiledTemplate.Run(tuple);
    }

    public Mail CreateMailRule206(List<ResultModel> data)
    {
        return new Mail
        {
            Title = "Data Consistency Customer Setting Checking service",
            Subject = $"Data Consistency Customer Setting Checking - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            Body = LoadBodyRule206(data)
        };
    }

    private static string LoadBodyRule206(List<ResultModel> data)
    {
        const string templatePath = "Templates/EmailTemplateRule206.cshtml";
        var hashCode = templatePath.GetHashCode();
        var compiledTemplate = CompiledTemplateCache.GetOrAdd(hashCode, _ =>
        {
            var template = File.ReadAllText(templatePath);
            var razorEngine = new RazorEngine();
            return razorEngine.Compile(template, builder =>
            {
                builder.AddUsing("System");
            });
        });

        var tuple = new Tuple<List<ResultModel>>(data);
        return compiledTemplate.Run(tuple);
    }

    public Mail CreateMailRule207(List<ResultModel> data)
    {
        return new Mail
        {
            Title = "Data Consistency Customer Setting Checking service",
            Subject = $"Data Consistency Customer Setting Checking - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            Body = LoadBodyRule207(data)
        };
    }

    private static string LoadBodyRule207(List<ResultModel> data)
    {
        const string templatePath = "Templates/EmailTemplateRule207.cshtml";
        var hashCode = templatePath.GetHashCode();
        var compiledTemplate = CompiledTemplateCache.GetOrAdd(hashCode, _ =>
        {
            var template = File.ReadAllText(templatePath);
            var razorEngine = new RazorEngine();
            return razorEngine.Compile(template, builder =>
            {
                builder.AddUsing("System");
            });
        });

        var tuple = new Tuple<List<ResultModel>>(data);
        return compiledTemplate.Run(tuple);
    }
}