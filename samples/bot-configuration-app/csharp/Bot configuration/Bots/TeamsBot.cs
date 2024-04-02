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
    public class TeamsBot : TeamsActivityHandler
    {
        private string _chosenFlow = "";

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var imagePath = "Images/configbutton.png";
            var imageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));

            var adaptiveCardJson = @"
            {
                ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
                ""type"": ""AdaptiveCard"",
                ""version"": ""1.0"",
                ""body"": [
                            {
                                ""type"": ""TextBlock"",
                                ""text"": ""Hello and welcome! With this sample, you can experience the functionality of bot configuration. To access Bot configuration, click on the settings button in the bot description card."",
                                ""wrap"": true,
                                ""size"": ""large"",
                                ""weight"": ""bolder""
                            },
                            {
                                ""type"": ""Image"",
                                ""url"": ""data:image/png;base64," + imageData + @""",
                                ""size"": ""auto""
                            }
                          ],
                ""fallbackText"": ""This card requires Adaptive Card support.""
            }";

            var attachment = new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson)
            };

            var reply = MessageFactory.Attachment(attachment);
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text != null)
            {
                string text = turnContext.Activity.Text.ToLower().Trim();
                if (text == "chosen flow" || text == "<at>typeahead search adaptive card</at> chosen flow")
                {
                    await turnContext.SendActivityAsync($"Bot configured for {_chosenFlow} flow", cancellationToken: cancellationToken);
                }
            }
            else if (turnContext.Activity.Value != null)
            {
                var choiceselect = turnContext.Activity.Value;
                await turnContext.SendActivityAsync($"Selected option is: {choiceselect}", cancellationToken: cancellationToken);
            }
        }

        protected override Task<ConfigResponseBase> OnTeamsConfigFetchAsync(ITurnContext<IInvokeActivity> turnContext, JObject configData, CancellationToken cancellationToken)
        {
            ConfigResponseBase response = adaptiveCardForContinue();
            return Task.FromResult(response);
        }

        protected override Task<ConfigResponseBase> OnTeamsConfigSubmitAsync(ITurnContext<IInvokeActivity> turnContext, JObject configData, CancellationToken cancellationToken)
        {
            try
            {
                JObject data = JsonConvert.DeserializeObject<JObject>(turnContext.Activity.Value.ToString());

                string dropdown01Value = data["data"]?["dropdown01"].ToString();
                string dropdown02Value = data["data"]?["dropdown02"]?.ToString();
                string dropdown1Value = data["data"]?["dropdown1"]?.ToString();
                string dropdown2Value = data["data"]?["dropdown2"]?.ToString();
                string dropdown3Value = data["data"]?["dropdown3"]?.ToString();
                string dropdown4Value = data["data"]?["dropdown4"]?.ToString();
                string togglestatus = data["data"]?["togglestatus"]?.ToString();
                string toggleAssign = data["data"]?["toggleAssign"]?.ToString();
                string toggleComment = data["data"]?["toggleComment"]?.ToString();
                string toggleTransition = data["data"]?["toggleTransition"]?.ToString();


                AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                {
                    Body = new List<AdaptiveElement>()
                    {
                        new AdaptiveTextBlock()
                        {
                            Text = "The selection you requested is as follows:",
                            Weight = AdaptiveTextWeight.Bolder,
                            Type = "TextBlock",
                            Wrap = true
                        }
                    }
                };

                if (!string.IsNullOrEmpty(dropdown01Value))
                {
                    card.Body.Add(new AdaptiveTextBlock()
                    {
                        Text = "Type : " + dropdown01Value,
                        Wrap = true
                    });
                }

                if (!string.IsNullOrEmpty(dropdown02Value))
                {
                    card.Body.Add(new AdaptiveTextBlock()
                    {
                        Text = "Priority : " + dropdown02Value,
                        Wrap = true
                    });
                }

                if (!string.IsNullOrEmpty(dropdown1Value))
                {
                    card.Body.Add(new AdaptiveTextBlock()
                    {
                        Text = "Issue : " + dropdown1Value,
                        Wrap = true
                    });
                }

                if (!string.IsNullOrEmpty(dropdown2Value))
                {
                    card.Body.Add(new AdaptiveTextBlock()
                    {
                        Text = "Comment : " + dropdown2Value,
                        Wrap = true
                    });
                }

                if (!string.IsNullOrEmpty(dropdown3Value))
                {
                    card.Body.Add(new AdaptiveTextBlock()
                    {
                        Text = "Assignee : " + dropdown3Value,
                        Wrap = true
                    });
                }

                if (!string.IsNullOrEmpty(dropdown4Value))
                {
                    card.Body.Add(new AdaptiveTextBlock()
                    {
                        Text = "Status : " + dropdown4Value,
                        Wrap = true
                    });
                }

                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = "Actions to be displayed:",
                    Weight = AdaptiveTextWeight.Bolder,
                    Type = "TextBlock"
                });

                if (togglestatus == "True")
                {
                    card.Body.Add(new AdaptiveTextBlock()
                    {
                        Text = "Status : " + togglestatus,
                        Wrap = true
                    });
                }

                if (toggleAssign == "True")
                {
                    card.Body.Add(new AdaptiveTextBlock()
                    {
                        Text = "Assign : " + toggleAssign,
                        Wrap = true
                    });
                }

                if (toggleComment == "True")
                {
                    card.Body.Add(new AdaptiveTextBlock()
                    {
                        Text = "Comment : " + toggleComment,
                        Wrap = true
                    });
                }

                if (toggleTransition=="True")
                {
                    card.Body.Add(new AdaptiveTextBlock()
                    {
                        Text = "Transition : " + toggleTransition,
                        Wrap = true
                    });
                }

                string json = JsonConvert.SerializeObject(card);

                var attachment = new Microsoft.Bot.Schema.Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(json)
                };

                var reply = MessageFactory.Attachment(attachment);
                turnContext.SendActivityAsync(reply, cancellationToken);

                ConfigResponseBase response = new ConfigResponse<TaskModuleResponseBase>
                {
                    Config = new TaskModuleMessageResponse
                    {
                        Type = "message",
                        Value = "Your request has been submitted successfully!"
                    }
                };

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private ConfigResponseBase adaptiveCardForContinue()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 4))
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text = "For issues that match these criteria:",
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Type = "TextBlock"
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text = "Type",
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Type = "TextBlock"
                                    },
                                    new AdaptiveChoiceSetInput()
                                    {
                                        Id = "dropdown01",
                                        Choices = new List<AdaptiveChoice>()
                                        {
                                            new AdaptiveChoice() { Title = "Bug", Value = "Bug" },
                                            new AdaptiveChoice() { Title = "Feature Request", Value = "Feature Request" },
                                            new AdaptiveChoice() { Title = "Task", Value = "Task" }
                                        }
                                    }
                                }
                            },
                            new AdaptiveColumn()
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text = "Priority",
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Type = "TextBlock"
                                    },
                                    new AdaptiveChoiceSetInput()
                                    {
                                        Id = "dropdown02",
                                        Choices = new List<AdaptiveChoice>()
                                        {
                                            new AdaptiveChoice() { Title = "Low", Value = "Low" },
                                            new AdaptiveChoice() { Title = "Medium", Value = "Medium" },
                                            new AdaptiveChoice() { Title = "High", Value = "High" }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text = "Post to channel when :",
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Type = "TextBlock"
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text = "Issue",
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Type = "TextBlock"
                                    },
                                    new AdaptiveChoiceSetInput()
                                    {
                                        Id = "dropdown1",
                                        IsMultiSelect = true,
                                        Choices = new List<AdaptiveChoice>()
                                        {
                                            new AdaptiveChoice() { Title = "Software Issue", Value = "Software Issue" },
                                            new AdaptiveChoice() { Title = "Server Issue", Value = "Server Issue" },
                                            new AdaptiveChoice() { Title = "Network Issue", Value = "Network Issue" }
                                        }
                                    }
                                }
                            },
                            new AdaptiveColumn()
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text = "Comment",
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Type = "TextBlock"
                                    },
                                    new AdaptiveChoiceSetInput()
                                    {
                                        Id = "dropdown2",
                                        Choices = new List<AdaptiveChoice>()
                                        {
                                            new AdaptiveChoice() { Title = "Network problem in server", Value = "Network problem in server" },
                                            new AdaptiveChoice() { Title = "Loadbalancer issue", Value = "Loadbalancer issue" },
                                            new AdaptiveChoice() { Title = "Software needs to be updated", Value = "Software needs to be updated" }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text = "Assignee",
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Type = "TextBlock"
                                    },
                                    new AdaptiveChoiceSetInput()
                                    {
                                        Id = "dropdown3",
                                        Choices = new List<AdaptiveChoice>()
                                        {
                                            new AdaptiveChoice() { Title = "Jasmine Smith", Value = "Jasmine Smith" },
                                            new AdaptiveChoice() { Title = "Ethan Johnson", Value = "Ethan Johnson" },
                                            new AdaptiveChoice() { Title = "Maya Rodriguez", Value = "Maya Rodriguez" }
                                        }
                                    }
                                }
                            },
                            new AdaptiveColumn()
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text = "Status changed",
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Type = "TextBlock"
                                    },
                                    new AdaptiveChoiceSetInput()
                                    {
                                        Id = "dropdown4",
                                        Choices = new List<AdaptiveChoice>()
                                        {
                                            new AdaptiveChoice() { Title = "Open", Value = "Open" },
                                            new AdaptiveChoice() { Title = "Inprogress", Value = "Inprogress" },
                                            new AdaptiveChoice() { Title = "Completed", Value = "Completed" }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text = "Actions to display",
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Type = "TextBlock"
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveToggleInput()
                                    {
                                        Title = "Assign",
                                        Id = "toggleAssign",
                                        Value = "false"
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveToggleInput()
                                    {
                                        Title = "Comment",
                                        Id = "toggleComment",
                                        Value = "false"
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveToggleInput()
                                    {
                                        Title = "Transition",
                                        Id = "toggleTransition",
                                        Value = "false"
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveToggleInput()
                                    {
                                        Title = "Update status",
                                        Id = "togglestatus",
                                        Value = "false"
                                    }
                                }
                            }
                        }
                    }
                },
                Actions = new List<AdaptiveAction>()
                {
                    new AdaptiveSubmitAction()
                    {
                        Type = AdaptiveSubmitAction.TypeName,
                        Id = "submit",
                        Title = "Submit"
                    }
                }
            };

            ConfigResponseBase response = new ConfigResponse<TaskModuleResponseBase>
            {
                Config = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Height = 500,
                        Width = 600,
                        Title = "Task module fetch response",
                        Card = new Microsoft.Bot.Schema.Attachment
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
    }
}