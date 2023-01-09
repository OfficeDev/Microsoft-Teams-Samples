import { Document, DocumentListDto, DocumentInput } from 'models';
import { authFetch } from './fetchClient';

async function getDocument(documentId: string, isAnonymousUser: boolean) {
  return await authFetch<DocumentListDto>(
    `document/${documentId}/`,
    isAnonymousUser,
    { method: 'GET' },
  );
}

async function getAllDocuments(isAnonymousUser: boolean) {
  return await authFetch<DocumentListDto>('document/', isAnonymousUser, {
    method: 'GET',
  });
}

async function createDocument(
  document: DocumentInput,
  isAnonymousUser: boolean,
) {
  return await authFetch<Document>('document/', isAnonymousUser, {
    method: 'POST',
    body: JSON.stringify(document),
  });
}

export { getDocument, getAllDocuments, createDocument };
