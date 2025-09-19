// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { DialogSet, DialogTurnStatus, WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');

const ROOT_DIALOG = 'RootDialog';
const ROOT_WATERFALL_DIALOG = 'RootWaterfallDialog';
const HELLO = 'Hello';
const HELP = 'Help';
const HEROCARD = 'HeroCard';
const MESSAGEBACK = 'msgback';
const MULTIDIALOG2 = 'MultiDialog2';
const MULTIDIALOG1 = 'MultiDialog1';
const THUMBNAILCARD = 'ThumbnailCard';
const ADAPTIVECARD = "AdaptiveCardDialog";
const O365CONNECTORECARD = 'O365ConnectorCard';
const POPUPSIGNINCARD = 'PopupSignInCard';
const BEGINDIALOG = 'BeginDialog';
const QUIZFULLDIALOG = 'QuizFullDialog';
const PROMPTDIALOG = 'PromptDialog';
const LISTNAMES = 'ListNames';
const FETCHROSTER = 'FetchRoster';
const DISPLAYCARDS = 'DisplayCards';
const FETCHTEAMINFO = 'FetchTeamInfo';
const DEEPLINKTAB = 'DeepLinkTab';
const ATMENTION = 'AtMention';
const O365CONNECTORCARDACTION = 'O365ConnectorCardAction';
const CONVERSATION_DATA_PROPERTY = 'conversationData';
const GETLASTDIALOG = 'lastDialog';
const SETUPTEXTMESSAGE = 'SetupTextMessage';
const UPDATETEXTMESSAGE = 'UpdateTextMessage';
const AUTHCARD = 'AuthCard';
const FACEBOOKAUTH = 'FacebookAuth';
const UPDATECARDMESSAGE = 'UpdateCardMessage';
const UPDATECARDSETUP = 'UpdatecardSetupMsg';
const PROACTIVEMESSAGE = 'ProactiveMessage';
const LOGOUT = 'Logout';

const { HelloDialog } = require('./basic/helloDialog');
const { HelpDialog } = require('./basic/helpDialog');
const { HeroCardDialog } = require('./basic/heroCardDialog');
const { MessageBackDialog } = require('./basic/messagebackDialog');
const { MultiDialog1 } = require('./basic/multiDialog1');
const { MultiDialog2 } = require('./basic/multiDialog2');
const { ThumbnailCardDialog } = require('./basic/thumbnailCardDialog');
const { AdaptiveCardDialog } = require('./basic/adaptiveCardDialog');
const { O365ConnectorCardDialog } = require('./basic/o365connectorCardDialog');
const { PopupSigninCardDialog } = require('./basic/popupSigninCardDialog');
const { BeginDialogExampleDialog } = require('./moderate/beginDialogExampleDailog');
const { QuizFullDialog } = require('./moderate/quizFullDialog');
const { PromptDialog } = require('./moderate/promptDialog');
const { ListNamesDialog } = require('./moderate/listNamesDialog');
const { FetchRosterDialog } = require('./teams/fetchRosterDialog');
const { DisplayCardsDialog } = require('./teams/displayCardsDialog');
const { FetchTeamInfoDialog } = require('./teams/fetchTeamInfoDialog');
const { DeepLinkStaticTabDialog } = require('./teams/deepLinkStaticTabDialog');
const { AtMentionDialog } = require('./teams/atMentionDialog');
const { O365ConnectorCardActionDialog } = require('./basic/o365ConnectorCardActionDialog');
const { GetLastDialogUsedDialog } = require('./basic/getLastDialogUsedDialog');
const { UpdateTextMsgDialog } = require('./teams/updateTextMsgDialog');
const { UpdateTextMsgSetupDialog } = require('./teams/updateTextMsgSetupDialog');
const { UpdateCardMsgSetupDialog } = require('./teams/updateCardMsgSetupDialog');
const { UpdateCardMsgDialog } = require('./teams/updateCardMsgDialog');
const { ProactiveMsgTo1to1Dialog } = require('./teams/proactiveMsgTo1to1Dialog');
const { AuthCardDialog } = require('./auth/authDialog');
const { SimpleFacebookAuthDialog } = require('./auth/simpleFacebookAuthDialog');
const { LogoutDialog } = require('./auth/logoutDialog');

class RootDialog extends ComponentDialog {

