using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class DeeplinkHelper
    {
        static Dictionary<string, string> task1Values = new Dictionary<string, string>
            {
                {"subEntityId","topic1" }
            };
        static string jsoncontext1 = JsonConvert.SerializeObject(task1Values);
        public static string task1Context = HttpUtility.UrlEncode(jsoncontext1);

        public static string Task1Deeplink { get; set; } = 

          $"https://teams.microsoft.com/l/entity/MICROSOFT-APP-ID/com.contoso.DeeplLinkBot.help?context=" + task1Context;


        static Dictionary<string, string> task2Values = new Dictionary<string, string>
            {
                {"subEntityId","topic2" }
            };

        static string jsoncontext2 = JsonConvert.SerializeObject(task2Values);
        public static string task2Context = HttpUtility.UrlEncode(jsoncontext2);

        public static string Task2Deeplink { get; set; } =
          $"https://teams.microsoft.com/l/entity/MICROSOFT-APP-ID/com.contoso.DeeplLinkBot.help?context=" + task2Context;


        static Dictionary<string, string> task3Values = new Dictionary<string, string>
            {
                {"subEntityId","topic3" }
            };
        static string jsoncontext3 = JsonConvert.SerializeObject(task3Values);
        public static string task3Context = HttpUtility.UrlEncode(jsoncontext3);
        public static string Task3Deeplink { get; set; } =
         $"https://teams.microsoft.com/l/entity/MICROSOFT-APP-ID/com.contoso.DeeplLinkBot.help?context=" + task3Context;

    }
}
