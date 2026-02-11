// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { 
  AdaptiveCard, 
  TextBlock, 
  TextInput, 
  SubmitAction, 
  OpenUrlAction, 
  ShowCardAction, 
  ToggleVisibilityAction
} from "@microsoft/teams.cards";

// Send Adaptive Card with various actions
export async function SendAdaptiveCardActions(context: any) {
  // Build innermost nested card
  const nestedCard = new AdaptiveCard(
    new TextBlock('Welcome To New Card')
  ).withActions(
    new SubmitAction({
      title: 'Click Me',
      data: { value: 'Button has Clicked' }
    })
  );

  // Build middle card
  const showCard = new AdaptiveCard(
    new TextBlock("This card's action will show another card")
  ).withActions(
    new ShowCardAction({
      title: 'Action.ShowCard',
      card: nestedCard
    })
  );

  // Build submit card
  const submitCard = new AdaptiveCard(
    new TextInput({
      id: 'name',
      label: 'Please enter your name:',
      isRequired: true,
      errorMessage: 'Name is required'
    })
  ).withActions(
    new SubmitAction({ title: 'Submit' })
  );

  // Build main card
  const card = new AdaptiveCard(
    new TextBlock('Adaptive Card Actions')
  ).withActions(
    new OpenUrlAction('https://adaptivecards.io', {
      title: 'Action Open URL'
    }),
    new ShowCardAction({
      title: 'Action Submit',
      card: submitCard
    }),
    new ShowCardAction({
      title: 'Action ShowCard',
      card: showCard
    })
  );
  
  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.adaptive',
      content: JSON.parse(JSON.stringify(card))
    }]
  });
}

// Send Toggle Visibility Card
export async function SendToggleVisibilityCard(context: any) {
  const card = new AdaptiveCard(
    new TextBlock('**Action.ToggleVisibility example**: click the button to show or hide a welcome message', {
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
  
  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.adaptive',
      content: JSON.parse(JSON.stringify(card))
    }]
  });
}
