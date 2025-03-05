using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;

namespace AppCompleteSample.Utility
{
    /// <summary>
    /// Middleware for handling adaptive card submit actions.
    /// </summary>
    public static partial class Middleware
    {
        public static string AdaptiveCardActionKey = "dialog";

        /// <summary>
        /// Sets activity text to the message text from an adaptive card submit action.
        /// </summary>
        /// <param name="activity">The activity to process.</param>
        /// <returns>The processed activity.</returns>
        public static Activity AdaptiveCardSubmitActionHandler(Activity activity)
        {
            // Check if the activity is from an adaptive card submit action
            if (string.IsNullOrEmpty(activity.Text) && activity.ReplyToId != null && activity.Value is JObject jsonObject && jsonObject.Count > 0)
            {
                var isFromAdaptiveCard = Convert.ToString(jsonObject["isFromAdaptiveCard"]);
                var messageText = Convert.ToString(jsonObject["messageText"]);

                if (!string.IsNullOrEmpty(isFromAdaptiveCard) && isFromAdaptiveCard == "true" && !string.IsNullOrEmpty(messageText))
                {
                    // Set activity text to the message text from the adaptive card
                    activity.Text = messageText;
                }
            }

            return activity;
        }
    }
}