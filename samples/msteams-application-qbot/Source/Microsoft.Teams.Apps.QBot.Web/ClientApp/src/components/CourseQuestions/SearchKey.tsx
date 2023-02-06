import { Image, Text } from '@fluentui/react-northstar';
import React from 'react';
import { FormattedMessage } from 'react-intl';

function SearchKeyPresent({ searchKey }: { searchKey: string | undefined }) {
  const marginBottom = '1.5rem !important';
  return (
    <>
      <Image
        styles={{ marginBottom: marginBottom }}
        src={'/SearchFailure.png'}
      />
      <Text
        weight="semibold"
        content={
          <FormattedMessage
            id="noQuestions.noQuestionsSearchKey"
            description="When no questions asked by the selected user"
            defaultMessage={`We coudn't find any results for '${searchKey}'.`}
          />
        }
      />
    </>
  );
}

export default SearchKeyPresent;
