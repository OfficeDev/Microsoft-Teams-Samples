import { ReactNode, useEffect, useState } from 'react';
import * as microsoftTeams from '@microsoft/teams-js';
import { Flex, Form, FormInput, Header, Text } from '@fluentui/react-northstar';
import { useMutation } from 'react-query';
import { createSupportDepartment } from 'api';
import {
  ApiErrorCode,
  SupportDepartment,
  SupportDepartmentInput,
} from 'models';

function Configure() {
  const [departmentTitle, setDepartmentTitle] = useState<string>('');
  const [departmentDescription, setDepartmentDescription] =
    useState<string>('');
  const createSupportDepartmentMutation = useMutation<
    SupportDepartment,
    Error,
    SupportDepartmentInput
  >((supportDepartmentInput: SupportDepartmentInput) =>
    createSupportDepartment(supportDepartmentInput),
  );

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
                  suggestedDisplayName: data.title
                });
                saveEvent.notifySuccess();
              },
            },
          );
        });
      });
      microsoftTeams.settings.setValidityState(true);
    }
  }, [departmentTitle, departmentDescription, createSupportDepartmentMutation]);

  useEffect(() => {
    if (isError(ApiErrorCode.ChannelActivityNotFound)) {
      microsoftTeams.settings.setValidityState(false);
    }
  }, [createSupportDepartmentMutation]);

  const isError = (errorCode: ApiErrorCode): boolean =>
    createSupportDepartmentMutation.isError &&
    createSupportDepartmentMutation.error.message.startsWith(errorCode);

  const getErrorNode = (): ReactNode => {
    if (isError(ApiErrorCode.ChannelActivityNotFound)) {
      return (
        <>
          <Header
            content="We are unable to create your support department."
            description="To create your tab, you need to first to configure the application's bot in this channel."
          />
          <Text content="To configure the bot, send a message to the bot 'Conversational Tabs' in this channel." />
        </>
      );
    } else if (isError(ApiErrorCode.AuthConsentRequired)) {
      return (
        <Header content="We need you to consent to complete that action." />
      );
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
      {createSupportDepartmentMutation.isError && getErrorNode()}
      {!createSupportDepartmentMutation.isError && (
        <>
          <Header content="Create support department" />
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
