import React, { useCallback, useEffect, useState } from 'react';
import {
  Box,
  FormDropdown,
  Form,
  FormButton,
  ComponentEventHandler,
  FormProps,
  DropdownProps,
} from '@fluentui/react-northstar';
import { defineMessages, FormattedMessage, useIntl } from 'react-intl';
import { useSelector } from 'react-redux';
import { selectKnowledgeBases } from 'selectors/knowledgeBaseSelectors';
import { useActionCreator } from 'actionCreators';
import { selectCurrentPathCourse } from 'selectors';
import { Course } from 'models';

const labelId = 'choose-knowledge-base-id';
type EditCourse = Pick<Course, 'knowledgeBaseId'>;

interface KnowledgeBaseDropdownItem {
  key: string;
  header: string;
}

const courseGeneralConfigurationPageMessages = defineMessages({
  unknownKnowledgeBase: {
    id: 'courseGeneralConfigurationPageMessages.unknownKnowledgeBase',
    defaultMessage: 'Unknown ({kbId})',
    description:
      'default text to show when a course is configured to use an unknown knowledge base',
  },
  knowledgeBasePlaceholder: {
    id: 'courseGeneralConfigurationPageMessages.knowledgeBasePlaceholder',
    defaultMessage: 'Select a knowledge base',
    description: 'placeholder text to show in the knowledge base selector',
  },
});

export function CourseGeneralConfigurationPage() {
  const intl = useIntl();
  const loadKnowledgeBasesCommand = useActionCreator(
    (s) => s.knowledgeBase.loadKnowledgeBases,
  );
  const updateCourseCommand = useActionCreator((s) => s.course.setCourse); // I'm sailing away...
  const knowledgeBases = useSelector(selectKnowledgeBases);
  const course = useSelector(selectCurrentPathCourse);

  const [editCourse, setEditCourse] = useState<EditCourse>({
    knowledgeBaseId: course?.knowledgeBaseId,
  });

  useEffect(() => {
    if (course) {
      setEditCourse({
        knowledgeBaseId: course.knowledgeBaseId,
      });
    }
  }, [course, setEditCourse]);

  useEffect(() => {
    loadKnowledgeBasesCommand();
  }, [loadKnowledgeBasesCommand]);

  const onSubmit: ComponentEventHandler<FormProps> = useCallback(
    (evt, props) => {
      if (!course) return;
      updateCourseCommand({
        course: {
          ...course,
          ...editCourse,
        },
      });
    },
    [updateCourseCommand, course, editCourse],
  );

  // Build list of knowledge bases to populate the dropdown
  let items: KnowledgeBaseDropdownItem[] = knowledgeBases.map((kb) => ({
    key: kb.id,
    header: kb.name,
  }));
  // Check if we need to add the "unknown knowledge base" item to the list
  if (
    course?.knowledgeBaseId &&
    !knowledgeBases.find((kb) => kb.id === course.knowledgeBaseId)
  ) {
    const unknownKnowledgeBaseItem = {
      key: course.knowledgeBaseId,
      header: intl.formatMessage(
        courseGeneralConfigurationPageMessages.unknownKnowledgeBase,
        { kbId: course.knowledgeBaseId },
      ),
    };
    items = [unknownKnowledgeBaseItem, ...items];
  }

  const knowledgeBaseItem = items.find(
    (kb) => kb.key === editCourse.knowledgeBaseId,
  );

  const [kbSearchQuery, setKbSearchQuery] = useState<string | undefined>();
  const onKbDropdownSearchQueryChange = (evt: any, props: DropdownProps) => {
    setKbSearchQuery(props.searchQuery);
  };
  const onKbDropdownOpenChange = (evt: any, props: DropdownProps) => {
    // When the dropdown appears, clear the search query so we show all KBs
    setKbSearchQuery(props.open ? '' : knowledgeBaseItem?.header);
  };
  const onKbDropdownChange = useCallback(
    (evt, props: DropdownProps) => {
      setEditCourse({
        knowledgeBaseId: (props.value as KnowledgeBaseDropdownItem)?.key,
      });
      setKbSearchQuery(props.value ? props.searchQuery : '');
    },
    [setEditCourse, setKbSearchQuery],
  );

  return (
    <Box>
      <Form onSubmit={onSubmit}>
        <FormDropdown
          label={{
            content: (
              <FormattedMessage
                id="courseGeneralConfigurationPage.KnowledgeBaseDropdownLabel"
                defaultMessage="Knowledge base"
              />
            ),
            id: labelId,
          }}
          placeholder={intl.formatMessage(
            courseGeneralConfigurationPageMessages.knowledgeBasePlaceholder,
            {},
          )}
          searchQuery={kbSearchQuery ?? knowledgeBaseItem?.header}
          items={items}
          defaultValue={knowledgeBaseItem}
          aria-labelledby={labelId}
          search
          clearable={!!editCourse.knowledgeBaseId}
          itemToValue={(item) => (item as KnowledgeBaseDropdownItem)?.key}
          onChange={onKbDropdownChange}
          onOpenChange={onKbDropdownOpenChange}
          onSearchQueryChange={onKbDropdownSearchQueryChange}
        />
        <FormButton
          content={
            <FormattedMessage
              id="courseGeneralConfigurationPage.saveButton"
              defaultMessage="Save"
            />
          }
          primary
        />
      </Form>
    </Box>
  );
}
