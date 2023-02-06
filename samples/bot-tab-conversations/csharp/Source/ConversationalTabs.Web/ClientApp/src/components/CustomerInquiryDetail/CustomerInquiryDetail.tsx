import {
  Button,
  ChatIcon,
  Flex,
  FlexItem,
  Header,
  Label,
  Text,
} from '@fluentui/react-northstar';
import { CustomerInquiry } from 'models';
import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';

dayjs.extend(relativeTime);

type CustomerInquiryDetailProps = {
  customerInquiry: CustomerInquiry;
  isChatOpen: boolean;
  onOpenConversation: () => void;
  onCloseConversation: () => void;
};

function CustomerInquiryDetail({
  customerInquiry,
  isChatOpen,
  onOpenConversation,
  onCloseConversation,
}: CustomerInquiryDetailProps) {
  return (
    <Flex column>
      <Flex vAlign="center" gap="gap.medium">
        <Header as="h1" content={customerInquiry.customerName} />
        <FlexItem push>
          <Button
            tabIndex={-1}
            icon={<ChatIcon outline />}
            tinted
            content={isChatOpen ? 'Close conversation' : 'Open conversation'}
            onClick={isChatOpen ? onCloseConversation : onOpenConversation}
          />
        </FlexItem>
      </Flex>
      <Flex vAlign="center" gap="gap.medium">
        <Label
            content={customerInquiry.active ? 'Active' : 'Inactive'}
            circular
            color={customerInquiry.active ? 'green' : 'red'}
            fluid={false}
          />
          <Text content={`Created ${dayjs(customerInquiry.createdDateTime).fromNow()}`} />
      </Flex>
      <Header as="h2" content="Inquiry" />
      <Text content={customerInquiry.question} />
    </Flex>
  );
}

export { CustomerInquiryDetail };
