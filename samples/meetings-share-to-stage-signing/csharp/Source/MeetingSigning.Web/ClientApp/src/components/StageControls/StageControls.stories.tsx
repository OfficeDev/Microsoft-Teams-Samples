import { ComponentStory, ComponentMeta } from '@storybook/react';

import { StageControls } from '.';

export default {
  title: 'Stage Controls',
  component: StageControls,
} as ComponentMeta<typeof StageControls>;

const Template: ComponentStory<typeof StageControls> = (args) => (
  <StageControls {...args}
  />
);

export const StageControlsInControl = Template.bind({});
StageControlsInControl.args = {
  localUserInControl: true,
  localUserCanTakeControl: true,
  userInControl: true,
  followSuspended: false,
  nameOfUserInControl: 'Bill Gates',
  takeControl: () => {
    console.log('takeControl');
  },
  endSuspension: () => {
    console.log('endSuspension');
  },
}

export const StageControlsFollowingCanControl = Template.bind({});
StageControlsFollowingCanControl.args = {
  localUserInControl: false,
  localUserCanTakeControl: true,
  userInControl: true,
  followSuspended: false,
  nameOfUserInControl: 'Bill Gates',
  takeControl: () => {
    console.log('takeControl');
  },
  endSuspension: () => {
    console.log('endSuspension');
  },
}

export const StageControlsFollowSuspended = Template.bind({});
StageControlsFollowSuspended.args = {
  localUserInControl: false,
  localUserCanTakeControl: true,
  userInControl: true,
  followSuspended: true,
  nameOfUserInControl: 'Bill Gates',
  takeControl: () => {
    console.log('takeControl');
  },
  endSuspension: () => {
    console.log('endSuspension');
  },
}

export const StageControlsFollowingCannotControl = Template.bind({});
StageControlsFollowingCannotControl.args = {
  localUserInControl: false,
  localUserCanTakeControl: false,
  userInControl: true,
  followSuspended: false,
  nameOfUserInControl: 'Bill Gates',
  takeControl: () => {
    console.log('takeControl');
  },
  endSuspension: () => {
    console.log('endSuspension');
  },
}
