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

export interface EditTutorialGroupsDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onUpdate: (tutorialGroup: TutorialGroup) => void;
  onConfirm: () => void;
  tutorialGroup: TutorialGroup;
  existingTutorialGroups: TutorialGroup[];
}

function EditTutorialGroupsDialogHeader({
  closeDialog,
}: {
  closeDialog: VoidFunction;
}) {
  return (
    <Button icon={<CloseIcon outline />} text iconOnly onClick={closeDialog} />
  );
}
// eslint-disable-next-line max-lines-per-function
function EditTutorialGroupDialogContent({
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
              id="editTutorialGroupDialog.namelabel"
              description="The label for the tutorial group name input field when editing an existing tutorial group"
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
              id="editTutorialGroupDialog.codeLabel"
              description="The label for the tutorial group short-code input field when editing an existing tutorial group"
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
export function EditTutorialGroupsDialog({
  isOpen,
  onClose,
  onConfirm,
  onUpdate,
  tutorialGroup,
  existingTutorialGroups,
}: EditTutorialGroupsDialogProps): JSX.Element {
  const otherTutorialGroups = existingTutorialGroups.filter(
    (tg) => tg.id !== tutorialGroup.id,
  );
  const nameErrorMessage = otherTutorialGroups.some(
    (tg) => tg.displayName === tutorialGroup.displayName,
  ) ? (
    <FormattedMessage
      id="editTutorialGroupDialog.InvalidName"
      description="Warning to show users when they've entered a name that already exists"
      defaultMessage="That name already exists"
    />
  ) : undefined;
  const codeErrorMessage = otherTutorialGroups.some(
    (tg) => tg.shortName === tutorialGroup.shortName,
  ) ? (
    <FormattedMessage
      id="editTutorialGroupDialog.InvalidCode"
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
          id="editTutorialGroupDialog.headerText"
          description="The header text for the dialog when editing an existing tutorial group"
          defaultMessage="Edit Tutorial Group"
        />
      }
      headerAction={<EditTutorialGroupsDialogHeader closeDialog={onClose} />}
      style={{ minHeight: 600 }}
      onConfirm={validateAndConfirm}
      confirmButton={{
        content: (
          <FormattedMessage
            id="editTutorialGroupDialog.confirmButton"
            description="The confirm button text text for the dialog when editing an existing tutorial group"
            defaultMessage="Save"
          />
        ),
        primary: true,
        disabled: !!hasValidationErrors,
      }}
      content={
        <EditTutorialGroupDialogContent
          tutorialGroup={tutorialGroup}
          onUpdate={onUpdate}
          nameErrorMessage={nameErrorMessage}
          codeErrorMessage={codeErrorMessage}
        />
      }
    />
  );
}
