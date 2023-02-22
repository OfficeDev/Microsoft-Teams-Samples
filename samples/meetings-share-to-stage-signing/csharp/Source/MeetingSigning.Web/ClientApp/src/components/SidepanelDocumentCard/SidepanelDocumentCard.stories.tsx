import { ComponentStory, ComponentMeta } from '@storybook/react';

import { DocumentType } from 'models';
import { DocumentState } from 'models/Document';
import { SidepanelDocumentCard } from '.';

export default {
  title: 'Sidepanel/Document Card',
  component: SidepanelDocumentCard,
} as ComponentMeta<typeof SidepanelDocumentCard>;

const Template: ComponentStory<typeof SidepanelDocumentCard> = (args) => (
  <SidepanelDocumentCard {...args} />
);

export const DocumentCard = Template.bind({});
DocumentCard.args = {
  id: '12300000-0000-0000-0000-000000000001',
  documentType: DocumentType.PurchaseAgreement,
  documentState: DocumentState.Active,
  loggedInUser: {
    userId: '00000000-0000-0000-0000-000000000001',
    name: 'You',
    email: 'you@live.com',
  },
  signatures: [
    {
      signer: {
        userId: '00000000-0000-0000-0000-000000000001',
        name: 'Bill Gates',
      },
      text: '',
      id: '10000000-0000-0000-0000-000000000001',
      signedDateTime: new Date(0),
      isSigned: false,
    },
    {
      signer: {
        userId: '00000000-0000-0000-0000-000000000002',
        name: 'Satya Nadella',
      },
      text: '',
      id: '1000000-0000-0000-0000-000000000002',
      signedDateTime: new Date(0),
      isSigned: false,
    },
    {
      signer: {
        userId: '00000000-0000-0000-0000-000000000003',
        name: 'Big G',
      },
      text: '',
      id: '1000000-0000-0000-0000-000000000003',
      signedDateTime: new Date(0),
      isSigned: false,
    },
  ],
};
