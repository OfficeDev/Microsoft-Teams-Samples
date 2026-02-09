// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Send Suggested Actions with buttons
export async function sendSuggestedActions(context: any) {
  const message = {
    type: 'message',
    text: 'What is your favorite color?',
    suggestedActions: {
      actions: [
        {
          type: 'imBack',
          title: 'Red',
          value: 'Red'
        },
        {
          type: 'imBack',
          title: 'Yellow',
          value: 'Yellow'
        },
        {
          type: 'imBack',
          title: 'Blue',
          value: 'Blue'
        }
      ]
    }
  };
  await context.send(message);
}
