import { Question } from 'models';
import { User } from 'models/User';
import React, { useMemo } from 'react';
import { countBy, sortBy } from 'lodash';
import {
  Dropdown,
  DropdownItemProps,
  DropdownProps,
  ShorthandCollection,
  Flex,
  Text,
} from '@fluentui/react-northstar';
import { useCallback } from 'react';
import { useDefaultColorScheme } from 'hooks/useColorScheme';
import { UserIcon } from 'components/UserIcon/UserIcon';
import { FormattedMessage, defineMessages, useIntl } from 'react-intl';

const userPickerMessages = defineMessages({
  userPickerPlaceholder: {
    id: 'userPickerDropdown.userPickerPlaceholder',
    defaultMessage: 'Any User',
    description: 'Placeholder for user dropdown.',
  },
  noUsersFound: {
    id: 'userPickerDropdown.noUsersFound',
    defaultMessage: 'No users found',
    description: 'No users found in the user picker dropdown.',
  },
});

export type UserWithQuestionCount = User & {
  questionCount: number;
  hasAskedQuestion: boolean;
};
export interface UserPickerProps {
  users: User[];
  questions: Question[];
  disabledClassName: string;
  onUserSelect: (user?: User) => void;
  disabled: boolean;
}

export function UserPicker({
  users,
  questions,
  disabledClassName,
  onUserSelect,
  disabled,
}: UserPickerProps): JSX.Element {
  const intl = useIntl();
  const colorScheme = useDefaultColorScheme();
  const usersWithQuestionCount: UserWithQuestionCount[] = useMemo(() => {
    const countByAuthorId = countBy(questions, (q) => q.authorId);
    return users.map((user) => ({
      ...user,
      questionCount: user.id in countByAuthorId ? countByAuthorId[user.id] : 0,
      hasAskedQuestion: user.id in countByAuthorId,
    }));
  }, [users, questions]);

  const items: ShorthandCollection<
    DropdownItemProps,
    Record<string, unknown>
  > = sortBy(usersWithQuestionCount, (user) => -user.questionCount).map(
    (user) => ({
      key: user.id,
      header: user.name,
      className: user.hasAskedQuestion ? undefined : disabledClassName,
      disabled: !user.hasAskedQuestion,
      image: user,
    }),
  );
  const onChange = useCallback(
    (
      event:
        | React.MouseEvent<Element, MouseEvent>
        | React.KeyboardEvent<Element>
        | null,
      data: DropdownProps,
    ) => {
      const value = data.value as { key: string; header: string };
      const userId = value?.key ? value.key : undefined;
      const user = userId
        ? users.find((user) => user.id === userId)
        : undefined;
      onUserSelect(user);
    },
    [users, onUserSelect],
  );
  return (
    <div style={{ color: colorScheme.foreground2 }}>
      <Text style={{ paddingRight: '.5em' }}>
        <FormattedMessage
          id="userPickerDropdown.user"
          description="User dropdown text."
          defaultMessage="Asked by"
        />
        :
      </Text>
      <Dropdown
        disabled={disabled}
        className="user-dropdown"
        placeholder={intl.formatMessage(
          userPickerMessages.userPickerPlaceholder,
          {},
        )}
        noResultsMessage={intl.formatMessage(
          userPickerMessages.noUsersFound,
          {},
        )}
        inverted
        fluid
        inline
        clearable
        search
        items={items}
        onChange={onChange}
        renderItem={(Item, props: DropdownItemProps) => {
          const user = props.image as UserWithQuestionCount;
          return (
            <Item
              {...props}
              header={
                <Flex gap="gap.small" vAlign="center">
                  <UserIcon user={user} />
                  <Flex.Item>
                    <Text>{user.name}</Text>
                  </Flex.Item>
                  <Flex.Item push>
                    <Text>
                      {user.hasAskedQuestion
                        ? `(${user.questionCount})`
                        : undefined}
                    </Text>
                  </Flex.Item>
                </Flex>
              }
              image={undefined}
              content={undefined}
            />
          );
        }}
      />
    </div>
  );
}
