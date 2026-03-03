// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Teams.Cards;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Api;
using Microsoft.Teams.Common;
using Microsoft.Teams.Apps;

namespace Microsoft.Teams.Samples.BotCards.Handlers;

public static class Cards
{
    // Send Adaptive Card with various actions
    public static async Task SendAdaptiveCardActions<T>(IContext<T> context) where T : IActivity
    {
        var adaptiveCard = new AdaptiveCard
        {
            Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
            Body = new List<CardElement>
            {
                new TextBlock("Adaptive Card Actions")
            },
            Actions = new List<Microsoft.Teams.Cards.Action>
            {
                new OpenUrlAction("https://adaptivecards.io")
                {
                    Title = "Action Open URL"
                },
                new ShowCardAction
                {
                    Title = "Action Submit",
                    Card = new AdaptiveCard
                    {
                        Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
                        Body = new List<CardElement>
                        {
                            new TextInput
                            {
                                Id = "name",
                                Label = "Please enter your name:",
                                IsRequired = true,
                                ErrorMessage = "Name is required"
                            }
                        },
                        Actions = new List<Microsoft.Teams.Cards.Action>
                        {
                            new ExecuteAction
                            {
                                Title = "Submit",
                                AssociatedInputs = AssociatedInputs.Auto,
                                Data = new Union<string, SubmitActionData>(new SubmitActionData
                                {
                                    NonSchemaProperties = new Dictionary<string, object?>
                                    {
                                        { "action", "submit_name" }
                                    }
                                })
                            }
                        }
                    }
                },
                new ShowCardAction
                {
                    Title = "Action ShowCard",
                    Card = new AdaptiveCard
                    {
                        Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
                        Body = new List<CardElement>
                        {
                            new TextBlock("This card's action will show another card")
                        },
                        Actions = new List<Microsoft.Teams.Cards.Action>
                        {
                            new ShowCardAction
                            {
                                Title = "Action.ShowCard",
                                Card = new AdaptiveCard
                                {
                                    Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
                                    Body = new List<CardElement>
                                    {
                                        new TextBlock("**Welcome To Your New Card**"),
                                        new TextBlock("This is your new card inside another card")
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        await context.Send(adaptiveCard);
    }

    // Send Toggle Visibility Card
    public static async Task SendToggleVisibilityCard<T>(IContext<T> context) where T : IActivity
    {
        var adaptiveCard = new AdaptiveCard
        {
            Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
            Body = new List<CardElement>
            {
                new TextBlock("Click to show or hide the message"),
                new TextBlock("**Hello World!**")
                {
                    Id = "helloWorld",
                    IsVisible = false,
                    Size = TextSize.ExtraLarge
                }
            },
            Actions = new List<Microsoft.Teams.Cards.Action>
            {
                new ToggleVisibilityAction
                {
                    Title = "Click me!",
                    TargetElements = new Union<IList<string>, IList<TargetElement>>(new List<string> { "helloWorld" })
                }
            }
        };

        await context.Send(adaptiveCard);
    }
}
