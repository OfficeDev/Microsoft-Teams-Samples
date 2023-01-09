import { Signature, User } from '.';

// This is a design quirk of our proof of concept. Instead of creating, storing
// and managing documents on the backend we "store" these documents on the frontend.
// To be able to differentiate between these documents we have the below types that
// lead to documents being selectively rendered.
// In a full application, this would be replaced with actual document content or a
// raw file.
export enum DocumentType {
  PurchaseAgreement = 'Purchase Agreement',
  PurchaseOrderDocument = 'Purchase Order Document',
  TaxFilings = 'Tax Filings',
  LeaseAgreement = 'Lease Agreement',
  ExpenseReport = 'Expense Report',
  Payroll = 'Payroll',
  Invoice = 'Invoice',
  Payment = 'Payment',
}

// This is an enum representation of all possible states of a document
export enum DocumentState {
  Active,
  Complete,
}

// This is a Typescript representation of a C# class https://msteams-captain.visualstudio.com/xGrowth%20App%20Templates/_git/msteams-poc-meetingsigning?path=Source/MeetingSigning.Domain/Models/Document.cs
export type Document = {
  id: string;
  documentType: DocumentType;
  owner: User;
  documentState: DocumentState;
  viewers: User[];
  signatures: Signature[];
};

// This is a Typescript representation of a C# class https://msteams-captain.visualstudio.com/xGrowth%20App%20Templates/_git/msteams-poc-meetingsigning?path=Source/MeetingSigning.Domain/Models/DocumentInput.cs
export type DocumentInput = {
  documentType: DocumentType;
  viewers: User[];
  signers: User[];
};

// This is a Typescript representation of a C# class https://msteams-captain.visualstudio.com/xGrowth%20App%20Templates/_git/msteams-poc-meetingsigning?path=Source/MeetingSigning.Domain/Models/DocumentUpdateInput.cs
export type DocumentUpdateInput = {
  documentState: DocumentState;
};

export type DocumentListDto = {
  documents: Document[];
  callerUser: User;
};
