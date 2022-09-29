import { Signature } from 'models';
import { authFetch } from './fetchClient';

// React-Query has a limitation of only allowing one variable for mutations
// To solve this limitation, we merge the documentId and SignDocumentModel for postSignDocument
// It is kept separate from the other models in /models because it is not to be used except in this situation.
type SignDocumentModel = {
  documentId: string;
  signature: Signature;
};

async function postSignDocument(
  model: SignDocumentModel,
  isAnonymousUser: boolean,
) {
  return await authFetch<Signature>(
    `document/${model.documentId}/sign`,
    isAnonymousUser,
    {
      method: 'POST',
      body: JSON.stringify(model.signature),
    },
  );
}

export { postSignDocument };
export type { SignDocumentModel };
