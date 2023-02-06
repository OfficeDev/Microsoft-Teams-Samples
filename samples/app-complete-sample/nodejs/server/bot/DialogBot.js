// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, ActionTypes, CardFactory } = require('botbuilder');
const request = require('request-promise')
const searchApiUrlFormat = "https://en.wikipedia.org/w/api.php?action=query&list=search&srsearch=[keyword]&srlimit=[limit]&sroffset=[offset]&format=json";

class DialogBot extends TeamsActivityHandler {
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

  async handleTeamsMessagingExtensionQuery(context, query) {
    // get the parameters that were passed into the compose extension
    let manifestInitialRun = "initialRun";
    let manifestParameterName = "query";
    let initialRunParameter = this.getQueryParameterByName(query, manifestInitialRun);
    let queryParameter = this.getQueryParameterByName(query, manifestParameterName);
    var userData = await this.userStateAccessor.get(context, {});
    userData.botId = context.activity.recipient.id;
    await this.userState.saveChanges(context, false);

    if (userData == null) {
      return {
        composeExtension: {
          type: 'message',
          text: 'ERROR: No user data'
        }
      };
    }

    /**
    * Below are the checks for various states that may occur
    * Note that the order of many of these blocks of code do matter
    */

    // situation where the incoming payload was received from the config popup
    if (query.state) {
      let settingsState = JSON.parse(query.state);
      if (settingsState.cardType) {
        userData.composeExtensionCardType = settingsState.cardType;
        await this.userState.saveChanges(context, false);
      }
      // need to keep going to return a response so do not return here

      // these variables are changed so if the word 'setting' kicked off the compose extension,
      // then the word setting will not retrigger the config experience
      queryParameter = "";
      initialRunParameter = "true";
      }

    // This is for MS-Teams support
    if (!userData.composeExtensionCardType && userData.channelId == "msteams") {
      let configResponse = this.getConfigResponse();
      return configResponse;
    }
   
    // this is the situation where the user has entered the word 'reset' and wants
    // to clear his/her settings
    // resetKeyword for English is "reset"
    if (queryParameter.toLowerCase() === 'reset') {
      userData.composeExtensionCardType = null;
      // this line is used to save the state for later use by the compose extension

      await this.userState.saveChanges(context, false);
      return {
        composeExtension: {
          type: 'message',
          text: 'your compose extension state has been reset'
        }
      };
    }

    if (
      queryParameter.toLowerCase() === 'setting' ||
      queryParameter.toLowerCase() === 'settings'
    ) {
      let configResponse = this.getConfigResponse();
      return configResponse;
    }

    // this is the situation where the user in on the initial run of the compose extension
    // e.g. when the user first goes to the compose extension and the search bar is still blank
    // in order to get the compose extension to run the initial run, the setting "initialRun": true
    // must be set in the manifest for the compose extension
    if (initialRunParameter) {
      return {
        composeExtension: {
          type: 'message',
          text: 'This Compose Extension is used to make queries to Wikipedia. To change your settings either enter the word\'settings\' or change your settings on the settings menu option. To reset your configuration, simply enter the word \'reset\'.'
        }
      };
    }

    /**
    * Below here is simply the logic to call the Wikipedia API and create the response for
    * a query; the general flow is to call the Wikipedia API for the query and then call the
    * Wikipedia API for each entry for the query. once all
    * of the Promises are resolved, the response is sent back to Teams
    */

    let searchApiUrl = searchApiUrlFormat.replace("[keyword]", queryParameter);
    searchApiUrl = searchApiUrl.replace("[limit]", query.queryOptions.count + "");
    searchApiUrl = searchApiUrl.replace("[offset]", query.queryOptions.skip + "");
    searchApiUrl = encodeURI(searchApiUrl);
    let promisesOfCardsAsAttachments = [];

    // call Wikipedia API to search
    return new Promise(function (resolve, reject) {
      request(searchApiUrl, (error, res, body) => {
        let wikiResults = JSON.parse(body).query.search;
        wikiResults.forEach((wikiResult) => {
          // highlight matched keyword
          let highlightedTitle = wikiResult.title;

          if (queryParameter) {
            let matches = highlightedTitle.match(new RegExp(queryParameter, "gi"));
            if (matches && matches.length > 0) {
              highlightedTitle = highlightedTitle.replace(new RegExp(queryParameter, "gi"), "<b>" + matches[0] + "</b>");
            }
          }

          // make title into a link
          highlightedTitle = "<a href=\"https://en.wikipedia.org/wiki/" + encodeURI(wikiResult.title) + "\" target=\"_blank\">" + highlightedTitle + "</a>";

          let cardText = wikiResult.snippet + " ...";

          // create the card itself and the preview card based upon the information

          // HeroCard extends ThumbnailCard so we can use ThumbnailCard as the overarching type
          let card = null;

          // This is for multi hub support (outlook and office)
          if(userData.channelId != "msteams")
          {
            userData.composeExtensionCardType = "thumbnail";            
          }

          // check user preference for which type of card to create
          if (userData.composeExtensionCardType === "thumbnail") {
            card = CardFactory.thumbnailCard(highlightedTitle, undefined, null, { text: cardText });
          } 
          else {
            // at this point session.userData.composeExtensionCardType === "hero"
            card = CardFactory.heroCard(highlightedTitle, undefined, null, { text: cardText });
          }
 
          // build the preview card that will show in the search results
          // Note: this is only needed if you want the cards in the search results to look
          // different from what is placed in the compose box
          let previewCard = CardFactory.thumbnailCard(highlightedTitle, undefined, null, { text: cardText });
          const attachment = { ...card, previewCard };
          promisesOfCardsAsAttachments.push(attachment);
          let response = {
            composeExtension: {
              type: 'result',
              attachmentLayout: 'list',
              attachments: promisesOfCardsAsAttachments
            }
          }
          resolve(response);
        });
      });
    });
  }

  async handleTeamsMessagingExtensionSelectItem(context, obj) {
    return {
      composeExtension: {
        type: 'result',
        attachmentLayout: 'list',
        attachments: [CardFactory.thumbnailCard(obj.description)]
      }
    };
  }

  async handleTeamsO365ConnectorCardAction(context, query) {
    var o365ActionQuery = query;
    var text = "Thanks, " + context.activity.from.name + "\nYour input action ID:" + o365ActionQuery.actionId + "\nYour input body:" + o365CardQuery.Body;
    await context.sendActivity(text);
  }

  // return the value of the specified query parameter
  getQueryParameterByName(query, name) {
    let matchingParams = (query.parameters || []).filter(p => p.name === name);
    return matchingParams.length ? matchingParams[0].value : "";
  }

  getConfigResponse() {
    // the width and height parameters are optional, but will be used to try and create a popup of that size
    // if that size popup cannot be created, as in this example, then Teams will create the largest allowed popup
    let hardCodedUrl = process.env.BaseUri + "/composeExtension/composeExtensionSettings.html?width=5000&height=5000";
    let cardAction = {
      type: ActionTypes.OpenUrl,
      title: "Config",
      value: hardCodedUrl
    }
    let response = {
      composeExtension: {
        type: 'config',
        text: 'ERROR: No user data',
        suggestedActions: {
          actions: [cardAction]
        }
      }
    }
    return response;
  }
}

module.exports.DialogBot = DialogBot;