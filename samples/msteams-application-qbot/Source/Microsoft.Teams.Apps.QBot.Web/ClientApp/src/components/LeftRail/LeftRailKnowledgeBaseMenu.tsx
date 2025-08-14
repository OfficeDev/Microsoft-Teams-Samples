import React, { useCallback, useEffect, useState } from 'react';
import { LeftRailMenu } from './LeftRailMenu';
import {
  MenuItem,
  Flex,
  Box,
  AddIcon,
  Divider,
  Button,
} from '@fluentui/react-northstar';
import { KnowledgeBase } from 'models';
import { useDispatch, useSelector } from 'react-redux';
import { usePushRelativePath } from 'hooks';
import { useActionCreator } from 'actionCreators';
import { useVirtualizedItemRenderer } from 'hooks/useVirtualizedItemRenderer';
import {
  selectIfFirstLoadingKnowledgeBases,
  selectKnowledgeBases,
  selectPathKnowledgeBaseIndex,
} from 'selectors/knowledgeBaseSelectors';
import { AutoSizer } from 'react-virtualized';
import { FormattedMessage } from 'react-intl';
import { CreateKnowledgeBaseDialog } from './CreateKnowledgeBaseDialog';
import { pick } from 'lodash';

// eslint-disable-next-line max-lines-per-function
function LeftRailKnowledgeBaseMenuItem({
  knowledgeBase,
}: {
  knowledgeBase: KnowledgeBase;
}) {
  return (
    <Button
      text
      content={knowledgeBase.name}
      styles={{ justifyContent: 'flex-start' }}
    />
  );
}

// eslint-disable-next-line sonarjs/cognitive-complexity
export function LeftRailKnowledgeBaseMenu(): JSX.Element {
  const dispatch = useDispatch();
  const knowledgeBases = useSelector(selectKnowledgeBases);
  const isFirstLoad = useSelector(selectIfFirstLoadingKnowledgeBases);
  const activeIndex = useSelector(selectPathKnowledgeBaseIndex);
  const push = usePushRelativePath();
  const loadKnowledgeBasesCommand = useActionCreator(
    (s) => s.knowledgeBase.loadKnowledgeBases,
  );

  const createKnowledgeBaseCommand = useActionCreator(
    (s) => s.knowledgeBase.createKnowledgeBases,
  );
  const [isCreateDialogOpen, setCreateDialogOpen] = useState(false);
  const [newKnowledgeBase, setNewKnowledgeBase] = useState<
    Pick<KnowledgeBase, 'name'>
  >({ name: 'New Knowledge Base' }); //TODO(nibeauli): localize this

  const openDialog = useCallback(() => setCreateDialogOpen(true), [
    setCreateDialogOpen,
  ]);

  const closeDialog = useCallback(() => setCreateDialogOpen(false), [
    setCreateDialogOpen,
  ]);

  useEffect(() => {
    // When this component loads, make sure to load all the courses
    loadKnowledgeBasesCommand();
  }, [loadKnowledgeBasesCommand]);
  useEffect(() => {
    if (activeIndex < 0 && knowledgeBases.length > 0) {
      console.log('No active, course, navigating to first course');
      // initial load scenario (load thee default course)
      dispatch(push(`/knowledgeBases/${knowledgeBases[0].id}`));
    }
  }, [dispatch, activeIndex, knowledgeBases, push]);
  const onNewItemSelected = useCallback(
    (knowledgeBase: KnowledgeBase) => {
      dispatch(push(`/knowledgeBases/${knowledgeBase.id}`));
    },
    [dispatch, push],
  );

  const onCreateNewKnowledgeBase = useCallback(() => {
    createKnowledgeBaseCommand({ knowledgeBase: newKnowledgeBase });
    closeDialog();
  }, [closeDialog, newKnowledgeBase, createKnowledgeBaseCommand]);
  // TODO(nibeauli): have this rener a skeleton when course isn't defined
  const renderRow = useVirtualizedItemRenderer<KnowledgeBase, typeof MenuItem>(
    knowledgeBases,
    (Component, props, knowledgeBase) => {
      console.log('props?', { props });
      return (
        <Component
          {...props}
          key={knowledgeBase ? knowledgeBase.id : props.index}
          styles={{
            ...props.styles,
            paddingBottom: 0,
            paddingTop: 0,
          }}
          onClick={() => knowledgeBase && onNewItemSelected(knowledgeBase)}
        >
          {knowledgeBase ? (
            <Flex styles={pick(props.styles, 'height', 'width')}>
              <LeftRailKnowledgeBaseMenuItem knowledgeBase={knowledgeBase} />
            </Flex>
          ) : null}
        </Component>
      );
    },
    [onNewItemSelected],
  );
  return (
    <Flex column styles={{ height: '100%' }}>
      <CreateKnowledgeBaseDialog
        knowledgeBase={newKnowledgeBase}
        isOpen={isCreateDialogOpen}
        onClose={closeDialog}
        onSubmit={onCreateNewKnowledgeBase}
        onKnowledgeBaseChanged={setNewKnowledgeBase}
      />
      <Flex.Item grow>
        <Box>
          <AutoSizer>
            {({ width, height }) => (
              <LeftRailMenu
                activeIndex={activeIndex}
                rowCount={isFirstLoad ? 5 : knowledgeBases.length}
                rowRenderer={renderRow}
                height={height}
                width={width}
              />
            )}
          </AutoSizer>
        </Box>
      </Flex.Item>
      <Divider />
      <Button
        text
        content={
          <FormattedMessage
            id="leftRailKnowledgeBaseMenu.createButton"
            description="Action for creating a new knowledge base"
            defaultMessage="Create Knowledge Base"
          />
        }
        onClick={openDialog}
        icon={<AddIcon />}
      />
    </Flex>
  );
}
