using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Api.TaskModules;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Activities.Invokes;
using Microsoft.Teams.Apps.Annotations;
using Microsoft.Teams.Cards;
using System.Text.Json;
using AdaptiveCard = Microsoft.Teams.Cards.AdaptiveCard;

namespace Bot_Configuration.Controllers
{
    [TeamsController]
    public class Controller()
    {
        private string chosenFlow = "";

        [Conversation.MembersAdded]
        public async System.Threading.Tasks.Task OnMembersAdded([Context] ConversationUpdateActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("[MEMBERS_ADDED] Members added to conversation");

            var imagePath = "Images/configbutton.png";
            string imageData = "";

            if (File.Exists(imagePath))
            {
                imageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));
            }

            var card = new AdaptiveCard
            {
                Body = new List<CardElement>
                {
                    new TextBlock("Hello and welcome! With this sample, you can experience the functionality of bot configuration.")
                    {
                        Wrap = true,
                        Size = TextSize.Large,
                        Weight = TextWeight.Bolder
                    },
                    new TextBlock("Click the button below to open the bot configuration dialog.")
                    {
                        Wrap = true,
                        Size = TextSize.Medium
                    }
                }
            };

            
            if (!string.IsNullOrEmpty(imageData))
            {
                card.Body.Add(new Image($"data:image/png;base64,{imageData}"));
            }

            
            card.Actions = new List<Microsoft.Teams.Cards.Action>
            {
                new TaskFetchAction(new Dictionary<string, object?> { { "opendialogtype", "bot_configuration" } })
                {
                    Title = "Open Bot Configuration"
                }
            };