    /**  @param {ConversationState} conversationState */
    constructor(conversationState) {
        super(ROOT_DIALOG);
        this.conversationDataAccessor = conversationState.createProperty(CONVERSATION_DATA_PROPERTY);
        this.addDialog(new WaterfallDialog(ROOT_WATERFALL_DIALOG, [
            this.promptStep.bind(this),
        ]));
        this.addDialog(new HelloDialog(HELLO, this.conversationDataAccessor));
        this.addDialog(new HelpDialog(HELP, this.conversationDataAccessor));
        this.addDialog(new HeroCardDialog(HEROCARD, this.conversationDataAccessor));
        this.addDialog(new MessageBackDialog(MESSAGEBACK, this.conversationDataAccessor));
        this.addDialog(new MultiDialog1(MULTIDIALOG1, this.conversationDataAccessor));
        this.addDialog(new MultiDialog2(MULTIDIALOG2, this.conversationDataAccessor));
        this.addDialog(new ThumbnailCardDialog(THUMBNAILCARD, this.conversationDataAccessor));
        this.addDialog(new AdaptiveCardDialog(ADAPTIVECARD, this.conversationDataAccessor));
        this.addDialog(new O365ConnectorCardDialog(O365CONNECTORECARD, this.conversationDataAccessor));
        this.addDialog(new PopupSigninCardDialog(POPUPSIGNINCARD, this.conversationDataAccessor));
        this.addDialog(new BeginDialogExampleDialog(BEGINDIALOG, this.conversationDataAccessor));
        this.addDialog(new QuizFullDialog(QUIZFULLDIALOG, this.conversationDataAccessor));
        this.addDialog(new PromptDialog(PROMPTDIALOG, this.conversationDataAccessor));
        this.addDialog(new ListNamesDialog(LISTNAMES, this.conversationDataAccessor));
        this.addDialog(new FetchRosterDialog(FETCHROSTER, this.conversationDataAccessor));
        this.addDialog(new DisplayCardsDialog(DISPLAYCARDS, this.conversationDataAccessor));
        this.addDialog(new FetchTeamInfoDialog(FETCHTEAMINFO, this.conversationDataAccessor));
        this.addDialog(new DeepLinkStaticTabDialog(DEEPLINKTAB, this.conversationDataAccessor));
        this.addDialog(new AtMentionDialog(ATMENTION, this.conversationDataAccessor));
        this.addDialog(new O365ConnectorCardActionDialog(O365CONNECTORCARDACTION, this.conversationDataAccessor));
        this.addDialog(new GetLastDialogUsedDialog(GETLASTDIALOG, this.conversationDataAccessor));
        this.addDialog(new UpdateTextMsgDialog(UPDATETEXTMESSAGE, this.conversationDataAccessor));
        this.addDialog(new UpdateTextMsgSetupDialog(SETUPTEXTMESSAGE, this.conversationDataAccessor));
        this.addDialog(new AuthCardDialog(AUTHCARD, this.conversationDataAccessor));
        this.addDialog(new SimpleFacebookAuthDialog(FACEBOOKAUTH, this.conversationDataAccessor));
        this.addDialog(new LogoutDialog(LOGOUT, process.env.ConnectionName));
        this.addDialog(new UpdateCardMsgDialog(UPDATECARDMESSAGE, this.conversationDataAccessor));
        this.addDialog(new UpdateCardMsgSetupDialog(UPDATECARDSETUP, this.conversationDataAccessor));
        this.addDialog(new ProactiveMsgTo1to1Dialog(PROACTIVEMESSAGE, this.conversationDataAccessor));

        this.initialDialogId = ROOT_WATERFALL_DIALOG;
    }

    /**
     * The run method handles the incoming activity (in the form of a DialogContext) and passes it through the dialog system.
     * If no dialog is active, it will start the default dialog.
     * @param {*} dialogContext
     */
    async run(context, accessor) {
        const dialogSet = new DialogSet(accessor);
        dialogSet.add(this);
        
        const dialogContext = await dialogSet.createContext(context);
        const results = await dialogContext.continueDialog();
        console.log(results);

        if (results !== undefined && results.status === DialogTurnStatus.empty) {
            await dialogContext.beginDialog(this.id);
        }
    }

