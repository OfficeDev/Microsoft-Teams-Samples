// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { App, IActivityContext } from '@microsoft/teams.apps';
import { 
  AdaptiveCardInvokeActivity,
  IMessageActivity
} from '@microsoft/teams.api';
import type { AdaptiveCardActionResponse, AdaptiveCardActionMessageResponse } from '@microsoft/teams.api';
import { 
  AdaptiveCard, 
  TextBlock, 
  TextInput, 
  ExecuteAction, 
  OpenUrlAction, 
  ShowCardAction, 
  ToggleVisibilityAction
} from '@microsoft/teams.cards';

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
      id: 'name'
    })
      .withLabel('Please enter your name:')
      .withIsRequired(true)
      .withErrorMessage('Name is required')
  ).withActions(
    new ExecuteAction({ 
      title: 'Submit',
      verb: 'submit_name',
      data: { action: 'submit_name' },
      associatedInputs: 'auto'
    })
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
    new TextBlock('Click to show or hide the message'),
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

app.on('card.action', async (context: IActivityContext<AdaptiveCardInvokeActivity>): Promise<AdaptiveCardActionResponse> => {
  const data = context.activity.value.action.data;
  await context.send(`Data Submitted: ${data.name}`);

  return {
    statusCode: 200,
    type: 'application/vnd.microsoft.activity.message',
    value: 'Action processed successfully'
  };
});


app.on('message', async (context) => {
  const { activity } = context;
  const messageActivity = activity as IMessageActivity;
  const text = (messageActivity.text || '').trim().toLowerCase();

  if (text.includes('card actions')) {
    await context.send(createCardActionsCard());
  } else if (text.includes('toggle visibility')) {
    await context.send(createToggleVisibilityCard());
  } else {
    await context.send("Welcome to the Cards Bot! To interact with me, send one of the following commands: 'card actions' or 'toggle visibility'");
  }
});

app.start().catch(console.error);