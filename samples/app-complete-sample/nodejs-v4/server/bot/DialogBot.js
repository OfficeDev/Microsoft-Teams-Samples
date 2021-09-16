// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler } = require('botbuilder');

const searchApiUrlFormat = "https://en.wikipedia.org/w/api.php?action=query&list=search&srsearch=[keyword]&srlimit=[limit]&sroffset=[offset]&format=json";
const imageApiUrlFormat = "https://en.wikipedia.org/w/api.php?action=query&formatversion=2&format=json&prop=pageimages&piprop=thumbnail&pithumbsize=250&titles=[title]";
class DialogBot extends TeamsActivityHandler  {
   /**
     *
     * @param {ConversationState} conversationState
     * @param {UserState} userState
     * @param {Dialog} dialog
     */
    constructor(conversationState, userState, dialog) {
      super();
      if (!conversationState) throw new Error('[DialogBot]: Missing parameter. conversationState is required');
      if (!userState) throw new Error('[DialogBot]: Missing parameter. userState is required');
      if (!dialog) throw new Error('[DialogBot]: Missing parameter. dialog is required');

      this.conversationState = conversationState;
      this.userState = userState;
      this.dialog = dialog;
      this.dialogState = this.conversationState.createProperty('DialogState');
      this.userStateAccessor = this.userState.createProperty('userdata')

      this.onMessage(async (context, next) => {
          console.log('Running dialog with Message Activity.');

          // Run the Dialog with the new message Activity.
          await this.dialog.run(context, this.dialogState);

          await next();
      });
  }

  /**
   * Override the ActivityHandler.run() method to save state changes after the bot logic completes.
   */
  async run(context) {
      await super.run(context);

      // Save any state changes. The load happened during the execution of the Dialog.
      await this.conversationState.saveChanges(context, false);
      await this.userState.saveChanges(context, false);
  }

  async handleTeamsMessagingExtensionQuery(context, query){
    // get the parameters that were passed into the compose extension
    let manifestInitialRun = "initialRun";
    let manifestParameterName = "query";
    let initialRunParameter = this.getQueryParameterByName(query, manifestInitialRun);
    let queryParameter = this.getQueryParameterByName(query, manifestParameterName);
}

// return the value of the specified query parameter
getQueryParameterByName(query, name) {
let matchingParams = (query.parameters || []).filter(p => p.name === name);
return matchingParams.length ? matchingParams[0].value : "";
}
}

module.exports.DialogBot = DialogBot;