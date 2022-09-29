import { Flex, Header, Loader } from '@fluentui/react-northstar';
import { useEffect, useState } from 'react';
import { useQuery } from 'react-query';
import { useUserIsAnonymous } from 'utils/TeamsProvider/hooks';
import { getAllDocuments } from 'api/documentApi';
import { AnonymousPage } from 'components/AnonymousPage';
import { CreateDocumentButton } from 'components/CreateDocumentButton';
import { SidepanelDocumentCard } from 'components/SidepanelDocumentCard';
import { DocumentListDto } from 'models';

/**
 * List documents for use in a Sidepanel
 *
 * @param documents
 * @returns A vertical list of documents styled as cards
 */
export function SidepanelDocumentCardList() {
  const pollingInterval = 5000;
  const userIsAnonymous = useUserIsAnonymous();

  const { data, error, isError } = useQuery<DocumentListDto, Error>(
    ['getAllDocuments', userIsAnonymous],
    () => getAllDocuments(userIsAnonymous),
    { refetchInterval: pollingInterval },
  );

  const [showLoader, setShowLoader] = useState<boolean>(true);
  const showLoaderTimeout = 5000;

  const anonymousUserHasToken = userIsAnonymous && data !== undefined;
  const userCanTryViewDocumentList = anonymousUserHasToken || !userIsAnonymous;

  useEffect(() => {
    const timer = setTimeout(() => {
      setShowLoader(false);
    }, showLoaderTimeout);

    return () => clearTimeout(timer);
  }, []);

  return (
    <Flex column gap="gap.medium">
      {!userCanTryViewDocumentList && <AnonymousPage />}
      {userCanTryViewDocumentList && (
        <>
          <CreateDocumentButton userIsAnonymous={userIsAnonymous} />
          {data && data.documents.length === 0 && (
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

          {data &&
            data.documents.map((d, index) => (
              <SidepanelDocumentCard
                key={index}
                {...d}
                loggedInUser={data.callerUser}
              />
            ))}
        </>
      )}
    </Flex>
  );
}
