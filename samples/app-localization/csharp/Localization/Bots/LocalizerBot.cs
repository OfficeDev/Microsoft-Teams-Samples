using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using System.Collections.Generic;
using Bogus;
using Microsoft.Extensions.Localization;

namespace Localization
{
    public class LocalizerBot : TeamsActivityHandler
    {
        private readonly IStringLocalizer<LocalizerBot> _localizer;

        public LocalizerBot(IStringLocalizer<LocalizerBot> localizer)
        {
            _localizer = localizer;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.Activity.RemoveRecipientMention();
            var text = turnContext.Activity.Text.Trim();

            var replyText = $"You said:" + _localizer[text];
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

    }
}
