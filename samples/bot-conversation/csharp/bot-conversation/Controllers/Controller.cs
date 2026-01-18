using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Annotations;
using Microsoft.Teams.Api.Clients;
using Microsoft.Teams.Api.Auth;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Teams.Common.Http;
using SysHttpClient = System.Net.Http.HttpClient;

namespace bot_conversation.Controllers
{
    [TeamsController]
    public class Controller
    {
        private readonly Microsoft.Teams.Apps.App _teamsApp;
        private readonly Microsoft.Teams.Common.Http.IHttpCredentials? _credentials;
        private static int _counter = 0;
        private static List<string> users = new List<string>();
        private static ConcurrentDictionary<string, string> teamMemberDetails = new ConcurrentDictionary<string, string>();
        private static ConcurrentDictionary<string, string> teamMemberMessageIdDetails = new ConcurrentDictionary<string, string>();

        private readonly string _adaptiveCardTemplate = Path.Combine(".", "Resources", "UserMentionCardTemplate.json");
        private readonly string _immersiveReaderCardTemplate = Path.Combine(".", "Resources", "ImmersiveReaderCard.json");

        public Controller(
            Microsoft.Teams.Apps.App teamsApp,
            Microsoft.Teams.Common.Http.IHttpCredentials? credentials = null)
        {
            _teamsApp = teamsApp;
            _credentials = credentials;
        }

        [Message]
        public async Task OnMessage(IContext<MessageActivity> context, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("Message received");
            
            var text = context.Activity.Text?.Trim().ToLower() ?? "";

            if (text.Contains("mentionme") || text.Contains("mention me"))
                await MentionAdaptiveCardActivityAsync(context.Activity, context);
            else if (text.Contains("mention"))
                await MentionActivityAsync(context.Activity, context);
            else if (text.Contains("show welcome") || text.Contains("welcome"))
                await SendWelcomeCard(context);
            else if (text.Contains("messageallmembersusingaadid") || text.Contains("aadid"))
                await MessageAllMembersAsync(context, true);
            else if (text.Contains("messageallmembers") || text.Contains("message"))
                await MessageAllMembersAsync(context, false);
            else if (text.Contains("immersivereader"))
                await SendImmersiveReaderCardAsync(context);
            else if (text.Contains("check") && text.Contains("read"))
                await CheckReadUserCount(context);
            else if (text.Contains("reset") && text.Contains("read"))
                await ResetReadUserCount(context);
            else if (text.Contains("ai") && text.Contains("label"))
                await AddAILabel(context);
            else if (text.Contains("feedback"))
                await AddFeedbackButtons(context);
            else if (text.Contains("sensitivity"))
                await AddSensitivityLabel(context);
            else if (text.Contains("citation"))
                await AddCitations(context);
            else if (text.Contains("sendaitext") || text.Contains("aitext"))
                await SendAIMessage(context);
            else
                await context.Send($"You said: '{context.Activity.Text}'. Try 'MentionMe', 'Show Welcome', 'MessageAllMembers', 'AI label', 'sensitivity', 'feedback', 'citation', or 'SendAIText'");
        }

        [Conversation.MembersAdded]
        public async Task OnMembersAdded(IContext<ConversationUpdateActivity> context)
        {
            foreach (var member in context.Activity.MembersAdded)
            {
                if (member.Id != context.Activity.Recipient.Id)
                {
                    await context.Send($"Welcome to the team {member.Name}! This bot demonstrates Teams conversation events and adaptive cards.");
                }
            }
        }

        // Note: Additional event handlers like MembersRemoved, ChannelCreated, ChannelDeleted,
        // ChannelRenamed, TeamRenamed, ReactionsAdded, ReactionsRemoved, MessageEdited,
        // MessageDeleted, and MessageUndeleted are part of the Bot Framework v4 SDK.
        // Teams SDK V2 handles events differently and these specific attributes may not be
        // directly available. Consult Teams SDK V2 documentation for equivalent event handling.

