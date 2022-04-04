import { useEffect, useState } from 'react';
import { Flex, Loader } from '@fluentui/react-northstar';
import { isEqual } from 'lodash';
import { useParams } from 'react-router-dom';
import { useApi, useDefaultColorScheme, useInterval } from 'hooks';
import { useAADId } from 'utils/TeamsProvider/hooks';
import documentApi from 'api/documentApi';
import { DocumentChooser } from 'components/Documents';
import { Document } from 'models';

/**
 * A component that calls the `getDocument` API, get's the document and
 * renders it correctly on the Teams stage.
 *
 * @returns A component with a Document renderd on the stage
 */
export function DocumentStage() {
  const params = useParams();
  const getDocumentApi = useApi(documentApi.getDocument);

  const [document, setDocument] = useState<Document | undefined>();

  const colorScheme = useDefaultColorScheme();
  const stageInlineStyles = { background: colorScheme.background };
  const loggedInAADId = useAADId();
  const pollingInterval = 2000;

  const [showLoader, setShowLoader] = useState<boolean>(true);
  const showLoaderTimeout = 5000;

  useEffect(() => {
    const timer = setTimeout(() => {
      setShowLoader(false);
    }, showLoaderTimeout);

    return () => clearTimeout(timer);
  }, []);

  useEffect(() => {
    getDocumentApi.request(params.documentId);
  }, [params.documentId]);

  //using polling
  useInterval(() => {
    getDocumentApi.request(params.documentId);
  }, pollingInterval);

  useEffect(() => {
    if (
      getDocumentApi.data &&
      !isEqual(document as Document, getDocumentApi.data as Document)
    ) {
      setDocument(getDocumentApi.data as Document);
    }
  }, [getDocumentApi.data, document]);

  return (
    <>
      <Flex styles={stageInlineStyles}>
        {getDocumentApi.error &&
          ((showLoader && <Loader />) || (
            <h1>Error loading document: {getDocumentApi.error}</h1>
          ))}
        {document && (
          <>
            <DocumentChooser
              documentId={document.id}
              documentType={document.documentType}
              loggedInAadId={loggedInAADId}
              signatures={document.signatures}
              clickable
            />
          </>
        )}
      </Flex>
    </>
  );
}
