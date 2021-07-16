// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Generated with Bot Builder V4 SDK Template for Visual Studio v4.14.0

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TabWithAdpativeCardFlow.Bots
{
    /// <summary>
    /// Bot Activity handler class
    /// </summary>
    public class ActivityBot : TeamsActivityHandler
    {
        /// <summary>
        /// Invoked when an invoke activity is received from the connector. Invoke activities can be used to communicate many different things.
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Name == "tab/fetch")
            {
                return Task.FromResult(CreateInvokeResponse(new TabResponse
                {
                    Tab = new TabResponsePayload
                    {
                        Type = "continue",
                        Value = new TabResponseCards
                        {
                            Cards = new List<TabResponseCard>
                            {
                                new TabResponseCard
                                {
                                    Card = GetAdaptiveCard1()
                                },
                                new TabResponseCard
                                {
                                    Card = GetAdaptiveCard2()
                                },
                            },
                        },
                    },
                }));
            }
            else if (turnContext.Activity.Name == "tab/submit")
            {
                return Task.FromResult(CreateInvokeResponse(new TabResponse
                {
                    Tab = new TabResponsePayload
                    {
                        Type = "continue",
                        Value = new TabResponseCards
                        {
                            Cards = new List<TabResponseCard>
                            {
                                new TabResponseCard
                                {
                                    Card = GetAdaptiveCard2()
                                },
                            },
                        },
                    },
                }));
            }

            return null;
        }

        /// <summary>
        /// Sample Adaptive card
        /// </summary>
        /// <returns></returns>
        private AdaptiveCard GetAdaptiveCard1()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Test the tab view with Adaptive card",
                        Weight = AdaptiveTextWeight.Bolder,
                    },
                },

                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Action 1",
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = "Action 2",
                    },
                },
            };

            return card;
        }

        /// <summary>
        /// Sample Adaptive card
        /// </summary>
        /// <returns></returns>
        private AdaptiveCard GetAdaptiveCard2()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveImage
                    {
                        Url = new Uri("https://cdn.vox-cdn.com/thumbor/Ndb49Uk3hjiquS041NDD0tPDPAs=/0x169:1423x914/fit-in/1200x630/cdn.vox-cdn.com/uploads/chorus_asset/file/7342855/microsoftteams.0.jpg"),
                        AltText = "AlternativeText",
                        PixelHeight = 300,
                        PixelWidth = 400,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "tab/fetch is the first invoke request that your bot receives when a user opens an Adaptive Card tab. When your bot receives the request, it either sends a tab continue response or a tab auth response",
                        Wrap = true,
                        Spacing = AdaptiveSpacing.Medium,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "tab/submit request is triggered to your bot with the corresponding data through the Action.Submit function of Adaptive Card",
                        Spacing = AdaptiveSpacing.Medium,
                        Wrap = true,
                    },
                },

                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Click to submit",
                    },
                },
            };

            return card;
        }
    }
}
