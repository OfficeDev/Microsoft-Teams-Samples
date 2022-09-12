import React, { useCallback } from 'react';
import {
  Button,
  MenuButton,
  Text,
  ChevronDownMediumIcon,
  Divider,
  Flex,
  Box,
} from '@fluentui/react-northstar';
import { useIntl, defineMessages, FormattedMessage } from 'react-intl';
import { Route, Switch } from 'react-router';
import { useDispatch } from 'react-redux';

import { usePushRelativePath } from 'hooks';
import { LeftRailCourseMenu } from './LeftRailCouseMenu';
import { LeftRailKnowledgeBaseMenu } from './LeftRailKnowledgeBaseMenu';

const leftRailMessages = defineMessages({
  courseMenuItem: {
    id: 'leftRail.courseMenuItem',
    description: 'Menu item for selecting the courses mode of the app',
    defaultMessage: 'Courses',
  },
  knowledgeBaseItem: {
    id: 'leftRail.knowledgeBaseItem',
    description: 'Menu item for selecting the knowledge base mode of the app',
    defaultMessage: 'Knowledge Bases',
  },
});

// eslint-disable-next-line max-lines-per-function, sonarjs/cognitive-complexity
export default function LeftRail(): JSX.Element {
  const dispatch = useDispatch();
  const push = usePushRelativePath();
  const intl = useIntl();
  const onMenuChange = useCallback(
    (evt: unknown, props?: { index?: number }) => {
      if (props?.index === undefined) return;
      const slug = props.index === 0 ? 'courses' : 'knowledgeBases';
      dispatch(push(`/${slug}`));
    },
    [dispatch, push],
  );

  const items = [
    intl.formatMessage(leftRailMessages.courseMenuItem, {}),
    intl.formatMessage(leftRailMessages.knowledgeBaseItem, {}),
  ];
  return (
    <>
      <Flex vAlign="center">
        <MenuButton
          trigger={
            <Button
              style={{
                paddingTop: '1em',
                paddingLeft: '1.5em',
              }}
              text
              content={
                <>
                  <Text style={{ paddingRight: '1em' }}>
                    <Switch>
                      <Route path="/:context/courses">
                        <FormattedMessage
                          id="leftRail.courseMenuItem"
                          description="Menu item for selecting the courses mode of the app"
                          defaultMessage="Courses"
                        />
                      </Route>
                      <Route path="/:context/knowledgeBases">
                        <FormattedMessage
                          id="leftRail.knowledgeBaseItem"
                          description="Menu item for selecting the knowledge base mode of the app"
                          defaultMessage="Knowledge Bases"
                        />
                      </Route>
                    </Switch>
                  </Text>
                  <ChevronDownMediumIcon />
                </>
              }
            />
          }
          menu={items}
          onMenuItemClick={onMenuChange}
        />
      </Flex>
      <Divider />
      <Box styles={{ height: '100%', width: '100%' }}>
        <Switch>
          <Route path="/:context/courses">
            <LeftRailCourseMenu />
          </Route>
          <Route path="/:context/knowledgeBases">
            <LeftRailKnowledgeBaseMenu />
          </Route>
        </Switch>
      </Box>
    </>
  );
}
