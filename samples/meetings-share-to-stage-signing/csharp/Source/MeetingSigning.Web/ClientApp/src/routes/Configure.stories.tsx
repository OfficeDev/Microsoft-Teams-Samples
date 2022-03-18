import { ComponentStory, ComponentMeta } from '@storybook/react';

import Configure from './Configure';

export default {
  title: 'Routes/Configure',
  component: Configure,
} as ComponentMeta<typeof Configure>;

export const Template: ComponentStory<typeof Configure> = () => <Configure />;
