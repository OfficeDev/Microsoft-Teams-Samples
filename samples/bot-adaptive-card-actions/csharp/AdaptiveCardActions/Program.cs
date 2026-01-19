// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCardActions;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Cards;
using Microsoft.Teams.Api;
using Microsoft.Teams.Api.Cards;
using Microsoft.Teams.Common;

// Create web application builder and load configuration
var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration.Get<ConfigOptions>();

// Create Teams app builder
var appBuilder = Microsoft.Teams.Apps.App.Builder();

// Configure Teams services
builder.AddTeams(appBuilder);

// Build the application
var app = builder.Build();
var teams = app.UseTeams();
const string CommandString = "Please use one of these commands: **Card Actions** for Adaptive Card Actions, **Suggested Actions** for Bot Suggested Actions and **ToggleVisibility** for Action ToggleVisible Card";

// Handle new members added
teams.OnMembersAdded(async context =>
{
    var welcomeText = "Hello and Welcome!";    
    foreach (var member in context.Activity.MembersAdded)
    {
        if (member.Id != context.Activity.Recipient?.Id)
        {
            await context.Send(welcomeText);
            await context.Send(CommandString);
        }
    }
});

// Handle incoming messages
teams.OnMessage(async context =>
{
    context.Log.Info("Message received");
    var activity = context.Activity;
    if (activity.Value != null)
    {
        context.Log.Info($"Data submitted: {activity.Value}");
        await context.Send($"Data Submitted: {activity.Value}");
        return;
    }
    if (activity.Text != null)
    {
        var normalizedText = activity.Text.Trim().ToLowerInvariant();

        if (normalizedText.Contains("card actions"))
        {
            await SendAdaptiveCardActionsAsync(context);
        }
        else if (normalizedText.Contains("suggested actions"))
        {
            await context.Send("Please Enter a color from the suggested action choices");
            await SendSuggestedActionsCardAsync(context);
            await SendSuggestedActionsAsync(context);
        }
        else if (normalizedText.Contains("togglevisibility"))
        {
            await SendToggleVisibilityCardAsync(context);
        }
        else if (normalizedText.Contains("red") || normalizedText.Contains("blue") || normalizedText.Contains("yellow"))
        {
            var responseText = ProcessInput(normalizedText);
            await context.Send(responseText);
            await SendSuggestedActionsAsync(context);
        }
        else
        {
            await context.Send(CommandString);
        }
    }
});

// Run the application
app.Run();

// Helper methods
static string ProcessInput(string text)
{
    const string colorText = "is the best color, I agree.";
    var colorResponses = new Dictionary<string, string>
    {
        { "red", $"Red {colorText}" },
        { "yellow", $"Yellow {colorText}" },
        { "blue", $"Blue {colorText}" }
    };
    return colorResponses.TryGetValue(text, out var response) ? response : "Please select a color from the suggested action choices";
}

static async Task SendAdaptiveCardActionsAsync(IContext<MessageActivity> context)
{
    // Build nested card
    var nestedCard = new AdaptiveCard
    {
        Body = new List<CardElement>
        {
            new TextBlock("Welcome To New Card")
        },
        Actions = new List<Microsoft.Teams.Cards.Action>
        {
            new SubmitAction
            {
                Title = "Click Me",
                Data = new Union<string, SubmitActionData>("{\"value\": \"Button has Clicked\"}")
            }
        }
    };

    // Build middle card
    var showCard = new AdaptiveCard
    {
        Body = new List<CardElement>
        {
            new TextBlock("This card's action will show another card")
        },
        Actions = new List<Microsoft.Teams.Cards.Action>
        {
            new ShowCardAction
            {
                Title = "Action.ShowCard",
                Card = nestedCard
            }
        }
    };

    // Build submit card
    var submitCard = new AdaptiveCard
    {
        Version = new Microsoft.Teams.Cards.Version("1.5"),
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
            new SubmitAction { Title = "Submit" }
        }
    };

    // Build main card
    var card = new AdaptiveCard
    {
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
                Card = submitCard
            },
            new ShowCardAction
            {
                Title = "Action ShowCard",
                Card = showCard
            }
        }
    };
    await context.Send(card);
}

static async Task SendSuggestedActionsCardAsync(IContext<MessageActivity> context)
{
    var card = new AdaptiveCard
    {
        Body = new List<CardElement>
        {
            new TextBlock("**Welcome to bot Suggested actions**"),
            new TextBlock("please use below commands, to get response form the bot."),
            new TextBlock("- Red \r- Blue \r - Yellow")
            {
                Wrap = true
            }
        }
    };
    await context.Send(card);
}

static async Task SendToggleVisibilityCardAsync(IContext<MessageActivity> context)
{
    var card = new AdaptiveCard
    {
        Body = new List<CardElement>
        {
            new TextBlock("**Action.ToggleVisibility example**: click the button to show or hide a welcome message"),
            new TextBlock("**Hello World!**")
            {
                Id = "helloWorld",
                IsVisible = false,
                Size = new TextSize("extraLarge")
            }
        },
        Actions = new List<Microsoft.Teams.Cards.Action>
        {
            new ToggleVisibilityAction
            {
                Title = "Click me!",
                TargetElements = new Union<IList<string>, IList<TargetElement>>((IList<string>)new List<string> { "helloWorld" })
            }
        }
    };
    await context.Send(card);
}

static async Task SendSuggestedActionsAsync(IContext<MessageActivity> context)
{
    var message = new MessageActivity()
    {
        Text = "What is your favorite color?",
        SuggestedActions = new Microsoft.Teams.Api.SuggestedActions()
            .AddAction(new Microsoft.Teams.Api.Cards.Action(ActionType.IMBack)
            {
                Title = "Red",
                Value = "Red"
            })
            .AddAction(new Microsoft.Teams.Api.Cards.Action(ActionType.IMBack)
            {
                Title = "Yellow",
                Value = "Yellow"
            })
            .AddAction(new Microsoft.Teams.Api.Cards.Action(ActionType.IMBack)
            {
                Title = "Blue",
                Value = "Blue"
            })
    };
    await context.Send(message);
}