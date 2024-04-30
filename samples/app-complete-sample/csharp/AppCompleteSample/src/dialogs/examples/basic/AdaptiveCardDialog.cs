using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using AppCompleteSample.src.dialogs;
using AppCompleteSample.Utility;
using Newtonsoft.Json.Linq;
using AppCompleteSample;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;

namespace AppCompleteSample.Dialogs
{
    /// <summary>
    /// This is Adaptive Card Dialog Class. Main purpose of this class is to display the Adaptive Card example
    /// </summary>
    public class AdaptiveCardDialog : ComponentDialog
    {
        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;

        private readonly IOptions<AzureSettings> azureSettings;
        public AdaptiveCardDialog(IStatePropertyAccessor<RootDialogState> conversationState, IOptions<AzureSettings> azureSettings) : base(nameof(AdaptiveCardDialog))
        {
            this._conversationState = conversationState;
            this.azureSettings = azureSettings;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginAdaptiveCardDialogAsync,
            }));
        }

        private async Task<DialogTurnResult> BeginAdaptiveCardDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }

            //Set the Last Dialog in Conversation Data
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogAdaptiveCard;
            await this._conversationState.SetAsync(stepContext.Context, currentState);

            Activity activity = stepContext.Context.Activity as Activity;

            // if request is from submit action, process adaptive card values
            if (IsActivityFromAdaptiveCard(activity))
            {
                // handle adaptive card submit action
                await SendAdaptiveCardValues(stepContext, activity);
            }
            else
            {
                // create and send adaptive card
                var message = stepContext.Context.Activity;
                if (message.Attachments != null)
                {
                    message.Attachments = null;
                }

                if (message.Entities.Count >= 1)
                {
                    message.Entities.Remove(message.Entities[0]);
                }
                var attachment = GetAdaptiveCardAttachment(this.azureSettings.Value.BaseUri);
                message.Attachments = new List<Attachment>() { attachment };
                await stepContext.Context.SendActivityAsync(message);
            }
            await stepContext.Context.SendActivityAsync(Strings.HelloDialogMsg);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        // Here is the example of adaptive card having image set, text block,
        // input box, date input, time input, toggle input, choice set(dropdown), choice set(dropdown) with multiselect etc.
        public static Attachment GetAdaptiveCardAttachment(string BaseUri)
        {
            string textToTriggerThisDialog = "adaptive card";

            var card = new AdaptiveCard()
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            // TextBlock Item allows for the inclusion of text, with various font sizes, weight and color
                            new AdaptiveTextBlock()
                            {
                                Text = "Adaptive Card!",
                                Weight = AdaptiveTextWeight.Bolder, // set the weight of text e.g. Bolder, Light, Normal
                                Size = AdaptiveTextSize.Large, // set the size of text e.g. Extra Large, Large, Medium, Normal, Small
                            },
                            // Adaptive FactSet item makes it simple to display a series of facts (e.g. name/value pairs) in a tabular form
                            new AdaptiveFactSet
                            {
                                Separator = true,
                                Facts =
                                {
                                    // Describes a fact in a Adaptive FactSet as a key/value pair
                                    new AdaptiveFact
                                    {
                                        Title = "Board:",
                                        Value = "Adaptive Card"
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "List:",
                                        Value = "Backlog"
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Assigned to:",
                                        Value = "Matt Hidinger"
                                    },
                                    new AdaptiveFact
                                    {
                                        Title = "Due date:",
                                        Value = "Not set"
                                    }
                                }
                            },
                            // ImageSet allows for the inclusion of a collection images like a photogallery
                            new AdaptiveImageSet
                            {
                                ImageSize = AdaptiveImageSize.Medium,
                                Images =
                                {
                                    // Image Item allows for the inclusion of images
                                    new AdaptiveImage
                                    {
                                        Url = new Uri(BaseUri + "/public/assets/computer.jpg")
                                    },
                                    new AdaptiveImage
                                    {
                                        Url = new Uri(BaseUri + "/public/assets/computer_person.jpg")
                                    },
                                    new AdaptiveImage
                                    {
                                        Url = new Uri(BaseUri + "/public/assets/computer.jpg")
                                    },
                                }
                            },// wrap the text in textblock
                            new AdaptiveTextBlock()
                            {
                                // markdown example for bold text
                                Text = "'**Matt H. said** \"I'm compelled to give this place 5 stars due to the number of times I've chosen to eat here this past year!\',",
                                Wrap = true, // True if text is allowed to wrap
                            },
                            new AdaptiveTextBlock()
                            {
                                Text = "Place your text here:",
                                Separator = true,
                            },
                            // text input collects text from the user
                            new AdaptiveTextInput()
                            {
                                Id = "textInputId",
                                Placeholder = "Text Input",
                                Style = AdaptiveTextInputStyle.Text // set the type of input box e.g Text, Tel, Email, Url
                            },
                            new AdaptiveTextBlock()
                            {
                                Text = "Please select Date here?"
                            },
                            // date input collects Date from the user
                            new AdaptiveDateInput()
                            {
                                Id = "dateInput",
                            },
                            new AdaptiveTextBlock()
                            {
                                Text = "Please enter time here?"
                            },
                            // time input collects time from the user
                            new AdaptiveTimeInput()
                            {
                                Id = "timeInput"
                            },
                            new AdaptiveTextBlock()
                            {
                                Separator = true,
                                Text = "Please select your choice here? (Compact Dropdown)"
                            },
                            // Shows an array of Choice objects
                            new AdaptiveChoiceSetInput()
                            {
                               Id = "choiceSetCompact",
                               Value = "1", // please set default value here
                               Style = AdaptiveChoiceInputStyle.Compact, // set the style of Choice set to compact
                               Choices =
                               {
                                  // describes a choice input. the value should be a simple string without a ","
                                  new AdaptiveChoice
                                  {
                                      Title = "Red",
                                      Value = "1" // do not use a “,” in the value, since MultiSelect ChoiceSet returns a comma-delimited string of choice values
                                  },
                                  new AdaptiveChoice
                                  {
                                      Title = "Green",
                                      Value = "2"
                                  },
                                  new AdaptiveChoice
                                  {
                                      Title = "Blue",
                                      Value = "3"
                                  },
                                  new AdaptiveChoice
                                  {
                                      Title = "White",
                                      Value = "4"
                                  }
                                }
                            },
                            new AdaptiveTextBlock()
                            {
                                Text = "Please select your choice here? (Expanded Dropdown)"
                            },
                            // Shows an array of Choice objects
                            new AdaptiveChoiceSetInput()
                            {
                               Id= "choiceSetExpandedRequired",
                               Value = "1", // please set default value here
                               Style = AdaptiveChoiceInputStyle.Expanded, // set the style of Choice set to expanded
                               Choices =
                               {
                                    new AdaptiveChoice
                                    {
                                        Title = "Red",
                                        Value = "1"
                                    },
                                    new AdaptiveChoice
                                    {
                                        Title = "Green",
                                        Value = "2"
                                    },
                                    new AdaptiveChoice
                                    {
                                        Title = "Blue",
                                        Value = "3"
                                    },
                                    new AdaptiveChoice
                                    {
                                        Title = "White",
                                        Value = "4"
                                    }
                               }
                            },
                            new AdaptiveTextBlock()
                            {
                                Text = "Please select multiple items here? (Multiselect Dropdown)"
                            },
                            // Shows an array of Choice objects (Multichoice)
                            new AdaptiveChoiceSetInput()
                            {
                               Id = "choiceSetExpanded",
                               Value = "1,2", // The initial choice (or set of choices) that should be selected. For multi-select, specifcy a comma-separated string of values
                               Style = AdaptiveChoiceInputStyle.Expanded, // // set the style of Choice set to expanded
                               IsMultiSelect = true, // allow multiple choices to be selected
                               Choices =
                               {
                                    new AdaptiveChoice
                                    {
                                        Title = "Red",
                                        Value = "1"
                                    },
                                    new AdaptiveChoice
                                    {
                                        Title = "Green",
                                        Value = "2"
                                    },
                                    new AdaptiveChoice
                                    {
                                        Title = "Blue",
                                        Value = "3"
                                    },
                                    new AdaptiveChoice
                                    {
                                        Title = "White",
                                        Value = "4"
                                    }
                               }
                            },                            
                            // column set divides a region into Column's allowing elements to sit side-by-side
                            new AdaptiveColumnSet()
                            {
                                Separator = true,
                                Columns = new List<AdaptiveColumn>()
                                {
                                    // defines a container that is part of a column set
                                    new AdaptiveColumn()
                                    {
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveImage()
                                            {
                                                Url = new Uri("https://placeholdit.imgix.net/~text?txtsize=65&txt=Adaptive+Cards&w=300&h=300"),
                                                Size = AdaptiveImageSize.Medium,
                                                Style = AdaptiveImageStyle.Person
                                            }
                                        }
                                    },
                                    new AdaptiveColumn()
                                    {
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveTextBlock()
                                            {
                                                Text =  "Hello!",
                                                Weight = AdaptiveTextWeight.Bolder,
                                                IsSubtle = true
                                            },
                                            new AdaptiveTextBlock()
                                            {
                                                Text = "Are you looking for a Tab or Bot?",
                                                Wrap = true
                                            }
                                        }
                                    }
                                }
                            },
                            //  input toggle collects a true/false response from the user
                            new AdaptiveToggleInput
                            {
                                Id = "AcceptsTerms",
                                Title = "I accept the terms and conditions (True/False)",
                                ValueOff = "false", // the value when toggle is off (default: false)
                                ValueOn = "true"  // the value when toggle is on (default: true)
                            },
                        }
                    }
                },
                Actions = new List<AdaptiveAction>()
                {
                    // submit action gathers up input fields, merges with optional data field and generates event to client asking for data to be submitted
                    new AdaptiveSubmitAction()
                    {
                        Title = "Submit",
                        DataJson = "{\"isFromAdaptiveCard\": \"true\", \"messageText\": \""+ textToTriggerThisDialog +"\" }",
                    },
                    // submit action can also act similar to messageBack, with custom display text
                    // See https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/cards/cards-actions#adaptive-card-actions
                    new AdaptiveSubmitAction()
                    {
                        Title = "Submit (MessageBack)",
                        DataJson = "{\"isFromAdaptiveCard\": \"true\", \"messageText\": \""+ textToTriggerThisDialog +"\", \"msteams\":{\"type\":\"messageBack\", \"displayText\":\"Card submitted!\"}}",
                    },
                    // show action defines an inline AdaptiveCard which is shown to the user when it is clicked
                    new AdaptiveShowCardAction()
                    {
                        Card = new AdaptiveCard()
                        {
                            Version = "1.0",
                            Body = new List<AdaptiveElement>()
                            {
                                new AdaptiveContainer()
                                {
                                    Items = new List<AdaptiveElement>()
                                    {
                                        new AdaptiveTextInput()
                                        {
                                            Id = "Text",
                                            Placeholder = "text here",
                                            Style = AdaptiveTextInputStyle.Text
                                        },
                                    }
                                }
                            },
                            Actions = new List<AdaptiveAction>()
                            {
                                new AdaptiveSubmitAction()
                                {
                                    Title = "Submit",
                                    DataJson = "{\"isFromAdaptiveCard\": \"true\", \"messageText\": \""+ textToTriggerThisDialog +"\" }",
                                },
                            }
                        }
                    },
                    // open url show the given url, either by launching it to an external web browser
                    new AdaptiveOpenUrlAction()
                    {
                        Title = "Open Url",
                        Url = new Uri("http://adaptivecards.io/explorer/Action.OpenUrl.html")
                    }
                }
            };

            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            return attachment;
        }

        /// <summary>
        /// Check if submit action request is from an adaptive card
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static bool IsActivityFromAdaptiveCard(Activity activity)
        {
            // Check for the property in the value set by the adaptive card submit action
            if (activity.ReplyToId != null && activity?.Value != null)
            {
                JObject jsonObject = activity.Value as JObject;

                if (jsonObject != null && jsonObject.Count > 0)
                {
                    string isFromAdaptiveCard = Convert.ToString(jsonObject["isFromAdaptiveCard"]);

                    if (!string.IsNullOrEmpty(isFromAdaptiveCard) && isFromAdaptiveCard == "true")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Handle adaptive card request
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private async Task SendAdaptiveCardValues(WaterfallStepContext context, Activity activity)
        {
            var submitValue = context.Context.Activity;
            submitValue.Text = Convert.ToString(activity.Value);
            await context.Context.SendActivityAsync(submitValue);
        }
    }
}