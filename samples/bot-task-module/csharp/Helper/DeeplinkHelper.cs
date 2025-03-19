// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System.Text.RegularExpressions;
using System.Web;

namespace Microsoft.Teams.Samples.TaskModule.Web.Helper
{
    public  class DeeplinkHelper
    {
        public  string DeepLink { get; set; }
        public  string DeepLinkToAdaptiveCard { get; set; }
        public DeeplinkHelper(string TeamsAppId ,string BaseUrl)
        {
            TeamsAppId = TeamsAppId.Replace('"', ' ').Trim();
            BaseUrl = BaseUrl.Replace('"', ' ').Trim();

            DeepLink = string.Format("https://teams.microsoft.com/l/task/{0}?url={1}&height={2}&width={3}&title={4}&completionBotId={5}",
              TeamsAppId,
              HttpUtility.UrlEncode(BaseUrl + "/customForm"),
              TaskModuleUIConstants.CustomForm.Height,
              TaskModuleUIConstants.CustomForm.Width,
              HttpUtility.UrlEncode(TaskModuleUIConstants.CustomForm.Title),
              TeamsAppId);
        }
    }
}