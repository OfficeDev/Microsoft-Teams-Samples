import {
  Button,
  gridCellWithFocusableElementBehavior,
  ArrowRightIcon,
  ShorthandCollection,
  Table,
  TableCellProps,
} from '@fluentui/react-northstar';
import { CustomerInquiry } from 'models';
import { Link } from 'react-router-dom';
import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';

type CustomerInquiryTableProps = {
  entityId: string;
  customerInquiries: CustomerInquiry[];
};

function createRowItemsFromCategoryItem(
  entityId: string,
  inquiry: CustomerInquiry,
  index: number,
): ShorthandCollection<TableCellProps> {
  dayjs.extend(relativeTime)

  return [
    {
      content: inquiry.customerName,
      key: `${index}-customerName`,
    },
    {
      content: inquiry.question,
      truncateContent: true,
      key: `${index}-question`,
    },
    {
      content: dayjs(inquiry.createdDateTime).fromNow(),
      truncateContent: true,
      key: `${index}-createdDateTime`,
    },
    {
      key: `${index}-button`,
      content: (
        <Link to={`/support-department/${entityId}/inquiry/${inquiry.subEntityId}`}>
          <Button
            tabIndex={-1}
            icon={<ArrowRightIcon />}
            iconOnly
            title="Open item"
          />
        </Link>
      ),
      truncateContent: true,
      accessibility: gridCellWithFocusableElementBehavior,
    },
  ];
}

function CustomerInquiryTable({
  entityId,
  customerInquiries,
}: CustomerInquiryTableProps) {
  const header = {
    key: 'header',
    items: [
      {
        content: 'Customer',
        key: 'customerName',
      },
      {
        content: 'Question',
        key: 'question',
      },
      {
        content: 'Created',
        key: 'createdDateTime',
      },
      {
        key: 'open-item',
        'aria-label': 'Open item',
      },
    ],
  };

  const customerInquiriesRows = customerInquiries.map((inquiry, index) => {
    return {
      key: index,
      items: createRowItemsFromCategoryItem(entityId, inquiry, index),
    };
  });

  return (
    <Table
      header={header}
      rows={customerInquiriesRows}
      aria-label="Customer inquiries"
    />
  );
}

export { CustomerInquiryTable };
