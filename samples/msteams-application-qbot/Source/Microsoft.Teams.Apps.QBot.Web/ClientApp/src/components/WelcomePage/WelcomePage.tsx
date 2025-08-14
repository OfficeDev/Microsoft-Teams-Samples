import React from 'react';
import { FormattedMessage } from 'react-intl';

export default function WelcomePage(): JSX.Element {
  return (
    <div>
      <FormattedMessage
        id="welcomePage.welcome"
        description="welcome text"
        defaultMessage="Welcome"
      />
    </div>
  );
}
