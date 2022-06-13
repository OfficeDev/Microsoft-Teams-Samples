import { Document, DocumentInput } from 'models';
import { authFetch } from './fetchClient';

async function getDocument(documentId: string) {
  return await authFetch<Document>(`document/${documentId}/`, { method: 'GET' });
};

async function getAllDocuments() {
  return await authFetch<Document[]>('document', { method: 'GET' });
};

async function createDocument(document: DocumentInput) {
  return await authFetch<Document>('document', {
    method: 'POST',
    body: JSON.stringify(document),
  });
};

export { getDocument, getAllDocuments, createDocument };
