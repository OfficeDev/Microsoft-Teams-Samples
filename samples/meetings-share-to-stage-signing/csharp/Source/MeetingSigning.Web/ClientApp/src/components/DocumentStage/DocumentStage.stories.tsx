import { ComponentStory, ComponentMeta } from '@storybook/react';

import { DocumentStage } from '.';

export default {
  title: 'Document Stage',
  component: DocumentStage,
} as ComponentMeta<typeof DocumentStage>;

const Template: ComponentStory<typeof DocumentStage> = () => <DocumentStage />;

export const DocumentStageView = Template.bind({});
