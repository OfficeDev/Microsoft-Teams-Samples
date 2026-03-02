// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { SimpleGraphClient } = require('../simpleGraphClient');
const { polyfills } = require('isomorphic-fetch');
const msal = require('@azure/msal-node');

var delegatedToken = "";

const getGraphAccessToken = async (req, res) => {
    var tid = process.env.MicrosoftAppTenantId;
    var token = req.query.ssoToken;
    var chatId = req.query.chatId;
    var scopes = ["https://graph.microsoft.com/User.Read"];

  // Creating MSAL client
  const msalClient = new msal.ConfidentialClientApplication({
    auth: {
      clientId: process.env.MicrosoftAppId,
      clientSecret: process.env.MicrosoftAppPassword
    }
  });

  var oboPromise = new Promise((resolve, reject) => {
    msalClient.acquireTokenOnBehalfOf({
      authority: `https://login.microsoftonline.com/${tid}`,
      oboAssertion: token,
      scopes: scopes,
      skipCache: true
    }).then(result => {
      delegatedToken = result.accessToken
      resolve();
    }).catch(error => {
      reject({ "error": error.errorCode });
    });
  });

  oboPromise.then(async function (result) {
    const client = new SimpleGraphClient(delegatedToken);
    try 
    {
    var pinnedMessageResponse = await client.getPinnedMessageList(chatId);
    var recentMessages = await client.getRecentMessageList(chatId);

    var messageList = new Array();
    recentMessages.value.map(message => {
        if(message.messageType == "message") {
            var messageDetails = {
                id: message.id,
                value: message.body.content.replace(/<[^>]+>/g, '')
            }
    
            messageList.push(messageDetails);
        }
    })

    var responseMessageData = {
        id: pinnedMessageResponse.value[0].id,
        message: pinnedMessageResponse.value[0].message.body.content.replace(/<[^>]+>/g, ''),
        messages: messageList
    }

    res.send(responseMessageData);
    }
    catch (ex) 
    {
    console.error(ex);
    res.status(500).send();
    }
    
  }, function (err) {
    console.log(err); // Error: "It broke"
    res.json(err);
  });
}

/** Pin new message in chat. */
const pinNewMessage = async(req, res) => {
    var chatId = req.query.chatId;
    var newMessageId = req.query.messageId;

    try 
    {
       const client = new SimpleGraphClient(delegatedToken);
       var newPinnedMessageResponse = await client.pinNewMessage(chatId, newMessageId);
        res.status(204).send();
    }
    catch (ex) 
    {
        console.error(ex);
        res.status(500).send();
    }
}

/** Unpin existing pinned message. */
const unpinMessage = async(req, res) => {
    var chatId = req.query.chatId;
    var pinnedMessageId = req.query.pinnedMessageId;

    try 
    {
       const client = new SimpleGraphClient(delegatedToken);
       var unpinnedMessageResponse = await client.unpinMessage(chatId, pinnedMessageId);
        res.status(204).send();
    }
    catch (ex) 
    {
        console.error(ex);
        res.status(500).send();
    }
}

module.exports = {
    getGraphAccessToken,
    pinNewMessage,
    unpinMessage
};