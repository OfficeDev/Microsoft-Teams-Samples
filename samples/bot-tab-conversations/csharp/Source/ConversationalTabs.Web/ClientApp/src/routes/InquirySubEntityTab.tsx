import { ReactNode, useEffect, useState } from 'react';
import {
  ArrowLeftIcon,
  Button,
  Flex,
  Header,
  Loader,
} from '@fluentui/react-northstar';
import { useNavigate, useParams } from 'react-router-dom';
import { useQuery, useQueryClient } from 'react-query';
import * as microsoftTeams from '@microsoft/teams-js';
import { getSupportDepartment, getSupportDepartmentItem } from 'api';
import { CustomerInquiryDetail } from 'components/CustomerInquiryDetail';
import { ConsentRequest } from 'components/ConsentRequest';
import { ApiErrorCode, CustomerInquiry, SupportDepartment } from 'models';
import { apiRetryQuery, isApiErrorCode } from 'utils/UtilsFunctions';

function InquirySubEntityTab() {
  const [isChatOpen, setIsChatOpen] = useState<boolean>(false);
  const [userHasConsented, setUserHasConsented] = useState<boolean>(false);
  const [onLoadConversationOpened, setOnLoadConversationOpened] =
    useState<boolean>(false);

  const navigate = useNavigate();
  const params = useParams();
  const queryClient = useQueryClient();

  const source: string = params.source ?? 'support-department';
  const entityId: string = params.entityId ?? 'unknown';
  const subEntityId: string = params.subEntityId ?? 'unknown';

  useEffect(() => {
    // When we navigate away, close the chat.
    return () => closeConversation();
  }, []);

  const inquiry = useQuery<CustomerInquiry, Error>(
    ['getSupportDepartmentItem', { entityId,  subEntityId, userHasConsented }],
    () => getSupportDepartmentItem(entityId, subEntityId),
    {
      onSuccess: () => {
        if (!onLoadConversationOpened) {
          const successful = openConversation();
          setOnLoadConversationOpened(successful);
        }
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

  const supportDepartment = useQuery<SupportDepartment, Error>(
    ['getSupportDepartment', { entityId, userHasConsented }],
    () => getSupportDepartment(entityId),
    {
      onSuccess: () => {
        if (!onLoadConversationOpened) {
          const successful = openConversation();
          setOnLoadConversationOpened(successful);
        }
      },
      retry: (failureCount: number, error: Error) =>
        failureCount <= 3 &&
        isApiErrorCode(ApiErrorCode.AuthConsentRequired, error) &&
        userHasConsented,
    },
  );

  const openConversation = () => {
    if (inquiry.data && supportDepartment.data) {
      microsoftTeams.conversations.openConversation({
        entityId: supportDepartment.data.supportDepartmentId,
        subEntityId: inquiry.data.subEntityId,
        channelId: supportDepartment.data.teamChannelId,
        title: inquiry.data.question,
        conversationId: inquiry.data.conversationId,
        onCloseConversation: (_) => {
          setIsChatOpen(false);
        },
      });
      setIsChatOpen(true);
      return true;
    }

    return false;
  };

  const closeConversation = () => {
    microsoftTeams.conversations.closeConversation();
    setIsChatOpen(false);
  };

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
      (isApiErrorCode(ApiErrorCode.AuthConsentRequired, inquiry.error) ||
        isApiErrorCode(
          ApiErrorCode.AuthConsentRequired,
          supportDepartment.error,
        )) &&
      !userHasConsented
    ) {
      return <ConsentRequest callback={consentCallback} />;
    } else {
      return <Header content={inquiry.error?.message ?? 'Unknown error'} />;
    }
  };

  return (
    <Flex column>
      <Flex>
        <Button
          tabIndex={-1}
          icon={<ArrowLeftIcon />}
          secondary
          content="Back"
          onClick={() =>
            navigate(
              `/${source}${
                source === 'personal' ? '' : '/' + entityId
              }?ignoreContextRedirect`,
            )
          }
        />
      </Flex>
      {inquiry.isLoading && supportDepartment.isLoading && <Loader />}
      {inquiry.error && supportDepartment.error && getErrorNode()}
      {inquiry.data && supportDepartment.data && (
        <CustomerInquiryDetail
          customerInquiry={inquiry.data}
          isChatOpen={isChatOpen}
          onOpenConversation={openConversation}
          onCloseConversation={closeConversation}
        />
      )}
    </Flex>
  );
}

export { InquirySubEntityTab };
