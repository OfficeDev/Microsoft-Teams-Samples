import { Flex, Grid, useCSS } from '@fluentui/react-northstar';
import CourseQuestionsPage from 'components/CourseQuestions';
import CourseSetupPage from 'components/CourseSetup';
import LeftRail from 'components/LeftRail';
import React from 'react';
import { Route, Switch } from 'react-router';
import { useDefaultColorScheme } from 'hooks/useColorScheme';
import { selectGlobalErrorState, selectIfNoCourses } from 'selectors';
import { useSelector } from 'react-redux';
import { NoCourses } from './NoCourses';
import { GlobalErrorComp } from 'components/GlobalErrorComp';
import { KnowledgeBaseConfigurationPage } from 'components/KnowledgeBaseConfiguration';

// eslint-disable-next-line max-lines-per-function
export default function PersonalApp(): JSX.Element {
  const colorScheme = useDefaultColorScheme();
  const noCoursesPresent = useSelector(selectIfNoCourses);
  const isErrorOccurred = useSelector(selectGlobalErrorState);
  const LeftRailContainer = useCSS({
    ul: {
      backgroundColor: colorScheme.background3,
    },
    backgroundColor: colorScheme.background3,
    'li:hover': {
      background: colorScheme.backgroundFocus1,
    },
  });
  if (noCoursesPresent)
    return (
      <>
        {isErrorOccurred && <GlobalErrorComp />}
        <NoCourses />
      </>
    );

  return (
    <>
      {isErrorOccurred && <GlobalErrorComp />}
      <Grid
        styles={{ height: '100%', overflow: 'hidden' }}
        columns="270px 1fr"
        rows="auto"
      >
        <Flex
          className={LeftRailContainer}
          styles={{
            gridColumnStart: 1,
            gridColumnEnd: 2,
            gridRow: 1,
          }}
          gap="gap.small"
          column
        >
          <LeftRail />
        </Flex>
        <Flex
          styles={{
            gridColumnStart: 2,
            gridRow: 1,
            gridColumnEnd: 3,
            backgroundColor: colorScheme.background2,
          }}
        >
          <Switch>
            <Route
              path="/personal/courses/:courseId/configure/*"
              component={CourseSetupPage}
            />
            <Route
              path="/personal/courses/:courseId"
              component={CourseQuestionsPage}
            />
            <Route
              path="/personal/knowledgeBases/:knowledgeBaseId"
              component={KnowledgeBaseConfigurationPage}
            />
          </Switch>
        </Flex>
      </Grid>
    </>
  );
}
