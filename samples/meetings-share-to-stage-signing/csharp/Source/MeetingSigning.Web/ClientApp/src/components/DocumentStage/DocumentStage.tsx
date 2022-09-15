import { useContext, useEffect, useState } from 'react';
import { Flex, Loader } from '@fluentui/react-northstar';
import { useParams } from 'react-router-dom';
import { useDefaultColorScheme } from 'hooks';
import { useUserIsAnonymous } from 'utils/TeamsProvider/hooks';
import { getDocument } from 'api/documentApi';
import { DocumentChooser } from 'components/Documents';
import { DocumentDto } from 'models';
import { useQuery } from 'react-query';
import { AnonymousPage } from 'components/AnonymousPage';
import { TeamsContext } from 'utils/TeamsProvider/TeamsProvider';

/**
 * A component that calls the `getDocument` API, get's the document and
 * renders it correctly on the Teams stage.
 *
 * @returns A component with a Document rendered on the stage
 */
export function DocumentStage() {
  const params = useParams();
  const documentId: string = params.documentId ?? 'unknown';
  const pollingInterval = 2000;
  const userIsAnonymous = useUserIsAnonymous();
  const { anonymousUserAccessToken } = useContext(TeamsContext);

  // We are using https://react-query.tanstack.com/ for handling the calls to our APIs.
  // Here when the documentId changes, React Query will fetch the document from the API.
  // We are also using the `refetchInterval` to query the API every 2 seconds.
  const { data, error } = useQuery<DocumentDto, Error>(
    ['getDocument', { documentId, userIsAnonymous, anonymousUserAccessToken }],
    () => getDocument(documentId, userIsAnonymous, anonymousUserAccessToken),
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
  }, []);

  return (
    <>
      <Flex styles={stageInlineStyles}>
        {userIsAnonymous && !data && <AnonymousPage />}
        {error &&
          !userIsAnonymous &&
          ((showLoader && <Loader />) || (
            <h1>Error loading document: {error.message ?? error}</h1>
          ))}
        {data && (
          <>
            <DocumentChooser
              documentId={data.document.id}
              documentType={data.document.documentType}
              loggedInUser={data.callerUser}
              signatures={data.document.signatures}
              clickable
            />
          </>
        )}
      </Flex>
    </>
  );
}
