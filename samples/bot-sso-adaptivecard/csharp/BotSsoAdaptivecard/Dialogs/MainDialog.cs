using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    public class MainDialog : ComponentDialog
    {
        public MainDialog(ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
    }
}
