const fs = require('fs');

/**
 * Teams Bot Configuration class
 * Handles configuration-related functionality using configurable adaptive cards
 */
class TeamsBot {
  constructor(storage) {
    this.storage = storage;
  }

  /**
   * Handle member added event - show welcome card with immediate configuration interface
   */
  async handleMembersAdded(context) {
    const membersAdded = context.activity.membersAdded;
    
    if (!membersAdded || membersAdded.length === 0) {
      return;
    }
    
    for (const member of membersAdded) {
      if (member.id !== context.activity.recipient.id) {
        
        try {
          // Show the welcome message first
          await context.send("**Bot Configuration Demo** - Hello! This bot demonstrates configurable adaptive cards.");
          
          // For personal conversations, just show a simple message
          if (context.activity.conversation.conversationType === 'personal') {
            await context.send("Add me to a group chat or team to see the full configuration interface!");
            return;
          }
          
          // For group/team conversations, show the configuration form
          const configCard = this.getConfigurationCard();
          
          // Send text message first
          await context.send("**Bot Configuration Setup** - Please configure the bot settings below:");
          
          // Then send the adaptive card separately
          await context.send(configCard);
          
        } catch (error) {
          console.error("Error sending messages:", error);
          // Fallback to simple text message
          await context.send("Hello! Bot configuration is available. Type 'config' to see options.");
        }
      }
    }
  }

  /**
   * Handle incoming messages and adaptive card actions
   */
  async handleMessage(context) {
    // Check if this is a configuration form submission
    if (context.activity.value && (
        context.activity.value.dropdown01 || 
        context.activity.value.dropdown02 || 
        context.activity.value.dropdown1)) {
      // Process the configuration submission
      await this.processConfigurationSubmission(context);
      return;
    }

    // Handle text commands
    const messageText = context.activity.text ? context.activity.text.toLowerCase().trim() : '';
    
    if (messageText === 'config' || messageText === 'configure' || messageText === 'setup') {
      try {
        // Show the configuration form
        await context.send("**Bot Configuration** - Please configure the bot settings below:");
        
        const configCard = this.getConfigurationCard();
        await context.send(configCard);
        
      } catch (error) {
        console.error("Error sending config card:", error);
        await context.send("Configuration interface is temporarily unavailable. Please try again later.");
      }
      return;
    }

    // Default message response - show config for any message in group chat
    if (context.activity.conversation.conversationType === 'groupChat' || 
        context.activity.conversation.conversationType === 'channel') {
      
      try {
        await context.send("**Bot Configuration** - Here is the configuration interface:");
        
        const configCard = this.getConfigurationCard();
        await context.send(configCard);
        
      } catch (error) {
        console.error("Error sending config card:", error);
        await context.send("Configuration interface is temporarily unavailable. Please try again later.");
      }
    } else {
      await context.send("Hello! This bot demonstrates configurable adaptive cards. Type 'config' to configure the bot, or add the bot to a team/group chat to see the configuration interface automatically.");
    }
  }

  /**
   * Handles the Teams configuration fetch event - called when user opens bot settings
   * This method provides the configuration form/card that users will fill out
   */
  async handleTeamsConfigFetch(context, configData) {
    const adaptiveCard = this.getConfigurationCard();
    return {
      config: {
        value: {
          card: { contentType: 'application/vnd.microsoft.card.adaptive', content: adaptiveCard },
          height: 500,
          width: 600,
          title: 'Bot Configuration',
        },
        type: 'continue',
      },
    };
  }

