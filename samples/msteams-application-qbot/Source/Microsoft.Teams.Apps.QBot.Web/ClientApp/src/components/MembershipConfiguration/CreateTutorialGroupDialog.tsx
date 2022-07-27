import {
  Button,
  CloseIcon,
  Dialog,
  Flex,
  Form,
  FormInput,
} from '@fluentui/react-northstar';
import React from 'react';

import { TutorialGroup } from 'models';
import { FormattedMessage } from 'react-intl';

export interface CreateTutorialGroupsDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onUpdate: (tutorialGroup: TutorialGroup) => void;
  onConfirm: () => void;
  tutorialGroup: TutorialGroup;
  existingTutorialGroups: TutorialGroup[];
}

function CreateTutorialGroupsDialogHeader({
  closeDialog,
}: {
  closeDialog: VoidFunction;
}) {
  return (
    <Button icon={<CloseIcon outline />} text iconOnly onClick={closeDialog} />
  );
}
// eslint-disable-next-line max-lines-per-function
function CreateTutorialGroupDialogContent({
  onUpdate,
  tutorialGroup,
  nameErrorMessage,
  codeErrorMessage,
}: {
  onUpdate: (tutorialGroup: TutorialGroup) => void;
  tutorialGroup: TutorialGroup;
  nameErrorMessage: string | undefined;
  codeErrorMessage: string | undefined;
}) {
  function onDisplayNameChanged(
    event: React.SyntheticEvent<HTMLElement>,
    data?: { value: string },
  ) {
    onUpdate({ ...tutorialGroup, displayName: data?.value ?? '' });
  }
  function onShortNameChanged(
    event: React.SyntheticEvent<HTMLElement>,
    data?: { value: string },
  ) {
    onUpdate({ ...tutorialGroup, shortName: data?.value ?? '' });
  }
  return (
    <Flex style={{ minHeight: 500 }} column>
      <Form>
        <FormInput
          label={
            <FormattedMessage
              id="createTutorialGroupDialog.labelHeader"
              description="Name of the tutorial group"
              defaultMessage="Name"
            />
          }
          required
          value={tutorialGroup.displayName}
          onChange={onDisplayNameChanged}
          errorMessage={nameErrorMessage}
        />
        <FormInput
          label={
            <FormattedMessage
              id="createTutorialGroupDialog.labelCode"
              description="code"
              defaultMessage="Code"
            />
          }
          required
          value={tutorialGroup.shortName}
          onChange={onShortNameChanged}
          errorMessage={codeErrorMessage}
        />
      </Form>
    </Flex>
  );
}

// eslint-disable-next-line max-lines-per-function
export default function CreateTutorialGroupsDialog({
  isOpen,
  onClose,
  onConfirm,
  onUpdate,
  tutorialGroup,
  existingTutorialGroups,
}: CreateTutorialGroupsDialogProps): JSX.Element {
  const nameErrorMessage = existingTutorialGroups.some(
    (tg) => tg.displayName === tutorialGroup.displayName,
  )
    ? 'That name already exists'
    : undefined;
  const codeErrorMessage = existingTutorialGroups.some(
    (tg) => tg.shortName === tutorialGroup.shortName,
  )
    ? 'That code already exists'
    : undefined;
  const hasValidationErrors = nameErrorMessage || codeErrorMessage;
  function validateAndConfirm() {
    if (hasValidationErrors) {
      return;
    }
    onConfirm();
  }
  return (
    <Dialog
      open={isOpen}
      header={
        <FormattedMessage
          id="createTutorialGroupDialog.dialogHeader"
          description="Tutorial group header"
          defaultMessage="Create Tutorial Group"
        />
      }
      headerAction={<CreateTutorialGroupsDialogHeader closeDialog={onClose} />}
      style={{ minHeight: 600 }}
      onConfirm={validateAndConfirm}
      confirmButton={{
        content: (
          <FormattedMessage
            id="createTutorialGroupDialog.dialogHeader"
            description="Confirm button to create tutorial group"
            defaultMessage="Create Tutorial Group"
          />
        ),
        primary: true,
        disabled: !!hasValidationErrors,
      }}
      content={
        <CreateTutorialGroupDialogContent
          tutorialGroup={tutorialGroup}
          onUpdate={onUpdate}
          nameErrorMessage={nameErrorMessage}
          codeErrorMessage={codeErrorMessage}
        />
      }
    />
  );
}
