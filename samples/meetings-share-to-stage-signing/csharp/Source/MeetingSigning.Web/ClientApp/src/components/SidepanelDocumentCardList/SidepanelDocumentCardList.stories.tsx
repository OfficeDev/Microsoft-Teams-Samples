import { ComponentStory, ComponentMeta } from '@storybook/react';
import { DocumentType } from 'models';
import { SidepanelDocumentCardList } from '.';

export default {
  title: 'Sidepanel/Document List',
  component: SidepanelDocumentCardList,
} as ComponentMeta<typeof SidepanelDocumentCardList>;

const Template: ComponentStory<typeof SidepanelDocumentCardList> = (args) => (
  <SidepanelDocumentCardList />
);

export const DocumentCardList = Template.bind({});
DocumentCardList.args = {
  documents: [
    {
      id: '12300000-0000-0000-0000-000000000001',
      documentType: DocumentType.PurchaseAgreement,
      documentState: 'active',
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
    },
    {
      id: '12300000-0000-0000-0000-000000000001',
      documentType: DocumentType.TaxFilings,
      documentState: 'active',
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
      ],
    },
  ],
};
