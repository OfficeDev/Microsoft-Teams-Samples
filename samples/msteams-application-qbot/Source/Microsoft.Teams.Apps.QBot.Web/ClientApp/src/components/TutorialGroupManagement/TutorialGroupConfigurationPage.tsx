/* eslint-disable @typescript-eslint/no-empty-function */
import React, { useCallback, useEffect, useState, useMemo } from 'react';
import { useSelector } from 'react-redux';
import { useActionCreator } from 'actionCreators';
import {
  currentCourseTutorialGroupMembersSelector,
  selectCurrentPathCourse,
} from 'selectors';
import ReactDOM from 'react-dom';
import {
  gridCellBehavior,
  gridHeaderCellBehavior,
  gridNestedBehavior,
  gridRowBehavior,
  Table,
  Text,
  Flex,
  Box,
  UserFriendsIcon,
  Button,
  MenuButton,
  MoreIcon,
  EditIcon,
  TrashCanIcon,
} from '@fluentui/react-northstar';
import {
  AutoSizer,
  List as ReactVirtualizedList,
  ListRowProps,
  ListProps,
} from 'react-virtualized';
import { orderBy } from 'lodash';

import { TutorialGroup } from 'models/TutorialGroup';

import {
  TutorialGroupPageToolBar,
  TutorialGroupPageToolBarProps,
} from './TutorialGroupPageToolBar';
import {
  CreateTutorialGroupsDialog,
  CreateTutorialGroupsDialogProps,
} from './CreateTutorialGroupDialog';
import {
  EditTutorialGroupsDialog,
  EditTutorialGroupsDialogProps,
} from './EditTutorialGroupDialog';
import {
  DeleteTutorialGroupsDialog,
  DeleteTutorialGroupsDialogProps,
} from './DeleteTutorialGroupConfirmDialog';
import { FormattedMessage } from 'react-intl';

type TutorialGroupMembers = ReturnType<
  typeof currentCourseTutorialGroupMembersSelector
>[number];
// Overrides ARIA attributes assigned by default, which break accessibility
const accessibilityListProperties: Partial<ListProps> = {
  'aria-label': '',
  'aria-readonly': undefined,
  containerRole: 'presentation',
  role: 'presentation',
};

// TODO(nibeauli): refactor this into something clearer
// eslint-disable-next-line max-lines-per-function, sonarjs/cognitive-complexity
function TutorialGroupListRow({
  tutorialGroupMembership,
  onEditTutorialGroup,
  onDeleteTutorialGroup,
  style,
}: {
  tutorialGroupMembership: TutorialGroupMembers;
  style: React.CSSProperties;
  onEditTutorialGroup: (tutorialGroup: TutorialGroup) => void;
  onDeleteTutorialGroup: (tutorialGroup: TutorialGroup) => void;
}) {
  const menuItems = [
    {
      content: (
        <FormattedMessage
          id="tutorialGroupConfigurationPage.editItems"
          description="Edit item for tutorial group configuration page"
          defaultMessage="Edit"
        />
      ),
      key: 'edit',
      icon: <EditIcon />,
    },
    {
      content: (
        <FormattedMessage
          id="tutorialGroupConfigurationPage.deleteItem"
          description="Delete item for tutorial group configuration page"
          defaultMessage="Delete"
        />
      ),
      key: 'delete',
      icon: <TrashCanIcon />,
    },
  ];
  const callbacks = useMemo(
    () => [onEditTutorialGroup, onDeleteTutorialGroup],
    [onEditTutorialGroup, onDeleteTutorialGroup],
  );
  const onMenuItemClicked = useCallback(
    (evt: any, props?: { index?: number | undefined | string }) => {
      if (typeof props?.index !== 'number') return;
      const cb = callbacks[props.index];
      cb(tutorialGroupMembership.tutorialGroup);
    },
    [callbacks, tutorialGroupMembership.tutorialGroup],
  );
  return (
    <Table.Row
      key={tutorialGroupMembership.tutorialGroup.id}
      header
      accessibility={gridRowBehavior}
      style={style}
    >
      <Table.Cell
        accessibility={gridCellBehavior}
        as={Flex}
        gap="gap.medium"
        vAlign="center"
        style={{ width: 200, maxWidth: 200 }}
      >
        <Flex column vAlign="center">
          <Text size="medium">
            {tutorialGroupMembership.tutorialGroup.shortName}
          </Text>
        </Flex>
      </Table.Cell>
      <Table.Cell
        content={tutorialGroupMembership.tutorialGroup.displayName}
        accessibility={gridCellBehavior}
      />
      <Table.Cell accessibility={gridCellBehavior}>
        <Flex vAlign="center">
          <Text size="medium">{tutorialGroupMembership.members.length}</Text>
        </Flex>
      </Table.Cell>
      <Table.Cell style={{ width: 80, maxWidth: 80, paddingRight: '1em' }}>
        <Flex className="more-icon" vAlign="center">
          <MenuButton
            trigger={<Button icon={<MoreIcon />} iconOnly text />}
            menu={menuItems}
            onMenuItemClick={onMenuItemClicked}
          />
        </Flex>
      </Table.Cell>
    </Table.Row>
  );
}

