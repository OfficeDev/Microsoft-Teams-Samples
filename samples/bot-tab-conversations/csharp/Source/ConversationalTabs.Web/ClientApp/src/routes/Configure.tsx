import { ReactNode, useEffect, useState } from 'react';
import * as microsoftTeams from '@microsoft/teams-js';
import {
  Flex,
  Form,
  FormInput,
  Header,
  Loader,
  Text,
} from '@fluentui/react-northstar';
import { useMutation } from 'react-query';
import { createSupportDepartment } from 'api';
import { ConsentRequest } from 'components/ConsentRequest';
import {
  ApiErrorCode,
  SupportDepartment,
  SupportDepartmentInput,
} from 'models';
import { apiRetryQuery, isApiErrorCode } from 'utils/UtilsFunctions';

function Configure() {
  const [userHasConsented, setUserHasConsented] = useState<boolean>(false);
  const [departmentTitle, setDepartmentTitle] = useState<string>('');
  const [departmentDescription, setDepartmentDescription] =
    useState<string>('');
  const createSupportDepartmentMutation = useMutation<
    SupportDepartment,
    Error,
    SupportDepartmentInput
  >(
    (supportDepartmentInput: SupportDepartmentInput) =>
      createSupportDepartment(supportDepartmentInput),
    {
      retry: (failureCount: number, error: Error) =>
        apiRetryQuery(
          failureCount,
          error,
          userHasConsented,
          setUserHasConsented,
        ),
    },
  );

  useEffect(() => {
    // The Team context is read to default the department title to the channel name
    // This is set on first load, but can be changed by the user
    microsoftTeams.getContext((context) => {
      if (context.channelName !== undefined) {
        setDepartmentTitle(context.channelName);
      }
    });
  }, []);

  useEffect(() => {
    if (departmentTitle !== '' && departmentDescription !== '') {
      microsoftTeams.settings.registerOnSaveHandler((saveEvent) => {
        microsoftTeams.getContext((context) => {
          createSupportDepartmentMutation.mutate(
            {
              title: departmentTitle,
              description: departmentDescription,
              teamChannelId: context.channelId ?? 'unknown',
              teamId: context.teamId ?? 'unknown',
              groupId: context.groupId ?? 'unknown',
              tenantId: context.tid ?? 'unknown',
            },
            {
              onSuccess: (data: SupportDepartment) => {
                microsoftTeams.settings.setSettings({
                  contentUrl: `${window.location.origin}/support-department/${data.supportDepartmentId}`,
                  entityId: data.supportDepartmentId,
                  suggestedDisplayName: data.title,
                });
                saveEvent.notifySuccess();
              },
              onError: () => {
                saveEvent.notifyFailure();
              },
            },
          );
        });
      });

      if (
        isApiErrorCode(
          ApiErrorCode.ChannelActivityNotFound,
          createSupportDepartmentMutation.error,
        ) ||
        (isApiErrorCode(
          ApiErrorCode.AuthConsentRequired,
          createSupportDepartmentMutation.error,
        ) &&
          !userHasConsented)
      ) {
        microsoftTeams.settings.setValidityState(false);
      } else {
        microsoftTeams.settings.setValidityState(true);
      }
    }
  }, [
    departmentTitle,
    departmentDescription,
    createSupportDepartmentMutation,
    userHasConsented,
  ]);

  const consentCallback = (error?: string, result?: string) => {
    if (error) {
      console.log(`Error: ${error}`);
    }
    if (result) {
      setUserHasConsented(true);
      microsoftTeams.settings.setValidityState(true);
    }
  };

  const getErrorNode = (): ReactNode => {
    if (
      isApiErrorCode(
        ApiErrorCode.ChannelActivityNotFound,
        createSupportDepartmentMutation.error,
      )
    ) {
      return (
        <>
          <Header
            content="We are unable to create your support department."
            description="To create your tab, you need to first to configure the application's bot in this channel."
          />
          <Text content="To configure the bot, send a message to the bot 'Conversational Tabs' in this channel." />
        </>
      );
    } else if (
      isApiErrorCode(
        ApiErrorCode.AuthConsentRequired,
        createSupportDepartmentMutation.error,
      ) &&
      !userHasConsented
    ) {
      return <ConsentRequest callback={consentCallback} />;
    } else {
      return (
        <Header
          content="We are unable to create your support department."
          description="Something went wrong."
        />
      );
    }
  };

  return (
    <Flex column>
      {!userHasConsented &&
        createSupportDepartmentMutation.isError &&
        getErrorNode()}
      {createSupportDepartmentMutation.isLoading && <Loader />}
      {(!createSupportDepartmentMutation.isError || userHasConsented) && (
        <>
          <Header content="Create support department" />
          {userHasConsented && (
            <Text success as="p" content="Successfully consented." />
          )}
          <Form>
            <FormInput
              label="Department name"
              name="departmentTitle"
              id="department-title"
              required
              fluid
              showSuccessIndicator={false}
              onChange={(_, data) => setDepartmentTitle(data?.value ?? '')}
              value={departmentTitle}
            />
            <FormInput
              label="Department description"
              name="departmentDescription"
              id="department-description"
              required
              fluid
              showSuccessIndicator={false}
              onChange={(_, data) =>
                setDepartmentDescription(data?.value ?? '')
              }
              value={departmentDescription}
            />
          </Form>
        </>
      )}
    </Flex>
  );
}

export { Configure };
