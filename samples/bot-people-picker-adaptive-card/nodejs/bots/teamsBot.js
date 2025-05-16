// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory } = require("botbuilder");

/**
 * TeamsBot class that handles activities for a Teams Bot.
 */
class TeamsBot extends TeamsActivityHandler {
  constructor() {
    super();
    // Handling members added to the conversation
    this.onMembersAdded(async (context, next) => {
      const membersAdded = context.activity.membersAdded;

     for (const member of membersAdded) {
        if (member.id !== context.activity.recipient.id) {
          await context.sendActivity(
            "Hello and welcome! With this sample you can see the functionality of people-picker in adaptive card"
          );
        }
      }

      await next();
    });

    // Handling messages in the conversation
    this.onMessage(async (context, next) => {
        context.responded = true; // Set responded before proceeding

        const activity = this.removeMentionText(context.activity);

        if (activity.text) {
          const userCard =
            activity.conversation.conversationType === "personal"
              ? CardFactory.adaptiveCard(this.adaptiveCardForPersonalScope())
              : CardFactory.adaptiveCard(this.adaptiveCardForChannelScope());

          await context.sendActivity({ attachments: [userCard] });
        } else if (context.activity.value) {
          await context.sendActivity(
            `Task title: ${context.activity.value.taskTitle}, \n Task description: ${context.activity.value.taskDescription},\n Task assigned to: ${context.activity.value.userId}`
          );
        }

        await next(); // Proceed after setting responded
      });
  }

  /**
   * Removes the mention text from the activity if present.
   * @param {object} activity - The incoming activity.
   * @returns {object} - The updated activity without the mention text.
   */
  removeMentionText(activity) {
    if (activity.entities && activity.entities[0]?.type === "mention") {
      activity.text = activity.text.replace(activity.entities[0].text, "");
    }
    return activity;
  }


  /**
   * Adaptive card for personal scope.
   * @returns {object} - The adaptive card for personal scope.
   */
  
   adaptiveCardForPersonalScope = () => ({
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "version": "1.2",
    "type": "AdaptiveCard",
    "body": [
      {
        "type": "TextBlock",
        "size": "Medium",
        "weight": "Bolder",
        "text": "Task title"
      },
      {
        "type": "Input.Text",
        "placeholder": "Task title",
        "id": "taskTitle"
      },
      {
        "type": "TextBlock",
        "size": "Medium",
        "text": "Task description",
        "weight": "Bolder",
      },
      {
        "type": "Input.Text",
        "weight":"Bolder",
        "placeholder": "Task description",
        "id": "taskDescription"
      },
      {
        "columns": [
          {
            "width": "auto",
            "items": [
              {
                "text": "Select the member to assign the task:",
                "wrap": true,
                "height": "stretch",
                "type": "TextBlock",
                "weight":"Bolder",
              }
            ],
            "type": "Column"
          }
        ],
        "type": "ColumnSet"
      },
      {
        "columns": [
          {
            "width": "stretch",
            "items": [
              {
                "choices": [],
                "isMultiSelect": false,
                "style": "filtered",
                "choices.data": {
                  "type": "Data.Query",
                  "dataset": "graph.microsoft.com/users"
                },
                "id": "userId",
                "type": "Input.ChoiceSet"
              }
            ],
            "type": "Column"
          }
        ],
        "type": "ColumnSet"
      }
    ],
    "actions": [
      {
        "type": "Action.Submit",
        "id": "submitdynamic",
        "title": "Assign"
      }
    ]
  });

  // Adaptive card for groupchat and channel scope.
  adaptiveCardForChannelScope = () => ({
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "version": "1.2",
    "type": "AdaptiveCard",
    "body": [
      {
        "type": "TextBlock",
        "size": "Medium",
        "weight": "Bolder",
        "text": "Task title"
      },
      {
        "type": "Input.Text",
        "placeholder": "Task title",
        "id": "taskTitle",
      },
      {
        "type": "TextBlock",
        "size": "Medium",
        "weight": "Bolder",
        "text": "Task description"
      },
      {
        "type": "Input.Text",
        "placeholder": "Task description",
        "id": "taskDescription",
      },
      {
        "columns": [
          {
            "width": "auto",
            "items": [
              {
                "text": "Select the member to assign the task: ",
                "wrap": true,
                "height": "stretch",
                "type": "TextBlock",
                "weight":"Bolder",
              }
            ],
            "type": "Column"
          }
        ],
        "type": "ColumnSet"
      },
      {
        "columns": [
          {
            "width": "stretch",
            "items": [
              {
                "choices": [],
                "isMultiSelect": true,
                "style": "filtered",
                "choices.data": {
                  "type": "Data.Query",
                  "dataset": "graph.microsoft.com/users?scope=currentContext"
                },
                "id": "userId",
                "type": "Input.ChoiceSet"
              }
            ],
            "type": "Column"
          }
        ],
        "type": "ColumnSet"
      }
    ],
    "actions": [
      {
        "type": "Action.Submit",
        "id": "submitdynamic",
        "title": "Assign"
      }
    ]
  });
}

module.exports.TeamsBot = TeamsBot;