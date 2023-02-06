import {
  ComponentEventHandler,
  Flex,
  Form,
  FormButton,
  FormInput,
  FormProps,
  useCSS,
} from '@fluentui/react-northstar';
import { useActionCreator } from 'actionCreators';
import { trackPageComponent } from 'appInsights';
import { useDefaultColorScheme } from 'hooks/useColorScheme';
import { KnowledgeBase } from 'models';
import React, { useCallback, useEffect, useState } from 'react';
import { FormattedMessage } from 'react-intl';
import { useSelector } from 'react-redux';
import { selectPathKnowledgeBase } from 'selectors/knowledgeBaseSelectors';

// eslint-disable-next-line max-lines-per-function
function KnowledgeBaseConfigurationPage(): JSX.Element {
  const colorScheme = useDefaultColorScheme();
  const pageClass = useCSS({
    width: '100%',
    height: '100%',
    backgroundColor: colorScheme.background,
    marginLeft: '1rem',
    marginTop: '1rem',
    paddingTop: '0.625rem',
    paddingLeft: '1rem',
    paddingRight: '1rem',
  });
  const formClass = useCSS({
    display: 'block',
  });
  const knowledgeBase = useSelector(selectPathKnowledgeBase);
  const [tempKnowledgeBase, setTempKnowledgeBase] = useState<
    Pick<KnowledgeBase, 'name'>
  >({
    name: knowledgeBase?.name ?? '',
  });
  useEffect(() => {
    setTempKnowledgeBase({
      name: knowledgeBase?.name ?? '',
    });
  }, [setTempKnowledgeBase, knowledgeBase]);
  const updateKnowledgeBase = useActionCreator(
    (s) => s.knowledgeBase.updateKnowledgeBases,
  );
  const onNameChange: ComponentEventHandler<{
    value: string;
  }> = useCallback(
    (evt, props) => {
      if (!props) return;
      setTempKnowledgeBase({ ...tempKnowledgeBase, name: props.value });
    },
    [tempKnowledgeBase, setTempKnowledgeBase],
  );
  const onSubmit: ComponentEventHandler<FormProps> = useCallback(
    (evt, formProps) => {
      if (!knowledgeBase) {
        console.warn('Knowledge base not initialized');
        return;
      }
      updateKnowledgeBase({
        knowledgeBase: { ...knowledgeBase, ...tempKnowledgeBase },
      });
    },
    [tempKnowledgeBase, knowledgeBase, updateKnowledgeBase],
  );
  const isNameValid = tempKnowledgeBase.name.length > 0;
  return (
    <Flex className={pageClass}>
      <Form className={formClass} onSubmit={onSubmit}>
        <FormInput
          error={!isNameValid}
          errorMessage={
            isNameValid ? undefined : (
              <FormattedMessage
                id="knowledgeBaseConfigurationPage.nameError"
                description="error for the knowledge base's name being invalid"
                defaultMessage="Name cannot be empty"
              />
            )
          }
          label={
            <FormattedMessage
              id="knowledgeBaseConfigurationPage.nameLabel"
              description="label for the knowledge base's name in the configuration form"
              defaultMessage="Name"
            />
          }
          name="name"
          required
          value={tempKnowledgeBase.name}
          onChange={onNameChange}
        />
        <FormButton
          disabled={!isNameValid}
          content={
            <FormattedMessage
              id="knowledgeBaseConfigurationPage.saveButton"
              description="Save button for the knowledge base configuration page"
              defaultMessage="Save"
            />
          }
        />
      </Form>
    </Flex>
  );
}

export default trackPageComponent(KnowledgeBaseConfigurationPage);
