using AdaptiveCards;
using Bogus.DataSets;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.Xml;

namespace Localization.Bots
{
    public class TeamsBot : TeamsActivityHandler
    {
        private readonly IStringLocalizer<TeamsBot> _localizer;
        private string _chosenFlow = "";

        public TeamsBot(IStringLocalizer<TeamsBot> localizer)
        {
            _localizer = localizer;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Hello and welcome! With this sample you can see the functionality of static and dynamic search in adaptive card."), cancellationToken);
                }
            }
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
            ConfigResponseBase response = adaptiveCardForStaticSearch();
            return Task.FromResult(response);
        }

        protected override Task<ConfigResponseBase> OnTeamsConfigSubmitAsync(ITurnContext<IInvokeActivity> turnContext, JObject configData, CancellationToken cancellationToken)
        {
            JObject choice = JsonConvert.DeserializeObject<JObject>(turnContext.Activity.Value.ToString());
            _chosenFlow = choice["data"]["choiceselect"].ToString();

            if (_chosenFlow == "static_option_2")
            {
                ConfigResponseBase response = adaptiveCardForDynamicSearch();
                return Task.FromResult(response);
            }
            else
            {
                ConfigResponseBase response = new ConfigResponse<TaskModuleResponseBase>
                {
                    Config = new TaskModuleMessageResponse
                    {
                        Type = "message",
                        Value = "Submitted successfully!"
                    }
                };

                return Task.FromResult(response);
            }
        }

        private ConfigResponseBase adaptiveCardForDynamicSearch()
        {
            AdaptiveCard card = new AdaptiveCard("1.6")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Please search for the IDE from static list.",
                        Wrap = true,
                        Type = "TextBlock"
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = "auto",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = "IDE: ",
                                        Wrap = true,
                                        Height = AdaptiveHeight.Stretch,
                                        Type = "TextBlock"
                                    }
                                }
                            }
                        },
                        Type = "ColumnSet"
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveChoiceSetInput
                                    {
                                        Choices = new List<AdaptiveChoice>
                                        {
                                            new AdaptiveChoice
                                            {
                                                Title = "Visual studio",
                                                Value = "visual_studio"
                                            },
                                            new AdaptiveChoice
                                            {
                                                Title = "IntelliJ IDEA ",
                                                Value = "intelliJ_IDEA "
                                            },
                                            new AdaptiveChoice
                                            {
                                                Title = "Aptana Studio 3",
                                                Value = "aptana_studio_3"
                                            },
                                            new AdaptiveChoice
                                            {
                                                Title = "PyCharm",
                                                Value = "pycharm"
                                            },
                                            new AdaptiveChoice
                                            {
                                                Title = "PhpStorm",
                                                Value = "phpstorm"
                                            },
                                            new AdaptiveChoice
                                            {
                                                Title = "WebStorm",
                                                Value = "webstorm"
                                            },
                                            new AdaptiveChoice
                                            {
                                                Title = "NetBeans",
                                                Value = "netbeans"
                                            },
                                            new AdaptiveChoice
                                            {
                                                Title = "Eclipse",
                                                Value = "eclipse"
                                            },
                                            new AdaptiveChoice
                                            {
                                                Title = "RubyMine ",
                                                Value = "rubymine "
                                            },
                                            new AdaptiveChoice
                                            {
                                                Title = "Visual studio code",
                                                Value = "visual_studio_code"
                                            }
                                        },
                                        Id = "choiceselect",
                                        Type = "Input.ChoiceSet"
                                    }
                                }
                            }
                        },
                        Type = "ColumnSet"
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
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
                        Height = 400,
                        Width = 400,
                        Title = "Task module submit response",
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

        private ConfigResponseBase adaptiveCardForStaticSearch()
        {
            AdaptiveCard card = new AdaptiveCard("1.6")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Please search for npm packages using dynamic search control.",
                        Wrap = true,
                        Type = "TextBlock"
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = "auto",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = "NPM packages search: ",
                                        Wrap = true,
                                        Height = AdaptiveHeight.Stretch,
                                        Type = "TextBlock"
                                    }
                                }
                            }
                        },
                        Type = "ColumnSet"
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveChoiceSetInput
                                    {
                                        Choices = new List<AdaptiveChoice>
                                        {
                                            new AdaptiveChoice
                                            {
                                                Title = "Static Option 1",
                                                Value = "static_option_1"
                                            },
                                            new AdaptiveChoice
                                            {
                                                Title = "Static Option 2",
                                                Value = "static_option_2"
                                            },
                                            new AdaptiveChoice
                                            {
                                                Title = "Static Option 3",
                                                Value = "static_option_3"
                                            }
                                        },
                                        Value = "static_option_2",
                                        IsMultiSelect = false,
                                        Style = AdaptiveChoiceInputStyle.Compact,
                                        Id = "choiceselect",
                                        Type = "Input.ChoiceSet"
                                    }
                                }
                            }
                        },
                        Type = "ColumnSet"
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Id = "submitdynamic",
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
                        Height = 400,
                        Width = 400,
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
    }
}