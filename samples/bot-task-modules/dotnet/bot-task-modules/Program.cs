// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Api;
using Microsoft.Teams.Api.TaskModules;
using Microsoft.Teams.Cards;
using Microsoft.Teams.Common;
using System.Text.Json;
using BotTaskModules.Models;
using AdaptiveCardType = Microsoft.Teams.Cards.AdaptiveCard;

// Initialize Teams App
var builder = WebApplication.CreateBuilder(args);
builder.AddTeams();

var webApp = builder.Build();
var teamsApp = webApp.UseTeams(true);

// Serve static files from pages folder
webApp.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "pages")),
    RequestPath = ""
});

// Map routes for task module pages
webApp.MapGet("/customform", async context =>
{
    var filePath = Path.Combine(builder.Environment.ContentRootPath, "pages", "CustomForm", "index.html");
    await context.Response.SendFileAsync(filePath);
});

// Get base URL for task modules
var baseUrl = builder.Configuration["BaseUrl"];
if (string.IsNullOrEmpty(baseUrl))
{
    throw new InvalidOperationException("BaseUrl configuration is required. Please set it in appsettings.json or environment variables.");
}

// Handle incoming messages - send HeroCard and AdaptiveCard with task module options
teamsApp.OnMessage(async (IContext<MessageActivity> context) =>
{
    // Create hero card attachment
    var heroCard = CreateHeroCardAttachment();
    
    // Create adaptive card with task module options
    var adaptiveCard = CreateAdaptiveCardWithTaskModuleOptions();
    
    // Send both cards
    var heroMessage = new MessageActivity();
    heroMessage.Attachments = new List<Attachment> { heroCard };
    await context.Send(heroMessage);
    
    var adaptiveMessage = new MessageActivity();
    adaptiveMessage.Attachments = new List<Attachment>
    {
        new Attachment
        {
            ContentType = new ContentType("application/vnd.microsoft.card.adaptive"),
            Content = adaptiveCard
        }
    };
    await context.Send(adaptiveMessage);
});

// Handle task module fetch (dialog open)
teamsApp.OnActivity(ActivityType.Invoke, async (IContext<IActivity> context) =>
{
    var invokeActivity = context.Activity as InvokeActivity;
    var activityName = invokeActivity.Name;
    
    if (activityName == "task/fetch")
    {
        return await HandleTaskModuleFetch(invokeActivity, baseUrl);
    }
    else if (activityName == "task/submit")
    {
        return await HandleTaskModuleSubmit(context, invokeActivity);
    }
    
    return null;
});

// Create HeroCard with task module options
static Attachment CreateHeroCardAttachment()
{
    var heroCardJson = JsonSerializer.Serialize(new
    {
        title = "Task Module Invocation from Hero Card",
        buttons = new[]
        {
            new
            {
                type = "invoke",
                title = TaskModuleUIConstants.AdaptiveCard.ButtonTitle,
                value = new { type = "task/fetch", data = TaskModuleIds.AdaptiveCard }
            },
            new
            {
                type = "invoke",
                title = TaskModuleUIConstants.CustomForm.ButtonTitle,
                value = new { type = "task/fetch", data = TaskModuleIds.CustomForm }
            }
        }
    });
    
    return new Attachment
    {
        ContentType = new ContentType("application/vnd.microsoft.card.hero"),
        Content = JsonSerializer.Deserialize<JsonElement>(heroCardJson)
    };
}

// Create AdaptiveCard with task module options
static AdaptiveCardType CreateAdaptiveCardWithTaskModuleOptions()
{
    var card = new AdaptiveCardType
    {
        Body = new List<CardElement>
        {
            new TextBlock("Task Module Invocation from Adaptive Card")
            {
                Weight = TextWeight.Bolder,
                Size = TextSize.Large
            }
        },
        Actions = new List<Microsoft.Teams.Cards.Action>
        {
            new TaskFetchAction(new Dictionary<string, object?> { { "data", TaskModuleIds.AdaptiveCard } })
            {
                Title = TaskModuleUIConstants.AdaptiveCard.ButtonTitle
            },
            new TaskFetchAction(new Dictionary<string, object?> { { "data", TaskModuleIds.CustomForm } })
            {
                Title = TaskModuleUIConstants.CustomForm.ButtonTitle
            }
        }
    };
    
    return card;
}

// Create AdaptiveCard to be shown in task module
static AdaptiveCardType CreateAdaptiveCardForTaskModule()
{
    var card = new AdaptiveCardType
    {
        Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
        Body = new List<CardElement>
        {
            new TextBlock("Enter Text Here")
            {
                Weight = TextWeight.Bolder
            },
            new TextInput
            {
                Id = "UserText",
                Placeholder = "Add some text and submit",
                IsMultiline = true
            }
        },
        Actions = new List<Microsoft.Teams.Cards.Action>
        {
            new SubmitAction
            {
                Title = "Submit"
            }
        }
    };
    
    return card;
}

