// <copyright file="BotActivityHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.UserSpecificViews.Bot
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Samples.UserSpecificViews.Cards;
    using Newtonsoft.Json;

    /// <summary>
    /// Teams Bot Activity Handler.
    /// </summary>
    public class BotActivityHandler : TeamsActivityHandler
    {
        private const string AllUsersCardType = "All Users";
        private const string MeCardType = "Me";

        private readonly ICardFactory cardFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotActivityHandler"/> class.
        /// </summary>
        /// <param name="cardFactory">Card factory.</param>
        public BotActivityHandler(
            ICardFactory cardFactory)
        {
            this.cardFactory = cardFactory ?? throw new ArgumentNullException(nameof(cardFactory));
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Send initial card.
            var initialCard = this.cardFactory.GetSelectCardTypeCard();
            await turnContext.SendActivityAsync(MessageFactory.Attachment(initialCard), cancellationToken);
        }

        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            // Note: Make sure that the incoming invoke actions are processed <10 seconds to avoid timeouts in Teams clients.
            if (turnContext.Activity.Name == "adaptiveCard/action")
            {
                var actionData = JsonConvert.DeserializeObject<RefreshActionData>(turnContext.Activity.Value.ToString());

                // Increase the refresh count.
                actionData.action.data.RefreshCount++;
                switch (actionData.action.verb)
                {
                    case "me":
                        // Send an auto refresh user specific view card for the `sender`.
                        var card = this.cardFactory.GetAutoRefreshForSpecificUserBaseCard(turnContext.Activity.From.Id, MeCardType);
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(card), cancellationToken);
                        break;

                    case "allusers":
                        // Send an auto refresh user specific card for `all users` in the chat.
                        card = this.cardFactory.GetAutoRefreshForAllUsersBaseCard(AllUsersCardType);
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(card), cancellationToken);
                        break;

                    case "UpdateBaseCard":
                        // Update the base card for `all users`.
                        // Note: We remove the refresh section from the updated base card in this sample. No further refresh invoke actions will be fired for any user.
                        // You may decide to keep the refresh section and trigger auto-refresh for either all / list of users if required.
                        card = this.cardFactory.GetFinalBaseCard(actionData);
                        if (!string.IsNullOrEmpty(turnContext.Activity.ReplyToId))
                        {
                                var activity = MessageFactory.Attachment(card);
                                activity.Id = turnContext.Activity.ReplyToId;
                                await turnContext.UpdateActivityAsync(activity, cancellationToken);
                        }
                        else
                        {
                            return PrepareInvokeResponse(card);
                        }
                        break;

                    case "RefreshUserSpecificView":
                        // Update the card for the `user` for whom this invoke action was triggered.
                        card = this.cardFactory.GetUpdatedCardForUser(turnContext.Activity.From.Id, actionData);
                        return PrepareInvokeResponse(card);
                }
            }

            var adaptiveCardResponse = new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = "application/vnd.microsoft.activity.message",
                Value = "Success!" // Optional message to be shown to the user.
            };
            return ActivityHandler.CreateInvokeResponse(adaptiveCardResponse);
        }

        private static InvokeResponse PrepareInvokeResponse(Attachment card)
        {
            var newCardResponse = new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = card.ContentType,
                Value = card.Content
            };
            return ActivityHandler.CreateInvokeResponse(newCardResponse);
        }
    }
}
