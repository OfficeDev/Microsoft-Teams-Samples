using AdaptiveCards;
using Bogus.DataSets;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Botconfiguration.Bots
{
    /// <summary>
    /// TeamsBot handles Teams-specific activities.
    /// </summary>
    public class TeamsBot : TeamsActivityHandler
    {
        private string chosenFlow = "";

        /// <summary>
        /// Handles members added to the conversation.
        /// </summary>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var imagePath = "Images/configbutton.png";
            var imageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));

            var adaptiveCardJson = $@"
                {{
                    ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
                    ""type"": ""AdaptiveCard"",
                    ""version"": ""1.0"",
                    ""body"": [
                        {{
                            ""type"": ""TextBlock"",
                            ""text"": ""Hello and welcome! With this sample, you can experience the functionality of bot configuration. To access Bot configuration, click on the settings button in the bot description card."",
                            ""wrap"": true,
                            ""size"": ""large"",
                            ""weight"": ""bolder""
                        }},
                        {{
                            ""type"": ""Image"",
                            ""url"": ""data:image/png;base64,{imageData}"",
                            ""size"": ""auto""
                        }}
                    ],
                    ""fallbackText"": ""This card requires Adaptive Card support.""
                }}";

            var attachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson)
            };

            var reply = MessageFactory.Attachment(attachment);
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        /// <summary>
        /// Handles message activities.
        /// </summary>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text != null)
            {
                var text = turnContext.Activity.Text.ToLower().Trim();
                if (text == "chosen flow" || text == "<at>typeahead search adaptive card</at> chosen flow")
                {
                    await turnContext.SendActivityAsync($"Bot configured for {chosenFlow} flow", cancellationToken: cancellationToken);
                }
            }
            else if (turnContext.Activity.Value != null)
            {
                var choiceSelect = turnContext.Activity.Value;
                await turnContext.SendActivityAsync($"Selected option is: {choiceSelect}", cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// Handles configuration fetch requests.
        /// </summary>
        protected override Task<ConfigResponseBase> OnTeamsConfigFetchAsync(ITurnContext<IInvokeActivity> turnContext, JObject configData, CancellationToken cancellationToken)
        {
            var response = CreateAdaptiveCardForContinue();
            return Task.FromResult(response);
        }

        /// <summary>
        /// Handles configuration submit requests.
        /// </summary>
        protected override async Task<ConfigResponseBase> OnTeamsConfigSubmitAsync(ITurnContext<IInvokeActivity> turnContext, JObject configData, CancellationToken cancellationToken)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<JObject>(turnContext.Activity.Value.ToString());
                var card = CreateAdaptiveCardFromData(data);

                var attachment = new Attachment
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = card
                };

                var reply = MessageFactory.Attachment(attachment);
                await turnContext.SendActivityAsync(reply, cancellationToken);

                var response = new ConfigResponse<TaskModuleResponseBase>
                {
                    Config = new TaskModuleMessageResponse
                    {
                        Type = "message",
                        Value = "Your request has been submitted successfully!"
                    }
                };

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Creates an adaptive card for the continue response.
        /// </summary>
        private ConfigResponseBase CreateAdaptiveCardForContinue()
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 4))
            {
                Body = new List<AdaptiveElement>
                    {
                        new AdaptiveColumnSet
                        {
                            Columns = new List<AdaptiveColumn>
                            {
                                new AdaptiveColumn
                                {
                                    Width = "stretch",
                                    Items = new List<AdaptiveElement>
                                    {
                                        new AdaptiveTextBlock
                                        {
                                            Text = "For issues that match these criteria:",
                                            Weight = AdaptiveTextWeight.Bolder,
                                            Type = "TextBlock"
                                        }
                                    }
                                }
                            }
                        },
                        CreateDropdownColumnSet("Type", "dropdown01", new List<AdaptiveChoice>
                        {
                            new AdaptiveChoice { Title = "Bug", Value = "Bug" },
                            new AdaptiveChoice { Title = "Feature Request", Value = "Feature Request" },
                            new AdaptiveChoice { Title = "Task", Value = "Task" }
                        }),
                        CreateDropdownColumnSet("Priority", "dropdown02", new List<AdaptiveChoice>
                        {
                            new AdaptiveChoice { Title = "Low", Value = "Low" },
                            new AdaptiveChoice { Title = "Medium", Value = "Medium" },
                            new AdaptiveChoice { Title = "High", Value = "High" }
                        }),
                        new AdaptiveColumnSet
                        {
                            Columns = new List<AdaptiveColumn>
                            {
                                new AdaptiveColumn
                                {
                                    Width = "stretch",
                                    Items = new List<AdaptiveElement>
                                    {
                                        new AdaptiveTextBlock
                                        {
                                            Text = "Post to channel when :",
                                            Weight = AdaptiveTextWeight.Bolder,
                                            Type = "TextBlock"
                                        }
                                    }
                                }
                            }
                        },
                        CreateDropdownColumnSet("Issue", "dropdown1", new List<AdaptiveChoice>
                        {
                            new AdaptiveChoice { Title = "Software Issue", Value = "Software Issue" },
                            new AdaptiveChoice { Title = "Server Issue", Value = "Server Issue" },
                            new AdaptiveChoice { Title = "Network Issue", Value = "Network Issue" }
                        }, true),
                        CreateDropdownColumnSet("Comment", "dropdown2", new List<AdaptiveChoice>
                        {
                            new AdaptiveChoice { Title = "Network problem in server", Value = "Network problem in server" },
                            new AdaptiveChoice { Title = "Loadbalancer issue", Value = "Loadbalancer issue" },
                            new AdaptiveChoice { Title = "Software needs to be updated", Value = "Software needs to be updated" }
                        }),
                        CreateDropdownColumnSet("Assignee", "dropdown3", new List<AdaptiveChoice>
                        {
                            new AdaptiveChoice { Title = "Jasmine Smith", Value = "Jasmine Smith" },
                            new AdaptiveChoice { Title = "Ethan Johnson", Value = "Ethan Johnson" },
                            new AdaptiveChoice { Title = "Maya Rodriguez", Value = "Maya Rodriguez" }
                        }),
                        CreateDropdownColumnSet("Status changed", "dropdown4", new List<AdaptiveChoice>
                        {
                            new AdaptiveChoice { Title = "Open", Value = "Open" },
                            new AdaptiveChoice { Title = "Inprogress", Value = "Inprogress" },
                            new AdaptiveChoice { Title = "Completed", Value = "Completed" }
                        }),
                        new AdaptiveColumnSet
                        {
                            Columns = new List<AdaptiveColumn>
                            {
                                new AdaptiveColumn
                                {
                                    Width = "stretch",
                                    Items = new List<AdaptiveElement>
                                    {
                                        new AdaptiveTextBlock
                                        {
                                            Text = "Actions to display",
                                            Weight = AdaptiveTextWeight.Bolder,
                                            Type = "TextBlock"
                                        }
                                    }
                                }
                            }
                        },
                        CreateToggleColumnSet("Assign", "toggleAssign"),
                        CreateToggleColumnSet("Comment", "toggleComment"),
                        CreateToggleColumnSet("Transition", "toggleTransition"),
                        CreateToggleColumnSet("Update status", "toggleStatus")
                    },
                Actions = new List<AdaptiveAction>
                    {
                        new AdaptiveSubmitAction
                        {
                            Type = AdaptiveSubmitAction.TypeName,
                            Id = "submit",
                            Title = "Submit"
                        }
                    }
            };

            var response = new ConfigResponse<TaskModuleResponseBase>
            {
                Config = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Height = 500,
                        Width = 600,
                        Title = "Task module fetch response",
                        Card = new Attachment
                        {
                            ContentType = AdaptiveCard.ContentType,
                            Content = card
                        }
                    },
                    Type = "continue"
                }
            };

            return response;
        }

        /// <summary>
        /// Creates an adaptive card from the provided data.
        /// </summary>
        private AdaptiveCard CreateAdaptiveCardFromData(JObject data)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock
                        {
                            Text = "The selection you requested is as follows:",
                            Weight = AdaptiveTextWeight.Bolder,
                            Type = "TextBlock",
                            Wrap = true
                        }
                    }
            };

            AddTextBlockIfNotEmpty(card, "Type", data["data"]?["dropdown01"]?.ToString());
            AddTextBlockIfNotEmpty(card, "Priority", data["data"]?["dropdown02"]?.ToString());
            AddTextBlockIfNotEmpty(card, "Issue", data["data"]?["dropdown1"]?.ToString());
            AddTextBlockIfNotEmpty(card, "Comment", data["data"]?["dropdown2"]?.ToString());
            AddTextBlockIfNotEmpty(card, "Assignee", data["data"]?["dropdown3"]?.ToString());
            AddTextBlockIfNotEmpty(card, "Status", data["data"]?["dropdown4"]?.ToString());

            card.Body.Add(new AdaptiveTextBlock
            {
                Text = "Actions to be displayed:",
                Weight = AdaptiveTextWeight.Bolder,
                Type = "TextBlock"
            });

            AddTextBlockIfTrue(card, "Status", data["data"]?["toggleStatus"]?.ToString());
            AddTextBlockIfTrue(card, "Assign", data["data"]?["toggleAssign"]?.ToString());
            AddTextBlockIfTrue(card, "Comment", data["data"]?["toggleComment"]?.ToString());
            AddTextBlockIfTrue(card, "Transition", data["data"]?["toggleTransition"]?.ToString());

            return card;
        }

        /// <summary>
        /// Adds a text block to the card if the value is not empty.
        /// </summary>
        private void AddTextBlockIfNotEmpty(AdaptiveCard card, string label, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                card.Body.Add(new AdaptiveTextBlock
                {
                    Text = $"{label} : {value}",
                    Wrap = true
                });
            }
        }

        /// <summary>
        /// Adds a text block to the card if the value is "True".
        /// </summary>
        private void AddTextBlockIfTrue(AdaptiveCard card, string label, string value)
        {
            if (value == "True")
            {
                card.Body.Add(new AdaptiveTextBlock
                {
                    Text = $"{label} : {value}",
                    Wrap = true
                });
            }
        }

        /// <summary>
        /// Creates a dropdown column set.
        /// </summary>
        private AdaptiveColumnSet CreateDropdownColumnSet(string label, string id, List<AdaptiveChoice> choices, bool isMultiSelect = false)
        {
            return new AdaptiveColumnSet
            {
                Columns = new List<AdaptiveColumn>
                    {
                        new AdaptiveColumn
                        {
                            Width = "stretch",
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = label,
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Type = "TextBlock"
                                },
                                new AdaptiveChoiceSetInput
                                {
                                    Id = id,
                                    Choices = choices,
                                    IsMultiSelect = isMultiSelect
                                }
                            }
                        }
                    }
            };
        }

        /// <summary>
        /// Creates a toggle column set.
        /// </summary>
        private AdaptiveColumnSet CreateToggleColumnSet(string title, string id)
        {
            return new AdaptiveColumnSet
            {
                Columns = new List<AdaptiveColumn>
                    {
                        new AdaptiveColumn
                        {
                            Width = "stretch",
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveToggleInput
                                {
                                    Title = title,
                                    Id = id,
                                    Value = "false"
                                }
                            }
                        }
                    }
            };
        }
    }
}