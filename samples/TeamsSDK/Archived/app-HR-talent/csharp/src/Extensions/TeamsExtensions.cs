using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AdaptiveCards;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TeamsTalentMgmtApp.Extensions
{
    public static class TeamsExtensions
    {
        public static string GetTextWithoutCommand(this IMessageActivity activity, string commandMatch)
        {
            var query = activity.GetActivityTextWithoutMentions()?.Trim() ?? string.Empty;

            var regex = new Regex(commandMatch, RegexOptions.IgnoreCase);
            if (regex.Match(query).Success || query.IndexOf(commandMatch, StringComparison.OrdinalIgnoreCase) == 0)
            {
                query = regex.Replace(query, string.Empty, 1);
            }

            return query.Trim();
        }

        public static void RepresentAsBotBuilderAction(this AdaptiveSubmitAction action, CardAction targetAction)
        {
            var wrappedAction = new CardAction
            {
                Type = targetAction.Type,
                Value = targetAction.Value,
                Text = targetAction.Text,
                DisplayText = targetAction.DisplayText,
            };

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
            serializerSettings.NullValueHandling = NullValueHandling.Ignore;

            string jsonStr = action.DataJson == null ? "{}" : action.DataJson;
            JToken dataJson = JObject.Parse(jsonStr);
            dataJson["msteams"] = JObject.FromObject(wrappedAction, JsonSerializer.Create(serializerSettings));

            action.Title = targetAction.Title;
            action.DataJson = dataJson.ToString();
        }

        public static bool HasFileAttachments(this IMessageActivity activity)
        {
            return activity.Attachments != null && activity.Attachments.Any(x => string.Equals(x.ContentType, FileDownloadInfo.ContentType, StringComparison.OrdinalIgnoreCase));
        }

        public static string GetActivityTextWithoutMentions(this IMessageActivity activity)
        {
            // Case 1. No entities.
            if (activity.Entities?.Count == 0)
            {
                return activity.Text;
            }

            IEnumerable<Entity> mentionEntities = activity.Entities.Where(entity => entity.Type.Equals("mention", StringComparison.OrdinalIgnoreCase));

            // Case 2. No Mention entities.
            if (!mentionEntities.Any())
            {
                return activity.Text;
            }

            // Case 3. Mention entities.
            string strippedText = activity.Text;

            mentionEntities.ToList()
                .ForEach(entity =>
                {
                    strippedText = strippedText.Replace(entity.GetAs<Mention>().Text, string.Empty);
                });

            return strippedText.Trim();
        }
    }
}
