using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    // MainDialog is a class that represents the main dialog of the bot.
    // It inherits from ComponentDialog which allows adding and managing multiple dialogs.
    public class MainDialog : ComponentDialog
    {
        // Constructor that accepts a logger for logging purposes.
        public MainDialog(ILogger<MainDialog> logger)
            : base(nameof(MainDialog)) // Pass the dialog's name to the base constructor
        {
            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog); // Specifies that the first dialog to run is a WaterfallDialog.
        }
    }
}