  /**
   * Process configuration form submission from adaptive card action
   */
  async processConfigurationSubmission(context) {
    const data = context.activity.value;
    const {
      dropdown01: dropdown01Value,
      dropdown02: dropdown02Value,
      dropdown1: dropdown1Value,
      dropdown2: dropdown2Value,
      dropdown3: dropdown3Value,
      dropdown4: dropdown4Value,
      togglestatus,
      toggleAssign,
      toggleComment,
      toggleTransition
    } = data;

    // Create result card showing the selected configuration (same as original)
    const resultCard = {
      $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
      version: '1.2',
      type: 'AdaptiveCard',
      body: [
        {
          type: 'TextBlock',
          text: 'The selection you requested is as follows:',
          weight: 'bolder',
          wrap: true
        }
      ]
    };

    // Add selected values to the result card (same logic as original)
    if (dropdown01Value) {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Type : ${dropdown01Value}`,
        wrap: true
      });
    }

    if (dropdown02Value) {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Priority : ${dropdown02Value}`,
        wrap: true
      });
    }

    if (dropdown1Value) {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Issue : ${dropdown1Value}`,
        wrap: true
      });
    }

    if (dropdown2Value) {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Comment : ${dropdown2Value}`,
        wrap: true
      });
    }

    if (dropdown3Value) {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Assignee : ${dropdown3Value}`,
        wrap: true
      });
    }

    if (dropdown4Value) {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Status : ${dropdown4Value}`,
        wrap: true
      });
    }

    resultCard.body.push({
      type: 'TextBlock',
      text: 'Actions to be displayed:',
      weight: 'bolder',
      wrap: true
    });

    if (togglestatus === 'true') {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Status : ${togglestatus}`,
        wrap: true
      });
    }

    if (toggleAssign === 'true') {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Assign : ${toggleAssign}`,
        wrap: true
      });
    }

    if (toggleComment === 'true') {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Comment : ${toggleComment}`,
        wrap: true
      });
    }

    if (toggleTransition === 'true') {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Transition : ${toggleTransition}`,
        wrap: true
      });
    }

    // Send the result card
    try {
      await context.send("**Configuration Saved Successfully!**");
      await context.send(resultCard);
    } catch (error) {
      console.error("Error sending result card:", error);
      await context.send("Configuration saved successfully!");
    }

    // Send confirmation message
    await context.send("Your bot configuration has been submitted successfully!");
  }

  /**
   * Handles the Teams configuration submit event - called when user submits the config form
   * This processes the submitted form data and shows the results
   */
  async handleTeamsConfigSubmit(context, configData) {
    const data = context.activity.value.data;
    const {
      dropdown01: dropdown01Value,
      dropdown02: dropdown02Value,
      dropdown1: dropdown1Value,
      dropdown2: dropdown2Value,
      dropdown3: dropdown3Value,
      dropdown4: dropdown4Value,
      togglestatus,
      toggleAssign,
      toggleComment,
      toggleTransition
    } = data;

    // Create result card showing the selected configuration
    const resultCard = {
      $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
      version: '1.2',
      type: 'AdaptiveCard',
      body: [
        {
          type: 'TextBlock',
          text: 'The selection you requested is as follows:',
          weight: 'bolder',
          wrap: true
        }
      ]
    };

    // Add selected values to the result card
    if (dropdown01Value) {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Type : ${dropdown01Value}`,
        wrap: true
      });
    }

    if (dropdown02Value) {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Priority : ${dropdown02Value}`,
        wrap: true
      });
    }

    if (dropdown1Value) {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Issue : ${dropdown1Value}`,
        wrap: true
      });
    }

    if (dropdown2Value) {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Comment : ${dropdown2Value}`,
        wrap: true
      });
    }

    if (dropdown3Value) {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Assignee : ${dropdown3Value}`,
        wrap: true
      });
    }

    if (dropdown4Value) {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Status : ${dropdown4Value}`,
        wrap: true
      });
    }

    resultCard.body.push({
      type: 'TextBlock',
      text: 'Actions to be displayed:',
      weight: 'bolder',
      wrap: true
    });

    if (togglestatus === 'true') {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Status : ${togglestatus}`,
        wrap: true
      });
    }

    if (toggleAssign === 'true') {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Assign : ${toggleAssign}`,
        wrap: true
      });
    }

    if (toggleComment === 'true') {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Comment : ${toggleComment}`,
        wrap: true
      });
    }

    if (toggleTransition === 'true') {
      resultCard.body.push({
        type: 'TextBlock',
        text: `Transition : ${toggleTransition}`,
        wrap: true
      });
    }

    // Send the result card as a message
    await context.send({
      attachments: [{ contentType: 'application/vnd.microsoft.card.adaptive', content: resultCard }]
    });

    return {
      config: {
        type: 'message',
        value: 'Your request has been submitted successfully!',
      },
    };
  }
  /**
   * Generates the configuration adaptive card form
   */
  getConfigurationCard() {
    return {
      "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
      "version": "1.4",
      "type": "AdaptiveCard",
      "body": [
        {
          "type": "ColumnSet",
          "columns": [
            {
              "type": "Column",
              "width": "stretch",
              "items": [
                {
                  "type": "TextBlock",
                  "text": "For issues that match these criteria:",
                  "weight": "bolder"
                }
              ]
            }
          ]
        },
        {
          "type": "ColumnSet",
          "columns": [
            {
              "type": "Column",
              "width": "stretch",
              "items": [
                {
                  "type": "TextBlock",
                  "text": "Type",
                  "weight": "bolder"
                },
                {
                  "type": "Input.ChoiceSet",
                  "id": "dropdown01",
                  "choices": [
                    {
                      "title": "Bug",
                      "value": "Bug"
                    },
                    {
                      "title": "Feature Request",
                      "value": "Feature Request"
                    },
                    {
                      "title": "Task",
                      "value": "Task"
                    }
                  ]
                }
              ]
            },
            {
              "type": "Column",
              "width": "stretch",
              "items": [
                {
                  "type": "TextBlock",
                  "text": "Priority",
                  "weight": "bolder"
                },
                {
                  "type": "Input.ChoiceSet",
                  "id": "dropdown02",
                  "choices": [
                    {
                      "title": "Low",
                      "value": "Low"
                    },
                    {
                      "title": "Medium",
                      "value": "Medium"
                    },
                    {
                      "title": "High",
                      "value": "High"
                    }
                  ]
                }
              ]
            }
          ]
        },
        {
          "type": "ColumnSet",
          "columns": [
            {
              "type": "Column",
              "width": "stretch",
              "items": [
                {
                  "type": "TextBlock",
                  "text": "Post to channel when :",
                  "weight": "bolder"
                }
              ]
            }
          ]
        },
        {
          "type": "ColumnSet",
          "columns": [
            {
              "type": "Column",
              "width": "stretch",
              "items": [
                {
                  "type": "TextBlock",
                  "text": "Issue",
                  "weight": "bolder"
                },
                {
                  "type": "Input.ChoiceSet",
                  "id": "dropdown1",
                  "isMultiSelect": true,
                  "choices": [
                    {
                      "title": "Software Issue",
                      "value": "Software Issue"
                    },
                    {
                      "title": "Server Issue",
                      "value": "Server Issue"
                    },
                    {
                      "title": "Network Issue",
                      "value": "Network Issue"
                    }
                  ]
                }
              ]
            },
            {
              "type": "Column",
              "width": "stretch",
              "items": [
                {
                  "type": "TextBlock",
                  "text": "Comment",
                  "weight": "bolder"
                },
                {
                  "type": "Input.ChoiceSet",
                  "id": "dropdown2",
                  "choices": [
                    {
                      "title": "Network problem in server",
                      "value": "Network problem in server"
                    },
                    {
                      "title": "Loadbalancer issue",
                      "value": "Loadbalancer issue"
                    },
                    {
                      "title": "Software needs to be updated",
                      "value": "Software needs to be updated"
                    }
                  ]
                }
              ]
            }
          ]
        },
        {
          "type": "ColumnSet",
          "columns": [
            {
              "type": "Column",
              "width": "stretch",
              "items": [
                {
                  "type": "TextBlock",
                  "text": "Assignee",
                  "weight": "bolder"
                },
                {
                  "type": "Input.ChoiceSet",
                  "id": "dropdown3",
                  "choices": [
                    {
                      "title": "Jasmine Smith",
                      "value": "Jasmine Smith"
                    },
                    {
                      "title": "Ethan Johnson",
                      "value": "Ethan Johnson"
                    },
                    {
                      "title": "Maya Rodriguez",
                      "value": "Maya Rodriguez"
                    }
                  ]
                }
              ]
            },
            {
              "type": "Column",
              "width": "stretch",
              "items": [
                {
                  "type": "TextBlock",
                  "text": "Status changed",
                  "weight": "bolder"
                },
                {
                  "type": "Input.ChoiceSet",
                  "id": "dropdown4",
                  "choices": [
                    {
                      "title": "Open",
                      "value": "Open"
                    },
                    {
                      "title": "Inprogress",
                      "value": "Inprogress"
                    },
                    {
                      "title": "Completed",
                      "value": "Completed"
                    }
                  ]
                }
              ]
            }
          ]
        },
        {
          "type": "ColumnSet",
          "columns": [
            {
              "type": "Column",
              "width": "stretch",
              "items": [
                {
                  "type": "TextBlock",
                  "text": "Actions to display",
                  "weight": "bolder"
                }
              ]
            }
          ]
        },
        {
          "type": "ColumnSet",
          "columns": [
            {
              "type": "Column",
              "width": "stretch",
              "items": [
                {
                  "type": "Input.Toggle",
                  "title": "Assign",
                  "id": "toggleAssign",
                  "value": "false"
                }
              ]
            }
          ]
        },
        {
          "type": "ColumnSet",
          "columns": [
            {
              "type": "Column",
              "width": "stretch",
              "items": [
                {
                  "type": "Input.Toggle",
                  "title": "Comment",
                  "id": "toggleComment",
                  "value": "false"
                }
              ]
            }
          ]
        },
        {
          "type": "ColumnSet",
          "columns": [
            {
              "type": "Column",
              "width": "stretch",
              "items": [
                {
                  "type": "Input.Toggle",
                  "title": "Transition",
                  "id": "toggleTransition",
                  "value": "false"
                }
              ]
            }
          ]
        },
        {
          "type": "ColumnSet",
          "columns": [
            {
              "type": "Column",
              "width": "stretch",
              "items": [
                {
                  "type": "Input.Toggle",
                  "title": "Update status",
                  "id": "togglestatus",
                  "value": "false"
                }
              ]
            }
          ]
        }
      ],
      "actions": [
        {
          "type": "Action.Submit",
          "id": "submit",
          "title": "Submit"
        }
      ]
    };
  }
}

module.exports = { TeamsBot };