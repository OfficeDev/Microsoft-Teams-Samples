// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Cards;
using Microsoft.Teams.Common;

namespace Microsoft.Teams.Samples.BotCards.Handlers;

public static class Cards
{
    // Null properties must be dropped, otherwise empty schema values such as "requires" break card rendering in Teams
    private static readonly JsonSerializerOptions CardSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // Adaptive Card with various actions
    public static MessageActivityInput CreateAdaptiveCardActionsActivity()
    {
        var adaptiveCard = new AdaptiveCard
        {
            Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
            Body = new List<CardElement>
            {
                new TextBlock("Adaptive Card Actions")
            },
            Actions = new List<Teams.Cards.Action>
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
                        Actions = new List<Teams.Cards.Action>
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
                        Actions = new List<Teams.Cards.Action>
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

        return CreateCardActivity(adaptiveCard);
    }

    // Toggle Visibility Card
    public static MessageActivityInput CreateToggleVisibilityActivity()
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
            Actions = new List<Teams.Cards.Action>
            {
                new ToggleVisibilityAction
                {
                    Title = "Click me!",
                    TargetElements = new Union<IList<string>, IList<TargetElement>>(new List<string> { "helloWorld" })
                }
            }
        };

        return CreateCardActivity(adaptiveCard);
    }

    // SDK 2.1 requires cards to be serialized to JsonElement before being attached to an activity
    private static MessageActivityInput CreateCardActivity(AdaptiveCard card)
    {
        JsonElement cardElement = JsonSerializer.SerializeToElement(card, CardSerializerOptions);
        return new MessageActivityInput().WithAdaptiveCardAttachment(cardElement);
    }
}

