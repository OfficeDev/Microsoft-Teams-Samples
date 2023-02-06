import {
  Box,
  Button,
  CloseIcon,
  Dialog,
  Dropdown,
  DropdownProps,
  Flex,
  Text,
} from '@fluentui/react-northstar';
import React from 'react';

import { TutorialGroup } from 'models';
import { FormattedMessage, defineMessages, useIntl } from 'react-intl';
import { isNotNullOrUndefined } from 'util/isNotNullOrUndefined';

const assignTutorialGroupsDialogMessages = defineMessages({
  tutorialGroupPlaceholder: {
    id: 'assignTutorialGroupDialogContent.tutorialGroupPlaceholder',
    defaultMessage: 'Select tutorial groups',
    description: 'Placeholder for select tutorial groups',
  },
  noMatchesFound: {
    id: 'assignTutorialGroupDialogContent.noMatchesFound',
    defaultMessage: 'No matches',
    description: 'No matches found for tutorial groups.',
  },
});

export interface AssignTutorialGroupsDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  onSelectionUpdate: (tutorialGroups: TutorialGroup[]) => void;
  tutorialGroups: TutorialGroup[];
  selectedUserCount: number;
}

function AssignTutorialGroupsDialogHeader({
  closeDialog,
}: {
  closeDialog: VoidFunction;
}) {
  return (
    <Button icon={<CloseIcon outline />} text iconOnly onClick={closeDialog} />
  );
}

function AssignTutorialGroupDialogContent({
  tutorialGroups,
  onDropdownChange,
  selectedUserCount,
}: {
  tutorialGroups: TutorialGroup[];
  onDropdownChange: (
    event:
      | React.MouseEvent<Element, MouseEvent>
      | React.KeyboardEvent<Element>
      | null,
    data: DropdownProps,
  ) => void;
  selectedUserCount: number;
}) {
  const intl = useIntl();
  return (
    <Flex style={{ minHeight: 500 }} column>
      <Box>
        <Text weight="bold">
          <FormattedMessage
            id="assignTutorialGroupDialogContent.assignTutorialGroups"
            description="Assign tutorial group to the users."
            defaultMessage="Assign tutorial groups to {selectedUserCount} users"
            values={{
              selectedUserCount,
            }}
          />
        </Text>
      </Box>
      <Dropdown
        open
        fluid
        multiple
        search
        items={tutorialGroups.map((tg) => tg.shortName)}
        placeholder={intl.formatMessage(
          assignTutorialGroupsDialogMessages.tutorialGroupPlaceholder,
          {},
        )}
        noResultsMessage={intl.formatMessage(
          assignTutorialGroupsDialogMessages.noMatchesFound,
          {},
        )}
        onChange={onDropdownChange}
      />
    </Flex>
  );
}

export default function AssignTutorialGroupsDialog({
  isOpen,
  onClose,
  onConfirm,
  tutorialGroups,
  onSelectionUpdate,
  selectedUserCount,
}: AssignTutorialGroupsDialogProps): JSX.Element {
  function onDropdownChange(
    evt:
      | React.MouseEvent<Element, MouseEvent>
      | React.KeyboardEvent<Element>
      | null,
    props: DropdownProps | null,
  ) {
    if (!Array.isArray(props?.value)) return;

    const selectedTutorialGroups = (props?.value as string[])
      .map((shortName) =>
        tutorialGroups.find((tg) => tg.shortName === shortName),
      )
      .filter(isNotNullOrUndefined);
    onSelectionUpdate(selectedTutorialGroups);
  }
  return (
    <Dialog
      open={isOpen}
      header={
        <FormattedMessage
          id="assignTutorialGroupDialogContent.assignTutorialGroups"
          description="Assign member to the selected Tutorial Groups"
          defaultMessage="Assign Tutorial Groups"
        />
      }
      headerAction={<AssignTutorialGroupsDialogHeader closeDialog={onClose} />}
      style={{ minHeight: 600 }}
      onConfirm={onConfirm}
      confirmButton={
        <FormattedMessage
          id="assignTutorialGroupDialogContent.assignTutorialGroupButton"
          description="Confirm button to assign member to the selected Tutorial Groups"
          defaultMessage="Assign Tutorial Groups"
        />
      }
      content={
        <AssignTutorialGroupDialogContent
          tutorialGroups={tutorialGroups}
          selectedUserCount={selectedUserCount}
          onDropdownChange={onDropdownChange}
        />
      }
    />
  );
}
