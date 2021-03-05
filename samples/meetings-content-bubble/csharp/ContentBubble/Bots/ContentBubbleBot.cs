// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using AdaptiveCards.Templating;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Content_Bubble_Bot
{
    public class ContentBubbleBot : TeamsActivityHandler
    {
        private readonly IConfiguration _configuration;

        public ContentBubbleBot(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            if (turnContext.Activity.Value == null)
            {
                var path = "./Cards/adaptive.json";
                var adaptiveCardJson = File.ReadAllText(path);
                var adaptiveCardAttachment = new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = JsonConvert.DeserializeObject(adaptiveCardJson),
                };

                await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment));
            }

            else
            {               
                string selectedoption = string.Empty;
                JToken commandToken = JToken.Parse(turnContext.Activity.Value.ToString());
                string command = commandToken["action"].Value<string>();

                selectedoption = commandToken["myReview"].Value<string>();               
                
                if (selectedoption == "yes")
                {
                    await turnContext.SendActivityAsync(turnContext.Activity.From.Name + " : " + "**Yes** for " + "'" + command + "'");
                }

                else if (selectedoption == "no")
                {
                    await turnContext.SendActivityAsync(turnContext.Activity.From.Name + " : " + "**No** for " + "'" + command + "'");
                }

                else
                {
                    Titles.Title = selectedoption;

                    await FetchTask(turnContext);

                    var templateJson = @"
                    {
                        ""type"": ""AdaptiveCard"",
                         ""version"": ""1.2"",
                         ""body"": [
                            {
                                
                               ""type"": ""TextBlock"",
                                ""text"": ""Provide your Feedback!""
                            },
                            {

                               ""type"": ""TextBlock"",
                                ""text"": ""${option}?""
                            },
                    
                            {
                                 ""type"": ""Input.ChoiceSet"",
                                  ""id"": ""myReview"",
                                  ""style"": ""expanded"",
                                  ""isMultiSelect"": false,
                                ""wrap"": true,
                                ""value"": ""1"",
                                ""choices"": [
                                       {
                                          ""title"": ""Yes"",
                                         ""value"": ""yes""
                                         },
                                          {
                                          ""title"": ""No"",
                                         ""value"": ""no""
                                          }

                                       ]
                                  }
                             ],

                                ""actions"": [
                                         {
                                             ""type"": ""Action.Submit"",
                                             ""id"": ""submit"",
                                              ""title"": ""Submit"",
                                               ""data"": {
                                                             ""action"": ""${option}""
                                                       }
                                               } ]                                 
                                        }
                                  ";

                    AdaptiveCardTemplate template = new AdaptiveCardTemplate(templateJson);
                   
                    var myData = new
                    {
                        Option = selectedoption
                    };
                                       
                    string cardJson = template.Expand(myData);
                   
                    var adaptiveCardAttachment = new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = JsonConvert.DeserializeObject(cardJson),
                    };

                    await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment));
                }
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome to Content Bubble Bot !";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {

            var replyJson = JObject.Parse(taskModuleRequest.Data.ToString());
            var reply = Convert.ToString(replyJson["userValue"]);
            var title = Convert.ToString(replyJson["title"]);
            var selectedQuestion = title.Split('?');
            if (reply == "yes")
            {
                await turnContext.SendActivityAsync(turnContext.Activity.From.Name + " : " + "**Yes** for " + "'" + selectedQuestion[0] + "'");
            }
            else
            {
                await turnContext.SendActivityAsync(turnContext.Activity.From.Name + " : " + "**No** for " + "'" + selectedQuestion[0] + "'");
            }
            return null;
        }

        public async static Task FetchTask(ITurnContext turnContext)
        {
            Activity activity = MessageFactory.Text("**Please provide your valuable feedback**");

            activity.ChannelData = new TeamsChannelData
            {
                Notification = new NotificationInfo()
                {
                    AlertInMeeting = true,
                    ExternalResourceUrl = "https://teams.microsoft.com/l/bubble/"+_configuration["MicrosoftAppId"]+"?url="+_configuration["BaseUrl"]+"/ContentBubble&height=270&width=250&title=ContentBubble&completionBotId="++configuration["MicrosoftAppId"]+""
                }
            };
            await turnContext.SendActivityAsync(activity).ConfigureAwait(false);
        }
    }
}
