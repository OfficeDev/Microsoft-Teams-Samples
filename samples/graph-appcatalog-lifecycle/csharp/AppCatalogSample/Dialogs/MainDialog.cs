// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AppCatalogSample.Helper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace AppCatalogSample.Dialogs
{
    public class MainDialog : LogoutDialog
    {
        protected readonly ILogger Logger;
        public static List<CardData> taskInfoData = new List<CardData>();

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            Logger = logger;

            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = "Please Sign In",
                    Title = "Sign In",
                    Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
                }));

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptStepAsync,
                LoginStepAsync,
                CommandStepAsync,
                ProcessStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step. Note that we could also have gotten the
            // token directly from the prompt itself. There is an example of this in the next method.
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse?.Token != null)
            {
                var client = new AppCatalogHelper(tokenResponse.Token);
                await client.SendSuggestedActionsAsync(stepContext.Context, cancellationToken);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Type your actions") }, cancellationToken);
            }
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
        private async Task<DialogTurnResult> CommandStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["command"] = stepContext.Result;

            // Call the prompt again because we need the token. The reasons for this are:
            // 1. If the user is already logged in we do not need to store the token locally in the bot and worry
            // about refreshing it. We can always just call the prompt again to get the token.
            // 2. We never know how long it will take a user to respond. By the time the
            // user responds the token may have expired. The user would then be prompted to login again.
            //
            // There is no reason to store the token locally in the bot because we can always just call
            // the OAuth prompt to get the token or get a new token if needed.
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }
        private async Task<DialogTurnResult> ProcessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result != null)
            {
                // We do not need to store the token in the bot. When we need the token we can
                // send another prompt. If the token is valid the user will not need to log back in.
                // The token will be available in the Result property of the task.
                var tokenResponse = stepContext.Result as TokenResponse;

                // If we have the token use the user is authenticated so we may use it to make API calls.
                if (tokenResponse?.Token != null)
                {
                    string data = String.Empty;
                    IList<TeamsApp> teamsApps = null;
                    Microsoft.Bot.Schema.Attachment attachData = null;
                    string logintext = "User is not login.Type 'login' to proceed";
                    var command = ((string)stepContext.Values["command"] ?? string.Empty).ToLowerInvariant();
                    var client = new AppCatalogHelper(tokenResponse.Token);

                    switch (command)
                    {
                        case "listapp":
                            teamsApps = await client.GetAllapp();
                            if (teamsApps != null && teamsApps.Count > 0)
                            {
                                taskInfoData = client.ParseData(teamsApps);
                                attachData = client.AgendaAdaptiveList("listapp", taskInfoData);
                                taskInfoData.Clear();
                                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(attachData), cancellationToken);
                                await client.SendContinueMessageAsync(stepContext.Context, cancellationToken);
                            }
                            else
                            {
                                //login if not authenticated
                                await stepContext.Context.SendActivityAsync(MessageFactory.Text(logintext, logintext), cancellationToken);
                            }

                            break;
                        case "app":
                            teamsApps = await client.AppCatalogById();
                            if (teamsApps != null && teamsApps.Count > 0)
                            {
                                taskInfoData = client.ParseData(teamsApps);
                                attachData = client.AgendaAdaptiveList("App", taskInfoData);
                                taskInfoData.Clear();
                                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(attachData), cancellationToken);
                                await client.SendContinueMessageAsync(stepContext.Context, cancellationToken);
                            }
                            else
                            {
                                //login if not authenticated
                                await stepContext.Context.SendActivityAsync(MessageFactory.Text(logintext, logintext), cancellationToken);
                            }

                            break;
                        case "findapp":
                            teamsApps = await client.FindApplicationByTeamsId();
                            if (teamsApps != null && teamsApps.Count > 0)
                            {
                                taskInfoData = client.ParseData(teamsApps);
                                attachData = client.AgendaAdaptiveList("findapp", taskInfoData);
                                taskInfoData.Clear();
                                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(attachData), cancellationToken);
                                await client.SendContinueMessageAsync(stepContext.Context, cancellationToken);
                            }
                            else
                            {
                                //login if not authenticated
                                await stepContext.Context.SendActivityAsync(MessageFactory.Text(logintext, logintext), cancellationToken);
                            }


                            break;
                        case "status":
                            teamsApps = await client.AppStatus();
                            if (teamsApps != null && teamsApps.Count > 0)
                            {
                                taskInfoData = client.ParseData(teamsApps);
                                attachData = client.AgendaAdaptiveList("status", taskInfoData);
                                taskInfoData.Clear();
                                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(attachData), cancellationToken);
                                await client.SendContinueMessageAsync(stepContext.Context, cancellationToken);
                            }
                            else
                            {
                                //login if not authenticated
                                await stepContext.Context.SendActivityAsync(MessageFactory.Text(logintext, logintext), cancellationToken);
                            }

                            break;
                        case "bot":
                            teamsApps = await client.ListAppHavingBot();
                            if (teamsApps != null && teamsApps.Count > 0)
                            {
                                taskInfoData = client.ParseData(teamsApps);
                                attachData = client.AgendaAdaptiveList("bot", taskInfoData);
                                taskInfoData.Clear();
                                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(attachData), cancellationToken);
                                await client.SendContinueMessageAsync(stepContext.Context, cancellationToken);
                            }
                            else
                            {
                                //login if not authenticated
                                await stepContext.Context.SendActivityAsync(MessageFactory.Text(logintext, logintext), cancellationToken);
                            }

                            break;
                        case "update":
                            var upData = await client.UpdateFileAsync();
                            if (upData == "require login")
                                await stepContext.Context.SendActivityAsync(MessageFactory.Text(logintext, logintext), cancellationToken);
                            else
                            {
                                attachData = client.AdaptivCardList("Update", upData);
                                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(attachData), cancellationToken);
                                await client.SendContinueMessageAsync(stepContext.Context, cancellationToken);
                            }
                            break;
                        case "publish":
                            var pubData = await client.UploadFileAsync();
                            if (pubData == "require login")
                                await stepContext.Context.SendActivityAsync(MessageFactory.Text(logintext, logintext), cancellationToken);
                            else
                            {
                                attachData = client.AdaptivCardList("publish", pubData);
                                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(attachData), cancellationToken);
                                await client.SendContinueMessageAsync(stepContext.Context, cancellationToken);
                            }
                            break;

                        case "delete":
                            var delData = await client.DeleteApp();
                            if (delData != "Deleted")
                                await stepContext.Context.SendActivityAsync(MessageFactory.Text(logintext, logintext), cancellationToken);
                            else
                            {
                                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Delete app successfully"), cancellationToken);
                                await client.SendContinueMessageAsync(stepContext.Context, cancellationToken);
                            }
                            break;
                        case "list":
                            await client.SendContinueMessageAsync(stepContext.Context, cancellationToken);
                            break;
                        default:
                            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Your token is: {tokenResponse.Token}"), cancellationToken);
                            break;
                    }
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("We couldn't log you in. Please try again later."), cancellationToken);
                }
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("We couldn't log you in. Please try again later."), cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}