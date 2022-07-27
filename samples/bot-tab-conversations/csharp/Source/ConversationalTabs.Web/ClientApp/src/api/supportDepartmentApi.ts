import { CustomerInquiry, SupportDepartment, SupportDepartmentInput } from 'models';
import { authFetch } from './fetchClient';

const URL_ROOT: string = 'api/supportDepartments';
const INQUIRY_SEGMENT: string = 'inquiry';

async function getSupportDepartment(entityId: string) {
  return await authFetch<SupportDepartment>(`${URL_ROOT}/${entityId}/`, {
    method: 'GET',
  });
}

async function getAllSupportDepartments() {
  return await authFetch<SupportDepartment[]>(`${URL_ROOT}`, {
    method: 'GET',
  });
}

async function createSupportDepartment(input: SupportDepartmentInput) {
  return await authFetch<SupportDepartment>(`${URL_ROOT}`, {
    method: 'POST',
    body: JSON.stringify(input)
  });
}

async function getSupportDepartmentItem(entityId: string, subEntityId: string) {
  return await authFetch<CustomerInquiry>(
    `${URL_ROOT}/${entityId}/${INQUIRY_SEGMENT}/${subEntityId}`,
    {
      method: 'GET',
    },
  );
}

export {
  createSupportDepartment,
  getSupportDepartment,
  getAllSupportDepartments,
  getSupportDepartmentItem,
};
