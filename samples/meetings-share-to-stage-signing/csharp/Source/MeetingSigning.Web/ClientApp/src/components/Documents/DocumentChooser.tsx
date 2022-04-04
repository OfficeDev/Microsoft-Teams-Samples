import { Text } from '@fluentui/react-northstar';
import { ReactNode } from 'react';
import { Document } from '.';
import { DocumentType, Signature } from 'models';

type DocumentChooserProps = {
  documentId: string;
  documentType: DocumentType;
  loggedInAadId: string;
  signatures: Signature[];
  clickable?: boolean;
  className?: string;
};

/**
 * This is specific to our proof-of-concept. We do not store Documents anywhere.
 * Instead we store a type of document, and for each Type we return a the same document.
 *
 * This component takes that documentType, get's it's content and returns a `Document` component
 *
 * @param documentType This is specific to our proof-of-concept, and is used to return the document e.g. PurchaseAgreement
 * @param loggedInAadId The AAD Id of the logged in user
 * @param signatures The Signatures details of this document
 * @param clickable Should the signatures be clickable. Defaults to `true`
 * @returns
 */
export function DocumentChooser({
  documentId,
  documentType,
  loggedInAadId,
  signatures,
  clickable = true,
  className,
}: DocumentChooserProps) {
  let documentTitle = 'Default Agreement';
  let documentContent: ReactNode = {};

  switch (documentType) {
    case DocumentType.PurchaseAgreement:
      documentTitle = 'Purchase Agreement';
      documentContent = PurchaseAgreementContent;
      break;

    case DocumentType.PurchaseOrderDocument:
      documentTitle = 'Purchase Order Document';
      documentContent = PurchaseOrderDocument;
      break;

    default:
      documentContent = (
        <>
          <Text
            content="This agreement is by and between Contoso ('Buyer'), and Northwind Traders ('Seller')."
            as="p"
          />
          <Text
            content="We didn't decide what to sign, so let's sign this in good faith. Like a blank check"
            as="p"
          />
        </>
      );
  }

  return (
    <Document
      id={documentId}
      title={documentTitle}
      content={documentContent}
      loggedInAadId={loggedInAadId}
      signatures={signatures}
      clickable={clickable}
      className={className}
    />
  );
}

const PurchaseAgreementContent: ReactNode = (
  <>
    <header>
      <address>
        Northwind Traders
        <br />
        1 Northwind Traders Way
        <br />
        Chicago
        <br />
        IL 98052
      </address>

      <time dateTime="2022-01-17">25 February, 2022</time>
    </header>

    <section>
      <p>
        Northwind Traders (&lsquo;Buyer&rsquo;) agrees to acquire Contoso
        Corporation (&lsquo;Seller&rsquo;). The Buyer will pay $68.7 million for
        the company, staff, assets, and liabilities.
      </p>

      <p>This purchase is awaiting regulatory approval.</p>
    </section>

    <p>We the undersigned, agreed to the above:</p>
  </>
);

const PurchaseOrderDocument: ReactNode = (
  <>
    <header>
      <address>
        Contoso Corporation
        <br />
        143 Whitehall House
        <br />
        Boise
        <br />
        ID 94444
      </address>

      <time dateTime="2022-01-21">21 January, 2022</time>
    </header>
    <section>
      <h1>Purchase Order</h1>
      <table>
        <thead>
          <tr>
            <th>Item</th>
            <th>Description</th>
            <th>OTY</th>
            <th>Unit Price</th>
            <th>Total</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>14926</td>
            <td>Surface Laptop 4</td>
            <td>2</td>
            <td>$899.99</td>
            <td>$1799.98</td>
          </tr>
          <tr>
            <td>35178</td>
            <td>XBox Series X</td>
            <td>5</td>
            <td>$499</td>
            <td>$2495</td>
          </tr>
          <tr>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
          </tr>
          <tr>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
          </tr>
          <tr>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
          </tr>
          <tr>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
          </tr>
        </tbody>
        <tfoot>
          <tr>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td>$4294.98</td>
          </tr>
        </tfoot>
      </table>
    </section>
    <footer>
      <p>This order has been approved by the below:</p>
    </footer>
  </>
);
