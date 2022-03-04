// <copyright file="ITurnContextExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Web.Bot
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities;

    /// <summary>
    /// ITurnContext extensions.
    /// </summary>
    public static class ITurnContextExtensions
    {
        /// <summary>
        /// Determines if the user signed in.
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="oauthConnectionName">OAuth connection name.</param>
        /// <param name="state">Magic code.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if user is signed-in, false otherwise.</returns>
        public static async Task<bool> IsUserSignedInAsync(this ITurnContext<IInvokeActivity> turnContext, string oauthConnectionName, string state, CancellationToken cancellationToken)
        {
            var tokenResponse = await turnContext.GetTokenResponseAsync(oauthConnectionName, state, cancellationToken);
            return !string.IsNullOrEmpty(tokenResponse?.Token);
        }

        /// <summary>
        /// Prepares sign-in url.
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="oauthConnectionName">OAuthConnection name.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Sign-in Url.</returns>
        public static async Task<string> GetOAuthSignInUrlAsync(this ITurnContext<IInvokeActivity> turnContext, string oauthConnectionName, CancellationToken cancellationToken)
        {
            var client = turnContext.TurnState.Get<UserTokenClient>();
            var signInResource = await client
                ?.GetSignInResourceAsync(
                    activity: turnContext.Activity as Activity,
                    connectionName: oauthConnectionName,
                    finalRedirect: null,
                    cancellationToken: cancellationToken);
            return signInResource.SignInLink;
        }

        /// <summary>
        /// Signs the user out with the token server.
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="oauthConnectionName">OAuthConnection name.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task SignOutUserAsync(this ITurnContext<IInvokeActivity> turnContext, string oauthConnectionName, CancellationToken cancellationToken)
        {
            var client = turnContext.TurnState.Get<UserTokenClient>();
            await client.SignOutUserAsync(
                userId: turnContext.Activity.From.Id,
                connectionName: oauthConnectionName,
                channelId: turnContext.Activity.ChannelId,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read cached user token.
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="oauthConnectionName">OAuth Connection name.</param>
        /// <param name="state">Magic code.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="TokenResponse"/>.</returns>
        public static async Task<TokenResponse> GetTokenResponseAsync(this ITurnContext<IInvokeActivity> turnContext, string oauthConnectionName, string state, CancellationToken cancellationToken)
        {
            var client = turnContext.TurnState.Get<UserTokenClient>();

            var tokenResponse = await client.GetUserTokenAsync(
                userId: turnContext.Activity.From.Id,
                connectionName: oauthConnectionName,
                channelId: turnContext.Activity.ChannelId,
                magicCode: state,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return tokenResponse;
        }

        /// <summary>
        /// Prepares conversation context from turn context.
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <returns><see cref="ConversationContext"/>.</returns>
        public static ConversationContext GetConversationContext(this ITurnContext<IInvokeActivity> turnContext)
        {
            var meetingId = turnContext.Activity.TeamsGetMeetingInfo()?.Id;
            return new ConversationContext()
            {
                ConversationId = turnContext.Activity.Conversation.Id,
                ConversationType = turnContext.GetConversationType(),
                IsMeetingConversation = !string.IsNullOrEmpty(meetingId),
                MeetingId = meetingId,
            };
        }

        /// <summary>
        /// Reads conversation type from turn context.
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <returns><see cref="ConversationType"/>.</returns>
        public static ConversationType GetConversationType(this ITurnContext<IInvokeActivity> turnContext)
        {
            var conversationType = turnContext.Activity.Conversation.ConversationType;
            if (string.IsNullOrEmpty(conversationType))
            {
                // Known issue where conversation type isn't set for meeting group chat.
                return ConversationType.GroupChat;
            }

            return conversationType switch
            {
                "personal" => ConversationType.Personal,
                "groupChat" => ConversationType.GroupChat,
                "channel" => ConversationType.Channel,
                _ => throw new Exception("Unknown conversation type: " + conversationType),
            };
        }
    }
}
