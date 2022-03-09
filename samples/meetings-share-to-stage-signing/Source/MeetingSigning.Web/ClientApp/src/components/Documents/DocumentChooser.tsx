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
        Microsoft Corporation
        <br />
        1 Microsoft Way
        <br />
        Redmond
        <br />
        WA 98052
      </address>

      <time dateTime="2022-01-17">17 January, 2022</time>
    </header>

    <section>
      <p>
        Microsoft Corporation (&lsquo;Buyer&rsquo;) agrees to buy a whole bunch
        of games from Activision/Blizzard (&lsquo;Seller&rsquo;). The Buyer will
        pay $68.7 million for these games. The games will be delivered before
        July 2023.
      </p>

      <p>
        With three billion people actively playing games today, and fueled by a
        new generation steeped in the joys of interactive entertainment, gaming
        is now the largest and fastest-growing form of entertainment. Today,
        Microsoft Corp. (Nasdaq: MSFT) announced plans to acquire Activision
        Blizzard Inc. (Nasdaq: ATVI), a leader in game development and
        interactive entertainment content publisher. This acquisition will
        accelerate the growth in Microsoft&apos;s gaming business across mobile,
        PC, console and cloud and will provide building blocks for the
        metaverse.
      </p>

      <p>
        Microsoft will acquire Activision Blizzard for $95.00 per share, in an
        all-cash transaction valued at $68.7 billion, inclusive of Activision
        Blizzard&apos;s net cash. When the transaction closes, Microsoft will
        become the world&apos;s third-largest gaming company by revenue, behind
        Tencent and Sony. The planned acquisition includes iconic franchises
        from the Activision, Blizzard and King studios like
        &ldquo;Warcraft&rdquo;, &ldquo;Diablo&rdquo;, &ldquo;Overwatch&rdquo;,
        &ldquo;Call of Duty&rdquo; and &ldquo;Candy Crush,&rdquo; in addition to
        global eSports activities through Major League Gaming. The company has
        studios around the world with nearly 10,000 employees.
      </p>

      <p>
        Bobby Kotick will continue to serve as CEO of Activision Blizzard, and
        he and his team will maintain their focus on driving efforts to further
        strengthen the company&apos;s culture and accelerate business growth.
        Once the deal closes, the Activision Blizzard business will report to
        Phil Spencer, CEO, Microsoft Gaming.
      </p>

      <p>
        &ldquo;Gaming is the most dynamic and exciting category in entertainment
        across all platforms today and will play a key role in the development
        of metaverse platforms,&rdquo; said Satya Nadella, chairman and CEO,
        Microsoft. &ldquo;We&apos;re investing deeply in world-class content,
        community and the cloud to usher in a new era of gaming that puts
        players and creators first and makes gaming safe, inclusive and
        accessible to all.&rdquo;
      </p>

      <p>
        &ldquo;Players everywhere love Activision Blizzard games, and we believe
        the creative teams have their best work in front of them,&rdquo; said
        Phil Spencer, CEO, Microsoft Gaming. &ldquo;Together we will build a
        future where people can play the games they want, virtually anywhere
        they want.&rdquo;
      </p>
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
