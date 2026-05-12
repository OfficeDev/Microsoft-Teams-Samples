// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Models;

namespace Microsoft.BotBuilderSamples.Bots;

public class RegionSelectionBot : ActivityHandler
{
    private readonly BotState _userState;

    public RegionSelectionBot(UserState userState)
    {
        _userState = userState;
    }

    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        var welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
        var didBotWelcomeUser = await welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState(), cancellationToken);

        var text = turnContext.Activity.Text?.ToLowerInvariant() ?? string.Empty;

        if (didBotWelcomeUser.DidUserSelectDomain && (text == "change" || text == "yes"))
        {
            await SendChangeDomainConfirmationCardAsync(turnContext, cancellationToken);
            return;
        }

        switch (text)
        {
            case "reset":
            case "change":
            case "yes":
                await SendDomainListsCardAsync(turnContext, cancellationToken);
                break;

            case "no":
            case "cancel":
                await WelcomeCardAsync(turnContext, cancellationToken);
                break;

            default:
                await SendWelcomeIntroCardAsync(turnContext, cancellationToken);
                break;
        }
    }

    protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        foreach (var member in membersAdded)
        {
            if (member.Id != turnContext.Activity.Recipient.Id)
            {
                var welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
                var didBotWelcomeUser = await welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState(), cancellationToken);

                if (didBotWelcomeUser.DidUserSelectDomain)
                {
                    didBotWelcomeUser.DidUserSelectDomain = false;
                    didBotWelcomeUser.SelectedRegion = string.Empty;
                    didBotWelcomeUser.SelectedDomain = string.Empty;
                }

                await SendWelcomeIntroCardAsync(turnContext, cancellationToken);
            }
        }
    }

    private async Task SendWelcomeIntroCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        if (turnContext.Activity.Text != null && IsAnyDomainSelected(turnContext.Activity.Text))
        {
            await WelcomeCardAsync(turnContext, cancellationToken);
            return;
        }

        var welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
        var didBotWelcomeUser = await welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState(), cancellationToken);
        
        string domain;
        string region;
        
        if (didBotWelcomeUser.DidUserSelectDomain)
        {
            domain = didBotWelcomeUser.SelectedDomain;
            region = didBotWelcomeUser.SelectedRegion;
        }
        else
        {
            var data = GetDefaultInfo(turnContext);
            domain = data.domain;
            region = data.region;
        }

        string welcomeMsg = $"Your default Region is {region}.";

        var card = new HeroCard
        {
            Title = "Welcome to Region Selection App!",
            Subtitle = "This will help you to choose your data center's region.",
            Text = welcomeMsg + " Would you like to change region?",
            Buttons = new List<CardAction>
            {
                new CardAction(ActionTypes.MessageBack, "Yes", null, "Yes", "Yes"),
                new CardAction(ActionTypes.MessageBack, "No", null, "No", "No")
            }
        };

        var response = MessageFactory.Attachment(card.ToAttachment());
        await turnContext.SendActivityAsync(response, cancellationToken);
    }

    private (string region, string domain) GetDefaultInfo(ITurnContext turnContext)
    {
        string serviceUrl = turnContext.Activity.ServiceUrl;
        string domain = serviceUrl[(serviceUrl.LastIndexOf('.') + 1)..];
        string region = turnContext.Activity.Locale ?? "en-US";

        return (region, domain);
    }

    private (string region, string domain) GetSelectedInfo(string text)
    {
        string domain = text.Split("-").FirstOrDefault()?.Trim() ?? string.Empty;

        string file = Path.GetFullPath("ConfigData/Regions.json");
        string json = File.ReadAllText(file);
        var selectedInfo = JsonSerializer.Deserialize<RootObject>(json)?.RegionDomains?.FirstOrDefault(c => c.Region == domain);

        return selectedInfo != null ? (selectedInfo.Region, selectedInfo.Domain) : (string.Empty, string.Empty);
    }

    private async Task SendDomainListsCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        string file = Path.GetFullPath("ConfigData/Regions.json");

        string json = File.ReadAllText(file);
        var rootObject = JsonSerializer.Deserialize<RootObject>(json);

        if (rootObject?.RegionDomains == null || rootObject.RegionDomains.Length == 0)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text("No regions available."), cancellationToken);
            return;
        }

        var regionButtonList = rootObject.RegionDomains.Select(c =>
            new CardAction(
                ActionTypes.MessageBack,
                $"{c.Region} - {c.Country}",
                null,
                $"{c.Region} - {c.Country}",
                $"{c.Region} - {c.Country}"
            )).ToList();

        var card = new HeroCard
        {
            Text = "Please select your region,",
            Buttons = regionButtonList,
        };

        var response = MessageFactory.Attachment(card.ToAttachment());
        await turnContext.SendActivityAsync(response, cancellationToken);
    }

    private async Task WelcomeCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        var welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
        var didBotWelcomeUser = await welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState(), cancellationToken);

        var userName = turnContext.Activity.From.Name;
        var data = GetSelectedInfo(turnContext.Activity.Text ?? string.Empty);
        string domainName = data.domain;
        string regionName = data.region;

        if (string.IsNullOrEmpty(domainName) && didBotWelcomeUser.DidUserSelectDomain)
        {
            domainName = didBotWelcomeUser.SelectedDomain;
            regionName = didBotWelcomeUser.SelectedRegion;
        }

        if (string.IsNullOrEmpty(domainName))
        {
            var defaultData = GetDefaultInfo(turnContext);
            domainName = defaultData.domain;
            regionName = defaultData.region;
        }

        var card = new HeroCard
        {
            Title = $"Welcome {userName},",
            Subtitle = $"You are in {regionName} Region's Data Center",
            Text = "If you want to change data center's region, please enter text 'Change'.",
        };

        didBotWelcomeUser.DidUserSelectDomain = true;
        didBotWelcomeUser.SelectedDomain = domainName;
        didBotWelcomeUser.SelectedRegion = regionName;

        await _userState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);

        var response = MessageFactory.Attachment(card.ToAttachment());
        await turnContext.SendActivityAsync(response, cancellationToken);
    }

    private async Task SendChangeDomainConfirmationCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        var userName = turnContext.Activity.From.Name;
        var welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
        var didBotWelcomeUser = await welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState(), cancellationToken);

        var domainButtonList = new List<CardAction>
        {
            new CardAction(ActionTypes.MessageBack, "Reset", null, "Reset", "Reset"),
            new CardAction(ActionTypes.MessageBack, "Cancel", null, "Cancel", "Cancel"),
        };

        var card = new HeroCard
        {
            Text = $"Hi {userName}, You have already selected your data center region and that is {didBotWelcomeUser.SelectedRegion}. Would you like to change this?",
            Buttons = domainButtonList,
        };

        var response = MessageFactory.Attachment(card.ToAttachment());
        await turnContext.SendActivityAsync(response, cancellationToken);
    }

    private bool IsAnyDomainSelected(string text)
    {
        string domain = text.Split("-").FirstOrDefault()?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(domain))
            return false;

        string file = Path.GetFullPath("ConfigData/Regions.json");
        string json = File.ReadAllText(file);
        bool isAnyDomainSelected = JsonSerializer.Deserialize<RootObject>(json)?.RegionDomains?.Any(c => c.Region == domain) ?? false;

        return isAnyDomainSelected;
    }
}