// TODO(nibeauli): when sorts implemented factor out the cells into components
// eslint-disable-next-line max-lines-per-function
function TutorialGroupListHeader({ width }: { width: number }) {
  return (
    <Table.Row header accessibility={gridRowBehavior} style={{ width }}>
      <Table.Cell
        accessibility={gridHeaderCellBehavior}
        as={Flex}
        vAlign="center"
        style={{ width: 200, maxWidth: 200 }}
      >
        <Text> Code </Text>
      </Table.Cell>
      <Table.Cell
        accessibility={gridHeaderCellBehavior}
        as={Flex}
        vAlign="center"
      >
        <Text> Name </Text>
      </Table.Cell>
      <Table.Cell
        accessibility={gridHeaderCellBehavior}
        as={Flex}
        vAlign="center"
      >
        <UserFriendsIcon outline style={{ marginRight: '.5em' }} />
        <Text> Members </Text>
      </Table.Cell>
      <Table.Cell
        style={{ width: 80, maxWidth: 80 }}
        content=""
        accessibility={gridHeaderCellBehavior}
      />
    </Table.Row>
  );
}

export interface TutorialGroupListProps {
  tutorialGroupMemberships: TutorialGroupMembers[];
  onEditTutorialGroup: (tutorialGroup: TutorialGroup) => void;
  onDeleteTutorialGroup: (tutorialGroup: TutorialGroup) => void;
}
// eslint-disable-next-line max-lines-per-function
export default function TutorialGroupList(
  props: TutorialGroupListProps,
): JSX.Element {
  const {
    onEditTutorialGroup,
    onDeleteTutorialGroup,
    tutorialGroupMemberships,
  } = props;
  const orderedTutorialGroups = useMemo(
    () => orderBy(tutorialGroupMemberships, (tg) => tg.tutorialGroup.shortName),
    [tutorialGroupMemberships],
  );
  const rowCount = tutorialGroupMemberships.length;

  const rowRenderer = useCallback(
    ({ index, style }: ListRowProps) => {
      const tutorialGroupMembership = orderedTutorialGroups[index];
      return (
        <TutorialGroupListRow
          key={tutorialGroupMembership.tutorialGroup.id}
          style={style}
          tutorialGroupMembership={tutorialGroupMembership}
          onEditTutorialGroup={onEditTutorialGroup}
          onDeleteTutorialGroup={onDeleteTutorialGroup}
        />
      );
    },
    [onEditTutorialGroup, orderedTutorialGroups, onDeleteTutorialGroup],
  );

  return (
    <Box styles={{ height: '100%', width: '100%', marginTop: '10px' }}>
      <AutoSizer>
        {({ width, height }: { width: number; height: number }) => (
          <Table accessibility={gridNestedBehavior} aria-rowcount={rowCount}>
            <TutorialGroupListHeader width={width} />
            <ReactVirtualizedList
              disableHeader
              height={height - 75}
              rowCount={rowCount}
              width={width}
              rowHeight={55}
              rowRenderer={rowRenderer}
              overscanRowCount={5}
              // eslint-disable-next-line react/jsx-props-no-spreading
              {...accessibilityListProperties}
            />
          </Table>
        )}
      </AutoSizer>
    </Box>
  );
}
export interface TutorialGroupConfigurationPageProps {
  toolbarRef: HTMLElement;
}
// eslint-disable-next-line max-lines-per-function
export function TutorialGroupConfigurationPage(
  props: TutorialGroupConfigurationPageProps,
): JSX.Element {
  const { toolbarRef } = props;
  const course = useSelector(selectCurrentPathCourse);
  const [isAddDialogOpen, setAddDialogOpen] = useState(false);
  const [isEditDialogOpen, setEditDialogOpen] = useState(false);
  const [isDeleteConfirmDialogOpen, setDeleteConfirmDialogOpen] = useState(
    false,
  );
  const [newTutorialGroup, setNewTutorialGroup] = useState({
    displayName: '',
    shortName: '',
    courseId: '',
  } as TutorialGroup);

  const [editTutorialGroup, setEditTutorialGroup] = useState({
    displayName: '',
    shortName: '',
    courseId: '',
  } as TutorialGroup);

  const [deleteTutorialGroup, setDeleteTutorialGroup] = useState({
    displayName: '',
    shortName: '',
    courseId: '',
  } as TutorialGroup);

  const tutorialGroupMemberships = useSelector(
    currentCourseTutorialGroupMembersSelector,
  );
  const tutorialGroups = useMemo(
    () => tutorialGroupMemberships.map((tg) => tg.tutorialGroup),
    [tutorialGroupMemberships],
  );

  const loadCourseMembersCommand = useActionCreator(
    (s) => s.courseMember.loadCourseMembers,
  );
  const loadTutorialGroupsCommand = useActionCreator(
    (s) => s.tutorialGroup.loadTutorialGroups,
  );
  const createTutorialGroupCommand = useActionCreator(
    (s) => s.tutorialGroup.createTutorialGroup,
  );
  const editTutorialGroupCommand = useActionCreator(
    (s) => s.tutorialGroup.editTutorialGroup,
  );
  const deleteTutorialGroupCommand = useActionCreator(
    (s) => s.tutorialGroup.deleteCommandHandler,
  );

  useEffect(() => {
    if (!course?.id) return;
    loadCourseMembersCommand({ courseId: course.id });
    loadTutorialGroupsCommand({ courseId: course.id });
  }, [course?.id, loadCourseMembersCommand, loadTutorialGroupsCommand]);
  const onAddTutorialGroupButtonPressed = useCallback(() => {
    setAddDialogOpen(true);
  }, [setAddDialogOpen]);
  const onCreateNewTutorialGroup = useCallback(() => {
    if (!course?.id) {
      console.warn('No course! Not creating the tutorialGroup');
      return;
    }
    createTutorialGroupCommand({
      tutorialGroup: {
        ...newTutorialGroup,
        courseId: course?.id,
      },
    });

    setNewTutorialGroup({
      displayName: '',
      shortName: '',
      courseId: course.id,
      id: '',
    } as TutorialGroup);
    setAddDialogOpen(false);
  }, [
    setAddDialogOpen,
    setNewTutorialGroup,
    course?.id,
    newTutorialGroup,
    createTutorialGroupCommand,
  ]);
  const onEditTutorialGroup = useCallback(
    (tutorialGroup: TutorialGroup) => {
      setEditTutorialGroup(tutorialGroup);
      setEditDialogOpen(true);
    },
    [setEditTutorialGroup, setEditDialogOpen],
  );
  const onCommitEditTutorialGroup = useCallback(() => {
    console.log('onCommitEditTutorialGroup', { course, editTutorialGroup });

    if (!course) {
      console.warn('No course! Not editing the tutorialGroup');
      return;
    }
    editTutorialGroupCommand({ tutorialGroup: { ...editTutorialGroup } });

    setEditTutorialGroup({
      displayName: '',
      shortName: '',
      courseId: course.id,
      id: '',
    } as TutorialGroup);
    setEditDialogOpen(false);
  }, [
    setEditDialogOpen,
    setEditTutorialGroup,
    editTutorialGroupCommand,
    course,
    editTutorialGroup,
  ]);

  const tutorialGroupPageToolBarProps: TutorialGroupPageToolBarProps = {
    onAddTutorialGroup: onAddTutorialGroupButtonPressed,
  };

  const createTutorialGroupsDialogProps: CreateTutorialGroupsDialogProps = {
    existingTutorialGroups: tutorialGroups,
    isOpen: isAddDialogOpen,
    onClose: useCallback(() => {
      setAddDialogOpen(false);
    }, []),
    onConfirm: onCreateNewTutorialGroup,
    onUpdate: setNewTutorialGroup,
    tutorialGroup: newTutorialGroup,
  };

  const editTutorialGroupsDialogProps: EditTutorialGroupsDialogProps = {
    existingTutorialGroups: tutorialGroups,
    isOpen: isEditDialogOpen,
    onClose: useCallback(() => {
      setEditDialogOpen(false);
    }, []),
    onConfirm: onCommitEditTutorialGroup,
    onUpdate: setEditTutorialGroup,
    tutorialGroup: editTutorialGroup,
  };

  const onDeleteTutorialGroup = useCallback(
    (tutorialGroup: TutorialGroup) => {
      setDeleteTutorialGroup(tutorialGroup);
      setDeleteConfirmDialogOpen(true);
    },
    [setDeleteTutorialGroup, setDeleteConfirmDialogOpen],
  );
  const onCommitDeleteTutorialGroup = useCallback(() => {
    console.log('onCommitDeleteTutorialGroup', { course, deleteTutorialGroup });

    if (!course) {
      console.warn('No course! Not deleting the tutorialGroup');
      return;
    }
    deleteTutorialGroupCommand({ tutorialGroup: { ...deleteTutorialGroup } });

    setDeleteTutorialGroup({
      displayName: '',
      shortName: '',
      courseId: '',
      id: '',
    } as TutorialGroup);
    setDeleteConfirmDialogOpen(false);
  }, [
    deleteTutorialGroupCommand,
    course,
    deleteTutorialGroup,
    setDeleteTutorialGroup,
    setDeleteConfirmDialogOpen,
  ]);
  const deleteTutorialGroupsDialogProps: DeleteTutorialGroupsDialogProps = {
    isOpen: isDeleteConfirmDialogOpen,
    onClose: useCallback(() => {
      setDeleteConfirmDialogOpen(false);
    }, []),
    onConfirm: onCommitDeleteTutorialGroup,
    tutorialGroup: deleteTutorialGroup,
  };
  const ToolBar = ReactDOM.createPortal(
    <TutorialGroupPageToolBar {...tutorialGroupPageToolBarProps} />,
    toolbarRef,
  );

  return (
    <>
      {ToolBar}
      <CreateTutorialGroupsDialog {...createTutorialGroupsDialogProps} />
      <EditTutorialGroupsDialog {...editTutorialGroupsDialogProps} />
      <DeleteTutorialGroupsDialog {...deleteTutorialGroupsDialogProps} />
      <TutorialGroupList
        tutorialGroupMemberships={tutorialGroupMemberships}
        onEditTutorialGroup={onEditTutorialGroup}
        onDeleteTutorialGroup={onDeleteTutorialGroup}
      />
    </>
  );
}
