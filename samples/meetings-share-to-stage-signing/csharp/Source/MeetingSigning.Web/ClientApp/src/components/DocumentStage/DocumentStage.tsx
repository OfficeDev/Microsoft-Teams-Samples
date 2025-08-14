import { useEffect, useMemo, useRef, useState } from 'react';
import { Flex, Loader } from '@fluentui/react-northstar';
import { PresenceState } from '@microsoft/live-share';
import { useParams } from 'react-router-dom';
import {
  useDefaultColorScheme,
  useCursorLocationDom,
  useCursorLocationsLiveShare,
  useLiveShare,
  useScrollOffsetDom,
  useScrollOffsetLiveShare,
  useTakeControl,
} from 'hooks';
import { useUserIsAnonymous, useTeamsContext } from 'utils/TeamsProvider/hooks';
import { getDocument } from 'api/documentApi';
import { DocumentChooser } from 'components/Documents';
import { LiveSharePage } from 'components/LiveSharePage';
import { CursorsRenderer } from 'components/Cursor';
import { StageControls } from 'components/StageControls';
import { DocumentListDto } from 'models';
import { useQuery } from 'react-query';
import { AnonymousPage } from 'components/AnonymousPage';
import styles from './DocumentStage.module.css';

/**
 * A component that calls the `getDocument` API, get's the document and
 * renders it correctly on the Teams stage.
 *
 * @returns A component with a Document rendered on the stage
 */
export function DocumentStage() {
  const teamsContext = useTeamsContext();
  const params = useParams();
  const documentId: string = params.documentId ?? 'unknown';
  const pollingInterval = 2000;
  const userIsAnonymous = useUserIsAnonymous();
  var documentStageRef = useRef<HTMLDivElement>(null!);

  const { position, setPosition } = useScrollOffsetDom(
    documentStageRef.current,
  );
  const { cursorLocation } = useCursorLocationDom(documentStageRef.current);
  const {
    scrollOffsetEvent,
    cursorLocationsEvent,
    takeControlState,
    container,
    audience,
  } = useLiveShare();

  const {
    takeControlStarted,
    localUserInControl,
    localUserCanTakeControl,
    takeControl,
    clearControl,
  } = useTakeControl(takeControlState, teamsContext?.user, audience);

  const {
    scrollOffsetStarted,
    followSuspended,
    endSuspension,
    sendScrollOffset,
  } = useScrollOffsetLiveShare(
    scrollOffsetEvent,
    setPosition,
    takeControlState,
    localUserInControl,
    teamsContext?.user?.id,
  );

  const { cursorLocationsStarted, sendCursorLocation } =
    useCursorLocationsLiveShare(
      cursorLocationsEvent,
      teamsContext?.user?.displayName ?? teamsContext?.user?.userPrincipalName,
      teamsContext?.user?.id,
    );

  const started = useMemo(() => {
    return [
      takeControlStarted,
      scrollOffsetStarted,
      cursorLocationsStarted,
    ].every((value) => value === true);
  }, [takeControlStarted, scrollOffsetStarted, cursorLocationsStarted]);

  // We are using https://react-query.tanstack.com/ for handling the calls to our APIs.
  // Here when the documentId changes, React Query will fetch the document from the API.
  // We are also using the `refetchInterval` to query the API every 2 seconds.
  const { data, error } = useQuery<DocumentListDto, Error>(
    ['getDocument', { documentId, userIsAnonymous }],
    () => getDocument(documentId, userIsAnonymous),
    { refetchInterval: pollingInterval },
  );

  const colorScheme = useDefaultColorScheme();
  const stageInlineStyles = { background: colorScheme.background };

  const [showLoader, setShowLoader] = useState<boolean>(true);
  const showLoaderTimeout = 5000;

  useEffect(() => {
    const timer = setTimeout(() => {
      setShowLoader(false);
    }, showLoaderTimeout);

    return () => clearTimeout(timer);
  }, [setShowLoader]);

  useEffect(() => {
    if (!userIsAnonymous) {
      // When position is changed, we need to send the new scrollOffset to the other users via Live Share.
      sendScrollOffset(position);
    }
  }, [position, sendScrollOffset]);

  useEffect(() => {
    if (!userIsAnonymous) {
      sendCursorLocation(cursorLocation);
    }
  }, [cursorLocation, sendCursorLocation]);

  return (
    <>
      <LiveSharePage
        context={teamsContext}
        container={container}
        started={started}
        userIsAnonymous={userIsAnonymous}
      >
        <Flex
          className={styles.stageControlsDiv}
          hAlign="center"
          vAlign="center"
        >
          <StageControls
            localUserInControl={localUserInControl}
            localUserCanTakeControl={localUserCanTakeControl}
            userInControl={takeControlState?.data?.userId !== undefined}
            nameOfUserInControl={takeControlState?.data?.displayName}
            followSuspended={followSuspended}
            isLiveShareSupported={!userIsAnonymous}
            endSuspension={endSuspension}
            takeControl={takeControl}
            clearControl={clearControl}
          />
        </Flex>
        <Flex styles={stageInlineStyles}>
          {userIsAnonymous && !data && <AnonymousPage />}
          {error &&
            !userIsAnonymous &&
            ((showLoader && <Loader />) || (
              <h1>Error loading document: {error.message ?? error}</h1>
            ))}
          {data && (
            <div className={styles.documentChooser} ref={documentStageRef}>
              <DocumentChooser
                documentId={data.documents[0].id}
                documentType={data.documents[0].documentType}
                // You should not use user information from the context if you need to prove a user's identity.
                // Here, we are using it control the UI to highlight a user's signature box, so we feel comfortable using it.
                loggedInUser={data.callerUser}
                signatures={data.documents[0].signatures}
                clickable
              />
              {cursorLocationsEvent && (
                <CursorsRenderer
                  cursors={cursorLocationsEvent
                    .toArray()
                    .filter(
                      (p) =>
                        p.state === PresenceState.online &&
                        p.data !== undefined &&
                        p.userId !== teamsContext?.user?.id,
                    )}
                  parentBoundingBox={documentStageRef.current?.getBoundingClientRect()}
                />
              )}
            </div>
          )}
        </Flex>
      </LiveSharePage>
    </>
  );
}
