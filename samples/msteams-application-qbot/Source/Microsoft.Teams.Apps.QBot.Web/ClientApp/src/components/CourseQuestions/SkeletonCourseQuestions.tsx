import React from 'react';
import { Skeleton, Flex, useCSS, FlexItem } from '@fluentui/react-northstar';
import { range } from 'lodash';

function SkeletonCourseQuestions({
  selectedTab,
}: {
  selectedTab: number;
}): JSX.Element {
  const containerQuestionlayout = useCSS({
    paddingLeft: '0.4rem',
    paddingRight: '0.4rem',
  });
  return (
    <Flex column gap="gap.smaller">
      {range(0, 3).map((id) => {
        return (
          <Skeleton
            animation="wave"
            key={id}
            style={{
              backgroundColor: id % 2 === 0 ? 'whitesmoke' : 'transparent',
              mixBlendMode: 'luminosity',
            }}
          >
            <Flex
              gap="gap.smaller"
              vAlign="center"
              styles={{ padding: '0.4rem' }}
            >
              <Skeleton.Avatar size="smallest" />
              <Skeleton.Line width="13%" height="1.2rem" />
              <Skeleton.Line width="9%" height="1.2rem" />
            </Flex>
            <Flex className={containerQuestionlayout}>
              <Skeleton.Line width="70%" height="1.5rem" />
              <FlexItem push align="center">
                <Skeleton.Line width="12%" styles={{ marginRight: '0.2rem' }} />
              </FlexItem>
              <Skeleton.Shape
                height="1rem"
                width="1rem"
                style={{ borderRadius: '.3rem', marginTop: '0.2rem' }}
              />
            </Flex>
            <Flex gap="gap.smaller" styles={{ padding: '0.4rem' }}>
              <Skeleton.Shape
                height="1rem"
                width="1rem"
                style={{ borderRadius: '.3rem' }}
              />
              <Skeleton.Line width="13%" />
              <Skeleton.Line width="13%" />
            </Flex>
            {selectedTab === 1 && (
              <Flex column styles={{ padding: '0 0.4rem 0.4rem 0.4rem' }}>
                <Skeleton.Line width="80%" />
                <Skeleton.Line width="70%" />
                <Skeleton.Line width="5%" />
              </Flex>
            )}
          </Skeleton>
        );
      })}
    </Flex>
  );
}

export default SkeletonCourseQuestions;
