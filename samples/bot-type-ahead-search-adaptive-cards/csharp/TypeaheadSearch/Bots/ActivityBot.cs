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
                if (turnContext.Activity.Text.ToLower().Trim() == "staticsearch")
                {
                    string[] path = { ".", "Cards", "StaticSearchCard.json" };
                    var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
                    var initialAdaptiveCard = GetFirstOptionsAdaptiveCard(path, turnContext.Activity.From.Name, member.Id);

                    await turnContext.SendActivityAsync(MessageFactory.Attachment(initialAdaptiveCard), cancellationToken);
                }
                else if (turnContext.Activity.Text.ToLower().Trim() == "dynamicsearch")
                {
                    string[] path = { ".", "Cards", "DynamicSearchCard.json" };
                    var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
                    var initialAdaptiveCard = GetFirstOptionsAdaptiveCard(path, turnContext.Activity.From.Name, member.Id);

                    await turnContext.SendActivityAsync(MessageFactory.Attachment(initialAdaptiveCard), cancellationToken);
                }
                else if (turnContext.Activity.Text.ToLower().Trim() == "dependantdropdown")
                {
                    string[] path = { ".", "Cards", "DependentDropdown.json" };
                    var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
                    var initialAdaptiveCard = GetFirstOptionsAdaptiveCard(path, turnContext.Activity.From.Name, member.Id);

                    await turnContext.SendActivityAsync(MessageFactory.Attachment(initialAdaptiveCard), cancellationToken);
                }
            }
            else if (turnContext.Activity.Value != null)
            {
                var data = JsonConvert.DeserializeObject<StaticSearchCard>(turnContext.Activity.Value.ToString());
                await turnContext.SendActivityAsync(MessageFactory.Text("Selected option is: " + data.choiceSelect), cancellationToken);
            }
        }

        /// <summary>
        /// Invoked when bot (like a user) are added to the conversation.
        /// </summary>
        /// <param name="membersAdded">A list of all the members added to the conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
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
        ///  Invoked when an invoke activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            InvokeResponse adaptiveCardResponse;

            // Check if the activity is of the expected type
            if (turnContext.Activity.Name == "application/search")
            {
                // Deserialize the incoming activity value to get dropdown and search data
                var dropdownCard = JsonConvert.DeserializeObject<DependantDropdownCard>(turnContext.Activity.Value.ToString());
                var searchData = JsonConvert.DeserializeObject<DynamicSearchCard>(turnContext.Activity.Value.ToString());

                // Fetch package data from the external API
                var packageResult = JObject.Parse(await (new HttpClient()).GetStringAsync($"https://azuresearch-usnc.nuget.org/query?q=id:{searchData.queryText}&prerelease=true"));

                // Check if a country was specified in the dropdown data
                if (dropdownCard.Data.choiceSelect != "")
                {
                    Object searchResponseData;

                    // Define city options based on different countries
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

                    // Normalize the country name to lowercase for comparison
                    string country = dropdownCard.Data.choiceSelect.ToLower();

                    if (country == "usa")
                    {
                        searchResponseData = new
                        {
                            type = "application/vnd.microsoft.search.searchResponse",
                            value = new
                            {
                                results = usa
                            }
                        };
                    }
                    else if (country == "france")
                    {
                        searchResponseData = new
                        {
                            type = "application/vnd.microsoft.search.searchResponse",
                            value = new
                            {
                                results = france
                            }
                        };
                    }
                    else
                    {
                        searchResponseData = new
                        {
                            type = "application/vnd.microsoft.search.searchResponse",
                            value = new
                            {
                                results = india
                            }
                        };
                    }

                    // Serialize the response data to JSON
                    var jsonString = JsonConvert.SerializeObject(searchResponseData);
                    JObject jsonData = JObject.Parse(jsonString);

                    // Create the response with a 200 status code
                    adaptiveCardResponse = new InvokeResponse()
                    {
                        Status = 200,
                        Body = jsonData
                    };
                }
                else
                {
                    // If no country is specified, process the package results
                    var packages = packageResult["data"].Select(item => (item["id"].ToString(), item["description"].ToString()));
                    var packageList = packages.Select(item => { var obj = new { title = item.Item1, value = item.Item1 + " - " + item.Item2 }; return obj; }).ToList();

                    // Build the response data for the package list
                    var searchResponseData = new
                    {
                        type = "application/vnd.microsoft.search.searchResponse",
                        value = new
                        {
                            results = packageList
                        }
                    };

                    // Serialize the response data to JSON
                    var jsonString = JsonConvert.SerializeObject(searchResponseData);
                    JObject jsonData = JObject.Parse(jsonString);

                    // Create the response with a 200 status code
                    adaptiveCardResponse = new InvokeResponse()
                    {
                        Status = 200,
                        Body = jsonData
                    };
                }

                // Return the adaptive card response
                return adaptiveCardResponse;
            }

            // Return null if the activity is not recognized
            return null;
    }

        // Get intial card.
        private Attachment GetFirstOptionsAdaptiveCard(string[] filepath, string name = null, string userMRI = null)
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);
            var payloadData = new
            {
                createdById = userMRI,
                createdBy = name
            };

            //"Expand" the template -this generates the final Adaptive Card payload
            var cardJsonstring = template.Expand(payloadData);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJsonstring),
            };

            return adaptiveCardAttachment;
        }
    }
}