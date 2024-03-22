using System;
using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using TeamsTalentMgmtApp.Models.TemplateModels;

namespace TeamsTalentMgmtApp.Services.Templates
{
    public class BaseTemplateManager : TemplateManager
    {
        public static IMessageActivity BuildCards<TTemplate, TModel>(
            TTemplate data,
            Func<TTemplate, Attachment> singleItemFunc,
            Func<TTemplate, Attachment> multipleItemFunc)
            where TTemplate : BaseTemplateModel<TModel>
            where TModel : class
        {
            var candidates = data.Items.ToList();
            IMessageActivity activity;
            if (candidates.Any())
            {
                var attachment = candidates.Count == 1
                    ? singleItemFunc(data)
                    : multipleItemFunc(data);
                activity = MessageFactory.Attachment(attachment);
            }
            else
            {
                activity = MessageFactory.Text(data.NoItemsLabel);
            }

            return activity;
        }
    }
}
