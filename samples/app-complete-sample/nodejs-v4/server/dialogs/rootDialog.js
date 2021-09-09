// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { DialogSet, DialogTurnStatus, WaterfallDialog, ComponentDialog  } = require('botbuilder-dialogs');

const ROOT_DIALOG = 'RootDialog';
const ROOT_WATERFALL_DIALOG = 'RootWaterfallDialog';
const HELLO = 'Hello';
const HELP = 'Help';
const { HelloDialog } = require('./helloDialog');
const { HelpDialog } = require('./helpDialog');
class RootDialog extends ComponentDialog{

    constructor() {
        super(ROOT_DIALOG);
        this.addDialog(new WaterfallDialog(ROOT_WATERFALL_DIALOG, [
            this.promptStep.bind(this),
        ]));
        this.addDialog(new HelloDialog(HELLO));
        this.addDialog(new HelpDialog(HELP));

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
             if(command =="hello"){
                return await stepContext.beginDialog(HELLO);
            }
            else if(command =="help"){
                return await stepContext.beginDialog(HELP);
            }
        return await stepContext.endDialog();
        }
}
module.exports.RootDialog = RootDialog;