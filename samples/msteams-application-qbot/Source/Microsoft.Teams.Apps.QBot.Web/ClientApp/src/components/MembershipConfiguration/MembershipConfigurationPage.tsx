import React, { useEffect, useState, useCallback } from 'react';
import { useSelector } from 'react-redux';
import { produce } from 'immer';

import {
  CourseMember,
  User,
  CourseMemberRole,
  TutorialGroup,
  TutorialGroupMember,
} from 'models';
import {
  selectCurrentPathCourse,
  selectCurrentCourseMemberUsers,
  selectCurrentCourseUserMemberships,
  selectCurrentCourseTutorialGroups,
} from 'selectors';
import UserList from './UserList';
import TutorialGroupPageToolBar from './MembershipConfigurationPageToolBar';
import AssignTutorialGroupsDialog from './AssignTutorialGroupsDialog';
import SwitchRoleDialog from './SwitchRoleDialog';
import { trackPageComponent } from 'appInsights';
import { useActionCreator } from 'actionCreators';
import ReactDOM from 'react-dom';
import { groupBy } from 'lodash';
import { isNotNullOrUndefined } from 'util/isNotNullOrUndefined';

function useFilteredUsers(users: User[]) {
  const [selectedUserIds, setSelectedUserIds] = useState(new Set<string>());
  function isUserSelected(user: User) {
    return selectedUserIds.has(user.id);
  }
  const selectedUsers = users.filter(isUserSelected);
  function selectUser(user: User) {
    const nextSet = produce(selectedUserIds, (draftSet) => {
      draftSet.add(user.id);
    });
    setSelectedUserIds(nextSet);
  }
  function unselectUser(user: User) {
    const nextSet = produce(selectedUserIds, (draftSet) => {
      draftSet.delete(user.id);
    });
    setSelectedUserIds(nextSet);
  }
  function clearUserSelections() {
    setSelectedUserIds(new Set<string>());
  }

  return {
    selectedUsers,
    isUserSelected,
    selectUser,
    unselectUser,
    clearUserSelections,
  };
}

