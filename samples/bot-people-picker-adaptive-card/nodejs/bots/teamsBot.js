// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory } = require("botbuilder");

class TeamsBot extends TeamsActivityHandler {
  constructor() {
    super();
    this.onMembersAdded(async (context, next) => {
      const membersAdded = context.activity.membersAdded;
      for (let member = 0; member < membersAdded.length; member++) {
        if (membersAdded[member].id !== context.activity.recipient.id) {
          await context.sendActivity("Hello and welcome! With this sample you can see the functionality of people-picker in adaptive card");
        }
      }

      await next();
    });

    this.onMessage(async (context, next) => {
      var activity = this.removeMentionText(context.activity);
      if (activity.text != null) {
        if (activity.conversation.conversationType =="personal") {
          const userCard = CardFactory.adaptiveCard(this.adaptiveCardForPersonalScope());

          await context.sendActivity({ attachments: [userCard] });
        }
        else if (activity.conversation.conversationType !="personal") {
          const userCard = CardFactory.adaptiveCard(this.adaptiveCardForChannelScope());

          await context.sendActivity({ attachments: [userCard] });
        }
      }
      else if (context.activity.value != null) {
        await context.sendActivity(`Task title: ${context.activity.value.taskTitle}, \n Task description : ${context.activity.value.taskDescription},\n Task assigned to : ${context.activity.value.userId}` );
      }

      // By calling next() you ensure that the next BotHandler is run.
      await next();
    });
  }

  removeMentionText(activity) {
    var updatedActivity = activity;

    if (activity.entities[0].type == "mention") {
        updatedActivity.text = activity.text.replace(activity.entities[0].text, "");
        return updatedActivity;
    }

    return activity;
  }

  // Adaptive card for personal scope.
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