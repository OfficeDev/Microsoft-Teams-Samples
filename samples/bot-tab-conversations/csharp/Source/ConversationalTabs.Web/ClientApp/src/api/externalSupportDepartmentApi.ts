// This API calls an unauthenticated `External` API. This API is what would be called by an external service, like a job posting board, or support software.
// IMPORTANT: These calls should be authenticated, but we have not covered this scenario for this proof of concept.

import {
  CustomerInquiry,
  CustomerInquiryInput,
  SupportDepartment,
} from 'models';
import { unAuthFetch } from './fetchClient';

const URL_ROOT: string = 'api/external';
const INQUIRY_SEGMENT: string = 'inquiry';

async function getAllSupportDepartments() {
  return await unAuthFetch<SupportDepartment[]>(`${URL_ROOT}`, {
    method: 'GET',
  });
}

// React-Query has a limitation of only allowing one variable for mutations
// To solve this limitation, we merge the entityId and CustomerInquiryInput for createCustomerInquiry
// It is kept separate from the other models in /models because it is not to be used except in this situation.+
type CreateCustomerInquiryModel = {
  entityId: string;
  input: CustomerInquiryInput;
};

async function createCustomerInquiry(model: CreateCustomerInquiryModel) {
  return await unAuthFetch<CustomerInquiry>(
    `${URL_ROOT}/${model.entityId}/${INQUIRY_SEGMENT}`,
    {
      method: 'POST',
      body: JSON.stringify(model.input),
    },
  );
}

export { getAllSupportDepartments, createCustomerInquiry };

export type { CreateCustomerInquiryModel };
