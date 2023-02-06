import { useActionCreator } from 'actionCreators';
import React, { useEffect } from 'react';
import { useSelector } from 'react-redux';
import { Route, Switch } from 'react-router';
import {
  selectCourseIdFromUrl,
  selectPathQuestionId,
  selectPathChannelId,
} from 'selectors';
import { AnswerList } from './AnswerList';
import { ErrorComp } from './ErrorComp';
import { SelectedAnswer } from './SelectedAnswer';

// eslint-disable-next-line max-lines-per-function
export default function TaskModule(): JSX.Element {
  const loadAnswers = useActionCreator((s) => s.answer.loadAnswers);
  const courseId = useSelector(selectCourseIdFromUrl);
  const questionId = useSelector(selectPathQuestionId);
  const channelId = useSelector(selectPathChannelId);

  useEffect(() => {
    // When this component loads, make sure to load all the questions and answers.
    if (courseId && channelId && questionId) {
      loadAnswers({ courseId, channelId, questionId });
    }
  }, [loadAnswers, courseId, channelId, questionId]);

  return (
    <Switch>
      <Route
        path="/taskmodule/course/:courseId/channel/:channelId/question/:questionId/selectedResponse/:answerId"
        component={SelectedAnswer}
      />
      <Route
        path="/taskmodule/course/:courseId/channel/:channelId/question/:questionId"
        component={AnswerList}
      />
      <Route
        path="/taskmodule/course/:courseId/channel/:channelId/question/:questionId/postAnswer/error"
        component={ErrorComp}
      />
    </Switch>
  );
}
