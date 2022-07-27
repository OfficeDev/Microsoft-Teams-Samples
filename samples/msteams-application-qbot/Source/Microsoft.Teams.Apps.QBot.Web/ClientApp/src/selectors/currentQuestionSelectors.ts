import { createMatchSelector } from 'connected-react-router';
import { flow, memoize } from 'lodash';
import { createSelector } from 'reselect';
import { QBotState } from 'compositionRoot';
import { selectAnswers } from './answerSelectors';
import { selectCurrentPathAnswer } from './currentAnswerSelectors';
import { selectQuestions, selectQuestionsById } from './questionSelectors';
import { selectUsersById } from './userSelectors';

export const selectPathQuestionId = flow(
  createMatchSelector<QBotState, { questionId?: string }>(
    '/:context/course/:courseId/channel/:channelId/question/:questionId',
  ),
  memoize((match) => match?.params.questionId),
);

export const selectCurrentPathQuestion = createSelector(
  selectQuestions,
  selectPathQuestionId,
  (questions, questionId) => {
    if (questionId === undefined && questions.length === 0) return undefined;
    return questions.find((q) => q.id === questionId);
  },
);

export const selectAnswersForCurrentQuestion = createSelector(
  selectAnswers,
  selectUsersById,
  selectQuestionsById,
  selectPathQuestionId,
  (answers, usersById, questionsById, questionId) => {
    const answersOfSelectedQuestion = answers.filter(
      (a) => a.questionId === questionId,
    );
    return answersOfSelectedQuestion.flatMap((answer) => {
      const question =
        questionId === answer.questionId &&
        questionsById.get(answer.questionId);
      const user = usersById.get(answer.authorId);
      return {
        ...answer,
        user,
        question,
      };
    });
  },
);

export const selectAnswerForCurrentQuestion = createSelector(
  selectCurrentPathAnswer,
  (answer) => {
    if (answer === undefined) return undefined;
    // const user = usersById.get(answer.authorId);
    const user = {};
    const question = {};
    return {
      ...answer,
      user,
      question,
    };
  },
);
