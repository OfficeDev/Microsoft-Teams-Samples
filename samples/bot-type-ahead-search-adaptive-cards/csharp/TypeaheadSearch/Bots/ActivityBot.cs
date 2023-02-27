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
                    await turnContext.SendActivityAsync(MessageFactory.Text("Hello and welcome! With this sample you can see the functionality of static and dynamic search in adaptive card."), cancellationToken);
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
            if (turnContext.Activity.Name == "application/search")
            {
                var searchData = JsonConvert.DeserializeObject<DynamicSearchCard>(turnContext.Activity.Value.ToString());
                var packageResult = JObject.Parse(await (new HttpClient()).GetStringAsync($"https://azuresearch-usnc.nuget.org/query?q=id:{searchData.queryText}&prerelease=true"));
                if (packageResult == null)
                {
                    var searchResponseData = new
                    {
                        type = "application/vnd.microsoft.search.searchResponse"
                    };

                    var jsonString = JsonConvert.SerializeObject(searchResponseData);
                    JObject jsonData = JObject.Parse(jsonString);

                    adaptiveCardResponse = new InvokeResponse()
                    {
                        Status = 204,
                        Body = jsonData
                    };
                }
                else
                {
                    var packages = packageResult["data"].Select(item => (item["id"].ToString(), item["description"].ToString()));
                    var packageList = packages.Select(item => { var obj = new { title = item.Item1, value = item.Item1 + " - " + item.Item2 }; return obj; }).ToList();
                    var searchResponseData = new
                    {
                        type = "application/vnd.microsoft.search.searchResponse",
                        value = new
                        {
                            results = packageList
                        }
                    };

                    var jsonString = JsonConvert.SerializeObject(searchResponseData);
                    JObject jsonData = JObject.Parse(jsonString);

                    adaptiveCardResponse = new InvokeResponse()
                    {
                        Status = 200,
                        Body = jsonData
                    };
                }

                return adaptiveCardResponse;
            }

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