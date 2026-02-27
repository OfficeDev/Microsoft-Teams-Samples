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

var builder = WebApplication.CreateBuilder(args);
builder.AddTeams();

var webApp = builder.Build();
var teamsApp = webApp.UseTeams(true);

webApp.MapGet("/customform", async context =>
{
    await context.Response.SendFileAsync(Path.Combine(builder.Environment.ContentRootPath, "pages", "CustomForm", "index.html"));
});

var baseUrl = builder.Configuration["BotEndpoint"];
if (string.IsNullOrEmpty(baseUrl))
    throw new InvalidOperationException("BotEndpoint configuration is required.");

teamsApp.OnMessage(async (IContext<MessageActivity> context) =>
{
    var card = new AdaptiveCard
    {
        Body = new List<CardElement>
        {
            new TextBlock("Task Module Invocation from Adaptive Card") { Weight = TextWeight.Bolder, Size = TextSize.Large }
        },
        Actions = new List<Microsoft.Teams.Cards.Action>
        {
            new TaskFetchAction(new Dictionary<string, object?> { { "data", "AdaptiveCard" } }) { Title = "Adaptive Card" },
            new TaskFetchAction(new Dictionary<string, object?> { { "data", "CustomForm" } }) { Title = "Custom Form" },
            new TaskFetchAction(new Dictionary<string, object?> { { "data", "MultiStep" } }) { Title = "Multi-step Form" }
        }
    };

    await context.Send(new MessageActivity 
    { 
        Attachments = new List<Attachment> 
        { 
            new Attachment { ContentType = new ContentType("application/vnd.microsoft.card.adaptive"), Content = card } 
        } 
    });
});

teamsApp.OnActivity(ActivityType.Invoke, async (IContext<IActivity> context) =>
{
    var activity = context.Activity as InvokeActivity;
    if (activity == null) return null;

    if (activity.Name == "task/fetch")
    {
        var json = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(activity));
        var data = json.GetProperty("value").GetProperty("data").GetProperty("data").GetString();

        TaskInfo taskInfo;

        if (data == "CustomForm")
        {
            taskInfo = new TaskInfo
            {
                Title = "Custom Form",
                Width = new Union<int, Microsoft.Teams.Api.TaskModules.Size>(510),
                Height = new Union<int, Microsoft.Teams.Api.TaskModules.Size>(450),
                Url = $"{baseUrl}/customform",
                FallbackUrl = $"{baseUrl}/customform"
            };
        }
        else if (data == "MultiStep")
        {
            var step1Card = new AdaptiveCard
            {
                Body = new List<CardElement>
                {
                    new TextBlock("Step 1 of 2 - Your Name") { Size = TextSize.Large, Weight = TextWeight.Bolder },
                    new TextInput { Id = "name", Label = "Name", Placeholder = "Enter your name", IsRequired = true }
                },
                Actions = new List<Microsoft.Teams.Cards.Action> { new SubmitAction { Title = "Next" } }
            };

            taskInfo = new TaskInfo
            {
                Title = "Multi-step Form",
                Width = new Union<int, Microsoft.Teams.Api.TaskModules.Size>(400),
                Height = new Union<int, Microsoft.Teams.Api.TaskModules.Size>(300),
                Card = new Attachment { ContentType = new ContentType("application/vnd.microsoft.card.adaptive"), Content = step1Card }
            };
        }
        else
        {
            var dialogCard = new AdaptiveCard
            {
                Body = new List<CardElement>
                {
                    new TextBlock("Enter Text Here") { Weight = TextWeight.Bolder },
                    new TextInput { Id = "UserText", Placeholder = "add some text and submit", IsMultiline = true }
                },
                Actions = new List<Microsoft.Teams.Cards.Action> { new SubmitAction { Title = "Submit" } }
            };

            taskInfo = new TaskInfo
            {
                Title = "Adaptive Card: Inputs",
                Width = new Union<int, Microsoft.Teams.Api.TaskModules.Size>(400),
                Height = new Union<int, Microsoft.Teams.Api.TaskModules.Size>(200),
                Card = new Attachment { ContentType = new ContentType("application/vnd.microsoft.card.adaptive"), Content = dialogCard }
            };
        }

        return new { task = new { type = "continue", value = taskInfo } };
    }

    if (activity.Name == "task/submit")
    {
        var json = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(activity));
        var submitData = JsonSerializer.Deserialize<Dictionary<string, object>>(json.GetProperty("value").GetProperty("data").GetRawText());

        if (submitData?.ContainsKey("name") == true && submitData?.ContainsKey("email") == false && submitData?.ContainsKey("UserText") == false)
        {
            var name = submitData["name"]?.ToString();
            var step2Card = new AdaptiveCard
            {
                Body = new List<CardElement>
                {
                    new TextBlock("Step 2 of 2 - Your Email") { Size = TextSize.Large, Weight = TextWeight.Bolder },
                    new TextInput { Id = "name", Value = name, IsVisible = false },
                    new TextInput { Id = "email", Label = "Email", Placeholder = "Enter your email", IsRequired = true }
                },
                Actions = new List<Microsoft.Teams.Cards.Action> { new SubmitAction { Title = "Submit" } }
            };

            return new 
            { 
                task = new 
                { 
                    type = "continue", 
                    value = new TaskInfo
                    {
                        Title = "Multi-step Form: Step 2",
                        Width = new Union<int, Microsoft.Teams.Api.TaskModules.Size>(400),
                        Height = new Union<int, Microsoft.Teams.Api.TaskModules.Size>(300),
                        Card = new Attachment { ContentType = new ContentType("application/vnd.microsoft.card.adaptive"), Content = step2Card }
                    }
                } 
            };
        }

        if (submitData?.ContainsKey("name") == true && submitData?.ContainsKey("email") == true)
        {
            await context.Send($"Hi {submitData["name"]}, thanks for submitting! Your email is {submitData["email"]}");
            return new { task = new { type = "message", value = "Multi-step form completed!" } };
        }

        if (submitData?.GetValueOrDefault("submissiontype")?.ToString() == "custom_form")
        {
            await context.Send($"Hi {submitData["name"]}, thanks for submitting! Your email is {submitData["email"]}");
            return new { task = new { type = "message", value = "Form submitted successfully" } };
        }

        var usertext = submitData?.GetValueOrDefault("UserText")?.ToString();
        await context.Send($"You submitted: {usertext}");
        return new { task = new { type = "message", value = "Thanks for submitting!" } };
    }

    return null;
});

webApp.Run();
