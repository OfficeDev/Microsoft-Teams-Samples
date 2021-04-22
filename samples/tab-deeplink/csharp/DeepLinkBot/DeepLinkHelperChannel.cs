using Microsoft.Extensions.Configuration;
using System;
using System.Text;
using System.Web;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class DeepLinkHelperChannel
    {
        public static IConfiguration _configuration;
        public DeepLinkHelperChannel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
       
        public static string channelID = DeepLinkBot.channelID;
        public static string entityId = "DeepLinkApp";
        static string task1DeepLinkURL = GetDeepLinkToTabTask1(_configuration["MicrosoftAppId"],channelID, entityId);

        public static string Task1Deeplink { get; set; } = task1DeepLinkURL;

        static string task2DeepLinkURL = GetDeepLinkToTabTask2(_configuration["MicrosoftAppId"], channelID, entityId);
        public static string Task2Deeplink { get; set; } = task2DeepLinkURL;

        static string task3DeepLinkURL = GetDeepLinkToTabTask3(_configuration["MicrosoftAppId"], channelID, entityId);
        public static string Task3Deeplink { get; set; } = task3DeepLinkURL;


        public static string GetDeepLinkToTabTask1(string appID, string channelID, string entityId)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"subEntityId\":\"bot1\",");
            sb.Append("\"channelId\":\"" + channelID + "\"");
            sb.Append("}");
            string channelContext = sb.ToString();
            string deepLinkURL = $"https://teams.microsoft.com/l/entity/" + appID + "/" + entityId + "?webUrl={HttpUtility.UrlEncode(" + _configuration["BaseURL"]+"/DeepLinkChannel" + ")}&label=Topic1&context=";
            string channelDeepLink = deepLinkURL + HttpUtility.UrlEncode(channelContext);
            return channelDeepLink;
        }
        public static string GetDeepLinkToTabTask2(string appID, string channelID, string entityId)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"subEntityId\":\"bot2\",");
            sb.Append("\"channelId\":\"" + channelID + "\"");
            sb.Append("}");
            string channelContext = sb.ToString();
            string deepLinkURL = $"https://teams.microsoft.com/l/entity/" + appID + "/" + entityId + "?webUrl={HttpUtility.UrlEncode(" + _configuration["BaseURL"]+"/DeepLinkChannel" + ")}&label=Topic1&context=";
            string channelDeepLink = deepLinkURL + HttpUtility.UrlEncode(channelContext);
            return channelDeepLink;
        }
        public static string GetDeepLinkToTabTask3(string appID, string channelID, string entityId)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"subEntityId\":\"bot3\",");
            sb.Append("\"channelId\":\"" + channelID + "\"");
            sb.Append("}");
            string channelContext = sb.ToString();
            string deepLinkURL = $"https://teams.microsoft.com/l/entity/" + appID + "/" + entityId + "?webUrl={HttpUtility.UrlEncode(" + _configuration["BaseURL"]+"/DeepLinkChannel" + ")}&label=Topic1&context=";
            string channelDeepLink = deepLinkURL + HttpUtility.UrlEncode(channelContext);
            return channelDeepLink;
        }
    }
}
