// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { AdaptiveCard, TextBlock, InputText, InputChoiceSet, SubmitAction } from "@microsoft/teams.cards";
import { IAdaptiveCard } from "@microsoft/teams.cards";

export interface SubmitExampleData {
  MultiSelect: string;
  UserAttributionSelect: string;
  Option1: string;
  Option2: string;
  Option3: string;
  Question: string;
}

export function toSubmitExampleData(action: any): SubmitExampleData {
  const activityPreview = action.botActivityPreview[0];
  const attachmentContent = activityPreview.attachments[0].content;
  const userText = attachmentContent.body[1].text;
  const choiceSet = attachmentContent.body[3];
  const attributionFlag = attachmentContent.body[4].text.split(':')[1].trim();
  
  return {
    MultiSelect: choiceSet.isMultiSelect ? 'true' : 'false',
    UserAttributionSelect: attributionFlag,
    Option1: choiceSet.choices[0].title,
    Option2: choiceSet.choices[1].title,
    Option3: choiceSet.choices[2].title,
    Question: userText
  };
}

export function createAdaptiveCardEditor(
  userText: string | null = null,
  isMultiSelect: boolean = true,
  option1: string | null = null,
  option2: string | null = null,
  option3: string | null = null
): IAdaptiveCard {
  return {
    type: 'AdaptiveCard',
    version: '1.0',
    body: [
      {
        type: 'TextBlock',
        text: 'This is an Adaptive Card within a Task Module',
        weight: 'bolder'
      },
      { type: 'TextBlock', text: 'Enter text for Question:' },
      {
        type: 'Input.Text',
        id: 'Question',
        placeholder: 'Question text here',
        value: userText || undefined
      },
      { type: 'TextBlock', text: 'Options for Question:' },
      { type: 'TextBlock', text: 'Is Multi-Select:' },
      {
        type: 'Input.ChoiceSet',
        id: 'MultiSelect',
        style: 'expanded',
        isMultiSelect: false,
        value: isMultiSelect ? 'true' : 'false',
        choices: [
          { title: 'True', value: 'true' },
          { title: 'False', value: 'false' }
        ]
      },
      {
        type: 'Input.Text',
        id: 'Option1',
        placeholder: 'Option 1 here',
        value: option1 || undefined
      },
      {
        type: 'Input.Text',
        id: 'Option2',
        placeholder: 'Option 2 here',
        value: option2 || undefined
      },
      {
        type: 'Input.Text',
        id: 'Option3',
        placeholder: 'Option 3 here',
        value: option3 || undefined
      },
      { type: 'TextBlock', text: 'Do you want to send this card on behalf of the User?' },
      {
        type: 'Input.ChoiceSet',
        id: 'UserAttributionSelect',
        style: 'expanded',
        isMultiSelect: false,
        value: isMultiSelect ? 'true' : 'false',
        choices: [
          { title: 'Yes', value: 'true' },
          { title: 'No', value: 'false' }
        ]
      }
    ],
    actions: [
      {
        type: 'Action.Submit',
        title: 'Submit',
        data: {
          submitLocation: 'messagingExtensionFetchTask'
        }
      }
    ]
  };
}

export function createAdaptiveCardAttachment(data: SubmitExampleData): IAdaptiveCard {
  return {
    type: 'AdaptiveCard',
    version: '1.0',
    body: [
      {
        type: 'TextBlock',
        text: 'Adaptive Card from Task Module',
        weight: 'bolder'
      },
      {
        type: 'TextBlock',
        text: data.Question,
        id: 'Question'
      },
      {
        type: 'Input.Text',
        id: 'Answer',
        placeholder: 'Answer here...'
      },
      {
        type: 'Input.ChoiceSet',
        id: 'Choices',
        style: 'expanded',
        isMultiSelect: data.MultiSelect === 'true',
        choices: [
          { title: data.Option1, value: data.Option1 },
          { title: data.Option2, value: data.Option2 },
          { title: data.Option3, value: data.Option3 }
        ]
      },
      {
        type: 'TextBlock',
        text: `Sending card on behalf of user is set to: ${data.UserAttributionSelect}`,
        id: 'AttributionChoice'
      }
    ],
    actions: [
      {
        type: 'Action.Submit',
        title: 'Submit',
        data: { submitLocation: 'messagingExtensionSubmit' }
      }
    ]
  };
}
