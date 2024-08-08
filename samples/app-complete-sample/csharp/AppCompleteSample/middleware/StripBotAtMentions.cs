using Microsoft.Bot.Schema;
using System;

namespace AppCompleteSample.Utility
{
    public static partial class Middleware
    {
        public static Activity StripAtMentionText(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            foreach (var m in activity.GetMentions())
            {
                if (m.Mentioned.Id == activity.Recipient.Id)
                {
                    //Bot is in the @mention list.  
                    //The below example will strip the bot name out of the message, so you can parse it as if it wasn't included.
                    //Note that the Text object will contain the full bot name, if applicable.
                    if (m.Text != null)
                        activity.Text = activity.Text.Replace(m.Text, "").Trim();
                }
            }

            return activity;
        }
    }
}