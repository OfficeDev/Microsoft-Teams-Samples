// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.Bots
{
    /// <summary>
    /// Handles region selection activities for the bot.
    /// </summary>
    public class RegionSelectionBot : ActivityHandler
    {
        private readonly BotState _userState;

        public RegionSelectionBot(UserState userState)
        {
            _userState = userState;
        }

        /// <summary>
        /// Handles incoming message activities.
        /// </summary>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
            var didBotWelcomeUser = await welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState(), cancellationToken);

            var text = turnContext.Activity.Text.ToLowerInvariant();

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

        /// <summary>
        /// Handles members added activities.
        /// </summary>
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

        /// <summary>
        /// Sends a welcome introduction card.
        /// </summary>
        private async Task SendWelcomeIntroCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            string domain;
            string region;

            if (turnContext.Activity.Text != null && IsAnyDomainSelected(turnContext.Activity.Text))
            {
                await WelcomeCardAsync(turnContext, cancellationToken); // Set state
                return;
            }

            var welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
            var didBotWelcomeUser = await welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState(), cancellationToken);
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

        /// <summary>
        /// Gets the default region and domain information.
        /// </summary>
        private (string region, string domain) GetDefaultInfo(ITurnContext turnContext)
        {
            string serviceUrl = turnContext.Activity.ServiceUrl;
            string domain = serviceUrl.Substring(serviceUrl.LastIndexOf(".")).Trim('/');
            string region = turnContext.Activity.Locale;

            return (region, domain);
        }

        /// <summary>
        /// Gets the selected region and domain information based on the provided text.
        /// </summary>
        private (string region, string domain) GetSelectedInfo(string text)
        {
            string domain = text.Split("-").FirstOrDefault()?.Trim() ?? string.Empty;

            string file = Path.GetFullPath("ConfigData/Regions.json");
            string json = File.ReadAllText(file);
            var selectedInfo = JsonSerializer.Deserialize<RootObject>(json).RegionDomains.FirstOrDefault(c => c.Region == domain);

            return selectedInfo != null ? (selectedInfo.Region, selectedInfo.Domain) : (string.Empty, string.Empty);
        }

        /// <summary>
        /// Sends a card with a list of available domains.
        /// </summary>
        private async Task SendDomainListsCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Get the JSON file path
            string file = Path.GetFullPath("ConfigData/Regions.json");

            // Deserialize JSON from file
            string json = System.IO.File.ReadAllText(file);
            var rootObject = JsonSerializer.Deserialize<RootObject>(json);

            if (rootObject?.RegionDomains == null || !rootObject.RegionDomains.Any())
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
                    $"{c.Region} - {c.Country}",
                    "https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0"
                )).ToList();

            var card = new HeroCard
            {
                Text = "Please select your region,",
                Buttons = regionButtonList,
            };

            var response = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(response, cancellationToken);
        }

        /// <summary>
        /// Sends a welcome card with the user's selected region and domain.
        /// </summary>
        private async Task WelcomeCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
            var didBotWelcomeUser = await welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState(), cancellationToken);

            var userName = turnContext.Activity.From.Name;
            var data = GetSelectedInfo(turnContext.Activity.Text);
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

        /// <summary>
        /// Sends a confirmation card to change the domain.
        /// </summary>
        private async Task SendChangeDomainConfirmationCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var userName = turnContext.Activity.From.Name;
            var welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
            var didBotWelcomeUser = await welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState(), cancellationToken);

            var domainButtonList = new List<CardAction>
            {
                new CardAction(ActionTypes.MessageBack, "Reset", null, "Reset", "Reset", "https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0"),
                new CardAction(ActionTypes.MessageBack, "Cancel", null, "Cancel", "Cancel", "https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0"),
            };

            var card = new HeroCard
            {
                Text = $"Hi {userName}, You have already selected your data center region and that is {didBotWelcomeUser.SelectedRegion}. Would you like to change this?",
                Buttons = domainButtonList,
            };

            var response = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(response, cancellationToken);
        }

        /// <summary>
        /// Checks if any domain is selected based on the provided text.
        /// </summary>
        private bool IsAnyDomainSelected(string text)
        {
            string domain = text.Split("-").FirstOrDefault()?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(domain))
                return false;

            string file = Path.GetFullPath("ConfigData/Regions.json");
            string json = File.ReadAllText(file);
            bool isAnyDomainSelected = JsonSerializer.Deserialize<RootObject>(json).RegionDomains.Any(c => c.Region == domain);

            return isAnyDomainSelected;
        }
    }
}