        private async Task MentionActivityAsync(MessageActivity activity, IContext<MessageActivity> context)
        {
            // Simple mention using text formatting
            // Teams SDK V2 handles mentions differently than Bot Framework v4
            var mentionText = $"<at>{activity.From.Name}</at>";
            await context.Send($"Hello {mentionText}!");
        }

        private async Task MentionAdaptiveCardActivityAsync(MessageActivity activity, IContext<MessageActivity> context)
        {
            try
            {
                var templateJson = await File.ReadAllTextAsync(_adaptiveCardTemplate);
                
                // Replace template variables
                var cardJson = templateJson
                    .Replace("${userName}", activity.From.Name ?? "User")
                    .Replace("${userUPN}", activity.From.AadObjectId ?? activity.From.Id)
                    .Replace("${userAAD}", activity.From.AadObjectId ?? activity.From.Id);

                var cardElement = JsonSerializer.Deserialize<JsonElement>(cardJson);

                // Create a MessageActivity with adaptive card attachment
                var messageActivity = new MessageActivity
                {
                    Attachments = new List<Microsoft.Teams.Api.Attachment>
                    {
                        new Microsoft.Teams.Api.Attachment
                        {
                            ContentType = Microsoft.Teams.Api.ContentType.AdaptiveCard,
                            Content = cardElement
                        }
                    }
                };
                
                await context.Send(messageActivity);
            }
            catch (Exception ex)
            {
                await context.Send($"Error sending mention card: {ex.Message}");
            }
        }

        private async Task SendWelcomeCard(IContext<MessageActivity> context)
        {
            // Following the Node.js Teams SDK migration pattern
            var heroCardContent = new
            {
                title = "Teams Conversation Bot - Welcome!",
                text = "This bot demonstrates Teams conversation events and features. Select an option below:",
                buttons = new[]
                {
                    new
                    {
                        type = "imBack",
                        title = "Show Welcome",
                        value = "Show Welcome"
                    },
                    new
                    {
                        type = "imBack",
                        title = "Mention Me",
                        value = "mention me"
                    },
                    new
                    {
                        type = "imBack",
                        title = "Message All Members",
                        value = "MessageAllMembers"
                    },
                    new
                    {
                        type = "imBack",
                        title = "Message All Members (AAD)",
                        value = "MessageAllMembersUsingAadId"
                    },
                    new
                    {
                        type = "imBack",
                        title = "Immersive Reader",
                        value = "immersivereader"
                    },
                    new
                    {
                        type = "imBack",
                        title = "AI Label Example",
                        value = "AI label"
                    },
                    new
                    {
                        type = "imBack",
                        title = "Citations Example",
                        value = "citation"
                    },
                    new
                    {
                        type = "imBack",
                        title = "Feedback Buttons",
                        value = "feedback"
                    },
                    new
                    {
                        type = "imBack",
                        title = "Sensitivity Label",
                        value = "sensitivity"
                    },
                    new
                    {
                        type = "imBack",
                        title = "Send AI Text",
                        value = "SendAIText"
                    }
                }
            };

            var messageActivity = new MessageActivity
            {
                Type = ActivityType.Message,
                Attachments = new List<Microsoft.Teams.Api.Attachment>
                {
                    new Microsoft.Teams.Api.Attachment
                    {
                        ContentType = Microsoft.Teams.Api.ContentType.HeroCard,
                        Content = JsonSerializer.SerializeToElement(heroCardContent)
                    }
                }
            };
            
            await context.Send(messageActivity);
        }

