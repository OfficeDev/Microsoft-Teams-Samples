using System.Text;
using System.Web;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class DeepLinkHelperChannel
    {
        public static string channelID = DeepLinkBot.channelID;

        static string task1Values =>  task1Json();
        static string task1Json()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"subEntityId\":\"bot1\",");
            sb.Append("\"channelId\":\""+channelID+"\"");
            sb.Append("}");
            return sb.ToString();
        }
        
        public static string task1Context = HttpUtility.UrlEncode(task1Values);
        public static string Task1Deeplink { get; set; } =

          $"https://teams.microsoft.com/l/entity/MICROSOFT-APP-ID/DeepLinkApp?webUrl={HttpUtility.UrlEncode("BASE-URL/DeepLinkChannel")}&label=Topic1&context="+task1Context ;

        static string task2Values = task2Json();
        static string task2Json()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"subEntityId\":\"bot2\",");
            sb.Append("\"channelId\":\"" + channelID + "\"");
            sb.Append("}");
            return sb.ToString();
        }

        public static string task2Context = HttpUtility.UrlEncode(task2Values);
        public static string Task2Deeplink { get; set; } =
          $"https://teams.microsoft.com/l/entity/MICROSOFT-APP-ID/DeepLinkApp?webUrl={HttpUtility.UrlEncode("BASE-URL/DeepLinkChannel")}&label=Topic2&context=" + task2Context;

        static string task3Values = task3Json();
        static string task3Json()

        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"subEntityId\":\"bot3\",");
            sb.Append("\"channelId\":\"" + channelID + "\"");
            sb.Append("}");
            return sb.ToString();
        }
        public static string task3Context = HttpUtility.UrlEncode(task3Values);
        public static string Task3Deeplink { get; set; } =
         $"https://teams.microsoft.com/l/entity/MICROSOFT-APP-ID/DeepLinkApp?webUrl={HttpUtility.UrlEncode("BASE-URL/DeepLinkChannel")}&label=Topic3&context="+task3Context ;

    }
}
