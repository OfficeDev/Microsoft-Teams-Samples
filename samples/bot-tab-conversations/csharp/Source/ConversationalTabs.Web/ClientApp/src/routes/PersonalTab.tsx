import { Accordion, Flex, Header, Loader } from '@fluentui/react-northstar';
import { useQuery } from 'react-query';
import { getAllSupportDepartments } from 'api';
import { SupportDepartment } from 'models';
import { CustomerInquiryTable } from 'components/CustomerInquiryTable';

function PersonalTab() {
  const { data, error, isLoading } = useQuery<SupportDepartment[], Error>(
    ['getAllSupportDepartments'],
    () => getAllSupportDepartments(),
  );

  return (
    <Flex column>
      {isLoading && <Loader />}
      {error && <Header content={error.message} />}
      {data && data.length === 0 && (
        <Header content="No support departments found" />
      )}
      {data && data.length > 0 && (
        <>
          <Header content="Your support departments" description="Only the first 5 inquiries in each department are shown" />
          <Accordion
            defaultActiveIndex={Array.from(data, (_, i) => i)}
            panels={getAccordionPanels(data)}
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
            customerInquiries={d.subEntities.slice(0, 5)}
          />
        ),
    };
  });
}

export { PersonalTab };
