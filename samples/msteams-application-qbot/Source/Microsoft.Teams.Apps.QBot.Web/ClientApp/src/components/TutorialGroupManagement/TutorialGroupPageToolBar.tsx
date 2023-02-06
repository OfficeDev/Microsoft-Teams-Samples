import { Button, Flex, TeamCreateIcon } from '@fluentui/react-northstar';
import React from 'react';
import { FormattedMessage } from 'react-intl';

export interface TutorialGroupPageToolBarProps {
  onAddTutorialGroup: () => void;
}

function AddTutorialGroupButton({
  onAddTutorialGroup,
}: {
  onAddTutorialGroup: VoidFunction;
}) {
  return (
    <Button
      content={
        <FormattedMessage
          id="addTutorialGroupButton.content"
          description="Content of the add tutorial group toolbar action"
          defaultMessage="Add tutorial group"
        />
      }
      icon={<TeamCreateIcon outline />}
      primary
      onClick={onAddTutorialGroup}
    />
  );
}

export function TutorialGroupPageToolBar({
  onAddTutorialGroup,
}: TutorialGroupPageToolBarProps): JSX.Element {
  return (
    <Flex gap="gap.small">
      <AddTutorialGroupButton onAddTutorialGroup={onAddTutorialGroup} />
    </Flex>
  );
}
