import { Signature } from 'models';
import fetchClient from './fetchClient';

const postSignDocument = (
  token: string,
  documentId: string,
  signature: Signature,
): Promise<Response> => {
  return fetchClient(token, `document/${documentId}/sign`, {
    method: 'POST',
    body: JSON.stringify(signature),
  });
};

export default { postSignDocument };
