// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace Microsoft.Teams.Samples.TaskModule.Web.Helper
{
    /// <summary>
    /// Contains UI constants for task modules.
    /// </summary>
    public static class TaskModuleUIConstants
    {
        public static UIConstants YouTube { get; set; } =
            new UIConstants(1000, 700, "Microsoft Ignite 2018 Vision Keynote", TaskModuleIds.YouTube, "YouTube");
        public static UIConstants PowerApp { get; set; } =
            new UIConstants(720, 520, "PowerApp: Asset Checkout", TaskModuleIds.PowerApp, "Power App");
        public static UIConstants CustomForm { get; set; } =
            new UIConstants(510, 450, "Custom Form", TaskModuleIds.CustomForm, "Custom Form");
        public static UIConstants AdaptiveCard { get; set; } =
            new UIConstants(700, 500, "Adaptive Card: Inputs", TaskModuleIds.AdaptiveCard, "Adaptive Card");
    }

    /// <summary>
    /// Represents UI constants for a task module.
    /// </summary>
    public class UIConstants
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UIConstants"/> class.
        /// </summary>
        /// <param name="width">The width of the task module.</param>
        /// <param name="height">The height of the task module.</param>
        /// <param name="title">The title of the task module.</param>
        /// <param name="id">The ID of the task module.</param>
        /// <param name="buttonTitle">The button title of the task module.</param>
        public UIConstants(int width, int height, string title, string id, string buttonTitle)
        {
            Width = width;
            Height = height;
            Title = title;
            Id = id;
            ButtonTitle = buttonTitle;
        }

        /// <summary>
        /// Gets or sets the height of the task module.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the width of the task module.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the title of the task module.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the button title of the task module.
        /// </summary>
        public string ButtonTitle { get; set; }

        /// <summary>
        /// Gets or sets the ID of the task module.
        /// </summary>
        public string Id { get; set; }
    }

    /// <summary>
    /// Contains task module IDs.
    /// </summary>
    public static class TaskModuleIds
    {
        public const string YouTube = "youtube";
        public const string PowerApp = "powerapp";
        public const string CustomForm = "customform";
        public const string AdaptiveCard = "adaptivecard";
    }
}