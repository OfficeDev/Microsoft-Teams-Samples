import { Flex, Header, Loader } from '@fluentui/react-northstar';
import { useEffect, useState } from 'react';
import { useTeamsContext } from 'utils/TeamsProvider/hooks';
import { getAllDocuments } from 'api/documentApi';
import { Document } from 'models';
import { SidepanelDocumentCard } from 'components/SidepanelDocumentCard';
import { useQuery } from 'react-query';
import { useLiveShare, useTakeControl } from 'hooks';
import { LiveSharePage } from 'components/LiveSharePage';

/**
 * List documents for use in a Sidepanel
 *
 * @param documents
 * @returns A vertical list of documents styled as cards
 */
export function SidepanelDocumentCardList() {
  const teamsContext = useTeamsContext();
  const pollingInterval = 5000;

  const { data, error, isError } = useQuery<Document[], Error>(
    ['getAllDocuments'],
    () => getAllDocuments(),
    { refetchInterval: pollingInterval },
  );

  const [showLoader, setShowLoader] = useState<boolean>(true);
  const showLoaderTimeout = 5000;

  const {
    takeControlState,
    container,
    audience,
  } = useLiveShare();

  const {
    takeControlStarted,
    takeControl,
  } = useTakeControl(takeControlState, teamsContext?.user, audience);

  useEffect(() => {
    const timer = setTimeout(() => {
      setShowLoader(false);
    }, showLoaderTimeout);

    return () => clearTimeout(timer);
  }, []);

  return (
    <LiveSharePage
      context={teamsContext}
      container={container}
      started={takeControlStarted}
    >
    <Flex column gap="gap.medium">
      {data && data.length === 0 && (
        <Header
          as="h1"
          content="There are no documents available for you yet."
        />
      )}

      {isError &&
        ((showLoader && <Loader />) || (
          <Header
            as="h1"
            content="Something went wrong"
            description={JSON.stringify(error)}
          />
        ))}

      {data && data.map((d, index) => (
        <SidepanelDocumentCard
          key={index}
          {...d}
          // You should not use user information from the context if you need to prove a user's identity.
          // Here, we are using it control the UI to highlight a user's signature box, so we feel comfortable using it.
          loggedInAadId={teamsContext?.user?.id ?? ''}
          takeControl={takeControl}
        />
      ))}
      </Flex>
    </LiveSharePage>
  );
}
