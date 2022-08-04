// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Infrastructure.AdaptiveCards;

using System.Globalization;
using System.Web;
using global::AdaptiveCards;
using global::AdaptiveCards.Templating;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Factories;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;

public class AdaptiveCardFactory : IAdaptiveCardFactory
{
    private readonly IConfiguration _configuration;

    public AdaptiveCardFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Attachment CreateCustomerInquiryCard(SupportDepartment supportDepartment, CustomerInquiry inquiry)
    {
        var data = new
        {
            title = "New Inquiry!",
            customerName = inquiry.CustomerName,
            question = inquiry.Question,
            createdUtc = $"{inquiry.CreatedDateTime.DateTime.ToString("s", DateTimeFormatInfo.InvariantInfo)}Z",
            openDetails = "Open details",
            subEntityDeepLink = CreateTabDeepLink(
                _configuration.GetValue<string>("ExternalTeamsAppId"),
                supportDepartment.Id,
                supportDepartment.Title,
                inquiry.SubEntityId,
                inquiry.CustomerName,
                supportDepartment.TeamChannelId,
                $"{_configuration.GetValue<string>("HostUrl")}/support-department/{supportDepartment.Id}",
                $"{_configuration.GetValue<string>("HostUrl")}/support-department/{supportDepartment.Id}/inquiry/{inquiry.SubEntityId}")
        };

        var template = GetCardTemplate("customer-inquiry-card.json");

        var serializedJson = template.Expand(data);
        return CreateAttachment(serializedJson);
    }

    private AdaptiveCardTemplate GetCardTemplate(string fileName)
    {
        string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "templates", fileName);
        return new AdaptiveCardTemplate(File.ReadAllText(templatePath));
    }

    private Attachment CreateAttachment(string adaptiveCardJson)
    {
        var adaptiveCard = AdaptiveCard.FromJson(adaptiveCardJson);
        return new Attachment
        {
            ContentType = AdaptiveCard.ContentType,
            Content = adaptiveCard.Card,
        };
    }

    private string CreateTabDeepLink(
        string appId,
        string entityId,
        string entityLabel,
        string? subEntityId,
        string? subEntityLabel,
        string? teamChannelId,
        string? unencodedEntityWebUrl,
        string? unencodedSubEntityWebUrl)
    {
        var queryParams = new Dictionary<string, string?>{
            { "entityLabel", entityLabel },
        };

        if (subEntityLabel != null)
        {
            queryParams.Add("subEntityLabel", subEntityLabel);
        }
        if (unencodedEntityWebUrl != null)
        {
            queryParams.Add("webUrl", HttpUtility.UrlEncode(unencodedEntityWebUrl));
        }
        if (unencodedSubEntityWebUrl != null)
        {
            queryParams.Add("subEntityWebUrl", HttpUtility.UrlEncode(unencodedSubEntityWebUrl));
        }

        List<string> context = new List<string>();
        if (subEntityId != null)
        {
            context.Add($"\"subEntityId\": \"{subEntityId}\"");
        }
        if (teamChannelId != null)
        {
            context.Add($"\"channelId\": \"{teamChannelId}\"");
        }
        if (context.Count > 0)
        {
            queryParams.Add("context", "{" + string.Join(',', context) + "}");
        }

        return QueryHelpers.AddQueryString($"https://teams.microsoft.com/l/entity/{appId}/{entityId}", queryParams);
    }
}
