import { Image, Text } from '@fluentui/react-northstar';
import React from 'react';
import { FormattedMessage } from 'react-intl';

function NoSearchKey({
  selectedTab,
  selectedCourseUser,
}: {
  selectedTab: number;
  selectedCourseUser: string | undefined;
}) {
  const marginBottom = '1.5rem !important';
  const searchKeyWithoutText = (
    selectedTab: number,
    selectedCourseUser: string | undefined,
  ) => {
    if (selectedTab === 0 && selectedCourseUser !== undefined) {
      return (
        <FormattedMessage
          id="noQuestions.noUserQuestion"
          description="When no questions asked by the selected user"
          defaultMessage="No questions has been asked by the selected user. Be the first one to ask a question."
        />
      );
    } else if (selectedTab === 1 && selectedCourseUser !== undefined) {
      return (
        <FormattedMessage
          id="noQuestions.noUserAnsweredQuestion"
          description="When no questions answered by the selected user"
          defaultMessage="No questions has been answered by the selected user."
        />
      );
    } else if (selectedTab === 0 && selectedCourseUser === undefined) {
      return (
        <FormattedMessage
          id="noQuestions.unAnswered"
          description="When no questions are posted in the course or channel."
          defaultMessage="No questions asked here yet. Be the first one to ask a question."
        />
      );
    } else if (selectedTab === 1 && selectedCourseUser === undefined) {
      return (
        <FormattedMessage
          id="noQuestions.answered"
          description="When no questions are posted in the course or channel."
          defaultMessage="No questions has been answered yet!"
        />
      );
    }
  };
  return (
    <>
      <Image
        styles={{ marginBottom: marginBottom, width: '59%' }}
        src={'/Illustration.png'}
      />
      <Text
        weight="semibold"
        content={searchKeyWithoutText(selectedTab, selectedCourseUser)}
      />
    </>
  );
}

export default NoSearchKey;