        private async Task MessageAllMembersAsync(IContext<MessageActivity> context, bool useAadId)
        {
            await ResetReadUserCount(context);
            
            try
            {
                var conversationId = context.Activity.Conversation.Id;
                var serviceUrl = context.Activity.ServiceUrl;
                var tenantId = context.Activity.Conversation.TenantId;
                
                // Get team members using Teams SDK V2 API Client
                var members = await context.Api.Conversations.Members.GetAsync(conversationId);
                
                if (members == null || !members.Any())
                {
                    await context.Send("No members found in the conversation.");
                    return;
                }
                
                // Get bot authentication token
                string botToken = await GetBotTokenAsync(tenantId);
                
                if (string.IsNullOrEmpty(botToken))
                {
                    await context.Send("Unable to authenticate. Cannot send proactive messages.");
                    return;
                }
                
                int successCount = 0;
                int failureCount = 0;
                
                foreach (var member in members)
                {
                    if (member.Id == context.Activity.Recipient.Id)
                        continue; // Skip bot
                        
                    try
                    {
                        // Create 1:1 conversation using Teams SDK V2
                        var createRequest = new ConversationClient.CreateRequest
                        {
                            Bot = context.Activity.Recipient,
                            Members = useAadId 
                                ? new[] { new Microsoft.Teams.Api.Account { Id = member.AadObjectId } }
                                : new[] { new Microsoft.Teams.Api.Account { Id = member.Id, Name = member.Name } },
                            TenantId = tenantId,
                            IsGroup = false
                        };
                        
                        var conversationResource = await context.Api.Conversations.CreateAsync(createRequest);
                        
                        if (conversationResource != null && !string.IsNullOrEmpty(conversationResource.Id))
                        {
                            // Send proactive message using Bot Connector REST API
                            try
                            {
                                using var httpClient = new SysHttpClient();
                                httpClient.BaseAddress = new Uri(serviceUrl);
                                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", botToken);
                                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                
                                var activityPayload = new
                                {
                                    type = "message",
                                    from = new 
                                    { 
                                        id = context.Activity.Recipient.Id,
                                        name = context.Activity.Recipient.Name
                                    },
                                    conversation = new 
                                    { 
                                        id = conversationResource.Id,
                                        tenantId = tenantId
                                    },
                                    recipient = new 
                                    { 
                                        id = member.Id,
                                        name = member.Name
                                    },
                                    text = $"Hello {member.Name}. I'm a Teams conversation bot.",
                                    serviceUrl = serviceUrl,
                                    channelId = "msteams"
                                };
                                
                                var jsonPayload = JsonSerializer.Serialize(activityPayload);
                                var content = new System.Net.Http.StringContent(
                                    jsonPayload,
                                    System.Text.Encoding.UTF8,
                                    "application/json"
                                );
                                
                                var response = await httpClient.PostAsync(
                                    $"/v3/conversations/{Uri.EscapeDataString(conversationResource.Id)}/activities",
                                    content
                                );
                                
                                if (response.IsSuccessStatusCode)
                                {
                                    var responseContent = await response.Content.ReadAsStringAsync();
                                    var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                                    var activityId = responseData.GetProperty("id").GetString();
                                    
                                    teamMemberDetails.TryAdd(member.AadObjectId ?? member.Id, member.Name ?? "Unknown");
                                    teamMemberMessageIdDetails.TryAdd(member.AadObjectId ?? member.Id, activityId ?? "");
                                    successCount++;
                                }
                                else
                                {
                                    failureCount++;
                                }
                            }
                            catch
                            {
                                failureCount++;
                            }
                        }
                        else
                        {
                            failureCount++;
                        }
                    }
                    catch
                    {
                        failureCount++;
                    }
                }
                
                await context.Send("All messages have been sent.");
            }
            catch (Exception ex)
            {
                await context.Send($"Error: {ex.Message}");
            }
        }
        
