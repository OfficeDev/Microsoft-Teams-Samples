using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using AppCompleteSample.src.dialogs;
using AppCompleteSample;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace AppCompleteSample.Dialogs
{
    /// <summary>
    /// This is Actionable Card Dialog Class. Main purpose of this class is to show example of O365connector card actionable feature sample like 
    /// multi choice, date/time, input text and multiple sections examples
    /// </summary>

    [Serializable]
    public class O365ConnectorCardActionsDialog : ComponentDialog
    {
        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;
        public O365ConnectorCardActionsDialog(IStatePropertyAccessor<RootDialogState> conversationState) : base(nameof(O365ConnectorCardActionsDialog))
        {
            this._conversationState = conversationState;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginO365ConnectorCardActionsDialogAsync,
            }));
        }

        private async Task<DialogTurnResult> BeginO365ConnectorCardActionsDialogAsync(
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

            // get the input number for the example to show if the user passed it into the command - e.g. 'show connector card 2'
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
                // Actionable cards can have multiple sections, each with its own set of actions.
                // If a section contains only 1 card action, that is automatically expanded
                case "2":
                    attachment = O365ActionableCardMultipleSection();
            break;

                // this is the default example's content
                // multiple choice (compact & expanded), text input, date and placing images in card
                case "1":
                default:
                    attachment = O365ActionableCardDefault();
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
        /// this is the default example's content
        /// multiple choice (compact & expanded), text input, date and placing images in card
        /// </summary>
        /// <returns>The result card with actions.</returns>
        /// 
        public static Attachment O365ActionableCardDefault()
        {
            #region multi choice examples

            var multichoice = new O365ConnectorCardActionCard(O365ConnectorCardActionCard.Type)
            {
                Name = "Multiple Choice",
                Id = "Multiple Choice Card",
                Inputs = new List<O365ConnectorCardInputBase>
                {
                    // multiple choice control with required, multiselect, expanded style
                    new O365ConnectorCardMultichoiceInput (O365ConnectorCardMultichoiceInput.Type)
                    {
                        Id = "CardsType",
                        IsRequired = true,
                        Title = "Pick multiple options",
                        Value = null,
                        Choices = new List<O365ConnectorCardMultichoiceInputChoice>
                        {
                            new O365ConnectorCardMultichoiceInputChoice("Hero Card", "Hero Card"),
                            new O365ConnectorCardMultichoiceInputChoice("Thumbnail Card", "Thumbnail Card"),
                            new O365ConnectorCardMultichoiceInputChoice("O365 Connector Card", "O365 Connector Card")
                        },
                        Style = "expanded",
                        IsMultiSelect = true
                    },
                    // multiple choice control with required, multiselect, compact style
                    new O365ConnectorCardMultichoiceInput(O365ConnectorCardMultichoiceInput.Type)
                    {
                       Id = "Teams",
                       IsRequired = true,
                       Title = "Pick multiple options",
                       Value = null,
                       Choices = new List<O365ConnectorCardMultichoiceInputChoice>
                       {
                            new O365ConnectorCardMultichoiceInputChoice("Bot", "Bot"),
                            new O365ConnectorCardMultichoiceInputChoice("Tab", "Tab"),
                            new O365ConnectorCardMultichoiceInputChoice("Connector", "Connector"),
                            new O365ConnectorCardMultichoiceInputChoice("Compose Extension", "Compose Extension")
                       },
                       Style = "compact",
                       IsMultiSelect = true
                    },
                    // multiple choice control with single item select, expanded style
                    new O365ConnectorCardMultichoiceInput(O365ConnectorCardMultichoiceInput.Type)
                    {
                       Id = "Apps",
                       IsRequired = false,
                       Title = "Pick an App",
                       Value = null,
                       Choices = new List<O365ConnectorCardMultichoiceInputChoice>
                       {
                            new O365ConnectorCardMultichoiceInputChoice("VSTS", "VSTS"),
                            new O365ConnectorCardMultichoiceInputChoice("Wiki", "Wiki"),
                            new O365ConnectorCardMultichoiceInputChoice("Github", "Github")
                       },
                       Style = "expanded",
                       IsMultiSelect = false
                    },
                    // multiple choice control with single item select, compact style
                    new O365ConnectorCardMultichoiceInput(O365ConnectorCardMultichoiceInput.Type)
                    {
                        Id ="OfficeProduct",
                        IsRequired = false,
                        Title = "Pick an Office Product",
                        Value = null,
                        Choices = new List<O365ConnectorCardMultichoiceInputChoice>
                        {
                            new O365ConnectorCardMultichoiceInputChoice("Outlook", "Outlook"),
                            new O365ConnectorCardMultichoiceInputChoice("MS Teams", "MS Teams"),
                            new O365ConnectorCardMultichoiceInputChoice("Skype", "Skype")
                        },
                        Style ="compact",
                        IsMultiSelect = false
                    }
                },

                Actions = new List<O365ConnectorCardActionBase>
                {
                    new O365ConnectorCardHttpPOST(O365ConnectorCardHttpPOST.Type)
                    {
                        Name = "Send",
                        Id = "multichoice",
                        Body = @"{""CardsType"":""{{CardsType.value}}"", ""Teams"":""{{Teams.value}}"", ""Apps"":""{{Apps.value}}"", ""OfficeProduct"":""{{OfficeProduct.value}}""}"
                    }
                }
            };

            #endregion

            #region text input examples
            var inputCard = new O365ConnectorCardActionCard(O365ConnectorCardActionCard.Type)
            {
                Name = "Text Input",
                Id = "Input Card",
                Inputs = new List<O365ConnectorCardInputBase>
                {
                    // text input control with multiline
                    new O365ConnectorCardTextInput(O365ConnectorCardTextInput.Type)
                    {
                        Id = "text-1",
                        IsRequired = false,
                        Title = "multiline, no maxLength",
                        Value = null,
                        IsMultiline = true,
                        MaxLength = null
                    },
                    // text input control without multiline
                    new O365ConnectorCardTextInput(O365ConnectorCardTextInput.Type)
                    {
                        Id = "text-2",
                        IsRequired = false,
                        Title = "single line, no maxLength",
                        Value = null,
                        IsMultiline = false,
                        MaxLength = null
                    },
                    // text input control with multiline, reuired,
                    // and control the length of input box
                    new O365ConnectorCardTextInput(O365ConnectorCardTextInput.Type)
                    {
                       Id = "text-3",
                       IsRequired = true,
                       Title = "multiline, max len = 10, isRequired",
                       Value = null,
                       IsMultiline = true,
                       MaxLength = 10
                    },
                    // text input control without multiline, reuired,
                    // and control the length of input box
                    new O365ConnectorCardTextInput(O365ConnectorCardTextInput.Type)
                    {
                       Id = "text-4",
                       IsRequired = true,
                       Title = "single line, max len = 10, isRequired",
                       Value = null,
                       IsMultiline = false,
                       MaxLength = 10
                    }
                },
                Actions = new List<O365ConnectorCardActionBase>
                {
                    new O365ConnectorCardHttpPOST(O365ConnectorCardHttpPOST.Type)
                    {
                        Name = "Send",
                        Id = "inputText",
                        Body = @"{""text1"":""{{text-1.value}}"", ""text2"":""{{text-2.value}}"", ""text3"":""{{text-3.value}}"", ""text4"":""{{text-4.value}}""}"
                    }
                }
            };
            #endregion

            #region date/time input examples
            var dateCard = new O365ConnectorCardActionCard(O365ConnectorCardActionCard.Type)
            {
                Name = "Date Input",
                Id = "Date Card",
                Inputs = new List<O365ConnectorCardInputBase>
                {
                    // date input control, with date and time, required
                    new O365ConnectorCardDateInput (O365ConnectorCardDateInput.Type)
                    {
                        Id = "date-1",
                        IsRequired = true,
                        Title = "date with time",
                        Value = null,
                        IncludeTime = true
                    },
                    // date input control, only date, no time, not required
                    new O365ConnectorCardDateInput (O365ConnectorCardDateInput.Type)
                    {
                       Id = "date-2",
                       IsRequired = false,
                       Title = "date only",
                       Value = null,
                       IncludeTime = false
                    }
                },
                Actions = new List<O365ConnectorCardActionBase>
                {
                    new O365ConnectorCardHttpPOST (O365ConnectorCardHttpPOST.Type)
                    {
                        Name = "Send",
                        Id = "dateInput",
                        Body = @"{""date1"":""{{date-1.value}}"", ""date2"":""{{date-2.value}}""}"
                    }
                }
            };
            #endregion

            var section = new O365ConnectorCardSection
            {
                Title = "**section title**",
                Text = "section text",
                ActivityTitle = "activity title",
                ActivitySubtitle = "activity subtitle",
                ActivityText = "activity text",
                ActivityImage = "http://connectorsdemo.azurewebsites.net/images/MSC12_Oscar_002.jpg",
                ActivityImageType = null,
                Markdown = true,
                Facts = new List<O365ConnectorCardFact>
                {
                    new O365ConnectorCardFact
                    {
                        Name = "Fact name 1",
                        Value = "Fact value 1"
                    },
                    new O365ConnectorCardFact
                    {
                        Name = "Fact name 2",
                        Value = "Fact value 2"
                    },
                },
                Images = new List<O365ConnectorCardImage>
                {
                    new O365ConnectorCardImage
                    {
                        Image = "http://connectorsdemo.azurewebsites.net/images/MicrosoftSurface_024_Cafe_OH-06315_VS_R1c.jpg",
                        Title = "image 1"
                    },
                    new O365ConnectorCardImage
                    {
                        Image = "http://connectorsdemo.azurewebsites.net/images/WIN12_Scene_01.jpg",
                        Title = "image 2"
                    },
                    new O365ConnectorCardImage
                    {
                        Image = "http://connectorsdemo.azurewebsites.net/images/WIN12_Anthony_02.jpg",
                        Title = "image 3"
                    }
                }
            };

            O365ConnectorCard card = new O365ConnectorCard()
            {
                Summary = "O365 card summary",
                ThemeColor = "#E67A9E",
                Title = "card title",
                Text = "card text",
                Sections = new List<O365ConnectorCardSection> { section },
                PotentialAction = new List<O365ConnectorCardActionBase>
                {
                    multichoice,
                    inputCard,
                    dateCard,
                    new O365ConnectorCardViewAction (O365ConnectorCardViewAction.Type)
                    {
                        Name = "View Action",
                        Id = null,
                        Target = new List<string>
                        {
                            "http://microsoft.com"
                        }
                    },
                    new O365ConnectorCardOpenUri (O365ConnectorCardOpenUri.Type)
                    {
                        Name = "Open Uri",
                        Id = "open-uri",
                        Targets = new List<O365ConnectorCardOpenUriTarget>
                        {
                            new O365ConnectorCardOpenUriTarget
                            {
                                Os = "default",
                                Uri = "http://microsoft.com"
                            },
                            new O365ConnectorCardOpenUriTarget
                            {
                                Os = "iOS",
                                Uri = "http://microsoft.com"
                            },
                            new O365ConnectorCardOpenUriTarget
                            {
                                Os = "android",
                                Uri = "http://microsoft.com"
                            },
                            new O365ConnectorCardOpenUriTarget
                            {
                                Os = "windows",
                                Uri = "http://microsoft.com"
                            }
                        }
                    }
                }
            };

            Attachment attachment = new Attachment()
            {
                ContentType = O365ConnectorCard.ContentType,
                Content = card
            };
            return attachment;
        }

        /// <summary>
        /// Actionable cards can have multiple sections, each with its own set of actions.
        /// If a section contains only 1 card action, that is automatically expanded
        /// </summary>
        /// <returns>The result card with actions.</returns>
        /// 
        public static Attachment O365ActionableCardMultipleSection()
        {
            #region Section1

            #region Multichoice Card
            // multiple choice control with required, multiselect, compact style            
            var multichoiceCardSection1 = new O365ConnectorCardActionCard(O365ConnectorCardActionCard.Type)
            {
                Name = "Multiple Choice",
                Id = "Multiple Choice Card",
                Inputs = new List<O365ConnectorCardInputBase>
                {
                    new O365ConnectorCardMultichoiceInput (O365ConnectorCardMultichoiceInput.Type)
                    {
                        Id = "cardstype",
                        IsRequired = true,
                        Title = "Pick multiple options",
                        Value = null,
                        Choices = new List<O365ConnectorCardMultichoiceInputChoice>
                        {
                            new O365ConnectorCardMultichoiceInputChoice("Hero Card", "Hero Card"),
                            new O365ConnectorCardMultichoiceInputChoice("Thumbnail Card", "Thumbnail Card"),
                            new O365ConnectorCardMultichoiceInputChoice("O365 Connector Card", "O365 Connector Card")
                        },
                        Style = "compact",
                        IsMultiSelect = true
                    },
                },
                Actions = new List<O365ConnectorCardActionBase>
                {
                    new O365ConnectorCardHttpPOST (O365ConnectorCardHttpPOST.Type)
                    {
                        Name = "send",
                        Id = "multichoice",
                        Body = @"{""cardstype"":""{{cardstype.value}}"""
                    }
                }
            };

            #endregion

            var potentialActionSection1 = new List<O365ConnectorCardActionBase>
            {
                 multichoiceCardSection1
            };

            var section1 = new O365ConnectorCardSection
            {
                Title = "Section Title 1",
                Text = "",
                ActivityTitle = "",
                ActivitySubtitle = "",
                ActivityText = "",
                ActivityImage = null,
                ActivityImageType = null,
                Markdown = true,
                Facts = null,
                Images = null,
                PotentialAction = potentialActionSection1
            };
            #endregion 

            #region Section2

            #region Input Card
            // text input examples
            var inputCard = new O365ConnectorCardActionCard(O365ConnectorCardActionCard.Type)
            {
                Id = "Text Input",
                Name = "Input Card",
                // text input control with multiline
                Inputs = new List<O365ConnectorCardInputBase>
                {
                    new O365ConnectorCardTextInput (O365ConnectorCardTextInput.Type)
                    {
                       Id = "text-1",
                       IsRequired = false,
                       Title = "This is the title of text box",
                       Value = null,
                       IsMultiline = true,
                       MaxLength = null
                    }
                },
                Actions = new List<O365ConnectorCardActionBase>
                {
                    new O365ConnectorCardHttpPOST (O365ConnectorCardHttpPOST.Type)
                    {
                        Name = "Send",
                        Id = "inputText",
                        Body = @"{""text1"":""{{text-1.value}}""}"
                    }
                }
            };
            #endregion

            #region Multichoice Card For Section2
            // multiple choice control with not required, multiselect, compact style
            var multichoiceCardSection2 = new O365ConnectorCardActionCard(O365ConnectorCardActionCard.Type)
            {
                Name = "Multiple Choice",
                Id = "Multiple Choice Card",
                Inputs = new List<O365ConnectorCardInputBase>
                {
                    new O365ConnectorCardMultichoiceInput (O365ConnectorCardMultichoiceInput.Type)
                    {
                        Id = "CardsTypesection1", //please make sure that id of the control must be unique across card to work properly
                        IsRequired = false,
                        Title = "This is a title of combo box",
                        Value = null,
                        Choices = new List<O365ConnectorCardMultichoiceInputChoice>
                        {
                            new O365ConnectorCardMultichoiceInputChoice("Hero Card", "Hero Card"),
                            new O365ConnectorCardMultichoiceInputChoice("Thumbnail Card", "Thumbnail Card"),
                            new O365ConnectorCardMultichoiceInputChoice("O365 Connector Card", "O365 Connector Card")
                        },
                        Style = "compact",
                        IsMultiSelect = true
                    }
                },
                Actions = new List<O365ConnectorCardActionBase>
                {
                    new O365ConnectorCardHttpPOST (O365ConnectorCardHttpPOST.Type)
                    {
                        Name= "Send",
                        Id ="multichoice",
                        Body = @"{""CardsTypesection1"":""{{CardsTypesection1.value}}"""
                    }
                }
            };
            #endregion

            // please always attach new potential action to individual sections
            var potentialActionSection2 = new List<O365ConnectorCardActionBase>
            {
                 inputCard,
                 multichoiceCardSection2
            };

            var section2 = new O365ConnectorCardSection
            {
                Title = "Section Title 2",
                Text = "",
                ActivityTitle = "",
                ActivitySubtitle = "",
                ActivityText = "",
                ActivityImage = null,
                ActivityImageType = null,
                Markdown = true,
                Facts = null,
                Images = null,
                PotentialAction = potentialActionSection2
            };
            #endregion

            O365ConnectorCard card = new O365ConnectorCard()
            {
                Title = "This is Actionable Card Title",
                Summary = "O365 card summary",
                ThemeColor = "#E67A9E",
                Sections = new List<O365ConnectorCardSection> { section1, section2 },
            };

            Attachment attachment = new Attachment()
            {
                ContentType = O365ConnectorCard.ContentType,
                Content = card
            };
            return attachment;
        }
    }
}