import { useEffect } from 'react';
import { Flex, Header, Loader } from '@fluentui/react-northstar';
import * as microsoftTeams from '@microsoft/teams-js';
import { useParams } from 'react-router-dom';
import { useQuery } from 'react-query';
import { useNavigate } from 'react-router-dom';
import { getSupportDepartment } from 'api';
import { CustomerInquiryTable } from 'components/CustomerInquiryTable';
import { SupportDepartment } from 'models';

function SupportDepartmentChannelTab() {
  const navigate = useNavigate();

  useEffect(() => {
    microsoftTeams.getContext((context) => {
      let navigationUrl = `/support-department/${context.entityId}`;
      context.subEntityId && (navigationUrl = navigationUrl + '/inquiry/' + context.subEntityId);

      navigate(navigationUrl);
    });
  }, []);

  const params = useParams();
  const entityId: string = params.entityId ?? 'unknown';

  const { data, error, isLoading } = useQuery<SupportDepartment, Error>(
    ['getSupportDepartment', { entityId }],
    () => getSupportDepartment(entityId),
  );

  return (
    <Flex column>
      {isLoading && <Loader />}
      {error && <Header content={error.message} />}
      {data && (
        <>
          <Header content={data.title} description={data.description} />
          <CustomerInquiryTable entityId={entityId} customerInquiries={data.subEntities} />
        </>
      )}
    </Flex>
  );
}

export { SupportDepartmentChannelTab };
