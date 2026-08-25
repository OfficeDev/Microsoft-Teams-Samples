// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Schema;
using Microsoft.Teams.Apps.TaskModules;
using Microsoft.Teams.Cards;
using Microsoft.Teams.Common;
using Action = Microsoft.Teams.Cards.Action;
using TaskModuleTask = Microsoft.Teams.Apps.TaskModules.Response;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTeamsBotApplication();

var webApp = builder.Build();
var teams = webApp.UseTeamsBotApplication();

webApp.MapGet("/customform", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(Path.Combine(builder.Environment.ContentRootPath, "pages", "CustomForm", "index.html"));
});

// Must be the publicly reachable tunnel URL for the webpage dialog to render in Teams.
var botEndpoint = builder.Configuration["BotEndpoint"] is { Length: > 0 } configuredEndpoint
    ? configuredEndpoint
    : "http://localhost:3978";

teams.OnMessage(async (context, cancellationToken) =>
{
    var card = new AdaptiveCard
    {
        Body = new List<CardElement>
        {
            new TextBlock("Task Module Invocation from Adaptive Card") { Weight = TextWeight.Bolder, Size = TextSize.Large }
        },
        Actions = new List<Action>
        {
            CreateTaskFetchAction("Adaptive Card", "adaptive_card"),
            CreateTaskFetchAction("Custom Form", "custom_form"),
            CreateTaskFetchAction("Multi-step Form", "multi_step_form")
        }
    };

    await context.SendAsync(
        new MessageActivityInput().WithAdaptiveCardAttachment(ToCardJson(card)),
        cancellationToken);
});

teams.OnTaskFetch((context, cancellationToken) =>
{
    var data = context.Activity.Value?.Data as JsonElement?;
    var dialogType = data?.TryGetProperty("opendialogtype", out var dialogTypeElement) == true && dialogTypeElement.ValueKind == JsonValueKind.String
        ? dialogTypeElement.GetString()
        : null;

    return Task.FromResult(dialogType switch
    {
        "adaptive_card" => CreateAdaptiveCardDialog(),
        "custom_form" => CreateCustomFormDialog(botEndpoint),
        "multi_step_form" => CreateMultiStepFormDialog(),
        _ => TaskModuleResponse.CreateBuilder()
            .WithType(TaskModuleResponseTypes.Message)
            .WithMessage("Unknown dialog type")
            .Build()
    });
});

teams.OnTaskSubmit(async (context, cancellationToken) =>
{
    var data = context.Activity.Value?.Data as JsonElement?;
    if (data == null)
    {
        return TaskModuleResponse.CreateBuilder()
            .WithType(TaskModuleResponseTypes.Message)
            .WithMessage("No data found in the activity value")
            .Build();
    }

    string? GetFormValue(string key)
        => data.Value.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    switch (GetFormValue("submissiondialogtype"))
    {
        case "multi_step_1":
            return CreateMultiStepFormStep2Dialog(GetFormValue("name") ?? "Unknown");

        case "multi_step_2":
            await context.SendAsync($"Hi {GetFormValue("name")}, thanks for submitting! Your email is {GetFormValue("email")}", cancellationToken);
            return TaskModuleResponse.CreateBuilder()
                .WithType(TaskModuleResponseTypes.Message)
                .WithMessage("Multi-step form completed!")
                .Build();

        case "custom_form":
            await context.SendAsync($"Hi {GetFormValue("name")}, thanks for submitting! Your email is {GetFormValue("email")}", cancellationToken);
            return TaskModuleResponse.CreateBuilder()
                .WithType(TaskModuleResponseTypes.Message)
                .WithMessage("Form submitted successfully")
                .Build();

        default:
            await context.SendAsync($"You submitted: {GetFormValue("usertext")}", cancellationToken);
            return TaskModuleResponse.CreateBuilder()
                .WithType(TaskModuleResponseTypes.Message)
                .WithMessage("Thanks for submitting!")
                .Build();
    }
});

