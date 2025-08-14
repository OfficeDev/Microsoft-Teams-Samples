import { Flex, Image } from '@fluentui/react-northstar';
import React from 'react';

// eslint-disable-next-line sonarjs/cognitive-complexity
export function ErrorComp(): JSX.Element {
  return (
    <Flex
      styles={{ marginTop: '3.5rem' }}
      vAlign="center"
      hAlign="center"
      column
      gap="gap.small"
    >
      <Image styles={{ width: '100%' }} src={'/Illustration.png'} />
    </Flex>
  );
}
