// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// MIT License
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using Microsoft.Teams.Samples.TaskModule.Web.Helper;
using Microsoft.Teams.Samples.TaskModule.Web.Models;
using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public async System.Threading.Tasks.Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async System.Threading.Tasks.Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = (Activity)await argument;
            var reply = message.CreateReply();

            ThumbnailCard card = GetTaskModuleOptions();
            Attachment adaptiveCard = GetTaskModuleOptionsAdaptiveCard();

            reply.Attachments.Add(card.ToAttachment());
            reply.Attachments.Add(adaptiveCard);

            await context.PostAsync(reply);
            context.Wait(MessageReceivedAsync);
        }

        private static Attachment GetTaskModuleOptionsAdaptiveCard()
        {
            var card = new AdaptiveCard()
            {
                Body = new List<AdaptiveElement>()
                    {
                        new AdaptiveTextBlock(){Text="Task Module Invocation from Adaptive Card",Weight=AdaptiveTextWeight.Bolder,Size=AdaptiveTextSize.Large}
                    },
                Actions = new List<AdaptiveAction>()
                {
                     new AdaptiveSubmitAction()
                    {
                        Title = TaskModuleUIConstants.YouTube.ButtonTitle,
                        Data = new AdaptiveCardValue<string>() { Data = TaskModuleUIConstants.YouTube.Id}
                    },
                      new AdaptiveSubmitAction()
                    {
                        Title = TaskModuleUIConstants.PowerApp.ButtonTitle,
                        Data = new AdaptiveCardValue<string>() { Data = TaskModuleUIConstants.PowerApp.Id}
                    },
                    new AdaptiveSubmitAction()
                    {
                        Title = TaskModuleUIConstants.CustomForm.ButtonTitle,
                        Data = new AdaptiveCardValue<string>() { Data = TaskModuleUIConstants.CustomForm.Id}
                    },
                    new AdaptiveSubmitAction()
                    {
                        Title = TaskModuleUIConstants.AdaptiveCard.ButtonTitle,
                        Data = new AdaptiveCardValue<string>() { Data = TaskModuleUIConstants.AdaptiveCard.Id }
                    },
                    new AdaptiveOpenUrlAction()
                    {
                        Title = "Task Module - Deeplink",
                        Url = new Uri(DeeplinkHelper.DeepLink)
                    }
               },
            };
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        private static ThumbnailCard GetTaskModuleOptions()
        {
            ThumbnailCard card = new ThumbnailCard();
            card.Title = "Task Module Invocation from Thumbnail Card";
            card.Buttons = new List<CardAction>();
            card.Buttons.Add(new CardAction("invoke", TaskModuleUIConstants.YouTube.ButtonTitle, null,
                new BotFrameworkCardValue<string>()
                {
                    Data = TaskModuleUIConstants.YouTube.Id
                }));
            card.Buttons.Add(new CardAction("invoke", TaskModuleUIConstants.PowerApp.ButtonTitle, null,
                new BotFrameworkCardValue<string>()
                {
                    Data = TaskModuleUIConstants.PowerApp.Id
                }));
            card.Buttons.Add(new CardAction("invoke", TaskModuleUIConstants.CustomForm.ButtonTitle, null,
                new BotFrameworkCardValue<string>()
                {
                    Data = TaskModuleUIConstants.CustomForm.Id
                }));
            card.Buttons.Add(new CardAction("invoke", TaskModuleUIConstants.AdaptiveCard.ButtonTitle, null,
                new BotFrameworkCardValue<string>()
                {
                    Data = TaskModuleUIConstants.AdaptiveCard.Id
                }));
            card.Buttons.Add(new CardAction("openUrl", "Task Module - Deeplink", null, DeeplinkHelper.DeepLink));
            return card;
        }
    }
}
