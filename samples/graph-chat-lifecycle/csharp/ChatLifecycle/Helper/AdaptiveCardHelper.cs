using AdaptiveCards;
using AdaptiveCards.Templating;
using ChatLifecycle.Controllers;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace ChatLifecycle.Helper
{
    /// <summary>
    ///  Helper class which posts to the saved channel every 20 seconds.
    /// </summary>
    public static class AdaptiveCardHelper
    {
       
        public static AdaptiveChoice adaptiveChoice1;
        public static AdaptiveChoice adaptiveChoice2;
        public static AdaptiveChoice adaptiveChoice3;
        public static string accessToken;
        public static  Attachment GetAdaptiveCard()
        {
           
            accessToken = HomeController.accessToken;
          
            var graphClient = GraphClient.GetGraphClient(accessToken);

            var users = graphClient.Users
            .Request()
            .GetAsync().Result;

            string adaptiveCardJson = "{ \"$schema\": \"http://adaptivecards.io/schemas/adaptive-card.json\", \"type\": \"AdaptiveCard\", \"version\": \"1.0\", \"body\": [{ \"type\": \"ColumnSet\", \"columns\": [{ \"type\": \"Column\", \"items\": [{ \"type\": \"TextBlock\", \"weight\": \"Bolder\", \"text\": \"Group Chat Title: \", \"wrap\": true }], \"width\": \"auto\" }, { \"type\": \"Column\", \"items\": [{ \"type\": \"Input.Text\", \"placeholder\": \"Please enter the title of GroupChat\", \"wrap\": true, \"id\": \"title\" }], \"width\": \"stretch\" }] }, { \"type\": \"ColumnSet\", \"columns\": [{ \"type\": \"Column\", \"items\": [{ \"type\": \"TextBlock\", \"weight\": \"Bolder\", \"text\": \"Select Members: \", \"wrap\": true }], \"width\": \"auto\" }, { \"type\": \"Column\", \"items\": [{ \"type\": \"Input.ChoiceSet\", \"id\": \"users\", \"style\": \"compact\", \"isMultiSelect\": true, \"value\": \"\", \"choices\": [{ \"title\":  \"${user1Title}\", \"value\":  \"${user1Value}\" }, { \"title\": \"${user2Title}\", \"value\": \"${user2Value}\" }, { \"title\": \"${user3Title}\", \"value\": \"${user3Value}\" }, { \"title\": \"${user4Title}\", \"value\": \"${user4Value}\" }, { \"title\": \"${user5Title}\", \"value\": \"${user5Value}\" }, { \"title\": \"${user6Title}\", \"value\": \"${user6Value}\" }] }] }] }, { \"type\": \"ColumnSet\", \"columns\": [{ \"type\": \"Column\", \"items\": [{ \"type\": \"TextBlock\", \"weight\": \"Bolder\", \"text\": \"\", \"wrap\": true, \"height\": \"stretch\" }], \"width\": \"stretch\" }] },{ \"type\": \"ColumnSet\", \"columns\": [{ \"type\": \"Column\", \"items\": [{ \"type\": \"TextBlock\", \"weight\": \"Bolder\", \"text\": \"\", \"wrap\": true, \"height\": \"stretch\" }], \"width\": \"stretch\" }] },{ \"type\": \"ColumnSet\", \"columns\": [ { \"type\": \"Column\", \"items\": [ { \"type\": \"TextBlock\", \"text\": \"**Note**: Selected Members will be added into a group chat and based on the count selected, members will be added to the chat using different scenarios: with all chat history, no chat history, chat history with no. of days accordingly.\", \"height\": \"stretch\", \"wrap\": true } ], \"width\": \"stretch\" } ] } ], \"actions\": [{ \"type\": \"Action.Submit\", \"title\": \"Submit\", \"card\": { \"version\": 1.0, \"type\": \"AdaptiveCard\", \"$schema\": \"http://adaptivecards.io/schemas/adaptive-card.json\" } }] }";

            // Create a Template instance from the template payload
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);

            // You can use any serializable object as your data
            var payloadData = new
            {
                user1Title = users.CurrentPage[0].DisplayName,
                user1Value = users.CurrentPage[0].Id,
                user2Title = users.CurrentPage[1].DisplayName,
                user2Value = users.CurrentPage[1].Id,
                user3Title = users.CurrentPage[2].DisplayName,
                user3Value = users.CurrentPage[2].Id,
                user4Title = users.CurrentPage[3].DisplayName,
                user4Value = users.CurrentPage[3].Id,
                user5Title = users.CurrentPage[4].DisplayName,
                user5Value = users.CurrentPage[4].Id,
                user6Title = users.CurrentPage[5].DisplayName,
                user6Value = users.CurrentPage[5].Id,
            };

            //"Expand" the template -this generates the final Adaptive Card payload
            string cardJson = template.Expand(payloadData);

            var adaptiveCardAttachmnt = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJson),
            };
            return adaptiveCardAttachmnt;
        }
            
    }
}
