import React from 'react';
import {
  Menu,
  Divider,
  Skeleton,
  MenuItem,
  Flex,
  useCSS,
} from '@fluentui/react-northstar';
import { range } from 'lodash';

export interface SkeletonCourseMenuProps {
  count: number;
}

// eslint-disable-next-line max-lines-per-function
function SkeletonCourseMenuItem({ id }: { id: number }) {
  const containerClassName = useCSS({
    paddingLeft: 0,
    paddingRight: 0,
    height: '32px',
  });
  return (
    <Skeleton
      animation="wave"
      key={id}
      style={{
        backgroundColor: id % 2 === 0 ? 'whitesmoke' : 'transparent',
      }}
    >
      <MenuItem
        index={id}
        styles={{
          paddingTop: 0,
          paddingBottom: 0,
        }}
      >
        <Flex gap="gap.small" className={containerClassName} vAlign="center">
          <Skeleton.Shape
            height="18.2px"
            width="18.2px"
            style={{ borderRadius: '.3rem' }}
          />
          <Skeleton.Button size="small" fluid />
        </Flex>
      </MenuItem>
    </Skeleton>
  );
}

export function SkeletonCourseMenu({
  count,
}: SkeletonCourseMenuProps): JSX.Element {
  const items = range(0, count).map((id) => (
    <SkeletonCourseMenuItem key={id} id={id} />
  ));
  return (
    <>
      <Divider content="Courses" />
      <Skeleton animation="wave">
        <Menu vertical pointing="start" styles={{ width: '100%' }}>
          {items}
        </Menu>
      </Skeleton>
    </>
  );
}

export default SkeletonCourseMenu;
