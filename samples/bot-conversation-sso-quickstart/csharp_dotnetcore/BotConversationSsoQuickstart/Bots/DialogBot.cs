// <copyright file="DialogBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.Bot.Connector.Authentication;
using AdaptiveCards.Templating;
using AdaptiveCards;
using Microsoft.Bot.Schema.Teams;
using System;
using System.Collections.Generic;

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
        private readonly string _connectionName = "UniversalAdaptiveCardsConnection"; //only testing perforce

        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
        {
            _conversationState = conversationState;
            _userState = userState;
            _dialog = dialog;
            _logger = logger;
        }

        private async Task<string> GetSignInLinkAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            var resource = await userTokenClient.GetSignInResourceAsync(_connectionName, turnContext.Activity as Activity, null, cancellationToken).ConfigureAwait(false);
            return resource.SignInLink;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

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
            else if (turnContext.Activity.Text.Contains("ABSSORefresh"))
            {
                string[] striPath = { ".", "Resources", "ABSSORefresh.json" };
                var varMember = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
                var varInitialAdaptiveCard = GetFirstOptionsAdaptiveCard(striPath, signInLink, turnContext.Activity.From.Name, varMember.Id);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(varInitialAdaptiveCard), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Please send 'login' for options"), cancellationToken);
            }
        }

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

                string verb = actiondata["verb"].ToString();
                JObject authentication = null;

                string state = null;
                if (value["state"] != null)
                {
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
                // authToken or state is present. Verify token/state in invoke payload and return AC response
                else
                {
                    return createAdaptiveCardInvokeResponseAsync(authentication, state);
                }
            }

            return null;
        }

        private async Task<InvokeResponse> initiateSSOAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var signInLink = await GetSignInLinkAsync(turnContext, cancellationToken).ConfigureAwait(false);
            var oAuthCard = new OAuthCard
            {
                Text = "Signin Text",
                ConnectionName = "newConnection",
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

        private InvokeResponse createAdaptiveCardInvokeResponseAsync(JObject authentication, string state, bool isBasicRefresh = false, string fileName = "adaptiveCardResponseJson.json")
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
            var payloadData = new
            {
                authResult = authResultData
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