    /**
     * Prompts the user with the appropriate dialog based on the command.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async promptStep(stepContext) {
        const activity = this.removeMentionText(stepContext.context._activity);
        if (activity.text) {
            // Clean up the command.
            const command = activity.text.replace(/\s+/g, ' ').trim().toLowerCase();
            switch (command) {
                case "hello":
                case "hi":
                    return await stepContext.beginDialog(HELLO);
                case "help":
                    return await stepContext.beginDialog(HELP);
                case "herocard":
                    return await stepContext.beginDialog(HEROCARD);
                case "msgback":
                    return await stepContext.beginDialog(MESSAGEBACK);
                case "multi dialog 1":
                    return await stepContext.beginDialog(MULTIDIALOG1);
                case "multi dialog 2":
                    return await stepContext.beginDialog(MULTIDIALOG2);
                case "thumbnailcard":
                    return await stepContext.beginDialog(THUMBNAILCARD);
                case "adaptivecard":
                    return await stepContext.beginDialog(ADAPTIVECARD);
                case "timezone":
                    await stepContext.context.sendActivity("Here is UTC time -" + stepContext.context._activity.timestamp);
                    await stepContext.context.sendActivity('Here is Local Time - ' + stepContext.context._activity.localTimestamp);
                    return await stepContext.endDialog();
                case "connector card 1":
                case "connector card 2":
                case "connector card 3":
                    return await stepContext.beginDialog(O365CONNECTORECARD);
                case "Connector Card Actions 2":
                case "Connector Card Actions":
                    return await stepContext.beginDialog(O365CONNECTORCARDACTION);
                case "signin":
                    return await stepContext.beginDialog(POPUPSIGNINCARD);
                case "dialog flow":
                    await stepContext.context.sendActivity("This is step1 in Root Dialog");
                    await stepContext.context.sendActivity("This is step2 in Root Dialog");
                    await stepContext.beginDialog(BEGINDIALOG);
                    await stepContext.context.sendActivity("This is step3 in Root Dialog After triggering the Hello Dialog");
                    return await stepContext.endDialog();
                case "quiz":
                    await stepContext.context.sendActivity("Hi, Welcome to the fun quiz. Let's get started..");
                    return await stepContext.beginDialog(QUIZFULLDIALOG);
                case "prompt":
                    return await stepContext.beginDialog(PROMPTDIALOG);
                case "names":
                    return await stepContext.beginDialog(LISTNAMES);
                case "roster":
                    return await stepContext.beginDialog(FETCHROSTER);
                case "display cards":
                    return await stepContext.beginDialog(DISPLAYCARDS);
                case "team info":
                    return await stepContext.beginDialog(FETCHTEAMINFO);
                case "deep link":
                    return await stepContext.beginDialog(DEEPLINKTAB);
                case "at mention":
                    return await stepContext.beginDialog(ATMENTION);
                case "last dialog":
                    return await stepContext.beginDialog(GETLASTDIALOG);
                case "setup text message":
                    return await stepContext.beginDialog(SETUPTEXTMESSAGE);
                case "update text message":
                    return await stepContext.beginDialog(UPDATETEXTMESSAGE);
                case "auth":
                    return await stepContext.beginDialog(AUTHCARD);
                case "fblogin":
                    return await stepContext.beginDialog(FACEBOOKAUTH);
                case "setup card message":
                    return await stepContext.beginDialog(UPDATECARDSETUP);
                case "update card message":
                    return await stepContext.beginDialog(UPDATECARDMESSAGE);
                case "send message to 1:1":
                    return await stepContext.beginDialog(PROACTIVEMESSAGE);
                case "logout":
                    return await stepContext.beginDialog(LOGOUT);
                default:
                    await stepContext.context.sendActivity('Sorry, cannot recognize the command');
                    return await stepContext.endDialog();
            }
        } else if (activity.value != null) {
            return await stepContext.beginDialog(ADAPTIVECARD);
        } else {
            await stepContext.context.sendActivity('Sorry, cannot recognize the command');
            return await stepContext.endDialog();
        }
    }

    /**
     * Removes the mention text from the activity.
     * @param {Activity} activity - The activity from which to remove the mention text.
     * @returns {Activity} The updated activity.
     */
    removeMentionText(activity) {
        const updatedActivity = activity;

        if (activity.entities[0].type === "mention") {
            updatedActivity.text = activity.text.replace(activity.entities[0].text, "");
            return updatedActivity;
        }

        return activity;
    }
}

module.exports.RootDialog = RootDialog;