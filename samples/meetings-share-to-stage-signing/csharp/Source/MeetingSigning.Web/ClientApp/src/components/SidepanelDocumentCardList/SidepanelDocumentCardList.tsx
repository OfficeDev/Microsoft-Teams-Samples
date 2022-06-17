import { Flex, Header, Loader } from '@fluentui/react-northstar';
import { useEffect, useState } from 'react';
import { useAADId } from 'utils/TeamsProvider/hooks';
import { getAllDocuments } from 'api/documentApi';
import { Document } from 'models';
import { SidepanelDocumentCard } from 'components/SidepanelDocumentCard';
import { useQuery } from 'react-query';

/**
 * List documents for use in a Sidepanel
 *
 * @param documents
 * @returns A vertical list of documents styled as cards
 */
export function SidepanelDocumentCardList() {
  const pollingInterval = 5000;

  const { data, error, isError } = useQuery<Document[], Error>(
    ['getAllDocuments'],
    () => getAllDocuments(),
    { refetchInterval: pollingInterval },
  );

  const loggedInAADId = useAADId();

  const [showLoader, setShowLoader] = useState<boolean>(true);
  const showLoaderTimeout = 5000;

  useEffect(() => {
    const timer = setTimeout(() => {
      setShowLoader(false);
    }, showLoaderTimeout);

    return () => clearTimeout(timer);
  }, []);

  return (
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
          loggedInAadId={loggedInAADId}
        />
      ))}
    </Flex>
  );
}
