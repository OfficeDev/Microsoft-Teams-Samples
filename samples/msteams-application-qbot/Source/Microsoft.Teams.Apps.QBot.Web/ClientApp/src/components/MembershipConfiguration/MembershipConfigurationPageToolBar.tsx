import {
  Button,
  CloseIcon,
  ContactGroupIcon,
  Flex,
  RetryIcon,
  Text,
} from '@fluentui/react-northstar';
import { CourseMemberRole } from 'models';
import React from 'react';

import { FormattedMessage } from 'react-intl';

export interface TutorialGroupPageToolBarProps {
  currentCourseRole: CourseMemberRole;
  userCount: number;
  onClearSelection: () => void;
  onSwitchRole: () => void;
  onAssignTutorialGroups: () => void;
}

function UserSelectedSection({
  userCount,
  onClearSelection,
}: {
  userCount: number;
  onClearSelection: VoidFunction;
}) {
  const hasUsersSelected = userCount > 0;
  return (
    <>
      {!hasUsersSelected ? null : (
        <Flex vAlign="center" gap="gap.medium">
          <Text>
            {userCount}{' '}
            <FormattedMessage
              id="TutorialGroupPageToolBar.headerText"
              description="Header of the tutorial group page"
              defaultMessage="Selected"
            />{' '}
          </Text>
          <Button
            icon={<CloseIcon outline />}
            iconOnly
            text
            onClick={onClearSelection}
          />
        </Flex>
      )}
    </>
  );
}

function SwitchRoleButton({
  hasUsersSelected,
  onSwitchRole,
}: {
  hasUsersSelected: boolean;
  onSwitchRole: VoidFunction;
}) {
  return (
    <Button
      content={
        <FormattedMessage
          id="TutorialGroupPageToolBar.switchRole"
          description="To the switch the role of the user"
          defaultMessage="Switch role"
        />
      }
      icon={<RetryIcon outline />}
      text
      disabled={!hasUsersSelected}
      onClick={onSwitchRole}
    />
  );
}

function AssignTutorialGroupsButton({
  hasUsersSelected,
  onAssignTutorialGroups,
}: {
  hasUsersSelected: boolean;
  onAssignTutorialGroups: VoidFunction;
}) {
  return (
    <Button
      content={
        <FormattedMessage
          id="TutorialGroupPageToolBar.assignTutorialGroup"
          description="Assign tutorial groups"
          defaultMessage="Assign tutorial groups"
        />
      }
      icon={<ContactGroupIcon outline />}
      text
      disabled={!hasUsersSelected}
      onClick={onAssignTutorialGroups}
    />
  );
}

export default function TutorialGroupPageToolBar({
  currentCourseRole,
  userCount,
  onClearSelection,
  onSwitchRole,
  onAssignTutorialGroups,
}: TutorialGroupPageToolBarProps): JSX.Element {
  const hasUsersSelected = userCount > 0;
  return (
    <Flex gap="gap.small">
      <UserSelectedSection
        onClearSelection={onClearSelection}
        userCount={userCount}
      />
      {currentCourseRole === 'Educator' && (
        <SwitchRoleButton
          hasUsersSelected={hasUsersSelected}
          onSwitchRole={onSwitchRole}
        />
      )}
      {currentCourseRole === 'Educator' && (
        <AssignTutorialGroupsButton
          hasUsersSelected={hasUsersSelected}
          onAssignTutorialGroups={onAssignTutorialGroups}
        />
      )}
    </Flex>
  );
}
