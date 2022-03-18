import { DocumentInput } from 'models';
import fetchClient from './fetchClient';

const getDocument = (token: string, documentId: string): Promise<Response> => {
  return fetchClient(token, `document/${documentId}/`, { method: 'GET' });
};

const getAllDocuments = (token: string): Promise<Response> => {
  return fetchClient(token, 'document', { method: 'GET' });
};

const createDocument = (
  token: string,
  document: DocumentInput,
): Promise<Response> => {
  return fetchClient(token, 'document', {
    method: 'POST',
    body: JSON.stringify(document),
  });
};

export default { getDocument, getAllDocuments, createDocument };
