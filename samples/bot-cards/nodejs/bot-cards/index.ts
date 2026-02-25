// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { stripMentionsText } from "@microsoft/teams.api";
import { App } from "@microsoft/teams.apps";
import { 
  AdaptiveCard, 
  TextBlock, 
  TextInput, 
  SubmitAction, 
  OpenUrlAction, 
  ShowCardAction, 
  ToggleVisibilityAction
} from "@microsoft/teams.cards";

const app = new App();

function createCardActionsCard(): AdaptiveCard {
  const innermostCard = new AdaptiveCard(
    new TextBlock('**Welcome To Your New Card**'),
    new TextBlock('This is your new card inside another card')
  );

  const middleCard = new AdaptiveCard(
    new TextBlock("This card's action will show another card")
  ).withActions(
    new ShowCardAction({
      title: 'Action.ShowCard',
      card: innermostCard
    })
  );

  const submitFormCard = new AdaptiveCard(
    new TextInput({
      id: 'name',
      label: 'Please enter your name:',
      isRequired: true,
      errorMessage: 'Name is required'
    })
  ).withActions(
    new SubmitAction({ title: 'Submit' })
  );

  return new AdaptiveCard(
    new TextBlock('Adaptive Card Actions')
  ).withActions(
    new OpenUrlAction('https://adaptivecards.io', {
      title: 'Action Open URL'
    }),
    new ShowCardAction({
      title: 'Action Submit',
      card: submitFormCard
    }),
    new ShowCardAction({
      title: 'Action ShowCard',
      card: middleCard
    })
  );
}

function createToggleVisibilityCard(): AdaptiveCard {
  return new AdaptiveCard(
    new TextBlock('**Action.ToggleVisibility example**: click the button to show or hide the welcome message', {
      wrap: true
    }),
    new TextBlock('**Hello World!**', {
      id: 'helloWorld',
      isVisible: false,
      size: 'ExtraLarge'
    })
  ).withActions(
    new ToggleVisibilityAction({
      title: 'Click me!',
      targetElements: ['helloWorld']
    })
  );
}

app.on("message", async (context) => {
  const activity = context.activity;
  const text = stripMentionsText(activity) || "";
  
  if (activity.value) {
    await context.send(`Data Submitted: ${activity.value.name}`);
  } else if (text) {
    const normalizedText = text.trim().toLowerCase();
    
    if (normalizedText.includes('card actions')) {
      const card = createCardActionsCard();
      await context.send({
        type: 'message',
        attachments: [{
          contentType: 'application/vnd.microsoft.card.adaptive',
          content: JSON.parse(JSON.stringify(card))
        }]
      });
    } 
    else if (normalizedText.includes('toggle visibility')) {
      const card = createToggleVisibilityCard();
      await context.send({
        type: 'message',
        attachments: [{
          contentType: 'application/vnd.microsoft.card.adaptive',
          content: JSON.parse(JSON.stringify(card))
        }]
      });
    } 
    else {
      await context.send("Welcome to the Cards Bot! To interact with me, send one of the following commands: 'card actions' or 'toggle visibility'");
    }
  }
});

app.start().catch(console.error);