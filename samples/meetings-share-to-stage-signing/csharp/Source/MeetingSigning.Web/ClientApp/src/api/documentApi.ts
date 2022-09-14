import { Document, DocumentDto, DocumentListDto, DocumentInput } from 'models';
import { authFetch } from './fetchClient';

async function getDocument(
  documentId: string,
  isAnonymousUser: boolean,
  token?: string,
) {
  return await authFetch<DocumentDto>(
    `document/${documentId}/`,
    isAnonymousUser,
    { method: 'GET' },
    token,
  );
}

async function getAllDocuments(isAnonymousUser: boolean, token?: string) {
  return await authFetch<DocumentListDto>(
    'document/',
    isAnonymousUser,
    { method: 'GET' },
    token,
  );
}

async function createDocument(
  document: DocumentInput,
  isAnonymousUser: boolean,
  token?: string,
) {
  return await authFetch<Document>(
    'document/',
    isAnonymousUser,
    {
      method: 'POST',
      body: JSON.stringify(document),
    },
    token,
  );
}

export { getDocument, getAllDocuments, createDocument };