        private async Task<string> GetBotTokenAsync(string? tenantId)
        {
            try
            {
                if (_credentials == null)
                {
                    return string.Empty;
                }
                
                var tokenCredType = _credentials.GetType();
                var tokenProperty = tokenCredType.GetProperty("Token");
                
                if (tokenProperty != null)
                {
                    var tokenFactory = tokenProperty.GetValue(_credentials);
                    
                    if (tokenFactory != null)
                    {
                        var scopes = new[] { "https://api.botframework.com/.default" };
                        var factoryType = tokenFactory.GetType();
                        var invokeMethod = factoryType.GetMethod("Invoke");
                        
                        if (invokeMethod != null)
                        {
                            var result = invokeMethod.Invoke(tokenFactory, new object?[] { tenantId, scopes });
                            
                            if (result is Task<ITokenResponse> tokenTask)
                            {
                                var tokenResponse = await tokenTask;
                                return tokenResponse?.AccessToken ?? string.Empty;
                            }
                        }
                    }
                }
                
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task SendImmersiveReaderCardAsync(IContext<MessageActivity> context)
        {
            try
            {
                var cardJson = await File.ReadAllTextAsync(_immersiveReaderCardTemplate);
                var cardElement = JsonSerializer.Deserialize<JsonElement>(cardJson);
                
                var messageActivity = new MessageActivity
                {
                    Attachments = new List<Microsoft.Teams.Api.Attachment>
                    {
                        new Microsoft.Teams.Api.Attachment
                        {
                            ContentType = Microsoft.Teams.Api.ContentType.AdaptiveCard,
                            Content = cardElement
                        }
                    }
                };
                
                await context.Send(messageActivity);
            }
            catch (Exception ex)
            {
                await context.Send($"Error sending immersive reader card: {ex.Message}");
            }
        }

        private async Task CheckReadUserCount(IContext<MessageActivity> context)
        {
            await context.Send($"Total read count: {_counter}. Users who read: {string.Join(", ", users)}");
        }

        private async Task ResetReadUserCount(IContext<MessageActivity> context)
        {
            _counter = 0;
            users.Clear();
            teamMemberDetails.Clear();
            teamMemberMessageIdDetails.Clear();
            await context.Send("Read count has been reset");
        }

        private async Task AddAILabel(IContext<MessageActivity> context)
        {
            // Note: AI labels, sensitivity labels, citations, and feedback buttons in Teams SDK V2
            // require specific message formatting that may differ from Bot Framework v4.
            // This is a simplified demonstration.
            await context.Send("🤖 **AI Generated**\n\nThis is an AI-generated message. The AI label indicates that this content was created by artificial intelligence.");
        }

        private async Task AddFeedbackButtons(IContext<MessageActivity> context)
        {
            var messageActivity = new MessageActivity
            {
                Type = ActivityType.Message,
                Text = "This is an example for Feedback buttons that helps to provide feedback for a bot message",
                ChannelData = new Microsoft.Teams.Api.ChannelData
                {
                    FeedbackLoopEnabled = true
                }
            };
            
            await context.Send(messageActivity);
        }

        private async Task AddSensitivityLabel(IContext<MessageActivity> context)
        {
            await context.Send("🔒 **Confidential \\ Contoso FTE**\n\nThis is an example for sensitivity label that helps users identify the confidentiality of a message.\n\n_Please be mindful of sharing outside of your team_");
        }

        private async Task AddCitations(IContext<MessageActivity> context)
        {
            await context.Send("This message includes a citation [1] to reference the source\n\n**References:**\n[1] Example Citation - https://example.com/citation\n_\"This is an example citation excerpt\"_");
        }

        private async Task SendAIMessage(IContext<MessageActivity> context)
        {
            var messageActivity = new MessageActivity
            {
                Type = ActivityType.Message,
                Text = "Hey I'm a friendly AI bot. This message is generated via AI [1]\n\n**References:**\n[1] AI Knowledge Base - https://example.com/ai-source\n\n---\n_This message demonstrates: AI label, Citations, Feedback buttons, and Sensitivity features combined._",
                ChannelData = new Microsoft.Teams.Api.ChannelData
                {
                    FeedbackLoopEnabled = true
                }
            };
            
            await context.Send(messageActivity);
        }
    }
}