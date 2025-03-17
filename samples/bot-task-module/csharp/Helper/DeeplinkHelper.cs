// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System.Text.RegularExpressions;
using System.Web;

namespace Microsoft.Teams.Samples.TaskModule.Web.Helper
{
    /// <summary>
    /// Helper class for generating deep links.
    /// </summary>
    public class DeeplinkHelper
    {
        /// <summary>
        /// Gets the deep link.
        /// </summary>
        public string DeepLink { get; private set; }

        /// <summary>
        /// Gets the deep link to the adaptive card.
        /// </summary>
        public string DeepLinkToAdaptiveCard { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeeplinkHelper"/> class.
        /// </summary>
        /// <param name="microsoftAppId">The Microsoft application ID.</param>
        /// <param name="baseUrl">The base URL.</param>
        public DeeplinkHelper(string microsoftAppId, string baseUrl)
        {
            microsoftAppId = microsoftAppId.Replace('"', ' ').Trim();
            baseUrl = baseUrl.Replace('"', ' ').Trim();

            DeepLink = string.Format("https://teams.microsoft.com/l/task/{0}?url={1}&height={2}&width={3}&title={4}&completionBotId={5}",
              microsoftAppId,
              HttpUtility.UrlEncode(baseUrl + "/customForm"),
              TaskModuleUIConstants.CustomForm.Height,
              TaskModuleUIConstants.CustomForm.Width,
              HttpUtility.UrlEncode(TaskModuleUIConstants.CustomForm.Title),
              microsoftAppId);
        }
    }
}