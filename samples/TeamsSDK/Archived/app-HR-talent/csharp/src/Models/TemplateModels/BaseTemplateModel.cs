using System.Collections.Generic;

namespace TeamsTalentMgmtApp.Models.TemplateModels
{
    public class BaseTemplateModel<T>
        where T : class
    {
        public IEnumerable<T> Items { get; set; }

        public string NoItemsLabel { get; set; }

        public string BotCommand { get; set; }

        public string ListCardTitle { get; set; }
    }
}
