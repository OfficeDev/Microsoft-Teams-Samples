import { ComponentStory, ComponentMeta } from '@storybook/react';

import { CustomerInquiryTable } from '.';

export default {
  title: 'CustomerInquiryTable',
  component: CustomerInquiryTable,
} as ComponentMeta<typeof CustomerInquiryTable>;

const Template: ComponentStory<typeof CustomerInquiryTable> = (args) => (
  <CustomerInquiryTable {...args} />
);

export const CustomerInquiryTableDefault = Template.bind({});
CustomerInquiryTableDefault.args = {
  customerInquiries: [
    {
      subEntityId: '001',
      createdDateTime: '2022-04-05T22:13+500',
      customerName: 'Labrador Retriever',
      question:
        'The Labrador Retriever or Labrador is a British breed of retriever gun dog. It was developed in the United Kingdom ' +
        'from fishing dogs imported from the colony of Newfoundland (now a province of Canada), and was named after the Labrador' +
        ' region of that colony. It is among the most commonly kept dogs in several countries, particularly in the Western world.',
      conversationId: "12345",
      active: true,
    },
    {
      subEntityId: '002',
      createdDateTime: '2022-04-05T22:13+500',
      customerName: 'Bulldog',
      question:
        'The Bulldog is a British breed of dog of mastiff type. It may also be known as the English Bulldog or British Bulldog. ' +
        'It is of medium size, a muscular, hefty dog with a wrinkled face and a distinctive pushed-in nose. It is commonly kept ' +
        'as a companion dog; in 2013 it was in twelfth place on a list of the breeds most frequently registered worldwide.',
      conversationId: "12345",
      active: true,
    },
  ],
};
