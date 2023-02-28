// <copyright file="DeeplinkHelper.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class DeeplinkHelper
    {
        public string GetDeepLinkToTabTask(string teamsUrl, string appID,string entityId, string subEntityID)
        {
            Dictionary<string, string> task1Values = new Dictionary<string, string>
            {
                {"subEntityId",subEntityID }
            };
            string jsoncontext = JsonConvert.SerializeObject(task1Values);
            string taskContext = HttpUtility.UrlEncode(jsoncontext);
            string deepLinkURL = teamsUrl + appID + "/" + entityId + "?context=";
            string channelDeepLink = deepLinkURL + taskContext;
            return channelDeepLink;
        }

        public string GetDeepLinkToChannelTask(string teamsUrl, string appID,string baseUrl,string channelId, string entityId, string subEntityID)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"subEntityId\":\""+subEntityID+"\",");
            sb.Append("\"channelId\":\"" + channelId + "\"");
            sb.Append("}");
            string channelContext = sb.ToString();
            string encodedUrl = HttpUtility.UrlEncode(baseUrl + "/DeepLinkChannel");
            string taskContext = HttpUtility.UrlEncode(channelContext);
            string deepLinkURL = teamsUrl + appID + "/" + entityId + "?webUrl="+encodedUrl+"&label=Topic&context=";
            string channelDeepLink = deepLinkURL + taskContext;
            return channelDeepLink;
        }
    }
}