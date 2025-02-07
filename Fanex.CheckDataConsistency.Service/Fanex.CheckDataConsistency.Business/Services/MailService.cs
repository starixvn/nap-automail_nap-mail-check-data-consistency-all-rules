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
            return razorEngine.Compile(template);
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
            return razorEngine.Compile(template);
        });

        var tuple = new Tuple<List<MessageRule53Model>>(messageModels);
        return compiledTemplate.Run(tuple);
    }
}