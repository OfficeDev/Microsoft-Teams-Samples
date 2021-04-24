using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Localization.Bots
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
            // Set the current culture.
            CultureInfo.CurrentUICulture = new CultureInfo(turnContext.Activity.Locale, false);

            await turnContext.SendActivityAsync(MessageFactory.Text(_localizer["Hello"]), cancellationToken);
        }
    }
}