import { ComponentStory, ComponentMeta } from '@storybook/react';

import { Badge } from '.';

export default {
  title: 'Badge',
  component: Badge,
} as ComponentMeta<typeof Badge>;

const Template: ComponentStory<typeof Badge> = (args) => <Badge {...args} />;

export const BadgeMedium = Template.bind({});
BadgeMedium.args = {
  content: 'Active',
  backgroundColor: '#92C353',
};

const MultipleBadgesTemplate: ComponentStory<typeof Badge> = (args) => (
  <>
    <Badge {...args} />
    <Badge size="small" rectangular {...args} />
  </>
);

export const Multiple = MultipleBadgesTemplate.bind({});
Multiple.args = {
  content: 'Active',
  backgroundColor: '#ff0000',
};
