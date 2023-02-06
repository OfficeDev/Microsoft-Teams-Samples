using Microsoft.Bot.Schema;
using template_bot_master_csharp;

namespace Microsoft.Teams.TemplateBotCSharp.Utility
{
    public static class InvokeHandler
    {
        /// <summary>
        /// Parse the invoke value and change the activity type as message
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static IMessageActivity HandleInvokeRequest(IMessageActivity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            activity.Text = TemplateUtility.ParseInvokeRequestJson(activity.Value.ToString());

            //Change the Type of Activity to work in exisiting Root Dialog Architecture
            activity.Type = Strings.MessageActivity;

            return activity;
        }
    }
}