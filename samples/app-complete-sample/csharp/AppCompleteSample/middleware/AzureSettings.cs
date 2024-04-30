// <copyright file="AzureSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace AppCompleteSample.Utility
{
    public class AzureSettings
    {
        /// <summary>
        /// Gets or sets BotId.
        /// </summary>
        public string BotId { get; set; }

        /// <summary>
        /// Gets or sets Microsoft MicrosoftAppId.
        /// </summary>
        public string MicrosoftAppId { get; set; }

        /// <summary>
        /// Gets or sets Microsoft MicrosoftAppPassword.
        /// </summary>
        public string MicrosoftAppPassword { get; set; }

        /// <summary>
        /// Gets or sets Microsoft BaseUri.
        /// </summary>
        public string BaseUri { get; set; }

        /// <summary>
        /// Gets or sets Microsoft FBConnectionName.
        /// </summary>
        public string FBConnectionName { get; set; }

        /// <summary>
        /// Gets or sets Microsoft FBProfileUrl.
        /// </summary>
        public string FBProfileUrl { get; set; }

        /// <summary>
        /// Gets or sets Microsoft MaxComposeExtensionHistoryCount.
        /// </summary>
        public int MaxComposeExtensionHistoryCount { get; set; }

    }
}
