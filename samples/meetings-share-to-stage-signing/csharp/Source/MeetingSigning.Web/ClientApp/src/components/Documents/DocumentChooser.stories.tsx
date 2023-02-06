import { ComponentStory, ComponentMeta } from '@storybook/react';
import { DocumentType } from 'models';

import { DocumentChooser } from '.';

export default {
  title: 'Documents',
  component: DocumentChooser,
} as ComponentMeta<typeof DocumentChooser>;

const Template: ComponentStory<typeof DocumentChooser> = (args) => (
  <DocumentChooser {...args} />
);

export const PurchaseAgreementDocument = Template.bind({});
PurchaseAgreementDocument.args = {
  documentType: DocumentType.PurchaseAgreement,
  loggedInAadId: '00000000-0000-0000-0000-000000000001',
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
};

export const DefaultDocument = Template.bind({});
DefaultDocument.args = {
  documentType: DocumentType.TaxFilings,
  loggedInAadId: '00000000-0000-0000-0000-000000000001',
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
      id: '10000000-0000-0000-0000-000000000002',
      signedDateTime: new Date(0),
      isSigned: false,
    },
  ],
};
