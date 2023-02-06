import {
  ComponentEventHandler,
  Dialog,
  FormInput,
} from '@fluentui/react-northstar';
import { KnowledgeBase } from 'models';
import React, { useCallback } from 'react';
import { FormattedMessage } from 'react-intl';

type EditKnowledgeBase = Pick<KnowledgeBase, 'name'>;
export interface CreateKnowledgeBaseDialogProps {
  knowledgeBase: EditKnowledgeBase;
  isOpen: boolean;
  onClose: () => void;
  onSubmit: () => void;
  onKnowledgeBaseChanged: (knowledgeBase: EditKnowledgeBase) => void;
}
export function CreateKnowledgeBaseDialog({
  knowledgeBase,
  isOpen,
  onClose,
  onSubmit,
  onKnowledgeBaseChanged,
}: CreateKnowledgeBaseDialogProps) {
  const onNameChange: ComponentEventHandler<{
    value: string;
  }> = useCallback(
    (evt, props) => {
      if (!props) return;
      onKnowledgeBaseChanged({ ...knowledgeBase, name: props.value });
    },
    [knowledgeBase, onKnowledgeBaseChanged],
  );
  const isNameValid = knowledgeBase.name.length > 0;
  return (
    <Dialog
      open={isOpen}
      onCancel={onClose}
      onConfirm={onSubmit}
      header={
        <FormattedMessage
          id="createKnowledgeBaseDialog.headerText"
          description="The header text for the dialog when creating a new knowledge base"
          defaultMessage="Create Knowledge Base"
        />
      }
      style={{ minHeight: 600 }}
      confirmButton={{
        content: (
          <FormattedMessage
            id="createKnowledgeBaseDialog.confirmButton"
            description="The confirm button text text for the dialog when creating a new knowledge base"
            defaultMessage="Create Knowledge Base"
          />
        ),
        primary: true,
        disabled: !isNameValid,
      }}
      content={
        <>
          <FormInput
            error={!isNameValid}
            errorMessage={
              isNameValid ? undefined : (
                <FormattedMessage
                  id="createKnowledgeBaseDialog.nameError"
                  description="error for the knowledge base's name being invalid"
                  defaultMessage="Name cannot be empty"
                />
              )
            }
            label={
              <FormattedMessage
                id="createKnowledgeBaseDialog.nameLabel"
                description="label for the knowledge base's name in the configuration form"
                defaultMessage="Name"
              />
            }
            name="name"
            required
            value={knowledgeBase.name}
            onChange={onNameChange}
          />
        </>
      }
    />
  );
}
