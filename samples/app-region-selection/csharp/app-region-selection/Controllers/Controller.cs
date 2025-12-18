using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Annotations;
using app_region_selection.Models;
using System.Text.Json;

namespace app_region_selection.Controllers
{
    [TeamsController]
    public class Controller
    {
        // State storage - in-memory for simplicity (use proper state provider in production)
        private static readonly Dictionary<string, WelcomeUserState> _conversationState = new();

        [Message]
        public async Task OnMessage([Context] MessageActivity activity, [Context] IContext.Client client)
        {
            var conversationId = activity.Conversation?.Id ?? "default";
            if (!_conversationState.ContainsKey(conversationId))
            {
                _conversationState[conversationId] = new WelcomeUserState();
            }
            
            var welcomeUserState = _conversationState[conversationId];
            var text = activity.Text?.ToLowerInvariant() ?? string.Empty;

            // Check if user selected a region from the domain list
            if (IsAnyDomainSelected(activity.Text ?? string.Empty))
            {
                await WelcomeCardAsync(activity, client, conversationId, welcomeUserState);
                return;
            }

            // If user already selected a domain and types "change" or "yes"
            if (welcomeUserState.DidUserSelectDomain && (text == "change" || text == "yes"))
            {
                await SendChangeDomainConfirmationCardAsync(activity, client, welcomeUserState);
                return;
            }

            switch (text)
            {
                case "reset":
                    await SendDomainListsCardAsync(client);
                    break;

                case "change":
                case "yes":
                    await SendDomainListsCardAsync(client);
                    break;

                case "no":
                case "cancel":
                    await WelcomeCardAsync(activity, client, conversationId, welcomeUserState);
                    break;

                default:
                    await SendWelcomeIntroCardAsync(activity, client, welcomeUserState);
                    break;
            }
        }

        [Conversation.MembersAdded]
        public async Task OnMembersAdded([Context] ConversationUpdateActivity activity, [Context] IContext.Client client)
        {
            foreach (var member in activity.MembersAdded)
            {
                if (member.Id != activity.Recipient.Id)
                {
                    var conversationId = activity.Conversation?.Id ?? "default";
                    if (!_conversationState.ContainsKey(conversationId))
                    {
                        _conversationState[conversationId] = new WelcomeUserState();
                    }
                    
                    var welcomeUserState = _conversationState[conversationId];

                    if (welcomeUserState.DidUserSelectDomain)
                    {
                        welcomeUserState.DidUserSelectDomain = false;
                        welcomeUserState.SelectedRegion = string.Empty;
                        welcomeUserState.SelectedDomain = string.Empty;
                    }

                    await SendWelcomeIntroCardAsync(activity, client, welcomeUserState);
                }
            }
        }

        /// <summary>
        /// Sends a welcome introduction card.
        /// </summary>
        private async Task SendWelcomeIntroCardAsync(Activity activity, IContext.Client client, WelcomeUserState welcomeUserState)
        {
            string domain;
            string region;

            if (welcomeUserState.DidUserSelectDomain)
            {
                domain = welcomeUserState.SelectedDomain;
                region = welcomeUserState.SelectedRegion;
            }
            else
            {
                var data = GetDefaultInfo(activity);
                domain = data.domain;
                region = data.region;
            }

            string welcomeMsg = $"Your default Region is {region}.";

            var cardContent = new
            {
                type = "AdaptiveCard",
                schema = "http://adaptivecards.io/schemas/adaptive-card.json",
                version = "1.5",
                body = new object[]
                {
                    new
                    {
                        type = "TextBlock",
                        text = "Welcome to Region Selection App!",
                        size = "Large",
                        weight = "Bolder"
                    },
                    new
                    {
                        type = "TextBlock",
                        text = "This will help you to choose your data center's region.",
                        wrap = true
                    },
                    new
                    {
                        type = "TextBlock",
                        text = $"{welcomeMsg} Would you like to change region?",
                        wrap = true
                    }
                },
                actions = new object[]
                {
                    new
                    {
                        type = "Action.Submit",
                        title = "Yes",
                        data = new
                        {
                            msteams = new
                            {
                                type = "messageBack",
                                displayText = "Yes",
                                text = "Yes"
                            }
                        }
                    },
                    new
                    {
                        type = "Action.Submit",
                        title = "No",
                        data = new
                        {
                            msteams = new
                            {
                                type = "messageBack",
                                displayText = "No",
                                text = "No"
                            }
                        }
                    }
                }
            };

            await SendAdaptiveCardAsync(client, cardContent);
        }

        /// <summary>
        /// Gets the default region and domain information.
        /// </summary>
        private (string region, string domain) GetDefaultInfo(Activity activity)
        {
            string serviceUrl = activity.ServiceUrl;
            string domain = serviceUrl.Substring(serviceUrl.LastIndexOf(".")).Trim('/');
            string region = activity.Locale ?? "en-US";

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
            var selectedInfo = JsonSerializer.Deserialize<RootObject>(json)?.RegionDomains.FirstOrDefault(c => c.Region == domain);

            return selectedInfo != null ? (selectedInfo.Region, selectedInfo.Domain) : (string.Empty, string.Empty);
        }

