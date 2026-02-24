// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace BotTaskModules.Models;

// UI settings for task modules.
public class UISettings
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string Title { get; set; }
    public string Id { get; set; }
    public string ButtonTitle { get; set; }

    public UISettings(int width, int height, string title, string id, string buttonTitle)
    {
        Width = width;
        Height = height;
        Title = title;
        Id = id;
        ButtonTitle = buttonTitle;
    }
}

// Task module identifiers and settings.
public static class TaskModuleIds
{
    public const string CustomForm = "CustomForm";
    public const string AdaptiveCard = "AdaptiveCard";
}

// Task module UI constants.
public static class TaskModuleUIConstants
{
    public static readonly UISettings CustomForm = new(510, 450, "Custom Form", TaskModuleIds.CustomForm, "Custom Form");
    public static readonly UISettings AdaptiveCard = new(400, 200, "Adaptive Card: Inputs", TaskModuleIds.AdaptiveCard, "Adaptive Card");
}
