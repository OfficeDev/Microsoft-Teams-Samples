import {
  Alert,
  Flex,
  Form,
  FormButton,
  FormDropdown,
  FormInput,
  Header,
} from '@fluentui/react-northstar';
import { useMutation, useQuery } from 'react-query';
import {
  getAllSupportDepartments,
  createCustomerInquiry,
  CreateCustomerInquiryModel,
} from 'api/externalSupportDepartmentApi';
import { CustomerInquiry, SupportDepartment } from 'models';
import { useState } from 'react';

function Admin() {
  const [supportDepartment, setSupportDepartment] = useState<any>();
  const [customerName, setCustomerName] = useState<string>('');
  const [customerInquiry, setCustomerInquiry] = useState<string>('');

  const { data, error, isLoading, isSuccess } = useQuery<
    SupportDepartment[],
    Error
  >(['getAllSupportDepartments'], () => getAllSupportDepartments());

  const createCustomerInquiryMutation = useMutation<
    CustomerInquiry,
    Error,
    CreateCustomerInquiryModel
  >((model: CreateCustomerInquiryModel) => createCustomerInquiry(model));

  const submitForm = () => {
    createCustomerInquiryMutation.mutate(
      {
        entityId: supportDepartment.id,
        input: {
          customerName: customerName,
          question: customerInquiry,
        },
      },
      {
        onSuccess: (data: CustomerInquiry) => {
          setSupportDepartment(undefined);
          setCustomerName('');
          setCustomerInquiry('');
        },
      },
    );
  };

  return (
    <Flex column>
      <Header
        content="This Admin Portal is unauthenticated, and should not be used in a production environment."
        description="This is a stand-in for an external service, like a customer support service or job posting board, that will call the controller to when it needs to start a new conversation."
      />

      {isSuccess && (
        <>
          <Form>
            <FormDropdown
              label={{
                content: `Support Department:`,
                id: 'support-departments',
              }}
              items={data.map((d) => ({
                header: d.title,
                content: d.description,
                id: d.supportDepartmentId,
              }))}
              aria-labelledby="support-departments"
              search={true}
              placeholder="Choose a Support Department"
              value={supportDepartment}
              onChange={(_, data) => setSupportDepartment(data?.value ?? '')}
            />
            <FormInput
              label="Customer Name"
              name="customerName"
              id="customer-name"
              required
              showSuccessIndicator={false}
              value={customerName}
              onChange={(_, data) => setCustomerName(data?.value ?? '')}
            />
            <FormInput
              label="Customer Inquiry"
              name="customerInquiry"
              id="customer-inquiry"
              required
              showSuccessIndicator={false}
              value={customerInquiry}
              onChange={(_, data) => setCustomerInquiry(data?.value ?? '')}
            />
            <FormButton content="Submit" onClick={submitForm} />
          </Form>
          {createCustomerInquiryMutation.isSuccess && (
            <Alert
              attached="bottom"
              dismissible
              content={`Inquiry Created for '${createCustomerInquiryMutation.data.customerName}' (${createCustomerInquiryMutation.data.subEntityId})`}
            />
          )}
        </>
      )}
    </Flex>
  );
}

export { Admin };
