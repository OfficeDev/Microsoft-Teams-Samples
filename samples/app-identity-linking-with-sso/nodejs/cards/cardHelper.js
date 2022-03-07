const { ActionTypes } = require('botbuilder');

// Get user card.
const getAdaptiveCardUserDetails = (myDetails, userImage) => (
  [getAADDetailsCard(myDetails, userImage),
  {
    "contentType": "application/vnd.microsoft.card.hero",
    "content": {
      "buttons": [
        {
          "type": ActionTypes.ImBack,
          "title": "Connect to facebook",
          "value": "connectToFacebook"
        }
      ]
    }
  },
  {
    "contentType": "application/vnd.microsoft.card.hero",
    "content": {
      "buttons": [
        {
          "type": ActionTypes.ImBack,
          "title": "Connect to google",
          "value": "connectToGoogle"
        }
      ]
    }
  }
  ]);

// Aad details card.
const getAADDetailsCard = (myDetails, userImage) => (
  {
    "contentType": "application/vnd.microsoft.card.adaptive",
    "content": {
      "type": "AdaptiveCard",
      "version": "1.0",
      "body": [
        {
          "type": "TextBlock",
          "size": "Medium",
          "weight": "Bolder",
          "text": "User profile details are"
        },
        {
          "type": "Image",
          "size": "Medium",
          "url": userImage
        },
        {
          "type": "TextBlock",
          "size": "Medium",
          "weight": "Bolder",
          "wrap": true,
          "text": `Hello! ${myDetails.displayName}`
        },
        {
          "type": "TextBlock",
          "size": "Medium",
          "weight": "Bolder",
          "text": `Job title: ${myDetails.jobDetails ? myDetails.jobDetails : "Unknown"}`
        },
        {
          "type": "TextBlock",
          "size": "Medium",
          "weight": "Bolder",
          "text": `Email: ${myDetails.userPrincipalName}`
        }
      ]
    }
  });

  // Facebook details card.
const getFacebookDetailsCard = (facebookProfile) => ({
  "contentType": "application/vnd.microsoft.card.hero",
  "content": {
    "title": 'Hello: ' + facebookProfile.name,
    "images": [
      {
        "url": facebookProfile.picture.data.url
      }
    ],
    "buttons": [
      {
        "type": ActionTypes.ImBack,
        "title": "Disconnect from facebook",
        "value": "DisConnectFromFacebook"
      }
    ]
  }
});

// Google details card.
const getGoogleDetailsCard = (googleProfile) => ({
  "contentType": "application/vnd.microsoft.card.hero",
  "content": {
    "title": 'Hello: ' + googleProfile.names[0].displayName,
    "subtitle": 'Email: ' + googleProfile.emailAddresses[0].value,
    "images": [
      {
        "url": googleProfile.photos[0].url
      }
    ],
    "buttons": [
      {
        "type": ActionTypes.ImBack,
        "title": "Disconnect from google",
        "value": "DisConnectFromGoogle"
      }
    ]
  }
});

// Connect to facebook card.
const getConnectToFacebookCard = () => ({
  "contentType": "application/vnd.microsoft.card.hero",
  "content": {
    "buttons": [
      {
        "type": ActionTypes.ImBack,
        "title": "Connect to facebook",
        "value": "connectToFacebook"
      }
    ]
  }
});

// Connect to google card.
const getConnectToGoogleCard = () => ({
  "contentType": "application/vnd.microsoft.card.hero",
  "content": {
    "buttons": [
      {
        "type": ActionTypes.ImBack,
        "title": "Connect to google",
        "value": "connectToGoogle"
      }
    ]
  }
});

