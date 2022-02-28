const { ActionTypes } = require('botbuilder');

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

const getFacebookDetailsCard = (facbookProfile) => ({
    "contentType": "application/vnd.microsoft.card.hero",
    "content": {
      "title": 'Hello: ' + facbookProfile.name,
      "images": [
        {
          "url": facbookProfile.picture.data.url
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
  })
module.exports = {
    getGoogleDetailsCard,
    getFacebookDetailsCard,
    getAADDetailsCard,
    getAdaptiveCardUserDetails,
    getConnectToFacebookCard,
    getConnectToGoogleCard
};