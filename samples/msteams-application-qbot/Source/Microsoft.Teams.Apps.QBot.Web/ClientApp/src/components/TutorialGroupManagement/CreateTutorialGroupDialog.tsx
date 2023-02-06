import {
  Button,
  CloseIcon,
  Dialog,
  Flex,
  Form,
  FormInput,
} from '@fluentui/react-northstar';
import React from 'react';
import { FormattedMessage } from 'react-intl';

import { TutorialGroup } from 'models';

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
  nameErrorMessage: Parameters<typeof FormInput>[0]['errorMessage'];
  codeErrorMessage: Parameters<typeof FormInput>[0]['errorMessage'];
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
              id="createTutorialGroupDialog.namelabel"
              description="The label for the tutorial group name input field when creating a new tutorial group"
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
              id="createTutorialGroupDialog.codeLabel"
              description="The label for the tutorial group short-code input field when creating a new tutorial group"
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
export function CreateTutorialGroupsDialog({
  isOpen,
  onClose,
  onConfirm,
  onUpdate,
  tutorialGroup,
  existingTutorialGroups,
}: CreateTutorialGroupsDialogProps): JSX.Element {
  const nameErrorMessage = existingTutorialGroups.some(
    (tg) => tg.displayName === tutorialGroup.displayName,
  ) ? (
    <FormattedMessage
      id="createTutorialGroupDialog.InvalidName"
      description="Warning to show users when they've entered a name that already exists"
      defaultMessage="That name already exists"
    />
  ) : undefined;
  const codeErrorMessage = existingTutorialGroups.some(
    (tg) => tg.shortName === tutorialGroup.shortName,
  ) ? (
    <FormattedMessage
      id="createTutorialGroupDialog.InvalidCode"
      description="Warning to show users when they've entered a code that already exists"
      defaultMessage="That code already exists"
    />
  ) : undefined;
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
          id="createTutorialGroupDialog.headerText"
          description="The header text for the dialog when creating a new tutorial group"
          defaultMessage="Create Tutorial Group"
        />
      }
      headerAction={<CreateTutorialGroupsDialogHeader closeDialog={onClose} />}
      style={{ minHeight: 600 }}
      onConfirm={validateAndConfirm}
      confirmButton={{
        content: (
          <FormattedMessage
            id="createTutorialGroupDialog.confirmButton"
            description="The confirm button text text for the dialog when creating a new tutorial group"
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
