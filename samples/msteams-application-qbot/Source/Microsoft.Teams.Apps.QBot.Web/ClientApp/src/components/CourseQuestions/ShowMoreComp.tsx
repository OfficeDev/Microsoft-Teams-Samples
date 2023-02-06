import { Flex, Text } from '@fluentui/react-northstar';
import { useColorScheme } from 'hooks/useColorScheme';
import React, { SyntheticEvent, useState } from 'react';
import { FormattedMessage } from 'react-intl';

export interface ShowMoreCompProps {
  text: string;
  num?: number;
}

// eslint-disable-next-line sonarjs/cognitive-complexity
export const ShowMoreComp = ({
  text,
  num = 300,
}: ShowMoreCompProps): JSX.Element => {
  const [showMore, setShowMore] = useState<boolean>(false);
  const colorScheme = useColorScheme<'foreground1' | 'foreground2'>('brand');
  const getText = (num: number) => {
    // For Text that is shorter than desired length
    if (text.length <= num) return text;
    // If text is longer than desired length & showMore is true
    if (text.length > num && showMore) {
      return <>{text}</>;
    }
    if (text.length > num) {
      return <>{text}</>;
    }
  };

  const showMoreCondition = (num: number) => {
    if (text.length > num && showMore) {
      return (
        <>
          <Text
            className="see-more-button"
            styles={{ color: colorScheme.foreground2 }}
            size="small"
            content={
              <FormattedMessage
                id="ShowMoreComp.seeLess"
                description="Collapses the long text."
                defaultMessage="See less"
              />
            }
            onClick={(e: SyntheticEvent<HTMLSpanElement, MouseEvent>) => {
              e.stopPropagation();
              setShowMore(false);
            }}
          />
        </>
      );
    }
    //If text is longer than desired length & showMore is false
    if (text.length > num) {
      return (
        <>
          <Text
            className="see-more-button"
            styles={{ color: colorScheme.foreground2 }}
            size="small"
            content={
              <FormattedMessage
                id="ShowMoreComp.seeMore"
                description="Expands the long text."
                defaultMessage="See more"
              />
            }
            onClick={(e: SyntheticEvent<HTMLSpanElement, MouseEvent>) => {
              e.stopPropagation();
              setShowMore(true);
            }}
          />
        </>
      );
    }
  };

  return (
    <Flex styles={{ marginTop: '0' }} column>
      <Flex column>
        <Text
          className={
            !showMore && text.length > num
              ? 'less-answer-text'
              : 'more-answer-text'
          }
          size="medium"
          content={getText(num)}
        />
        {showMoreCondition(num)}
      </Flex>
    </Flex>
  );
};
