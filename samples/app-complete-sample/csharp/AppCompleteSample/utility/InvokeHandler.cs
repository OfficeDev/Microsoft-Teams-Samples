using Microsoft.Bot.Schema;
using AppCompleteSample;
using System;

namespace AppCompleteSample.Utility
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