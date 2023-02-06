import { Flex, Grid } from '@fluentui/react-northstar';
import { useActionCreator } from 'actionCreators';
import ChannelQuestionsPage from 'components/ChannelQuestions/ChannelQuestionsPage';
import React, { useEffect } from 'react';
import { Route, Switch } from 'react-router';

// eslint-disable-next-line max-lines-per-function
export default function SharedApp(): JSX.Element {
  const loadCourses = useActionCreator((s) => s.course.loadCourses);

  useEffect(() => {
    // When this component loads, make sure to load all the questions and answers.
    loadCourses();
  }, [loadCourses]);
  return (
    <Grid columns="270px 1fr" rows="0px auto">
      <Flex
        styles={{
          gridColumnStart: 1,
          gridRow: 1,
          gridColumnEnd: 3,
        }}
      >
        <Switch>
          <Route
            path="/share/courses/:courseId/channel/:channelId"
            component={ChannelQuestionsPage}
          />
        </Switch>
      </Flex>
    </Grid>
  );
}
