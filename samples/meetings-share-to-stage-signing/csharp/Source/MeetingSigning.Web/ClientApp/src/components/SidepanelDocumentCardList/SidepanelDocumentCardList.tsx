import { Flex, Header, Loader } from '@fluentui/react-northstar';
import { useEffect, useState } from 'react';
import { isEqual } from 'lodash';
import { useApi, useInterval } from 'hooks';
import { useAADId } from 'utils/TeamsProvider/hooks';
import documentApi from 'api/documentApi';
import { Document } from 'models';
import { SidepanelDocumentCard } from 'components/SidepanelDocumentCard';

/**
 * List documents for use in a Sidepanel
 *
 * @param documents
 * @returns A vertical list of documents styled as cards
 */
export function SidepanelDocumentCardList() {
  const getAllDocumentsApi = useApi(documentApi.getAllDocuments);
  const [documents, setDocuments] = useState<Document[]>([]);
  const loggedInAADId = useAADId();
  const pollingInterval = 5000;

  const [showLoader, setShowLoader] = useState<boolean>(true);
  const showLoaderTimeout = 5000;

  useEffect(() => {
    const timer = setTimeout(() => {
      setShowLoader(false);
    }, showLoaderTimeout);

    return () => clearTimeout(timer);
  }, []);

  useEffect(() => {
    getAllDocumentsApi.request();
  }, []);

  //using polling
  useInterval(() => {
    getAllDocumentsApi.request();
  }, pollingInterval);

  useEffect(() => {
    if (
      getAllDocumentsApi.data &&
      !isEqual(documents as Document[], getAllDocumentsApi.data as Document[])
    ) {
      setDocuments(getAllDocumentsApi.data as []);
    }
  }, [getAllDocumentsApi.data, documents]);

  return (
    <Flex column gap="gap.medium">
      {documents.length === 0 && !getAllDocumentsApi.error && (
        <Header
          as="h1"
          content="There are no documents available for you yet."
        />
      )}

      {getAllDocumentsApi.error &&
        ((showLoader && <Loader />) || (
          <Header
            as="h1"
            content="Something went wrong"
            description={JSON.stringify(getAllDocumentsApi)}
          />
        ))}

      {documents.map((d, index) => (
        <SidepanelDocumentCard
          key={index}
          {...d}
          loggedInAadId={loggedInAADId}
        />
      ))}
    </Flex>
  );
}
