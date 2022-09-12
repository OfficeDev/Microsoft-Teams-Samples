import { Alert } from '@fluentui/react-northstar';
import { useActionCreator } from 'actionCreators';
import React from 'react';
import { useSelector } from 'react-redux';
import { selectGlobalErrorText } from 'selectors';

// eslint-disable-next-line sonarjs/cognitive-complexity
export function GlobalErrorComp(): JSX.Element {
  const errorText = useSelector(selectGlobalErrorText);
  const closeAlertBox = useActionCreator((s) => s.globalError.unSetError);
  return (
    <Alert
      content={errorText}
      dismissible
      danger
      onVisibleChange={() => closeAlertBox({})}
      dismissAction={{
        'aria-label': 'close',
      }}
    />
  );
}
