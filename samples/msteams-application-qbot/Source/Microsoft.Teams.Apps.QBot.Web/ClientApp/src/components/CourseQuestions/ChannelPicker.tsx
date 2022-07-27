import { Question, Channel } from 'models';
import React, { useMemo } from 'react';
import { countBy, sortBy } from 'lodash';
import {
  Dropdown,
  DropdownItemProps,
  DropdownProps,
  ShorthandCollection,
  Text,
} from '@fluentui/react-northstar';
import { useCallback } from 'react';
import { useDefaultColorScheme } from 'hooks/useColorScheme';
import { FormattedMessage, defineMessages, useIntl } from 'react-intl';

const channelPickerMessages = defineMessages({
  channelPickerPlaceholder: {
    id: 'channelPickerDropdown.channelPickerPlaceholder',
    defaultMessage: 'Any Channel',
    description: 'Placeholder for channel dropdown.',
  },
  noChannelsFound: {
    id: 'channelPickerDropdown.noChannelsFound',
    defaultMessage: 'No channels found',
    description: 'No channels found in the channel picker dropdown.',
  },
});

export type ChannelWithQuestionCount = Channel & {
  questionCount: number;
  hasAskedQuestion: boolean;
};
export interface ChannelPickerProps {
  channels: Channel[];
  questions: Question[];
  disabledClassName: string;
  onChannelSelect: (channel?: Channel) => void;
  disabled: boolean;
}

export function ChannelPicker({
  channels,
  questions,
  disabledClassName,
  onChannelSelect,
  disabled,
}: ChannelPickerProps): JSX.Element {
  const intl = useIntl();
  const colorScheme = useDefaultColorScheme();
  const channelsWithQuestionCount: ChannelWithQuestionCount[] = useMemo(() => {
    const countByChannelId = countBy(questions, (q) => q.channelId);
    return channels.map((channel) => ({
      ...channel,
      questionCount:
        channel.id in countByChannelId ? countByChannelId[channel.id] : 0,
      hasAskedQuestion: channel.id in countByChannelId,
    }));
  }, [channels, questions]);

  const items: ShorthandCollection<
    DropdownItemProps,
    Record<string, unknown>
  > = sortBy(
    channelsWithQuestionCount,
    (channel) => -channel.questionCount,
  ).map((channel) => ({
    key: channel.id,
    header: channel.name,
    className: channel.hasAskedQuestion ? undefined : disabledClassName,
    content: channel.hasAskedQuestion
      ? `(${channel.questionCount})`
      : undefined,
    disabled: !channel.hasAskedQuestion,
  }));
  const onChange = useCallback(
    (
      event:
        | React.MouseEvent<Element, MouseEvent>
        | React.KeyboardEvent<Element>
        | null,
      data: DropdownProps,
    ) => {
      const value = data.value as { key: string; header: string };
      const ChannelId = value?.key ? value.key : undefined;
      const Channel = ChannelId
        ? channels.find((channel) => channel.id === ChannelId)
        : undefined;
      onChannelSelect(Channel);
    },
    [channels, onChannelSelect],
  );
  return (
    <div style={{ color: colorScheme.foreground2 }}>
      <Text style={{ paddingRight: '.5em' }}>
        <FormattedMessage
          id="channelPickerDropdown.channel"
          description="Channel dropdown text"
          defaultMessage="Channel"
        />
        :
      </Text>
      <Dropdown
        disabled={disabled}
        className="Channel-dropdown"
        placeholder={intl.formatMessage(
          channelPickerMessages.channelPickerPlaceholder,
          {},
        )}
        noResultsMessage={intl.formatMessage(
          channelPickerMessages.noChannelsFound,
          {},
        )}
        inverted
        fluid
        inline
        search
        clearable
        items={items}
        onChange={onChange}
      />
    </div>
  );
}