// Handle task module fetch - called when user selects an option
static System.Threading.Tasks.Task<object> HandleTaskModuleFetch(InvokeActivity activity, string baseUrl)
{
    string? cardData = null;
    
    try
    {
        var activityJson = JsonSerializer.Serialize(activity);
        var activityElement = JsonSerializer.Deserialize<JsonElement>(activityJson);
        
        if (activityElement.TryGetProperty("value", out var valueElement))
        {
            if (valueElement.TryGetProperty("data", out var dataElement))
            {
                if (dataElement.TryGetProperty("data", out var taskModuleIdElement) && taskModuleIdElement.ValueKind == JsonValueKind.String)
                {
                    cardData = taskModuleIdElement.GetString();
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error extracting task module data: {ex.Message}");
    }
    
    TaskInfo taskInfo;
    
    if (cardData == TaskModuleIds.CustomForm)
    {
        taskInfo = new TaskInfo
        {
            Title = TaskModuleUIConstants.CustomForm.Title,
            Width = new Union<int, Microsoft.Teams.Api.TaskModules.Size>(TaskModuleUIConstants.CustomForm.Width),
            Height = new Union<int, Microsoft.Teams.Api.TaskModules.Size>(TaskModuleUIConstants.CustomForm.Height),
            Url = $"{baseUrl}/customform",
            FallbackUrl = $"{baseUrl}/customform"
        };
    }
    else  // Default to ADAPTIVE_CARD
    {
        taskInfo = new TaskInfo
        {
            Title = TaskModuleUIConstants.AdaptiveCard.Title,
            Width = new Union<int, Microsoft.Teams.Api.TaskModules.Size>(TaskModuleUIConstants.AdaptiveCard.Width),
            Height = new Union<int, Microsoft.Teams.Api.TaskModules.Size>(TaskModuleUIConstants.AdaptiveCard.Height),
            Card = new Attachment
            {
                ContentType = new ContentType("application/vnd.microsoft.card.adaptive"),
                Content = CreateAdaptiveCardForTaskModule()
            }
        };
    }
    
    // Return TaskModuleResponse with TaskModuleContinueResponse
    var response = new
    {
        task = new
        {
            type = "continue",
            value = taskInfo
        }
    };
    
    return System.Threading.Tasks.Task.FromResult<object>(response);
}

// Handle task module submit - called when data is being returned
static async System.Threading.Tasks.Task<object> HandleTaskModuleSubmit(IContext<IActivity> context, InvokeActivity activity)
{
    Dictionary<string, object>? data = null;
    
    try
    {
        var activityJson = JsonSerializer.Serialize(activity);
        var activityElement = JsonSerializer.Deserialize<JsonElement>(activityJson);
        
        if (activityElement.TryGetProperty("value", out var valueElement))
        {
            if (valueElement.TryGetProperty("data", out var dataElement))
            {
                if (dataElement.ValueKind == JsonValueKind.Object)
                {
                    data = JsonSerializer.Deserialize<Dictionary<string, object>>(dataElement.GetRawText());
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error extracting submit data: {ex.Message}");
    }
    
    // Build Adaptive Card to display the submitted data
    var bodyItems = new List<CardElement>
    {
        new TextBlock("Task Module Submission Received")
        {
            Size = TextSize.Large,
            Weight = TextWeight.Bolder
        }
    };
    
    // Add each field from the submitted data
    if (data != null && data.Count > 0)
    {
        foreach (var kvp in data)
        {
            // Skip password fields
            if (kvp.Key.ToLower().Contains("password"))
                continue;
            
            // Format the key (capitalize, replace underscores with spaces)
            var formattedKey = kvp.Key.Replace("_", " ");
            formattedKey = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formattedKey);
            bodyItems.Add(new TextBlock($"**{formattedKey}:** {kvp.Value}")
            {
                Wrap = true
            });
        }
    }
    else
    {
        bodyItems.Add(new TextBlock("No data submitted")
        {
            IsSubtle = true
        });
    }
    
    var resultCard = new AdaptiveCardType
    {
        Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
        Body = bodyItems
    };
    
    // Send the formatted card
    var message = new MessageActivity("Task module submission received");
    message.Attachments = new List<Attachment>
    {
        new Attachment
        {
            ContentType = new ContentType("application/vnd.microsoft.card.adaptive"),
            Content = resultCard
        }
    };
    await context.Send(message);
    
    // Return a message response
    var response = new
    {
        task = new
        {
            type = "message",
            value = "Thanks!"
        }
    };
    
    return response;
}

// Starts the Teams bot application and listens for incoming requests
webApp.Run();
