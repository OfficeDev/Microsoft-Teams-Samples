import { Text, Flex, Loader } from '@fluentui/react-northstar';
import * as microsoftTeams from '@microsoft/teams-js';
import { IFluidContainer } from 'fluid-framework';
import { useMemo } from 'react';
import styles from './LiveSharePage.module.css';

export interface LiveSharePageProps {
  context: microsoftTeams.app.Context;
  container: IFluidContainer | undefined;
  started: boolean;
  children?: JSX.Element | JSX.Element[];
  userIsAnonymous: boolean;
}

/**
 * A component that shows a spinner while the Live Share is starting.
 * We wait for teamsContext to load, the Live Share container to start,
 * and the Live share objects have started.
 */
export const LiveSharePage = ({
  children,
  context,
  container,
  started,
  userIsAnonymous,
}: LiveSharePageProps) => {
  const loadText = useMemo(() => {
    if (userIsAnonymous)
    {
      // Anonymous users do not have an AzureAD account are not supported by Live Share, so skip waiting for it to load
      return undefined;
    }

    if (!context) {
      return 'Loading Teams Client SDK...';
    }
    if (!container) {
      return 'Joining Live Share session...';
    }
    if (!started) {
      return 'Starting sync...';
    }
    return undefined;
  }, [context, container, started]);

  return (
    <>
      {loadText && (
        <Flex
          column
          hAlign="center"
          vAlign="center"
          className={styles.liveSharePage}
        >
          <Loader />
          <Text>{loadText}</Text>
        </Flex>
      )}
      <div style={{ visibility: loadText ? 'hidden' : undefined }}>
        {children}
      </div>
    </>
  );
};
