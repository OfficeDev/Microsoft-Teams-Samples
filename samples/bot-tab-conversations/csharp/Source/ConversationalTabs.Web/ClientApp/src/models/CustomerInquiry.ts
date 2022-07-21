export type CustomerInquiry = {
  subEntityId: string;
  createdDateTime: string;
  customerName: string;
  question: string;
  conversationId: string;
  active: boolean;
};

export type CustomerInquiryInput = {
  customerName: string;
  question: string;
}
