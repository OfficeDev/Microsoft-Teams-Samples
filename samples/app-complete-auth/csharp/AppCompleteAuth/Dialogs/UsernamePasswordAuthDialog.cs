
using AdaptiveCards;
using AppCompleteAuth.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AppCompleteAuth.Dialogs
{
    public class UsernamePasswordAuthDialog : ComponentDialog
    {
        private readonly string _applicationBaseUrl;

        public UsernamePasswordAuthDialog(string applicationBaseUrl)
            : base(nameof(UsernamePasswordAuthDialog))
        {
            _applicationBaseUrl = applicationBaseUrl;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptStepAsync,
                LoginStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var value = JObject.FromObject(stepContext.Context.Activity.Value).ToString();
            if (value != "{}")
            {
                var asJobject = JObject.FromObject(stepContext.Context.Activity.Value);
                var state = (string)asJobject.ToObject<CardTaskFetchValue<string>>()?.State;
                if (state.ToString() == "CancelledByUser")
                {
                    await stepContext.Context.SendActivityAsync("Sign in cancelled by user");
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }
                else
                {
                    var cred = JObject.Parse(state);
                    var userName = (string)cred.ToObject<CardTaskFetchValue<string>>()?.UserName;
                    var password = (string)cred.ToObject<CardTaskFetchValue<string>>()?.Password;

                    if (userName == Constant.UserName && password == Constant.Password)
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login successful"), cancellationToken);
                        return await stepContext.PromptAsync(
                         nameof(TextPrompt),
                         new PromptOptions
                         {
                             Prompt = MessageFactory.Text("What is your preferred user name?"),
                         },
                         cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync("Invalid username or password");
                        return await stepContext.EndDialogAsync(null, cancellationToken);
                    }
                }
            }
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(GetPopUpSignInCard()), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userName = stepContext.Result as string;
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(GetProfileCard(userName)));
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

            // Get sign in card.
            private Attachment GetPopUpSignInCard()
        {
            var heroCard = new HeroCard
            {
                Title = "Sign in card",
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.Signin, "Sign in", value: _applicationBaseUrl + "/popUpSignin?from=bot&height=535&width=600"),
                }
            };

            return heroCard.ToAttachment();
        }

        // Get user profile card.
        private Microsoft.Bot.Schema.Attachment GetProfileCard(string userName)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"User Information",
                Size = AdaptiveTextSize.Default
            });

            card.Body.Add(new AdaptiveColumnSet()
            {
                Columns = new List<AdaptiveColumn>()
                {
                    new AdaptiveColumn()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveImage()
                            {
                                Url = new Uri("https://media.istockphoto.com/vectors/profile-placeholder-image-gray-silhouette-no-photo-vector-id1016744034?k=20&m=1016744034&s=612x612&w=0&h=kjCAwH5GOC3n3YRTHBaLDsLIuF8P3kkAJc9RvfiYWBY="),
                                Size = AdaptiveImageSize.Medium,
                                Style = AdaptiveImageStyle.Person
                            }
                        },
                        Width ="auto"
                    },
                    new AdaptiveColumn()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Text =  "Hello! Test user",
                                Weight = AdaptiveTextWeight.Bolder,
                                IsSubtle = true
                            }
                        },
                        Width ="stretch"
                    }
                }
            });
            card.Body.Add(new AdaptiveFactSet()
            {
                Separator = true,
                Facts =
                {
                    new AdaptiveFact
                    {
                        Title = "Job title :",
                        Value = "Data scientist"
                    },
                    new AdaptiveFact
                    {
                        Title = "Email :",
                        Value = "testaccount@test123.onmicrosoft.com"
                    },
                    new AdaptiveFact
                    {
                        Title = "User name:",
                        Value = userName
                    }
                }
            });

            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}
