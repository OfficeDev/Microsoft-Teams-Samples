import {
  Box,
  Button,
  CloseIcon,
  Dialog,
  Flex,
  RadioGroup,
  RadioGroupItemProps,
} from '@fluentui/react-northstar';

import React, { SyntheticEvent } from 'react';

import { CourseMemberRole } from 'models';
import { FormattedMessage } from 'react-intl';

function SwitchRoleDialogHeaderAction({
  onCloseDialog,
}: {
  onCloseDialog: VoidFunction;
}) {
  return (
    <Button
      icon={<CloseIcon outline />}
      text
      iconOnly
      onClick={onCloseDialog}
    />
  );
}

const SwitchRoleDialogRadioItems = [
  {
    name: 'role',
    key: 'Student',
    label: (
      <FormattedMessage
        id="switchRoleDialog.studentRole"
        description="Switch to student role."
        defaultMessage="Student"
      />
    ),
    value: 'Student',
  },
  {
    name: 'role',
    key: 'Tutor',
    label: (
      <FormattedMessage
        id="switchRoleDialog.demonstratorRole"
        description="Switch to tutor role."
        defaultMessage="Tutor"
      />
    ),
    value: 'Tutor',
  },
  {
    name: 'role',
    key: 'Educator',
    label: (
      <FormattedMessage
        id="switchRoleDialog.educatorRole"
        description="Switch to educator role."
        defaultMessage="Educator"
      />
    ),
    value: 'Educator',
  },
];

function SwitchRoleDialogRadio({
  onSelectionUpdate,
}: {
  onSelectionUpdate: (role: CourseMemberRole) => void;
}) {
  function onCheckedValueChange(
    evt: SyntheticEvent<HTMLElement, Event>,
    props: RadioGroupItemProps | undefined,
  ) {
    if (props?.value === undefined) return;
    if (typeof props.value === 'number') return;
    onSelectionUpdate(props.value as CourseMemberRole);
  }
  return (
    <RadioGroup
      vertical
      items={SwitchRoleDialogRadioItems}
      onCheckedValueChange={onCheckedValueChange}
    />
  );
}
export interface SwitchRoleDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  onSelectionUpdate: (role: CourseMemberRole) => void;
}
export default function SwitchRoleDialog(
  props: SwitchRoleDialogProps,
): JSX.Element {
  const { isOpen, onClose, onConfirm, onSelectionUpdate } = props;

  return (
    <Dialog
      open={isOpen}
      header={
        <FormattedMessage
          id="switchRoleDialog.assignRole"
          description="Assign role to the member"
          defaultMessage="Assign role"
        />
      }
      headerAction={<SwitchRoleDialogHeaderAction onCloseDialog={onClose} />}
      style={{
        minHeight: 600, // TODO: undo hard-coding?
      }}
      onConfirm={onConfirm}
      confirmButton={
        <FormattedMessage
          id="switchRoleDialog.switchRole"
          description="Switch role of the member"
          defaultMessage="Switch role"
        />
      }
      content={
        <Flex style={{ minHeight: 500 }} column>
          <Box>
            <SwitchRoleDialogRadio onSelectionUpdate={onSelectionUpdate} />
          </Box>
        </Flex>
      }
    />
  );
}
