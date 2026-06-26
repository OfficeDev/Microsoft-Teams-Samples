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

        // Initializes the bot with a localizer for localization of messages.
        public LocalizerBot(IStringLocalizer<LocalizerBot> localizer)
        {
            _localizer = localizer;
        }

        // Handles incoming message activities and responds with a localized greeting.
        // Sets the current culture to the user's locale.
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                // Set the current culture to match the user's locale.
                var userLocale = turnContext.Activity.Locale;
                if (CultureInfo.CurrentUICulture.Name != userLocale)
                {
                    CultureInfo.CurrentUICulture = new CultureInfo(userLocale, false);
                }

                // Send a localized greeting message to the user.
                await turnContext.SendActivityAsync(MessageFactory.Text(_localizer["Hello"]), cancellationToken);
            }
            catch (CultureNotFoundException ex)
            {
                // Log error or set to default culture
                CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            }
        }
    }
}
