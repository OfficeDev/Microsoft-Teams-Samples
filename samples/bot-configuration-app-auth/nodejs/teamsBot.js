const { stripMentionsText } = require("@microsoft/teams.api");
const { LocalStorage } = require("@microsoft/teams.common");
const fs = require('fs');
const path = require('path');

// Create storage for conversation history
const storage = new LocalStorage();

class TeamsBot {
  constructor() {
    this.storage = storage;
  }

  /**
   * Handle config/fetch invoke activity
   * This is called when the bot is added to a team or group chat or when user clicks settings
   */
  async handleConfigFetch(context) {
    // Return auth type with suggested actions (matching original sample)
    return {
      config: {
        type: "auth",
        suggestedActions: {
          actions: [
            {
              type: "openUrl",
              value: "https://example.com/auth/signin",
              title: "Sign in to this app"
            }
          ]
        }
      }
    };
  }

  /**
   * Handle config/submit invoke activity
   * This is called when the user submits the configuration card
   */
  async handleConfigSubmit(context) {
    // Return just the body
    return {
      config: {
        type: "message",
        value: "Configuration submitted. Please complete the sign-in process."
      }
    };
  }

  /**
   * Handle application/search invoke activity for type-ahead search
   * Not used in the original simplified sample, but kept for extensibility
   */
  async handleSearchInvoke(context) {
    // Original sample doesn't implement search
    // Return empty results
    const response = {
      status: 200,
      body: {
        type: "application/vnd.microsoft.search.searchResponse",
        value: {
          results: []
        }
      }
    };

    return response;
  }

  /**
   * Handle regular messages
   * In the original sample, the bot doesn't respond to regular messages
   * The main functionality is through the configuration flow
   */
  async handleMessage(context) {
    // The original implementation doesn't handle regular messages
    // All interaction is through the config/fetch and config/submit flow
    // You can add custom message handling here if needed
  }

  /**
   * Handle conversation update events
   */
  async handleConversationUpdate(context) {
    const activity = context.activity;
    
    if (activity.membersAdded && activity.membersAdded.length > 0) {
      for (const member of activity.membersAdded) {
        if (member.id !== activity.recipient.id) {
          // Try to read image, use placeholder if not found
          let imageBase64 = '';
          const imagePath = path.join(__dirname, 'Images', 'configbutton.png');
          
          try {
            if (fs.existsSync(imagePath)) {
              const imageBuffer = fs.readFileSync(imagePath);
              imageBase64 = imageBuffer.toString('base64');
            }
          } catch (error) {
            // Image not found, using text only card
          }

          const welcomeCard = {
            type: "AdaptiveCard",
            body: [
              {
                type: "TextBlock",
                text: "Hello and welcome! With this sample, you can experience the functionality of bot configuration. To access Bot configuration, click on the settings button in the bot description card.",
                wrap: true,
                size: "Large",
                weight: "Bolder"
              }
            ],
            $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
            version: "1.5",
            fallbackText: "This card requires Adaptive Card support."
          };

          // Add image if available
          if (imageBase64) {
            welcomeCard.body.push({
              type: "Image",
              url: `data:image/png;base64,${imageBase64}`,
              size: "Auto"
            });
          }

          await context.send({
            type: "message",
            text: '',
            attachments: [
              {
                contentType: "application/vnd.microsoft.card.adaptive",
                content: welcomeCard
              }
            ]
          });
        }
      }
    }
  }
}

module.exports = TeamsBot;
