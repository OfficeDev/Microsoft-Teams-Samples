import { ReactNode, useState } from 'react';
import { Accordion, Flex, Header, Loader } from '@fluentui/react-northstar';
import { useQuery, useQueryClient } from 'react-query';
import { getAllSupportDepartments } from 'api';
import { ApiErrorCode, SupportDepartment } from 'models';
import { CustomerInquiryTable } from 'components/CustomerInquiryTable';
import { ConsentRequest } from 'components/ConsentRequest';
import { apiRetryQuery, isApiErrorCode } from 'utils/UtilsFunctions';

function PersonalTab() {
  const [userHasConsented, setUserHasConsented] = useState<boolean>(false);
  const queryClient = useQueryClient();

  const allSupportDepartments = useQuery<SupportDepartment[], Error>(
    ['getAllSupportDepartments', { userHasConsented }],
    () => getAllSupportDepartments(),
    {
      onSuccess: () => {
        setUserHasConsented(false);
      },
      retry: (failureCount: number, error: Error) =>
        apiRetryQuery(
          failureCount,
          error,
          userHasConsented,
          setUserHasConsented,
        ),
    },
  );

  const consentCallback = (error?: string, result?: string) => {
    if (error) {
      console.log(`Error: ${error}`);
    }
    if (result) {
      setUserHasConsented(true);
      queryClient.invalidateQueries();
    }
  };

  const getErrorNode = (): ReactNode => {
    if (
      isApiErrorCode(
        ApiErrorCode.AuthConsentRequired,
        allSupportDepartments.error,
      ) &&
      !userHasConsented
    ) {
      return <ConsentRequest callback={consentCallback} />;
    } else {
      <Header
        content={allSupportDepartments.error?.message ?? 'Unknown error.'}
      />;
    }
  };

  return (
    <Flex column>
      {allSupportDepartments.isLoading && <Loader />}
      {allSupportDepartments.error && getErrorNode()}
      {allSupportDepartments.data &&
        allSupportDepartments.data.length === 0 && (
          <Header content="No support departments found" />
        )}
      {allSupportDepartments.data && allSupportDepartments.data.length > 0 && (
        <>
          <Header
            content="Your support departments"
            description="Only the first 5 inquiries in each department are shown"
          />
          <Accordion
            defaultActiveIndex={Array.from(
              allSupportDepartments.data,
              (_, i) => i,
            )}
            panels={getAccordionPanels(allSupportDepartments.data)}
          />
        </>
      )}
    </Flex>
  );
}

function getAccordionPanels(departments: SupportDepartment[]) {
  return departments.map((d) => {
    return {
      title: d.title,
      content:
        d.subEntities.length === 0 ? (
          <p>No inquiries found</p>
        ) : (
          <CustomerInquiryTable
            entityId={d.supportDepartmentId}
            source="personal"
            customerInquiries={d.subEntities.slice(0, 5)}
          />
        ),
    };
  });
}

export { PersonalTab };
