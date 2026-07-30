using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using AppCompleteSample.src.dialogs;
using AppCompleteSample;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Generic;

namespace AppCompleteSample.Dialogs
{
    /// <summary>
    /// This is Connector Card Dialog Class. Main purpose of this class is to display the Connector Card basic examples
    /// </summary>
    public class O365ConnectorCardDialog : ComponentDialog
    {
        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;
        public O365ConnectorCardDialog(IStatePropertyAccessor<RootDialogState> conversationState) : base(nameof(O365ConnectorCardDialog))
        {
            this._conversationState = conversationState;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginO365ConnectorCardDialogAsync,
            }));
        }

        private async Task<DialogTurnResult> BeginO365ConnectorCardDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }

            //Set the Last Dialog in Conversation Data
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogConnectorCardDialog;
            await this._conversationState.SetAsync(stepContext.Context, currentState);

            // Get the input number for the example to show if the user passed it into the command - e.g. 'show connector card 2'
            var activity = (IMessageActivity)stepContext.Context.Activity;

            string inputNumber = activity.Text.Substring(activity.Text.Length - 1, 1).Trim();
            Attachment attachment = null;

            /*
                * Below are a few more examples of more complex connector cards
                * To use: simply call 'connector card 2' or 'connector card 3'
                * Note: these examples are just filled with demo data and that demo data is NOT using the localization system
                * Note: these examples are leveraging an actual JSON string as their input content - more examples can be found at
                * https://messagecardplayground.azurewebsites.net/ - it is recommended that the developer use the method
                * shown above in order to get the benefits of type checking from the O365ConnectorCard class
            */

            switch (inputNumber)
            {
                case "3":
                    attachment = O365ConnectorCardImageInSection();
                    break;
                case "2":
                    attachment = O365ConnectorCardFactsInSection();
                    break;
                default:
                case "1":
                    attachment = O365ConnectorCardDefault();
                    break;
            }

            var message = stepContext.Context.Activity;
            if (message.Attachments != null)
            {
                message.Attachments = null;
            }

            if (message.Entities.Count >= 1)
            {
                message.Entities.Remove(message.Entities[0]);
            }
            message.Attachments = new List<Attachment>() { attachment };
            await stepContext.Context.SendActivityAsync(message);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        /// <summary>
        /// Connector card with text in section with card title sample
        /// </summary>
        /// <returns></returns>
        public static Attachment O365ConnectorCardDefault()
        {
            var o365connector = new O365ConnectorCard
            {
                Title = Strings.O365V1Title,
                Sections = new List<O365ConnectorCardSection>
                {
                    new O365ConnectorCardSection{ Text= Strings.O365V1Section1 },
                    new O365ConnectorCardSection{ Text= Strings.O365V1Section2 }
                },
            };

            Attachment attachment = new Attachment()
            {
                ContentType = O365ConnectorCard.ContentType,
                Content = o365connector
            };
            return attachment;
        }

        /// <summary>
        /// connector card with title, actvity title, facts in section sample
        /// </summary>
        /// <returns></returns>
        public static Attachment O365ConnectorCardFactsInSection()
        {
            var section = new O365ConnectorCardSection
            {
                Title = Strings.O365V2Title,
                ActivityTitle = Strings.O365V2ActivityTitle,
                Facts = new List<O365ConnectorCardFact>
                {
                    new O365ConnectorCardFact(Strings.O365V2Fact1Key,Strings.O365V2Fact1Value),
                    new O365ConnectorCardFact(Strings.O365V2Fact2Key,Strings.O365V2Fact2Value),
                    new O365ConnectorCardFact(Strings.O365V2Fact3Key,Strings.O365V2Fact3Value),
                    new O365ConnectorCardFact(Strings.O365V2Fact4Key,Strings.O365V2Fact4Value)
                }
            };

            var o365connector = new O365ConnectorCard
            {
                ThemeColor = Strings.O365V2themecolor,
                Sections = new List<O365ConnectorCardSection> { section },
            };

            Attachment attachment = new Attachment()
            {
                ContentType = O365ConnectorCard.ContentType,
                Content = o365connector
            };
            return attachment;
        }

        /// <summary>
        /// connector card with title, actvity title, activity subtitle, activity image, facts in section sample
        /// </summary>
        /// <returns></returns>
        public static Attachment O365ConnectorCardImageInSection()
        {
            var section = new O365ConnectorCardSection
            {
                ActivityTitle = Strings.O365V3ActivityTitle,
                ActivitySubtitle = Strings.O365V3ActivitySubtitle,
                ActivityImage = Strings.O365V3ImageUrl,
                Facts = new List<O365ConnectorCardFact>
                {
                    new O365ConnectorCardFact(Strings.O365V3Fact1Key,Strings.O365V3Fact1Value),
                    new O365ConnectorCardFact(Strings.O365V3Fact2Key,Strings.O365V3Fact2Value),
                }
            };

            var o365connector = new O365ConnectorCard
            {
                ThemeColor = Strings.O365V3ThemeColor,
                Summary = Strings.O365V3Summary,
                Title = Strings.O365V3Title,
                Sections = new List<O365ConnectorCardSection> { section },
                Text = Strings.O365V3Text
            };

            Attachment attachment = new Attachment()
            {
                ContentType = O365ConnectorCard.ContentType,
                Content = o365connector
            };
            return attachment;
        }
    }
}