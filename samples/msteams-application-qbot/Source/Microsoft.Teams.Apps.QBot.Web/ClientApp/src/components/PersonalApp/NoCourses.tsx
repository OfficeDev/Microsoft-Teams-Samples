import { Flex, Text, Image } from '@fluentui/react-northstar';
import React from 'react';
import { useDefaultColorScheme } from 'hooks/useColorScheme';
import { FormattedMessage } from 'react-intl';

// eslint-disable-next-line sonarjs/cognitive-complexity
export function NoCourses(): JSX.Element {
  const colorScheme = useDefaultColorScheme();
  return (
    <Flex
      styles={{ paddingTop: '5rem', background: colorScheme.background2 }}
      vAlign="center"
      hAlign="center"
      column
      gap="gap.large"
    >
      <Image styles={{ paddingTop: '2rem' }} src={'/Illustration.png'} />
      <Text
        styles={{ marginBottom: '0.2rem !important' }}
        size="large"
        weight="bold"
        content={
          <FormattedMessage
            id="noCourses.noCoursesMessage"
            description="Display message when no courses found."
            defaultMessage="No courses found!"
          />
        }
      />
    </Flex>
  );
}
