import React from 'react';
import {
  gridCellBehavior,
  gridHeaderCellBehavior,
  gridNestedBehavior,
  gridRowBehavior,
  Table,
  Text,
  Flex,
  CircleIcon,
  ArrowSortIcon,
  Button,
  CheckmarkCircleIcon,
} from '@fluentui/react-northstar';
import {
  AutoSizer,
  List as ReactVirtualizedList,
  ListRowProps,
  ListProps,
} from 'react-virtualized';
import { User, UserCourseMemberships } from 'models';
import { FormattedMessage } from 'react-intl';
import { UserIcon } from 'components/UserIcon/UserIcon';

export interface SelectedUserCourseMemberships extends UserCourseMemberships {
  isSelected: boolean;
}

// Overrides ARIA attributes assigned by default, which break accessibility
const accessibilityListProperties: Partial<ListProps> = {
  'aria-label': '',
  'aria-readonly': undefined,
  containerRole: 'presentation',
  role: 'presentation',
};

// TODO(nibeauli): refactor this into something clearer
// eslint-disable-next-line max-lines-per-function, sonarjs/cognitive-complexity
function UserListRow({
  userMembership,
  style,
  selectUser,
  unselectUser,
}: {
  userMembership: SelectedUserCourseMemberships;
  style: React.CSSProperties;
  selectUser: (user: User) => void;
  unselectUser: (user: User) => void;
}) {
  const userRole = userMembership.role;
  const tutorialGroupString =
    userMembership.tutorialGroups.length > 0
      ? userMembership.tutorialGroups.map((tg) => tg.shortName).join(', ')
      : '-';
  return (
    <Table.Row
      key={userMembership.user.id}
      header
      accessibility={gridRowBehavior}
      style={style}
    >
      <Table.Cell
        style={{ width: 40, maxWidth: 40 }}
        accessibility={gridCellBehavior}
        as={Flex}
        vAlign="center"
      >
        <Button
          icon={
            userMembership.isSelected ? (
              <CheckmarkCircleIcon outline />
            ) : (
              <CircleIcon outline />
            )
          }
          text
          iconOnly
          onClick={() => {
            if (userMembership.isSelected) {
              unselectUser(userMembership.user);
            } else {
              selectUser(userMembership.user);
            }
          }}
        />
      </Table.Cell>
      <Table.Cell
        accessibility={gridCellBehavior}
        as={Flex}
        gap="gap.medium"
        vAlign="center"
      >
        <UserIcon user={userMembership.user} />
        <Flex column vAlign="center">
          <Text size="medium">{userMembership.user.name}</Text>
        </Flex>
      </Table.Cell>
      <Table.Cell
        content={userMembership.user.upn}
        accessibility={gridCellBehavior}
      />
      <Table.Cell content={userRole} accessibility={gridCellBehavior} />
      <Table.Cell
        content={tutorialGroupString}
        accessibility={gridCellBehavior}
      />
    </Table.Row>
  );
}

// TODO(nibeauli): when sorts implemented factor out the cells into components
// eslint-disable-next-line max-lines-per-function
function UserListHeader({ width }: { width: number }) {
  return (
    <Table.Row header accessibility={gridRowBehavior} style={{ width }}>
      <Table.Cell
        style={{ width: 40, maxWidth: 40 }}
        content=""
        accessibility={gridHeaderCellBehavior}
      />
      <Table.Cell
        accessibility={gridHeaderCellBehavior}
        as={Flex}
        vAlign="center"
      >
        <Text>
          {' '}
          <FormattedMessage
            id="userList.name"
            description="List of users name"
            defaultMessage="Name"
          />{' '}
        </Text>
        <Flex.Item push>
          <Button icon={<ArrowSortIcon />} text iconOnly />
        </Flex.Item>
      </Table.Cell>
      <Table.Cell
        accessibility={gridHeaderCellBehavior}
        as={Flex}
        vAlign="center"
      >
        <Text>
          {' '}
          <FormattedMessage
            id="userList.email"
            description="List of users email"
            defaultMessage="Email"
          />{' '}
        </Text>
        <Flex.Item push>
          <Button icon={<ArrowSortIcon />} text iconOnly />
        </Flex.Item>
      </Table.Cell>
      <Table.Cell
        accessibility={gridHeaderCellBehavior}
        as={Flex}
        vAlign="center"
      >
        <Text>
          {' '}
          <FormattedMessage
            id="userList.role"
            description="List of users role"
            defaultMessage="Role"
          />{' '}
        </Text>
        <Flex.Item push>
          <Button icon={<ArrowSortIcon />} text iconOnly />
        </Flex.Item>
      </Table.Cell>
      <Table.Cell
        accessibility={gridHeaderCellBehavior}
        as={Flex}
        vAlign="center"
      >
        <Text>
          {' '}
          <FormattedMessage
            id="userList.tutorialGroups"
            description="List of tutorial groups"
            defaultMessage="Tutorial groups"
          />{' '}
        </Text>
        <Flex.Item push>
          <Button icon={<ArrowSortIcon />} text iconOnly />
        </Flex.Item>
      </Table.Cell>
    </Table.Row>
  );
}

export interface UserListProps {
  userMemberships: SelectedUserCourseMemberships[];
  selectUser: (user: User) => void;
  unselectUser: (user: User) => void;
}
// eslint-disable-next-line max-lines-per-function
export default function UserList(props: UserListProps): JSX.Element {
  const { userMemberships, selectUser, unselectUser } = props;
  const rowCount = userMemberships.length;

  function rowRenderer({ index, style }: ListRowProps) {
    const userMembership = userMemberships[index];
    const { user } = userMembership;

    if (!user) return null;
    return (
      <UserListRow
        key={userMembership.user.id}
        style={style}
        userMembership={userMembership}
        selectUser={selectUser}
        unselectUser={unselectUser}
      />
    );
  }

  return (
    <AutoSizer>
      {({ width }: { width: number }) => (
        <Table accessibility={gridNestedBehavior} aria-rowcount={rowCount}>
          <UserListHeader width={width} />
          <ReactVirtualizedList
            disableHeader
            height={600} // TODO: move this to proper css??
            rowCount={rowCount}
            width={width}
            rowHeight={50}
            rowRenderer={rowRenderer}
            overscanRowCount={5}
            // eslint-disable-next-line react/jsx-props-no-spreading
            {...accessibilityListProperties}
          />
        </Table>
      )}
    </AutoSizer>
  );
}
