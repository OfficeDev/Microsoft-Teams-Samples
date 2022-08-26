import { ComponentStory, ComponentMeta } from '@storybook/react';

import { CustomerInquiryDetail } from '.';

export default {
  title: 'CustomerInquiryDetail',
  component: CustomerInquiryDetail,
} as ComponentMeta<typeof CustomerInquiryDetail>;

const Template: ComponentStory<typeof CustomerInquiryDetail> = (args) => (
  <CustomerInquiryDetail {...args} />
);

export const CustomerInquiryDetailDefault = Template.bind({});
CustomerInquiryDetailDefault.args = {
  customerInquiry: {
    subEntityId: '001',
    createdDateTime: '2022-04-05T22:13+500',
    customerName: 'Labrador Retriever',
    question:
      'The Labrador Retriever or Labrador is a British breed of retriever gun dog. It was developed in the United Kingdom ' +
      'from fishing dogs imported from the colony of Newfoundland (now a province of Canada), and was named after the Labrador' +
      ' region of that colony. It is among the most commonly kept dogs in several countries, particularly in the Western world.',
    conversationId: '12345678',
    active: true,
  },
  isChatOpen: false,
  onOpenConversation: () => alert('open conversation'),
  onCloseConversation: () => alert('close conversation'),
};
