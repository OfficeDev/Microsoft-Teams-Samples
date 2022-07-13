// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using AdaptiveCards.Templating;
using Content_Bubble_Bot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;


namespace Content_Bubble_Bot
{
    public class ContentBubbleBot : TeamsActivityHandler
    {
        private readonly IConfiguration _config;
        private readonly MeetingAgenda _agenda;

        public ContentBubbleBot(IConfiguration configuration)
        {
            _config = configuration;
            _agenda = new MeetingAgenda
            {
                AgendaItems = new List<AgendaItem>() {
                                        new AgendaItem { Topic = "Approve 5% dividend payment to shareholders" , Id = 1 },
                                        new AgendaItem { Topic = "Increase research budget by 10%" , Id = 2},
                                        new AgendaItem { Topic = "Continue with WFH for next 3 months" , Id = 3}},
            };
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Value == null)
            {
                Attachment adaptiveCardAttachment = GetAdaptiveCardAttachment("AgendaCard.json", _agenda);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment));
            }
            else
            {
                await HandleActions(turnContext);
            }
        }

        private async Task HandleActions(ITurnContext<IMessageActivity> turnContext)
        {
            var action = Newtonsoft.Json.JsonConvert.DeserializeObject<ActionBase>(turnContext.Activity.Value.ToString());

            switch (action.Type)
            {
                case "PushAgenda":
                    var pushAgendaAction = Newtonsoft.Json.JsonConvert.DeserializeObject<PushAgendaAction>(turnContext.Activity.Value.ToString());
                    var agendaItem = _agenda.AgendaItems.First(a => a.Id.ToString() == pushAgendaAction.Choice);
                    
                    Attachment adaptiveCardAttachment = GetAdaptiveCardAttachment("QuestionTemplate.json", agendaItem);
                    var activity = MessageFactory.Attachment(adaptiveCardAttachment);
                    
                    activity.ChannelData = new
                    {
                        OnBehalfOf = new []
                        {
                            new
                            {
                                ItemId = 0,
                                MentionType = "person",
                                Mri = turnContext.Activity.From.Id,
                                DisplayName = turnContext.Activity.From.Name
                            }
                        },
                        Notification = new
                        {
                            AlertInMeeting = true,
                            ExternalResourceUrl = $"https://teams.microsoft.com/l/bubble/{ _config["MicrosoftAppId"] }?url=" +
                                                  HttpUtility.UrlEncode($"{_config["BaseUrl"]}/ContentBubble?topic={agendaItem.Topic}") +
                                                  $"&height=270&width=250&title=ContentBubble&completionBotId={_config["MicrosoftAppId"]}"
                        }
                    };
                    await turnContext.SendActivityAsync(activity);
                    break;
                case "SubmitFeedback":
                    var submitFeedback = Newtonsoft.Json.JsonConvert.DeserializeObject<SubmitFeedbackAction>(turnContext.Activity.Value.ToString());
                    var item = _agenda.AgendaItems.First(a => a.Id.ToString() == submitFeedback.Choice);
                    await turnContext.SendActivityAsync($"{turnContext.Activity.From.Name} voted **{submitFeedback.Feedback}** for '{item.Topic}'");
                    break;
                default:
                    break;
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome to Content Bubble Sample Bot! Send my hello to see today's agenda.";
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
            var submitFeedback = Newtonsoft.Json.JsonConvert.DeserializeObject<SubmitFeedbackAction>(taskModuleRequest.Data.ToString());
            await turnContext.SendActivityAsync($"{turnContext.Activity.From.Name} voted **{submitFeedback.Feedback}** for '{submitFeedback.Topic}'");

            return null;
        }

        private Attachment GetAdaptiveCardAttachment(string fileName, object cardData)
        {
            var templateJson = File.ReadAllText("./Cards/" + fileName);
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(templateJson);

            string cardJson = template.Expand(cardData);
            AdaptiveCardParseResult result = AdaptiveCard.FromJson(cardJson);

            // Get card from result
            AdaptiveCard card = result.Card;

            var adaptiveCardAttachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
            return adaptiveCardAttachment;
        }
    }
}
