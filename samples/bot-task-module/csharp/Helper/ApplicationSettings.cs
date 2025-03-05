// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using AdaptiveCards;
using System.Collections.Generic;
using System.Configuration;
using System.Web;

namespace Microsoft.Teams.Samples.TaskModule.Web.Helper
{
    /// <summary>
    /// Represents the application settings.
    /// </summary>
    public class ApplicationSettings
    {
        /// <summary>
        /// Gets or sets the base URL.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the Microsoft application ID.
        /// </summary>
        public string MicrosoftAppId { get; set; }
    }
}
