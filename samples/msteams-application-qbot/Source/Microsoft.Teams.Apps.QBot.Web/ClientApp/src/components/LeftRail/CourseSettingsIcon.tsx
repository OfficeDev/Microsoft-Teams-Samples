import { Button, SettingsIcon } from '@fluentui/react-northstar';
import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import { usePushRelativePath } from 'hooks';
import { Course } from 'models';

function CourseTutorialGroupsSettingsIcon({
  onSettingsClicked,
}: {
  onSettingsClicked: VoidFunction;
}) {
  const onSettingsClick = useCallback(
    (evt: React.SyntheticEvent<HTMLElement, Event>) => {
      evt.stopPropagation();
      evt.preventDefault();
      evt.nativeEvent.stopImmediatePropagation();
      onSettingsClicked();
    },
    [onSettingsClicked],
  );
  return (
    <Button text iconOnly icon={<SettingsIcon />} onClick={onSettingsClick} />
  );
}

export interface CourseSettingsIconProps {
  course: Course;
}

export function CourseSettingsIcon({
  course,
}: CourseSettingsIconProps): JSX.Element {
  const dispatch = useDispatch();
  const push = usePushRelativePath();

  if (course === undefined) {
    return <></>;
  }

  function onButtonClicked() {
    dispatch(push(`/courses/${course?.teamAadObjectId}/configure/general`));
  }

  return (
    <CourseTutorialGroupsSettingsIcon onSettingsClicked={onButtonClicked} />
  );
}

export default CourseSettingsIcon;
