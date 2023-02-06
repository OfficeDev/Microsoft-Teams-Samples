import {
  Alert,
  Avatar,
  Chat,
  ChatItemProps,
  Flex,
  Form,
  FormButton,
  FormDropdown,
  FormInput,
  Header,
  PersonIcon,
  Provider,
  ShorthandCollection,
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

  const { data, isSuccess } = useQuery<
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
          setCustomerName('');
          setCustomerInquiry('');
        },
      },
    );
  };

  const items: ShorthandCollection<ChatItemProps> = [
    {
      gutter: <Avatar icon={<PersonIcon />} />,
      message: (
        <Chat.Message
          content="Welcome to our example website"
          author="External Service"
          timestamp="Yesterday, 10:15 AM"
        />
      ),
      attached: 'top',
      key: 'message-id-4',
    },
    {
      gutter: <Avatar icon={<PersonIcon />} />,
      message: (
        <Chat.Message
          content="If you have any questions, please reach out to us below"
          author="External Service"
          timestamp="Yesterday, 10:15 AM"
        />
      ),
      attached: true,
      key: 'message-id-5',
    },
    {
      gutter: <Avatar icon={<PersonIcon />} />,
      message: (
        <Chat.Message
          content="Our example uses a support department, but you could be any external service like a job posting board."
          author="External Service"
          timestamp="Yesterday, 10:15 AM"
        />
      ),
      attached: 'bottom',
      key: 'message-id-6',
    },
    {
      message: (
        <Chat.Message
          content={
            <>
              <Form>
                <FormDropdown
                  label={{
                    content: `Support Department:`,
                    id: 'support-departments',
                  }}
                  items={
                    data &&
                    data.map((d) => ({
                      header: d.title,
                      content: d.description,
                      id: d.supportDepartmentId,
                    }))
                  }
                  aria-labelledby="support-departments"
                  placeholder="Choose a Support Department"
                  value={supportDepartment}
                  onChange={(_: any, data: any) =>
                    setSupportDepartment(data?.value ?? '')
                  }
                />
                <FormInput
                  label="Customer Name"
                  name="customerName"
                  id="customer-name"
                  required
                  showSuccessIndicator={false}
                  value={customerName}
                  onChange={(_: any, data: any) =>
                    setCustomerName(data?.value ?? '')
                  }
                />
                <FormInput
                  label="Customer Inquiry"
                  name="customerInquiry"
                  id="customer-inquiry"
                  required
                  showSuccessIndicator={false}
                  value={customerInquiry}
                  onChange={(_: any, data: any) =>
                    setCustomerInquiry(data?.value ?? '')
                  }
                />
                <FormButton content="Submit" onClick={submitForm} />
              </Form>
            </>
          }
          mine
          timestamp="Now"
        />
      ),
      contentPosition: 'end',
      key: 'message-id-7',
    },
  ];

  return (
    <Provider
      theme={{
        componentStyles: {
          Chat: {
            root: {
              backgroundColor: '#fffbfc',
              border: '0.3rem solid lightpink',
              paddingBottom: '1rem',
            },
          },
          ChatMessage: {
            root: ({ props }: { props: any }) => ({
              ...(!props.mine
                ? {
                    backgroundColor: '#ffdce8',
                  }
                : {
                    backgroundColor: 'lightpink',
                  }),
            }),
          },
        },
        componentVariables: {
          ChatMessage: (siteVars: any) => ({
            content: {
              focusOutlineColor: siteVars.colors.red[400],
            },
          }),
        },
      }}
    >
      <Flex column>
        <Header
          content="This Admin Portal is unauthenticated, and should not be used in a production environment."
          description="This is a stand-in for an external service, like a customer support service or job posting board, that will call the controller to when it needs to start a new conversation."
        />

        {isSuccess && (
          <>
            <Flex>
              <Flex.Item size="size.half">
                <Chat items={items} />
              </Flex.Item>
            </Flex>
            {createCustomerInquiryMutation.isSuccess && (
              <Alert
                attached="bottom"
                dismissible
                success
                content={`Inquiry Created for '${createCustomerInquiryMutation.data.customerName}' (${createCustomerInquiryMutation.data.subEntityId})`}
              />
            )}
            {createCustomerInquiryMutation.isError && (
              <Alert
                attached="bottom"
                dismissible
                danger
                content={createCustomerInquiryMutation.error.message}
              />
            )}
          </>
        )}
      </Flex>
    </Provider>
  );
}

export { Admin };