webApp.Run();

static SubmitAction CreateTaskFetchAction(string title, string dialogType) => new()
{
    Title = title,
    Data = new Union<string, SubmitActionData>(new SubmitActionData
    {
        Msteams = new TaskFetchSubmitActionData(),
        NonSchemaProperties = new Dictionary<string, object?> { { "opendialogtype", dialogType } }
    })
};

static SubmitAction CreateSubmitAction(string title, IDictionary<string, object?> data) => new()
{
    Title = title,
    Data = new Union<string, SubmitActionData>(new SubmitActionData { NonSchemaProperties = data })
};

static InvokeResponse<TaskModuleResponse> CreateAdaptiveCardDialog()
{
    var card = new AdaptiveCard
    {
        Body = new List<CardElement>
        {
            new TextBlock("Enter Text Here") { Weight = TextWeight.Bolder },
            new TextInput { Id = "usertext", Placeholder = "add some text and submit", IsMultiline = true }
        },
        Actions = new List<Action>
        {
            CreateSubmitAction("Submit", new Dictionary<string, object?> { { "submissiondialogtype", "adaptive_card" } })
        }
    };

    return BuildCardDialog("Adaptive Card: Inputs", card, height: 200, width: 400);
}

// The TaskModuleResponse builder supports card dialogs only, so URL dialogs are built manually.
static InvokeResponse<TaskModuleResponse> CreateCustomFormDialog(string botEndpoint)
    => new(200, new TaskModuleResponse
    {
        Task = new TaskModuleTask
        {
            Type = TaskModuleResponseTypes.Continue,
            Value = new
            {
                title = "Custom Form",
                url = $"{botEndpoint}/customform",
                fallbackUrl = $"{botEndpoint}/customform",
                height = 450,
                width = 510
            }
        }
    });

static InvokeResponse<TaskModuleResponse> CreateMultiStepFormDialog()
{
    var card = new AdaptiveCard
    {
        Body = new List<CardElement>
        {
            new TextBlock("Step 1 of 2 - Your Name") { Size = TextSize.Large, Weight = TextWeight.Bolder },
            new TextInput { Id = "name", Label = "Name", Placeholder = "Enter your name", IsRequired = true }
        },
        Actions = new List<Action>
        {
            CreateSubmitAction("Next", new Dictionary<string, object?> { { "submissiondialogtype", "multi_step_1" } })
        }
    };

    return BuildCardDialog("Multi-step Form", card, height: 300, width: 400);
}

static InvokeResponse<TaskModuleResponse> CreateMultiStepFormStep2Dialog(string name)
{
    var card = new AdaptiveCard
    {
        Body = new List<CardElement>
        {
            new TextBlock("Step 2 of 2 - Your Email") { Size = TextSize.Large, Weight = TextWeight.Bolder },
            new TextInput { Id = "email", Label = "Email", Placeholder = "Enter your email", IsRequired = true }
        },
        Actions = new List<Action>
        {
            CreateSubmitAction("Submit", new Dictionary<string, object?> { { "submissiondialogtype", "multi_step_2" }, { "name", name } })
        }
    };

    return BuildCardDialog("Multi-step Form: Step 2", card, height: 300, width: 400);
}

static InvokeResponse<TaskModuleResponse> BuildCardDialog(string title, AdaptiveCard card, int height, int width)
    => TaskModuleResponse.CreateBuilder()
        .WithType(TaskModuleResponseTypes.Continue)
        .WithTitle(title)
        .WithHeight(height)
        .WithWidth(width)
        .WithCard(TeamsAttachment.CreateBuilder().WithAdaptiveCard(ToCardJson(card)).Build())
        .Build();

// AdaptiveCard.Serialize() omits unset properties; JsonSerializer.SerializeToElement emits nulls that Teams rejects.
static JsonElement ToCardJson(AdaptiveCard card) => JsonDocument.Parse(card.Serialize()).RootElement;
