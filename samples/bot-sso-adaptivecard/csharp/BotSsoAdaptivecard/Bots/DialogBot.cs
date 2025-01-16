// <copyright file="DialogBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Connector.Authentication;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using AdaptiveCards.Templating;
using AdaptiveCards;
using BotSsoAdaptivecard.Helper;

namespace Microsoft.BotBuilderSamples
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class DialogBot<T> : TeamsActivityHandler where T : Dialog
    {
        protected readonly BotState _conversationState;  // Represents the conversation state
        protected readonly Dialog _dialog;               // The dialog logic to run
        protected readonly ILogger _logger;              // Logger for debugging and tracing
        protected readonly BotState _userState;          // Represents the user state
        protected string _connectionName { get; }        // Connection name for OAuth

        // Constructor to initialize the bot with necessary dependencies
        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger, string connectionName)
        {
            _conversationState = conversationState;
            _userState = userState;
            _dialog = dialog;
            _logger = logger;
            _connectionName = connectionName;
        }

        /// <summary>
        /// Get sign in link
        /// </summary>
        /// <param name="turnContext">The context for the current turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        // Get the sign-in link for OAuth
        private async Task<string> GetSignInLinkAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            var resource = await userTokenClient.GetSignInResourceAsync(_connectionName, turnContext.Activity as Activity, null, cancellationToken).ConfigureAwait(false);
            return resource.SignInLink;
        }

        /// <summary>
        /// Add logic to apply after the type-specific logic after the call to the base class method.
        /// </summary>
        /// <param name="turnContext">The context for the current turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        // OnTurnAsync: Handles parallel saving of conversation and user state changes
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes in parallel to improve performance
            await Task.WhenAll(
                _conversationState.SaveChangesAsync(turnContext, false, cancellationToken),
                _userState.SaveChangesAsync(turnContext, false, cancellationToken)
            );
        }

        /// <summary>
        /// Override this in a derived class to provide logic specific to Message activities, such as the conversational logic.
        /// </summary>
        /// <param name="turnContext">The context for the current turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        // Simplified message activity handling to trigger appropriate adaptive card based on the message command
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var signInLink = await GetSignInLinkAsync(turnContext, cancellationToken).ConfigureAwait(false);
            await HandleCommandAsync(turnContext.Activity.Text, turnContext, signInLink, cancellationToken);
        }

        // Helper function to handle commands and send the appropriate adaptive card
        private async Task HandleCommandAsync(string command, ITurnContext<IMessageActivity> turnContext, string signInLink, CancellationToken cancellationToken)
        {
            var commandToFileMap = new Dictionary<string, string>
            {
                { "login", "options.json" },
                { "PerformSSO", "AdaptiveCardWithSSOInRefresh.json" }
            };

            if (commandToFileMap.ContainsKey(command))
            {
                string[] path = { ".", "Resources", commandToFileMap[command] };
                var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
                var adaptiveCard = GetAdaptiveCardFromFileName(path, signInLink, turnContext.Activity.From.Name, member.Id);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCard), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Please send 'login' for options"), cancellationToken);
            }
        }

        /// <summary>
        /// The OAuth Prompt needs to see the Invoke Activity in order to complete the login process.
        /// </summary>
        /// <param name="turnContext">The context for the current turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        // Override to handle invoke activities, such as OAuth and adaptive card actions
        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Name == "adaptiveCard/action")
            {
                if (turnContext.Activity.Value == null)
                    return null;

                JObject value = JsonConvert.DeserializeObject<JObject>(turnContext.Activity.Value.ToString());

                if (value["action"] == null)
                    return null;

                JObject actionData = JsonConvert.DeserializeObject<JObject>(value["action"].ToString());

                if (actionData["verb"] == null)
                    return null;

                string verb = actionData["verb"].ToString();

                JObject authentication = null;
                string state = null;

                // Check for authentication token or state
                if (value["authentication"] != null)
                {
                    authentication = JsonConvert.DeserializeObject<JObject>(value["authentication"].ToString());
                }

                if (value["state"] != null)
                {
                    state = value["state"].ToString();
                }

                // Token and state are absent, initiate SSO
                if (authentication == null && state == null)
                {
                    if (verb == "initiateSSO")
                    {
                        return await InitiateSSOAsync(turnContext, cancellationToken);
                    }
                }
                else
                {
                    return CreateAdaptiveCardInvokeResponseAsync(authentication, state);
                }
            }

            return null;
        }

        /// <summary>
        /// Authentication success.
        /// AuthToken or state is present. Verify token/state in invoke payload and return AC response
        /// </summary>
        /// <param name="authentication">authToken are absent, handle verb</param>
        /// <param name="state">state are absent, handle verb</param>
        /// <param name="turnContext">The context for the current turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <param name="isBasicRefresh">Refresh type</param>
        /// <param name="fileName">AdaptiveCardResponse.json</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private InvokeResponse CreateAdaptiveCardInvokeResponseAsync(JObject authentication, string state, bool isBasicRefresh = false, string fileName = "AdaptiveCardResponse.json")
        {
            string authResultData = (authentication != null) ? "SSO success" : (state != null && state != "") ? "OAuth success" : "SSO/OAuth failed";

            if (isBasicRefresh)
            {
                authResultData = "Refresh done";
            }

            string[] filePath = { ".", "Resources", fileName };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(filePath));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);
            var payloadData = new { authResult = authResultData };
            var cardJsonString = template.Expand(payloadData);

            var adaptiveCardResponse = new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = AdaptiveCard.ContentType,
                Value = JsonConvert.DeserializeObject(cardJsonString)
            };

            return CreateInvokeResponse(adaptiveCardResponse);
        }

        /// <summary>
        /// when token is absent in the invoke. We can initiate SSO in response to the invoke
        /// </summary>
        /// <param name="turnContext">The context for the current turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task<InvokeResponse> InitiateSSOAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var signInLink = await GetSignInLinkAsync(turnContext, cancellationToken).ConfigureAwait(false);
            var oAuthCard = new OAuthCard
            {
                Text = "Please sign in",
                ConnectionName = _connectionName,
                TokenExchangeResource = new TokenExchangeResource
                {
                    Id = Guid.NewGuid().ToString()
                },
                Buttons = new List<CardAction>
                {
                    new CardAction
                    {
                        Type = ActionTypes.Signin,
                        Value = signInLink,
                        Title = "Sign In",
                    },
                }
            };

            var loginReqResponse = JObject.FromObject(new
            {
                statusCode = 401,
                type = "application/vnd.microsoft.activity.loginRequest",
                value = oAuthCard
            });

            return CreateInvokeResponse(loginReqResponse);
        }

        /// <summary>
        /// Get Adaptive Card
        /// </summary>
        /// <param name="filepath">json path</param>
        /// <param name="signInLink">Get sign in link</param>
        /// <param name="name">createdBy</param>
        /// <param name="userMRI">createdById</param>
        /// <returns></returns>
        // Method to retrieve adaptive card from a file and expand with dynamic data
        private Attachment GetAdaptiveCardFromFileName(string[] filepath, string signInLink, string name = null, string userMRI = null)
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);
            var payloadData = new
            {
                createdById = userMRI,
                createdBy = name
            };

            var cardJsonString = template.Expand(payloadData);
            var card = JsonConvert.DeserializeObject<JObject>(cardJsonString);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card,
            };

            return adaptiveCardAttachment;
        }
    }
}