// ME action response card.
const getMEResponseCard = (myDetails, userImage, facebookProfile, googleProfile) => {
  var card = {
    "contentType": "application/vnd.microsoft.card.adaptive",
    "content": {
      "type": "AdaptiveCard",
      "version": "1.0",
      "body": [
        {
          "type": "TextBlock",
          "size": "Medium",
          "weight": "Bolder",
          "text": "User sso details are"
        },
        {
          "type": "Image",
          "size": "Medium",
          "url": userImage
        },
        {
          "type": "TextBlock",
          "size": "Medium",
          "weight": "Bolder",
          "wrap": true,
          "text": `Hello! ${myDetails.displayName}`
        },
        {
          "type": "TextBlock",
          "size": "Medium",
          "weight": "Bolder",
          "text": `Job title: ${myDetails.jobDetails ? myDetails.jobDetails : "Unknown"}`
        },
        {
          "type": "TextBlock",
          "size": "Medium",
          "weight": "Bolder",
          "text": `Email: ${myDetails.userPrincipalName}`
        }
      ],
      "actions": [],
    }
  }

  if(facebookProfile.is_fb_signed_in){
    card.content.body.push({
      "type": "TextBlock",
      "size": "Medium",
      "weight": "Bolder",
      "text": "User facebook details are",
      "separator":true
    });
    card.content.body.push({
      "type": "Image",
      "size": "Medium",
      "url": facebookProfile.image
    });
    card.content.body.push({
      "type": "TextBlock",
      "size": "Medium",
      "weight": "Bolder",
      "wrap": true,
      "text": `Hello! ${facebookProfile.name}`
    });
  card.content.actions.push({
    "type": "Action.Submit",
    "title": "Disconnect from facebook",
    "data": {
      "msteams": {
        "id": "dicconnectFromFacebook"
      }
    }
});
  }
  else{
    card.content.actions.push({
      "type": "Action.Submit",
      "title": "Connect with facebook",
      "data": {
        "msteams": {
          "id": "connectWithFacebook"
        }
      }
    })
  }
  if(googleProfile.is_google_signed_in){
    card.content.body.push({
      "type": "TextBlock",
      "size": "Medium",
      "weight": "Bolder",
      "text": "User google details are",
      "separator":true
    });
    card.content.body.push({
      "type": "Image",
      "size": "Medium",
      "url": googleProfile.image
    });
    card.content.body.push({
      "type": "TextBlock",
      "size": "Medium",
      "weight": "Bolder",
      "wrap": true,
      "text": `Hello! ${googleProfile.name}`
    });
    card.content.body.push(
      {
        "type": "TextBlock",
        "when": !googleProfile.is_google_signed_in,
        "size": "Medium",
        "weight": "Bolder",
        "wrap": true,
        "text": 'Email: ' + googleProfile.email,
      });
      card.content.actions.push({
        "type": "Action.Submit",
        "title": "Disconnect from google",
        "data": {
          "msteams": {
            "id": "disConnectFromGoogle"
          }
        }
      });
  }
  else{
    card.content.actions.push({
      "type": "Action.Submit",
      "title": "Connect with google",
      "data": {
        "msteams": {
          "id": "connectWithGoogle"
        }
      }
    })
  }
  return card
};

const getMELinkUnfurlingCard = (myDetails, userImage, facebookProfile, googleProfile)=>{
  var card = {
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "type": "AdaptiveCard",
    "version": "1.0",
    "body": [
      {
        "type": "TextBlock",
        "size": "Medium",
        "weight": "Bolder",
        "text": "User sso details are"
      },
      {
        "type": "Image",
        "size": "Medium",
        "url": userImage
      },
      {
        "type": "TextBlock",
        "size": "Medium",
        "weight": "Bolder",
        "wrap": true,
        "text": `Hello! ${myDetails.displayName}`
      },
      {
        "type": "TextBlock",
        "size": "Medium",
        "weight": "Bolder",
        "text": `Job title: ${myDetails.jobDetails ? myDetails.jobDetails : "Unknown"}`
      },
      {
        "type": "TextBlock",
        "size": "Medium",
        "weight": "Bolder",
        "text": `Email: ${myDetails.userPrincipalName}`
      }
    ],
    "actions":[]
  }
  if(facebookProfile.is_fb_signed_in){
    card.body.push({
      "type": "TextBlock",
      "size": "Medium",
      "weight": "Bolder",
      "text": "User facebook details are",
      "separator":true
    });
    card.body.push({
      "type": "Image",
      "size": "Medium",
      "url": facebookProfile.image
    });
    card.body.push({
      "type": "TextBlock",
      "size": "Medium",
      "weight": "Bolder",
      "wrap": true,
      "text": `Hello! ${facebookProfile.name}`
    });
    card.actions.push({ 
      "type": "Action.Submit", 
      "title": "Disconnect from facebook",
       "data": { 
         "msteams": { 
           "type": "imBack",
            "text": "Disconnect from facebook", 
            "value": "DisconnectFromFacebookLinkUnfurl" 
          } 
        }
       });
  }
  if(googleProfile.is_google_signed_in){
    card.body.push({
      "type": "TextBlock",
      "size": "Medium",
      "weight": "Bolder",
      "text": "User google details are",
      "separator":true
    });
    card.body.push({
      "type": "Image",
      "size": "Medium",
      "url": googleProfile.image
    });
    card.body.push({
      "type": "TextBlock",
      "size": "Medium",
      "weight": "Bolder",
      "wrap": true,
      "text": `Hello! ${googleProfile.name}`
    });
    card.body.push(
      {
        "type": "TextBlock",
        "when": !googleProfile.is_google_signed_in,
        "size": "Medium",
        "weight": "Bolder",
        "wrap": true,
        "text": 'Email: ' + googleProfile.email,
      });
      card.actions.push({ 
        "type": "Action.Submit", 
        "title": "Disconnect from google",
         "data": { 
           "msteams": { 
             "type": "imBack",
              "text": "Disconnect from google", 
              "value": "DisconnectFromGoogleLinkUnfurl" 
            } 
          }
         });
  }

  return card;
}

module.exports = {
  getGoogleDetailsCard,
  getFacebookDetailsCard,
  getAADDetailsCard,
  getAdaptiveCardUserDetails,
  getConnectToFacebookCard,
  getConnectToGoogleCard,
  getMEResponseCard,
  getMELinkUnfurlingCard
};