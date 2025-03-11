// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Generated with Bot Builder V4 SDK Template for Visual Studio v4.14.0

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards.Templating;
using TypeaheadSearch.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;

namespace TypeaheadSearch.Bots
{
    /// <summary>
    /// Bot Activity handler class.
    /// </summary>
    public class ActivityBot : TeamsActivityHandler
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        /// <summary>
        /// Handle when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text != null)
            {
                await HandleTextMessageAsync(turnContext, cancellationToken);
            }
            else if (turnContext.Activity.Value != null)
            {
                var data = JsonConvert.DeserializeObject<StaticSearchCard>(turnContext.Activity.Value.ToString());
                await turnContext.SendActivityAsync(MessageFactory.Text("Selected option is: " + data.choiceSelect), cancellationToken);
            }
        }

        /// <summary>
        /// Handles text messages sent to the bot.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task HandleTextMessageAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var text = turnContext.Activity.Text.ToLower().Trim();
            string[] path;
            switch (text)
            {
                case "staticsearch":
                    path = new[] { ".", "Cards", "StaticSearchCard.json" };
                    break;
                case "dynamicsearch":
                    path = new[] { ".", "Cards", "DynamicSearchCard.json" };
                    break;
                case "dependantdropdown":
                    path = new[] { ".", "Cards", "DependentDropdown.json" };
                    break;
                default:
                    return;
            }

            var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
            var initialAdaptiveCard = GetFirstOptionsAdaptiveCard(path, turnContext.Activity.From.Name, member.Id);
            await turnContext.SendActivityAsync(MessageFactory.Attachment(initialAdaptiveCard), cancellationToken);
        }

        /// <summary>
        /// Invoked when bot (like a user) are added to the conversation.
        /// </summary>
        /// <param name="membersAdded">A list of all the members added to the conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Hello and welcome! With this sample you can see the functionality of static, dynamic and dependant dropdown search in adaptive card."), cancellationToken);
                }
            }
        }

        /// <summary>
        /// Invoked when an invoke activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Name == "application/search")
            {
                return await HandleSearchInvokeAsync(turnContext, cancellationToken);
            }

            return null;
        }

        /// <summary>
        /// Handles search invoke activities.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task<InvokeResponse> HandleSearchInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var dropdownCard = JsonConvert.DeserializeObject<DependantDropdownCard>(turnContext.Activity.Value.ToString());
            var searchData = JsonConvert.DeserializeObject<DynamicSearchCard>(turnContext.Activity.Value.ToString());

            var packageResult = JObject.Parse(await HttpClient.GetStringAsync($"https://azuresearch-usnc.nuget.org/query?q=id:{searchData.queryText}&prerelease=true"));

            if (!string.IsNullOrEmpty(dropdownCard.Data.choiceSelect))
            {
                var searchResponseData = GetCountrySpecificData(dropdownCard.Data.choiceSelect.ToLower());
                return new InvokeResponse
                {
                    Status = 200,
                    Body = JObject.Parse(JsonConvert.SerializeObject(searchResponseData))
                };
            }
            else
            {
                var packages = packageResult["data"].Select(item => new { title = item["id"].ToString(), value = item["id"] + " - " + item["description"] });
                var searchResponseData = new
                {
                    type = "application/vnd.microsoft.search.searchResponse",
                    value = new { results = packages }
                };

                return new InvokeResponse
                {
                    Status = 200,
                    Body = JObject.Parse(JsonConvert.SerializeObject(searchResponseData))
                };
            }
        }

        /// <summary>
        /// Gets country-specific data for the dropdown.
        /// </summary>
        /// <param name="country">The country name.</param>
        /// <returns>The country-specific data.</returns>
        private object GetCountrySpecificData(string country)
        {
            var usa = new[]
            {
                    new { title = "CA", value = "CA" },
                    new { title = "FL", value = "FL" },
                    new { title = "TX", value = "TX" }
                };

            var france = new[]
            {
                    new { title = "Paris", value = "Paris" },
                    new { title = "Lyon", value = "Lyon" },
                    new { title = "Nice", value = "Nice" }
                };

            var india = new[]
            {
                    new { title = "Delhi", value = "Delhi" },
                    new { title = "Mumbai", value = "Mumbai" },
                    new { title = "Pune", value = "Pune" }
                };

            return country switch
            {
                "usa" => new { type = "application/vnd.microsoft.search.searchResponse", value = new { results = usa } },
                "france" => new { type = "application/vnd.microsoft.search.searchResponse", value = new { results = france } },
                _ => new { type = "application/vnd.microsoft.search.searchResponse", value = new { results = india } }
            };
        }

        /// <summary>
        /// Gets the initial adaptive card.
        /// </summary>
        /// <param name="filepath">The file path to the adaptive card JSON.</param>
        /// <param name="name">The name of the user.</param>
        /// <param name="userMRI">The user MRI.</param>
        /// <returns>The adaptive card attachment.</returns>
        private Attachment GetFirstOptionsAdaptiveCard(string[] filepath, string name = null, string userMRI = null)
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);
            var payloadData = new
            {
                createdById = userMRI,
                createdBy = name
            };

            var cardJsonString = template.Expand(payloadData);
            return new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJsonString),
            };
        }
    }
}