        /// <summary>
        /// Sends a card with a list of available domains.
        /// </summary>
        private async Task SendDomainListsCardAsync(IContext.Client client)
        {
            string file = Path.GetFullPath("ConfigData/Regions.json");
            string json = File.ReadAllText(file);
            var rootObject = JsonSerializer.Deserialize<RootObject>(json);

            if (rootObject?.RegionDomains == null || !rootObject.RegionDomains.Any())
            {
                await client.Send("No regions available.");
                return;
            }

            var actions = rootObject.RegionDomains.Select(region => new
            {
                type = "Action.Submit",
                title = $"{region.Region} - {region.Country}",
                data = new
                {
                    msteams = new
                    {
                        type = "messageBack",
                        displayText = $"{region.Region} - {region.Country}",
                        text = $"{region.Region} - {region.Country}"
                    }
                }
            }).ToArray();

            var cardContent = new
            {
                type = "AdaptiveCard",
                schema = "http://adaptivecards.io/schemas/adaptive-card.json",
                version = "1.5",
                body = new object[]
                {
                    new
                    {
                        type = "TextBlock",
                        text = "Please select your region,",
                        wrap = true
                    }
                },
                actions
            };

            await SendAdaptiveCardAsync(client, cardContent);
        }

        /// <summary>
        /// Sends a welcome card with the user's selected region and domain.
        /// </summary>
        private async Task WelcomeCardAsync(Activity activity, IContext.Client client, string conversationId, WelcomeUserState welcomeUserState)
        {
            var userName = activity.From.Name;
            var text = activity is MessageActivity msgActivity ? msgActivity.Text ?? string.Empty : string.Empty;
            var data = GetSelectedInfo(text);
            string domainName = data.domain;
            string regionName = data.region;

            if (string.IsNullOrEmpty(domainName) && welcomeUserState.DidUserSelectDomain)
            {
                domainName = welcomeUserState.SelectedDomain;
                regionName = welcomeUserState.SelectedRegion;
            }

            if (string.IsNullOrEmpty(domainName))
            {
                var defaultData = GetDefaultInfo(activity);
                domainName = defaultData.domain;
                regionName = defaultData.region;
            }

            welcomeUserState.DidUserSelectDomain = true;
            welcomeUserState.SelectedDomain = domainName;
            welcomeUserState.SelectedRegion = regionName;
            _conversationState[conversationId] = welcomeUserState;

            var cardContent = new
            {
                type = "AdaptiveCard",
                schema = "http://adaptivecards.io/schemas/adaptive-card.json",
                version = "1.5",
                body = new object[]
                {
                    new
                    {
                        type = "TextBlock",
                        text = $"Welcome {userName},",
                        size = "Large",
                        weight = "Bolder"
                    },
                    new
                    {
                        type = "TextBlock",
                        text = $"You are in {regionName} Region's Data Center",
                        wrap = true
                    },
                    new
                    {
                        type = "TextBlock",
                        text = "If you want to change data center's region, please enter text 'Change'.",
                        wrap = true
                    }
                }
            };

            await SendAdaptiveCardAsync(client, cardContent);
        }

        /// <summary>
        /// Sends a confirmation card to change the domain.
        /// </summary>
        private async Task SendChangeDomainConfirmationCardAsync(Activity activity, IContext.Client client, WelcomeUserState welcomeUserState)
        {
            var cardContent = new
            {
                type = "AdaptiveCard",
                schema = "http://adaptivecards.io/schemas/adaptive-card.json",
                version = "1.5",
                body = new object[]
                {
                    new
                    {
                        type = "TextBlock",
                        text = $"You have already selected your data center region and that is {welcomeUserState.SelectedRegion}. Would you like to change this?",
                        wrap = true
                    }
                },
                actions = new object[]
                {
                    new
                    {
                        type = "Action.Submit",
                        title = "Reset",
                        data = new
                        {
                            msteams = new
                            {
                                type = "messageBack",
                                displayText = "Reset",
                                text = "Reset"
                            }
                        }
                    },
                    new
                    {
                        type = "Action.Submit",
                        title = "Cancel",
                        data = new
                        {
                            msteams = new
                            {
                                type = "messageBack",
                                displayText = "Cancel",
                                text = "Cancel"
                            }
                        }
                    }
                }
            };

            await SendAdaptiveCardAsync(client, cardContent);
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
            bool isAnyDomainSelected = JsonSerializer.Deserialize<RootObject>(json)?.RegionDomains.Any(c => c.Region == domain) ?? false;

            return isAnyDomainSelected;
        }

        /// <summary>
        /// Helper method to send adaptive card through Teams SDK V2 client
        /// </summary>
        private async Task SendAdaptiveCardAsync(IContext.Client client, object cardContent)
        {
            // Serialize the card content to JSON
            var cardJson = JsonSerializer.Serialize(cardContent, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });

            // Parse the JSON to get a JsonElement (required by Teams SDK V2)
            using var jsonDoc = JsonDocument.Parse(cardJson);
            var cardElement = jsonDoc.RootElement.Clone();

            // Create attachment using the correct Teams SDK V2 types
            var attachment = new Microsoft.Teams.Api.Attachment
            {
                ContentType = Microsoft.Teams.Api.ContentType.AdaptiveCard,
                Content = cardElement
            };

            // Create reply message with attachment
            var reply = new MessageActivity();
            reply.Attachments = new List<Microsoft.Teams.Api.Attachment> { attachment };

            await client.Send(reply);
        }
    }
}