export interface MembershipConfigurationPageProps {
  currentCourseRole: CourseMemberRole;
  toolbarRef: HTMLElement;
}
// TODO(nibeauli): find a way to refactor this into smaller chunks
// eslint-disable-next-line max-lines-per-function
function MembershipConfigurationPage(
  props: MembershipConfigurationPageProps,
): JSX.Element {
  const { currentCourseRole, toolbarRef } = props;
  const loadCourseMembers = useActionCreator(
    (s) => s.courseMember.loadCourseMembers,
  );
  const loadTutorialGroups = useActionCreator(
    (s) => s.tutorialGroup.loadTutorialGroups,
  );
  const assignRoleCommand = useActionCreator((s) => s.courseMember.assignRoles);

  const users = useSelector(selectCurrentCourseMemberUsers);
  const userMemberships = useSelector(selectCurrentCourseUserMemberships);
  const tutorialGroups = useSelector(selectCurrentCourseTutorialGroups);
  const course = useSelector(selectCurrentPathCourse);
  useEffect(() => {
    if (!course?.id) return;
    loadCourseMembers({ courseId: course.id });
    loadTutorialGroups({ courseId: course.id });
  }, [course?.id, loadCourseMembers, loadTutorialGroups]);
  const {
    selectedUsers,
    clearUserSelections,
    isUserSelected,
    selectUser,
    unselectUser,
  } = useFilteredUsers(users);
  const assignRole = useCallback(
    (members: CourseMember[]) => {
      const commandMembers = members
        .map<
          | {
              user: User;
              tutorialGroupMemberships: TutorialGroupMember[];
              courseMembership: CourseMember;
            }
          | undefined
        >((member) => {
          const userMembership = userMemberships.find(
            (m) => m.user.aadId === member.userId,
          );
          if (!userMembership) {
            console.warn('Unable to find user membership for user', { member });
            return undefined;
          }
          const courseId = userMembership.course.id;
          const userId = userMembership.user.id;
          return {
            user: userMembership.user,
            tutorialGroupMemberships: userMembership.tutorialGroups.map(
              (tg) => ({
                courseId,
                tutorialGroupId: tg.id,
                userId: userId,
              }),
            ),
            courseMembership: {
              courseId,
              userId,
              role: member.role,
            },
          };
        })
        .filter(isNotNullOrUndefined);
      assignRoleCommand({ members: commandMembers });
    },
    [assignRoleCommand, userMemberships],
  );

  const assignTutorialGroups = useCallback(
    (members: TutorialGroupMember[]) => {
      const membersByUserId = groupBy(members, (m) => m.userId);
      const commandMembers = Object.values(membersByUserId)
        .map((ums) => {
          const { userId: queryId } = ums[0];
          const userMembership = userMemberships.find(
            (m) => m.user.aadId === queryId,
          );
          if (!userMembership) {
            console.warn('Unable to find user membership for user', { ums });
            return undefined;
          }
          const courseId = userMembership.course.id;
          const userId = userMembership.user.id;
          return {
            user: userMembership.user,
            tutorialGroupMemberships: ums,
            courseMembership: {
              courseId,
              userId,
              role: userMembership.role,
            },
          };
        })
        .filter(isNotNullOrUndefined);
      assignRoleCommand({ members: commandMembers });
    },
    [assignRoleCommand, userMemberships],
  );

  // State for tutorial group assignment dialog
  const [
    isAssignTutorialGroupsDialogOpen,
    setIsAssignTutorialGroupsDialogOpen,
  ] = useState(false);
  const [
    selectedAssignTutorialGroups,
    setSelectedAssignTutorialGroups,
  ] = useState<TutorialGroup[]>([]);
  // State for switch role dialog
  const [isSwitchRoleDialogOpen, setIsSwitchRoleDialogOpen] = useState(false);
  const [
    currentlySelectedRole,
    setCurrentlySelectedRole,
  ] = useState<CourseMemberRole>('Student');

  const userMembershipsWithSelected = userMemberships.map((userMembership) => ({
    ...userMembership,
    isSelected: isUserSelected(userMembership.user),
  }));

  const nonDefaultTutorialGroups = tutorialGroups.filter(
    (tg) => tg.id !== course?.defaultTutorialGroupId,
  );

  function onConfirmAssignTutorialGroups() {
    if (course === undefined) return;
    const selectedUserMapObjects = userMemberships.filter((obj) =>
      isUserSelected(obj.user),
    );

    const members = selectedUserMapObjects.flatMap<TutorialGroupMember>(
      (user) =>
        selectedAssignTutorialGroups.map((tutorialGroup) => ({
          courseId: course.id,
          tutorialGroupId: tutorialGroup.id,
          userId: user.user.id,
        })),
    );

    setIsAssignTutorialGroupsDialogOpen(false);
    clearUserSelections();
    assignTutorialGroups(members);
  }

  function onConfirmSwitchRole() {
    if (!course) {
      return;
    }
    const selectedUserMemberships = userMemberships.filter((userMembership) =>
      isUserSelected(userMembership.user),
    );
    const courseMemberships: CourseMember[] = selectedUserMemberships.map(
      (memberships) => ({
        courseId: course.id,
        role: currentlySelectedRole,
        userId: memberships.user.id,
      }),
    );

    setIsSwitchRoleDialogOpen(false);
    setCurrentlySelectedRole('Student');
    // can re-use this callback for this purpose as well.
    assignRole(courseMemberships);
  }

  const ToolBar = ReactDOM.createPortal(
    <TutorialGroupPageToolBar
      currentCourseRole={currentCourseRole}
      userCount={selectedUsers.length}
      onClearSelection={clearUserSelections}
      onAssignTutorialGroups={() => setIsAssignTutorialGroupsDialogOpen(true)}
      onSwitchRole={() => setIsSwitchRoleDialogOpen(true)}
    />,
    toolbarRef,
  );
  return (
    <>
      {ToolBar}
      <AssignTutorialGroupsDialog
        isOpen={isAssignTutorialGroupsDialogOpen}
        onClose={() => setIsAssignTutorialGroupsDialogOpen(false)}
        onConfirm={onConfirmAssignTutorialGroups}
        onSelectionUpdate={(nextTutorialGroups) =>
          setSelectedAssignTutorialGroups(nextTutorialGroups)
        }
        tutorialGroups={nonDefaultTutorialGroups}
        selectedUserCount={selectedUsers.length}
      />
      <SwitchRoleDialog
        isOpen={isSwitchRoleDialogOpen}
        onClose={() => setIsSwitchRoleDialogOpen(false)}
        onConfirm={onConfirmSwitchRole}
        onSelectionUpdate={(newRole) => setCurrentlySelectedRole(newRole)}
      />
      <UserList
        unselectUser={unselectUser}
        selectUser={selectUser}
        userMemberships={userMembershipsWithSelected}
      />
    </>
  );
}

export default trackPageComponent(MembershipConfigurationPage);
