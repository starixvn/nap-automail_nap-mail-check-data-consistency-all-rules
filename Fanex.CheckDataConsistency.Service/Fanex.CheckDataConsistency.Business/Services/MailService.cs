using Fanex.CheckDataConsistency.Business.Model;
using Fanex.MailDelivery.Core.Models;
using RazorEngine;
using RazorEngine.Templating;
using System;
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
        var tuple = new Tuple<List<MessageRule4849Model>, DateTime>(messageModels, currentTime);
        var template = File.ReadAllText("Templates/EmailTemplateRule4849.cshtml");
        const string tplKey = "Fanex.CheckDataConsistency.Service-Rule4849";
        string result = Engine.Razor.IsTemplateCached(tplKey, null)
            ? Engine.Razor.Run(tplKey, null, tuple)
            : Engine.Razor.RunCompile(template, tplKey, model: tuple);

        return result;
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
        var tuple = new Tuple<List<MessageRule50Model>>(messageModels);
        var template = File.ReadAllText("Templates/EmailTemplateRule50.cshtml");
        const string tplKey = "Fanex.CheckDataConsistency.Service-Rule50";
        string result = Engine.Razor.IsTemplateCached(tplKey, null)
            ? Engine.Razor.Run(tplKey, null, tuple)
            : Engine.Razor.RunCompile(template, tplKey, model: tuple);

        return result;
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
        var tuple = new Tuple<List<MessageRule53Model>>(messageModels);
        var template = File.ReadAllText("Templates/EmailTemplateRule53.cshtml");
        const string tplKey = "Fanex.CheckDataConsistency.Service-Rule53";
        string result = Engine.Razor.IsTemplateCached(tplKey, null)
            ? Engine.Razor.Run(tplKey, null, tuple)
            : Engine.Razor.RunCompile(template, tplKey, model: tuple);

        return result;
    }
}