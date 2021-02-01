// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Models;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class DomainSelectionTab : ActivityHandler
    {
        private readonly BotState _userState;

        public DomainSelectionTab(UserState userState)
        {
            _userState = userState;
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
            var didBotWelcomeUser = await welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState(), cancellationToken);

            var text = turnContext.Activity.Text.ToLowerInvariant();

            if (didBotWelcomeUser.DidUserSelectedDomain == true && (text == "change" || text == "yes"))
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

                    if (didBotWelcomeUser.DidUserSelectedDomain == true)
                    {
                        didBotWelcomeUser.DidUserSelectedDomain = false;
                        didBotWelcomeUser.selectedRegion = string.Empty;
                        didBotWelcomeUser.selectedDomain = string.Empty;
                    }

                    await SendWelcomeIntroCardAsync(turnContext, cancellationToken);
                }
            }
        }

        private async Task SendWelcomeIntroCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            string domain = string.Empty;
            string region = string.Empty;

            if (turnContext.Activity.Text != null && IsAnyDomainSelected(turnContext.Activity.Text))
            {
                await WelcomeCardAsync(turnContext, cancellationToken); //set state
                return;
            }

            var welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
            var didBotWelcomeUser = await welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState(), cancellationToken);
            if (didBotWelcomeUser.DidUserSelectedDomain == true)
            {
                domain = didBotWelcomeUser.selectedDomain;
                region = didBotWelcomeUser.selectedRegion;
            }
            else
            {
                var data = getDefaultInfo(turnContext);

                domain = data.domain;
                region = data.region;
            }
            string welcomeMsg = $"Your default domain is {domain} and Region is {region}.";

            var domainButtonlist = new List<CardAction> {
                new CardAction(ActionTypes.MessageBack,"Yes",null,"Yes","Yes","https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0"),
                new CardAction( ActionTypes.MessageBack,"No", null,"No","No","https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0"),
            };

            var card = new HeroCard
            {
                Title = "Welcome to Domain Selection App!",
                Subtitle = "This will help you to choose your domain and region.",
                Text = welcomeMsg + " Would you like to change domain/region?",
                Buttons = domainButtonlist,
            };

            var response = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(response, cancellationToken);
        }

        private (string region, string domain) getDefaultInfo(ITurnContext turnContext)
        {
            string serviceUrl = turnContext.Activity.ServiceUrl;
            string _domain = serviceUrl.Substring(serviceUrl.LastIndexOf(".")).Trim('/');
            string _region = turnContext.Activity.Locale;

            return (_region, _domain);
        }

        private (string region, string domain) getSelectedInfo(string text)
        {
            string selectedDomain = string.Empty;
            string selectedRegion = string.Empty;
            string domain = string.Empty;
            if (text.Split("-").Count() > 1)
                domain = text.Split("-")[1].Trim();

            //get the Json filepath  
            string file = Path.GetFullPath("ConfigData/RegionDomains.json");
            string Json = System.IO.File.ReadAllText(file);
            var selectedInfo = JsonSerializer.Deserialize<Rootobject>(Json).regionDomains.Where(c => c.domain == domain).FirstOrDefault();

            if (selectedInfo != null)
            {
                selectedDomain = selectedInfo.domain;
                selectedRegion = selectedInfo.region;
            }

            return (selectedRegion, selectedDomain);
        }

        private async Task SendDomainListsCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {

            //get the Json filepath  
            string file = Path.GetFullPath("ConfigData/RegionDomains.json");

            //deserialize JSON from file  
            string Json = System.IO.File.ReadAllText(file);

            var domainButtonlist = JsonSerializer.Deserialize<Rootobject>(Json).regionDomains.Select(c =>
                new CardAction(
                        ActionTypes.MessageBack,
                        c.region + " - " + c.domain,
                        null,
                        c.region + " - " + c.domain,
                        c.region + " - " + c.domain,
                        "https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0"
                )).ToList();

            var card = new HeroCard
            {
                Text = @"Please select your region and domain,",
                Buttons = domainButtonlist,
            };

            var response = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(response, cancellationToken);
        }

        private async Task WelcomeCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
            var didBotWelcomeUser = await welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState(), cancellationToken);

            var userName = turnContext.Activity.From.Name;
            string domainName = string.Empty;
            string regionName = string.Empty;

            // Reset with new domain
            var data = getSelectedInfo(turnContext.Activity.Text);
            domainName = data.domain;
            regionName = data.region;

            if (string.IsNullOrEmpty(domainName) && didBotWelcomeUser.DidUserSelectedDomain)
            {
                domainName = didBotWelcomeUser.selectedDomain;
                regionName = didBotWelcomeUser.selectedRegion;
            }

            if (string.IsNullOrEmpty(domainName))
            {
                var defaultData = getDefaultInfo(turnContext);
                domainName = defaultData.domain;
                regionName = defaultData.region;
            }

            var card = new HeroCard
            {
                Title = $"Welcome { userName }, ",
                Subtitle = $"You are in Region {regionName} and domain is {domainName}",
                Text = @"If you want to change domain please enter text 'Change' ",
            };

            //Save any state changes.
            didBotWelcomeUser.DidUserSelectedDomain = true;
            didBotWelcomeUser.selectedDomain = domainName;
            didBotWelcomeUser.selectedRegion = regionName;

            await _userState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);

            var response = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(response, cancellationToken);
        }

        private async Task SendChangeDomainConfirmationCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var userName = turnContext.Activity.From.Name;
            var welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
            var didBotWelcomeUser = await welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState(), cancellationToken);

            var domainButtonlist = new List<CardAction> {
                new CardAction(ActionTypes.MessageBack,"Reset",null,"Reset","Reset","https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0"),
                new CardAction( ActionTypes.MessageBack,"Cancel", null,"Cancel","Cancel","https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0"),
            };

            var card = new HeroCard
            {
                Text = $"Hi { userName }, You have already selected domain and that domain name is { didBotWelcomeUser.selectedDomain} " +
                $"and region is {didBotWelcomeUser.selectedRegion}. Would you like to change domain ?",
                Buttons = domainButtonlist,
            };

            var response = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(response, cancellationToken);
        }

        private bool IsAnyDomainSelected(string text)
        {
            string domain = string.Empty;
            if (text.Split("-").Count() > 1)
                domain = text.Split("-")[1].Trim();

            if (string.IsNullOrEmpty(domain))
                return false;

            //deserialize JSON from file  
            //get the Json filepath  
            string file = Path.GetFullPath("ConfigData/RegionDomains.json");

            string Json = System.IO.File.ReadAllText(file);
            bool IsAnyDomainSelected = JsonSerializer.Deserialize<Rootobject>(Json).regionDomains.Where(c => c.domain == domain).Count() > 0;

            return IsAnyDomainSelected;

        }
    }
}
