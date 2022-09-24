import {
  Button,
  CloseIcon,
  Dialog,
  Flex,
  Text,
} from '@fluentui/react-northstar';
import React from 'react';
import { FormattedMessage } from 'react-intl';

import { TutorialGroup } from 'models';

export interface DeleteTutorialGroupsDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  tutorialGroup: TutorialGroup;
}

function DeleteTutorialGroupsDialogHeader({
  closeDialog,
}: {
  closeDialog: VoidFunction;
}) {
  return (
    <Button icon={<CloseIcon outline />} text iconOnly onClick={closeDialog} />
  );
}
// eslint-disable-next-line max-lines-per-function
function DeleteTutorialGroupDialogContent({
  tutorialGroup,
}: {
  tutorialGroup: TutorialGroup;
}) {
  return (
    <Flex style={{ minHeight: 500 }} column>
      <Text>
        <FormattedMessage
          id="deleteTutorialGroupDialog.confirmMessage"
          description="Confirmation message for the deletion of a tutorial group"
          defaultMessage="Are you sure you want to delete {name}?"
          values={{
            name: tutorialGroup.displayName,
          }}
        />
      </Text>
    </Flex>
  );
}

// eslint-disable-next-line max-lines-per-function
export function DeleteTutorialGroupsDialog({
  isOpen,
  onClose,
  onConfirm,
  tutorialGroup,
}: DeleteTutorialGroupsDialogProps): JSX.Element {
  return (
    <Dialog
      open={isOpen}
      header={
        <FormattedMessage
          id="deleteTutorialGroupDialog.header"
          description="Header for the delete tutorial group dialog"
          defaultMessage="Delete Tutorial Group"
        />
      }
      headerAction={<DeleteTutorialGroupsDialogHeader closeDialog={onClose} />}
      style={{ minHeight: 600 }}
      onConfirm={onConfirm}
      onCancel={onClose}
      confirmButton={{
        content: (
          <FormattedMessage
            id="deleteTutorialGroupDialog.confirmButton"
            description="Confirm button action for the delete dialog"
            defaultMessage="Delete"
          />
        ),
        primary: true,
      }}
      cancelButton={{
        content: (
          <FormattedMessage
            id="deleteTutorialGroupDialog.cancelButton"
            description="Cancel button action for the delete dialog"
            defaultMessage="Cancel"
          />
        ),
      }}
      content={
        <DeleteTutorialGroupDialogContent tutorialGroup={tutorialGroup} />
      }
    />
  );
}