            await client.Send(card);
        }

        [Message]
        public async System.Threading.Tasks.Task OnMessage([Context] Microsoft.Teams.Api.Activities.MessageActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("[MESSAGE] Message received");

            if (activity.Text != null)
            {
                var text = activity.Text.ToLower().Trim();
                if (text == "chosen flow" || text == "<at>bot configuration sample app</at> chosen flow")
                {
                    await client.Send($"Bot configured for {chosenFlow} flow");
                }
            }
            else if (activity.Value != null)
            {
                var choiceSelect = activity.Value;
                await client.Send($"Selected option is: {choiceSelect}");
            }
        }

        /// <summary>
        /// Handles the config/fetch invoke when bot is configured in a team or group chat
        /// </summary>
        [Invoke("config/fetch")]
        public object OnConfigFetch([Context] InvokeActivity activity, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("[CONFIG_FETCH] Bot configuration fetch received");

            
            var configCard = CreateConfigurationCard();

            return new
            {
                config = new
                {
                    type = "continue",
                    value = new
                    {
                        title = "Configure Bot",
                        width = 600,
                        height = 600,
                        card = new
                        {
                            contentType = "application/vnd.microsoft.card.adaptive",
                            content = configCard
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Handles the config/submit invoke when bot configuration is submitted
        /// </summary>
        [Invoke("config/submit")]
        public async System.Threading.Tasks.Task<object> OnConfigSubmit([Context] InvokeActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("[CONFIG_SUBMIT] Bot configuration submit received");

            try
            {
                if (activity.Value == null)
                {
                    log.Info("[CONFIG_SUBMIT] activity.Value is null");
                    return new
                    {
                        config = new
                        {
                            type = "message",
                            value = "No configuration data was provided."
                        }
                    };
                }

                var valueJson = JsonSerializer.Serialize(activity.Value);
                log.Info($"[CONFIG_SUBMIT] Full activity.Value: {valueJson}");

                var valueElement = JsonSerializer.Deserialize<JsonElement>(valueJson);

                
                string? GetFormValue(string key)
                {
                    
                    if (valueElement.TryGetProperty(key, out var val))
                    {
                        if (val.ValueKind == JsonValueKind.String)
                        {
                            var strValue = val.GetString();
                            log.Info($"[CONFIG_SUBMIT] {key} (root) = {strValue}");
                            return strValue;
                        }
                        if (val.ValueKind == JsonValueKind.True || val.ValueKind == JsonValueKind.False)
                        {
                            var boolValue = val.GetBoolean().ToString();
                            log.Info($"[CONFIG_SUBMIT] {key} (root) = {boolValue}");
                            return boolValue;
                        }
                    }

                    
                    if (valueElement.TryGetProperty("data", out var dataElement))
                    {
                        if (dataElement.TryGetProperty(key, out var dataVal))
                        {
                            if (dataVal.ValueKind == JsonValueKind.String)
                            {
                                var strValue = dataVal.GetString();
                                log.Info($"[CONFIG_SUBMIT] {key} (data) = {strValue}");
                                return strValue;
                            }
                            if (dataVal.ValueKind == JsonValueKind.True || dataVal.ValueKind == JsonValueKind.False)
                            {
                                var boolValue = dataVal.GetBoolean().ToString();
                                log.Info($"[CONFIG_SUBMIT] {key} (data) = {boolValue}");
                                return boolValue;
                            }
                        }
                    }

                    log.Info($"[CONFIG_SUBMIT] {key} not found in root or data");
                    return null;
                }

                
                var type = GetFormValue("dropdown01");
                var priority = GetFormValue("dropdown02");
                var issue = GetFormValue("dropdown1");
                var comment = GetFormValue("dropdown2");
                var assignee = GetFormValue("dropdown3");
                var status = GetFormValue("dropdown4");
                var toggleComment = GetFormValue("toggleComment");
                var toggleAssign = GetFormValue("toggleAssign");
                var toggleTransition = GetFormValue("toggleTransition");
                var toggleStatus = GetFormValue("toggleStatus");

                
                chosenFlow = type ?? "Bug";

                log.Info($"[CONFIG_SUBMIT] Bot configured with flow: {chosenFlow}");

                
                var summaryCard = new AdaptiveCard
                {
                    Body = new List<CardElement>
                    {
                        new TextBlock("The selection you requested is as follows:")
                        {
                            Wrap = true,
                            Size = TextSize.Medium
                        }
                    }
                };

                
                AddTextBlockIfNotEmpty(summaryCard, "Type", type);
                AddTextBlockIfNotEmpty(summaryCard, "Priority", priority);
                AddTextBlockIfNotEmpty(summaryCard, "Issue", issue);
                AddTextBlockIfNotEmpty(summaryCard, "Comment", comment);
                AddTextBlockIfNotEmpty(summaryCard, "Assignee", assignee);
                AddTextBlockIfNotEmpty(summaryCard, "Status", status);

                summaryCard.Body.Add(new TextBlock("Actions to be displayed:")
                {
                    Wrap = true
                });

                AddTextBlockIfTrue(summaryCard, "Comment", toggleComment);
                AddTextBlockIfTrue(summaryCard, "Assign", toggleAssign);
                AddTextBlockIfTrue(summaryCard, "Transition", toggleTransition);
                AddTextBlockIfTrue(summaryCard, "Update status", toggleStatus);

                
                await client.Send(summaryCard);

                
                return new
                {
                    config = new
                    {
                        type = "message",
                        value = "Your request has been submitted successfully!"
                    }
                };
            }
            catch (Exception ex)
            {
                log.Error($"[CONFIG_SUBMIT] Error processing configuration: {ex.Message}");
                log.Error($"[CONFIG_SUBMIT] Stack trace: {ex.StackTrace}");
                return new
                {
                    config = new
                    {
                        type = "message",
                        value = "An error occurred while saving the configuration."
                    }
                };
            }
        }

        private AdaptiveCard CreateConfigurationCard()
        {
            var card = new AdaptiveCard
            {
                Body = new List<CardElement>
                {
                    new TextBlock("Bot Configuration")
                    {
                        Size = TextSize.ExtraLarge,
                        Weight = TextWeight.Bolder
                    },
                    new TextBlock("For issues that match these criteria:")
                    {
                        Weight = TextWeight.Bolder
                    },
                    CreateDropdownSection("Type", "dropdown01", new List<Choice>
                    {
                        new Choice { Title = "Bug", Value = "Bug" },
                        new Choice { Title = "Feature Request", Value = "Feature Request" },
                        new Choice { Title = "Task", Value = "Task" }
                    }),
                    CreateDropdownSection("Priority", "dropdown02", new List<Choice>
                    {
                        new Choice { Title = "Low", Value = "Low" },
                        new Choice { Title = "Medium", Value = "Medium" },
                        new Choice { Title = "High", Value = "High" }
                    }),
                    new TextBlock("Post to channel when:")
                    {
                        Weight = TextWeight.Bolder
                    },
                    CreateDropdownSection("Issue", "dropdown1", new List<Choice>
                    {
                        new Choice { Title = "Software Issue", Value = "Software Issue" },
                        new Choice { Title = "Server Issue", Value = "Server Issue" },
                        new Choice { Title = "Network Issue", Value = "Network Issue" }
                    }, true),
                    CreateDropdownSection("Comment", "dropdown2", new List<Choice>
                    {
                        new Choice { Title = "Network problem in server", Value = "Network problem in server" },
                        new Choice { Title = "Loadbalancer issue", Value = "Loadbalancer issue" },
                        new Choice { Title = "Software needs to be updated", Value = "Software needs to be updated" }
                    }),
                    CreateDropdownSection("Assignee", "dropdown3", new List<Choice>
                    {
                        new Choice { Title = "Jasmine Smith", Value = "Jasmine Smith" },
                        new Choice { Title = "Ethan Johnson", Value = "Ethan Johnson" },
                        new Choice { Title = "Maya Rodriguez", Value = "Maya Rodriguez" }
                    }),
                    CreateDropdownSection("Status changed", "dropdown4", new List<Choice>
                    {
                        new Choice { Title = "Open", Value = "Open" },
                        new Choice { Title = "Inprogress", Value = "Inprogress" },
                        new Choice { Title = "Completed", Value = "Completed" }
                    }),
                    new TextBlock("Actions to display")
                    {
                        Weight = TextWeight.Bolder
                    },
                    new ToggleInput("Assign") { Id = "toggleAssign", Value = "false" },
                    new ToggleInput("Comment") { Id = "toggleComment", Value = "false" },
                    new ToggleInput("Transition") { Id = "toggleTransition", Value = "false" },
                    new ToggleInput("Update status") { Id = "toggleStatus", Value = "false" }
                },
                Actions = new List<Microsoft.Teams.Cards.Action>
                {
                    new SubmitAction
                    {
                        Title = "Submit"
                    }
                }
            };

            return card;
        }

        private CardElement CreateDropdownSection(string label, string id, List<Choice> choices, bool isMultiSelect = false)
        {
            return new Container
            {
                Items = new List<CardElement>
                {
                    new TextBlock(label)
                    {
                        Weight = TextWeight.Bolder
                    },
                    new ChoiceSetInput(choices.ToArray())
                    {
                        Id = id,
                        IsMultiSelect = isMultiSelect
                    }
                }
            };
        }

        private void AddTextBlockIfNotEmpty(AdaptiveCard card, string label, string? value)
        {
            if (!string.IsNullOrEmpty(value) && value != "null")
            {
                card.Body.Add(new TextBlock($"{label} : {value}")
                {
                    Wrap = true
                });
            }
        }

        private void AddTextBlockIfTrue(AdaptiveCard card, string label, string? value)
        {
            if (value?.ToLower() == "true")
            {
                card.Body.Add(new TextBlock($"{label} : True")
                {
                    Wrap = true
                });
            }
        }
    }
}