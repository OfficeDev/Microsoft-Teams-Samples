using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;

namespace AppCompleteSample.Utility
{
    public static partial class Middleware
    {
        public static string AdaptiveCardActionKey = "dialog";

        /// <summary>
        /// Set activity text to "adaptive card", if request is from an adaptive card
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static Activity AdaptiveCardSubmitActionHandler(Activity activity)
        {
            // Check event text is blank, replyToId is not null, event value has isFromAdaptiveCard and messageText in incoming payload to check if the incoming
            // payload is from a Submit action button click from an AdaptiveCard (this is set in …\src\dialogs\examples\basic\AdaptiveCardDialog.cs) in the Submit action
            // data field. If so, then set the text field of the incoming payload so the BotFramework regex recognizers will route the message to the desired dialog.
            if (string.IsNullOrEmpty(activity.Text) && activity.ReplyToId != null && activity?.Value != null)
            {
                JObject jsonObject = activity.Value as JObject;

                if (jsonObject != null && jsonObject.Count > 0)
                {
                    string isFromAdaptiveCard = Convert.ToString(jsonObject["isFromAdaptiveCard"]);
                    string messageText = Convert.ToString(jsonObject["messageText"]);

                    if (!string.IsNullOrEmpty(isFromAdaptiveCard) && isFromAdaptiveCard == "true" && !string.IsNullOrEmpty(messageText))
                    {
                        // set activity text "adaptive card"
                        activity.Text = messageText;
                    }
                }
            }

            return activity;
        }
    }
}