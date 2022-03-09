import { ComponentStory, ComponentMeta } from '@storybook/react';
import { StageWithNoDocument } from '.';

export default {
  title: 'Stage With No Document',
  component: StageWithNoDocument,
} as ComponentMeta<typeof StageWithNoDocument>;

const Template: ComponentStory<typeof StageWithNoDocument> = () => (
  <StageWithNoDocument />
);

export const StageWithNoDocumentView = Template.bind({});
