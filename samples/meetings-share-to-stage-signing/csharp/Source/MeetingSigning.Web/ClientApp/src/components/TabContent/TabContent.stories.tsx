import { ComponentStory, ComponentMeta } from '@storybook/react';
import { TabContent } from '.';

export default {
  title: 'TabContent',
  component: TabContent,
} as ComponentMeta<typeof TabContent>;

const Template: ComponentStory<typeof TabContent> = () => <TabContent />;

export const Default = Template.bind({});
