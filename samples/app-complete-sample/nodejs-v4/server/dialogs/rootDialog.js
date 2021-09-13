// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { DialogSet, DialogTurnStatus, WaterfallDialog, ComponentDialog, isComponentPathResolvers  } = require('botbuilder-dialogs');

const ROOT_DIALOG = 'RootDialog';
const ROOT_WATERFALL_DIALOG = 'RootWaterfallDialog';
const HELLO = 'Hello';
const HELP = 'Help';
const HEROCARD = 'HeroCard';
const MESSAGEBACK = 'msgback';
const MULTIDIALOG2 = 'MultiDialog2';
const MULTIDIALOG1 = 'MultiDialog1';
const THUMBNAILCARD = 'ThumbnailCard';
const ADAPTIVECARD ="AdaptiveCardDialog"

const { HelloDialog } = require('./helloDialog');
const { HelpDialog } = require('./helpDialog');
const { HeroCardDialog } = require('./heroCardDialog');
const { MessageBackDialog } = require('./messagebackDialog');
const { MultiDialog1 } = require('./multiDialog1');
const { MultiDialog2 } = require('./multiDialog2');
const { ThumbnailCardDialog } = require('./thumbnailCardDialog');
const { AdaptiveCardDialog } = require('./adaptiveCardDialog');
class RootDialog extends ComponentDialog{

    constructor() {
        super(ROOT_DIALOG);
        this.addDialog(new WaterfallDialog(ROOT_WATERFALL_DIALOG, [
            this.promptStep.bind(this),
        ]));
        this.addDialog(new HelloDialog(HELLO));
        this.addDialog(new HelpDialog(HELP));
        this.addDialog(new HeroCardDialog(HEROCARD));
        this.addDialog(new MessageBackDialog(MESSAGEBACK));
        this.addDialog(new MultiDialog1(MULTIDIALOG1));
        this.addDialog(new MultiDialog2(MULTIDIALOG2));
        this.addDialog(new ThumbnailCardDialog(THUMBNAILCARD));
        this.addDialog(new AdaptiveCardDialog(ADAPTIVECARD));

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
            if (results.status === DialogTurnStatus.empty) {
                await dialogContext.beginDialog(this.id);
            }
        }

        async promptStep(stepContext) {
             var command = stepContext.context._activity.text;
             console.log(command);
             if(command.trim() =="hello" || command=="hi"){
                return await stepContext.beginDialog(HELLO);
            }
            else if(command.trim() =="help"){
                return await stepContext.beginDialog(HELP);
            }
            else if(command.trim() =="herocard"){
                return await stepContext.beginDialog(HEROCARD);
            }
            else if(command.trim() =="msgback"){
                return await stepContext.beginDialog(MESSAGEBACK);
            }
            else if(command.trim() =="multi dialog 1"){
                return await stepContext.beginDialog(MULTIDIALOG1);
            }
            else if(command.trim() =="multi dialog 2"){
                return await stepContext.beginDialog(MULTIDIALOG2);
            }
            else if(command.trim() == "thumbnailcard"){
                return await stepContext.beginDialog(THUMBNAILCARD);
            }
            else if(command.trim() == "adaptivecard"){
                return await stepContext.beginDialog(ADAPTIVECARD);
            }
            await stepContext.context.sendActivity('Sorry,Cannot recognize the command');
        return await stepContext.endDialog();
        }
}
module.exports.RootDialog = RootDialog;