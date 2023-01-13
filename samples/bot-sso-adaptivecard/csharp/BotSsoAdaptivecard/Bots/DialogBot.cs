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
        protected readonly BotState _conversationState;
        protected readonly Dialog _dialog;
        protected readonly ILogger _logger;
        protected readonly BotState _userState;
        private readonly string _connectionName = "<<YOUR-CONNECTION-NAME>>";
        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
        {
            _conversationState = conversationState;
            _userState = userState;
            _dialog = dialog;
            _logger = logger;
        }

        /// <summary>
        /// Get sign in link
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<string> GetSignInLinkAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            var resource = await userTokenClient.GetSignInResourceAsync(_connectionName, turnContext.Activity as Activity, null, cancellationToken).ConfigureAwait(false);
            return resource.SignInLink;
        }

        /// <summary>
        /// Add logic to apply after the type-specific logic after the call to the base class method.
        /// </summary>
        /// <param name="turnContext"></param>
        /// The context object for this turn.
        /// <param name="cancellationToken"></param>
        /// A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        /// <returns></returns>
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// Override this in a derived class to provide logic specific to Message activities, such as the conversational logic.
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var signInLink = await GetSignInLinkAsync(turnContext, cancellationToken).ConfigureAwait(false);
            if (turnContext.Activity.Text.Contains("login"))
            {
                string[] path = { ".", "Resources", "options.json" };
                var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
                var initialAdaptiveCard = GetFirstOptionsAdaptiveCard(path, signInLink, turnContext.Activity.From.Name, member.Id);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(initialAdaptiveCard), cancellationToken);
            }
            else if (turnContext.Activity.Text.Contains("PerformSSO"))
            {
                string[] striPath = { ".", "Resources", "AdaptiveCardWithSSOInRefresh.json" };
                var varMember = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
                var varInitialAdaptiveCard = GetFirstOptionsAdaptiveCard(striPath, signInLink, turnContext.Activity.From.Name, varMember.Id);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(varInitialAdaptiveCard), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Please send 'login' for options"), cancellationToken);
            }
        }

        /// <summary>
        /// The OAuth Prompt needs to see the Invoke Activity in order to complete the login process.
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Name == "adaptiveCard/action")
            {

                if (turnContext.Activity.Value == null)
                    return null;

                JObject value = JsonConvert.DeserializeObject<JObject>(turnContext.Activity.Value.ToString());

                if (value["action"] == null)
                    return null;

                JObject actiondata = JsonConvert.DeserializeObject<JObject>(value["action"].ToString());

                if (actiondata["verb"] == null)
                    return null;

                //When adaptiveCard/action invoke activity from teams contains sso token
                string verb = actiondata["verb"].ToString();
                JObject authentication = null;
                if (value["authentication"] != null)
                {
                    authentication = JsonConvert.DeserializeObject<JObject>(value["authentication"].ToString());
                }
                
                string state = null;
                if (value["state"] != null)
                {
                    //when token is absent in the invoke. We can initiate SSO in response to the invoke
                    state = value["state"].ToString();
                }
                // authToken and state are absent, handle verb
                if (authentication == null && state == null)
                {
                    switch (verb)
                    {
                        case "initiateSSO":
                            return await initiateSSOAsync(turnContext, cancellationToken);
                    }
                }
                else
                {
                    return await createAdaptiveCardInvokeResponseAsync(authentication, state, turnContext, cancellationToken);

                }
            }

            return null;
        }

        /// <summary>
        /// Authentication success.
        /// AuthToken or state is present. Verify token/state in invoke payload and return AC response
        /// </summary>
        /// <param name="authentication"></param>
        /// <param name="state"></param>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="isBasicRefresh"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private async Task<InvokeResponse> createAdaptiveCardInvokeResponseAsync(JObject authentication, string state, ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken, bool isBasicRefresh = false, string fileName = "adaptiveCardResponseJson.json")
        {
            //verify token is present or not

            bool isTokenPresent = authentication != null ? true : false;
            bool isStatePresent = state != null && state != "" ? true : false;

            // TODO : Use token or state to perform operation on behalf of user

            string[] filepath = { ".", "Resources", fileName };

            var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);
            var authResultData = isTokenPresent ? "SSO success" : isStatePresent ? "OAuth success" : "SSO/OAuth failed";
            if (isBasicRefresh)
            {
                authResultData = "Refresh done";
            }
            // Pull in the data from the Microsoft Graph.
            string token = authentication["token"].ToString();
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            var tokenResource = await userTokenClient.ExchangeTokenAsync(turnContext.Activity.From.Id, _connectionName, turnContext.Activity.ChannelId, new TokenExchangeRequest(null, token), cancellationToken).ConfigureAwait(false);
            var client = new SimpleGraphClient(tokenResource.Token);
            var me = await client.GetMeAsync();
            var title = !string.IsNullOrEmpty(me.JobTitle) ?
                                me.JobTitle : "Unknown";

            var payloadData = new
            {
                authResult = authResultData,
                UserName = me.DisplayName,
                UserTitle = title
            };

            var cardJsonstring = template.Expand(payloadData);

            var adaptiveCardResponse = new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = AdaptiveCard.ContentType,
                Value = JsonConvert.DeserializeObject(cardJsonstring)
            };
            return CreateInvokeResponse(adaptiveCardResponse);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<InvokeResponse> initiateSSOAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var signInLink = await GetSignInLinkAsync(turnContext, cancellationToken).ConfigureAwait(false);
            var oAuthCard = new OAuthCard
            {
                Text = "Signin Text",
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
                            Title = "Please sign in",
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
        /// <param name="filepath"></param>
        /// <param name="signInLink"></param>
        /// <param name="name"></param>
        /// <param name="userMRI"></param>
        /// <returns></returns>
        private Attachment GetFirstOptionsAdaptiveCard(string[] filepath, string signInLink, string name = null, string userMRI = null)
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);
            var payloadData = new
            {
                createdById = userMRI,
                createdBy = name
            };
            var cardJsonstring = template.Expand(payloadData);
            var card = JsonConvert.DeserializeObject<JObject>(cardJsonstring);
            if (card["authentication"] != null && card["authentication"]["buttons"] != null && card["authentication"]["buttons"][0] != null)
            {
                card["authentication"]["buttons"][0]["value"] = signInLink;
            }
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card,
            };

            return adaptiveCardAttachment;
        }
    }
}