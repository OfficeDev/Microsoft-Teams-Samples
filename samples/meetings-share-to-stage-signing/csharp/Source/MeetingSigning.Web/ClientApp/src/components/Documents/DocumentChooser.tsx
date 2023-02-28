import { Text } from '@fluentui/react-northstar';
import { ReactNode } from 'react';
import { Document } from '.';
import { DocumentType, Signature, User } from 'models';

type DocumentChooserProps = {
  documentId: string;
  documentType: DocumentType;
  loggedInUser: User;
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
 * @param loggedInUser The User Id of the logged in user
 * @param signatures The Signatures details of this document
 * @param clickable Should the signatures be clickable. Defaults to `true`
 * @returns
 */
export function DocumentChooser({
  documentId,
  documentType,
  loggedInUser,
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

          <p>
            Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do
            eiusmod tempor incididunt ut labore et dolore magna aliqua. Velit ut
            tortor pretium viverra. Ut venenatis tellus in metus. Nullam
            vehicula ipsum a arcu. Elit pellentesque habitant morbi tristique.
            In iaculis nunc sed augue lacus viverra vitae congue. Aliquet nibh
            praesent tristique magna sit amet purus gravida. Arcu felis bibendum
            ut tristique et egestas quis. Pellentesque nec nam aliquam sem et
            tortor consequat id. Vitae turpis massa sed elementum tempus
            egestas. Varius sit amet mattis vulputate enim nulla aliquet. Turpis
            tincidunt id aliquet risus feugiat in ante metus. Eget nulla
            facilisi etiam dignissim diam quis enim lobortis. Risus pretium quam
            vulputate dignissim suspendisse in est. Rhoncus est pellentesque
            elit ullamcorper dignissim cras tincidunt lobortis. Arcu bibendum at
            varius vel pharetra vel turpis. A condimentum vitae sapien
            pellentesque habitant morbi tristique senectus. Id faucibus nisl
            tincidunt eget nullam non nisi. Aenean pharetra magna ac placerat
            vestibulum lectus mauris ultrices.
          </p>
          <p>
            In nibh mauris cursus mattis molestie. Scelerisque in dictum non
            consectetur. Dictum at tempor commodo ullamcorper a lacus vestibulum
            sed arcu. Scelerisque fermentum dui faucibus in ornare quam.
            Volutpat maecenas volutpat blandit aliquam etiam erat. Lacus
            suspendisse faucibus interdum posuere lorem ipsum dolor sit amet.
            Egestas pretium aenean pharetra magna ac placerat vestibulum. Ipsum
            dolor sit amet consectetur adipiscing elit pellentesque habitant.
            Consequat id porta nibh venenatis cras sed felis eget velit. Egestas
            purus viverra accumsan in nisl nisi scelerisque eu. Mollis aliquam
            ut porttitor leo a diam sollicitudin tempor id. Etiam non quam lacus
            suspendisse faucibus interdum posuere. Orci eu lobortis elementum
            nibh tellus molestie.
          </p>
          <p>
            Laoreet sit amet cursus sit amet dictum. Amet tellus cras adipiscing
            enim eu turpis. Integer malesuada nunc vel risus commodo viverra. In
            dictum non consectetur a erat nam at. Et magnis dis parturient
            montes. Sed risus ultricies tristique nulla aliquet. Ullamcorper
            velit sed ullamcorper morbi tincidunt ornare massa eget. Vulputate
            sapien nec sagittis aliquam malesuada bibendum arcu vitae elementum.
            Turpis in eu mi bibendum neque egestas congue quisque. Egestas
            congue quisque egestas diam in arcu cursus. Nisi est sit amet
            facilisis magna etiam.
          </p>
          <p>
            Maecenas volutpat blandit aliquam etiam erat. Massa massa ultricies
            mi quis. Senectus et netus et malesuada fames ac turpis egestas.
            Pellentesque eu tincidunt tortor aliquam nulla facilisi cras.
            Malesuada proin libero nunc consequat interdum varius sit amet
            mattis. Massa tincidunt nunc pulvinar sapien et ligula ullamcorper
            malesuada proin. Blandit volutpat maecenas volutpat blandit.
            Fringilla urna porttitor rhoncus dolor purus non enim praesent. Non
            tellus orci ac auctor augue mauris augue. Dui accumsan sit amet
            nulla facilisi. Ornare suspendisse sed nisi lacus sed viverra tellus
            in. Posuere lorem ipsum dolor sit amet. Aliquam ut porttitor leo a
            diam sollicitudin tempor id. Tellus integer feugiat scelerisque
            varius morbi enim. Mattis ullamcorper velit sed ullamcorper morbi
            tincidunt ornare. Malesuada pellentesque elit eget gravida cum
            sociis natoque penatibus et. Habitasse platea dictumst quisque
            sagittis. Eget nunc scelerisque viverra mauris in aliquam sem
            fringilla. Diam ut venenatis tellus in metus vulputate. Leo integer
            malesuada nunc vel risus commodo viverra maecenas accumsan.
          </p>
          <p>
            Rhoncus urna neque viverra justo nec ultrices dui. In ante metus
            dictum at tempor commodo ullamcorper a. Justo laoreet sit amet
            cursus sit amet dictum sit. Feugiat nisl pretium fusce id velit ut.
            Faucibus ornare suspendisse sed nisi lacus sed viverra tellus. Sit
            amet aliquam id diam maecenas. Ut morbi tincidunt augue interdum
            velit euismod in pellentesque. Pretium quam vulputate dignissim
            suspendisse in est ante in nibh. Sed enim ut sem viverra aliquet
            eget sit amet tellus. Id diam vel quam elementum pulvinar etiam non.
            Vulputate sapien nec sagittis aliquam malesuada bibendum arcu vitae
            elementum. Ornare suspendisse sed nisi lacus sed viverra. Sed
            euismod nisi porta lorem mollis aliquam ut porttitor. Vitae purus
            faucibus ornare suspendisse. Laoreet id donec ultrices tincidunt.
            Sagittis orci a scelerisque purus. Dignissim enim sit amet venenatis
            urna cursus eget nunc scelerisque. Sit amet porttitor eget dolor
            morbi non arcu. Netus et malesuada fames ac turpis. Pellentesque
            diam volutpat commodo sed egestas egestas fringilla.
          </p>
          <p>
            Vitae tortor condimentum lacinia quis vel eros donec. Turpis egestas
            integer eget aliquet nibh. Quisque sagittis purus sit amet. Urna
            molestie at elementum eu facilisis sed. Fringilla est ullamcorper
            eget nulla facilisi etiam dignissim diam. Aenean et tortor at risus
            viverra adipiscing at in. Ut etiam sit amet nisl purus in mollis.
            Egestas sed sed risus pretium. Iaculis eu non diam phasellus
            vestibulum lorem sed. Tincidunt lobortis feugiat vivamus at augue
            eget arcu dictum varius. Malesuada fames ac turpis egestas maecenas
            pharetra. Nulla posuere sollicitudin aliquam ultrices sagittis orci.
            At in tellus integer feugiat. Ornare lectus sit amet est placerat.
            Orci a scelerisque purus semper eget duis at tellus at. Molestie a
            iaculis at erat. Tristique nulla aliquet enim tortor at auctor urna.
            Cursus metus aliquam eleifend mi in nulla posuere sollicitudin.
          </p>
          <p>
            Netus et malesuada fames ac turpis egestas integer eget. Sodales ut
            eu sem integer vitae. Eleifend mi in nulla posuere sollicitudin
            aliquam. Mauris in aliquam sem fringilla ut. Auctor urna nunc id
            cursus metus aliquam eleifend. Sit amet nulla facilisi morbi tempus
            iaculis urna. Sit amet purus gravida quis blandit turpis cursus in.
            In hendrerit gravida rutrum quisque non tellus orci. Sed cras ornare
            arcu dui vivamus arcu felis bibendum ut. Sit amet consectetur
            adipiscing elit. Non curabitur gravida arcu ac tortor dignissim
            convallis aenean et. Amet cursus sit amet dictum sit amet justo
            donec. Massa sed elementum tempus egestas sed sed risus pretium
            quam. Lectus nulla at volutpat diam ut venenatis. Porta non pulvinar
            neque laoreet suspendisse interdum consectetur. Amet nisl purus in
            mollis. Aliquet lectus proin nibh nisl condimentum id venenatis a
            condimentum. Fames ac turpis egestas maecenas pharetra.
          </p>
          <p>
            Velit ut tortor pretium viverra suspendisse potenti nullam ac
            tortor. Et malesuada fames ac turpis. Enim praesent elementum
            facilisis leo vel fringilla est ullamcorper. Ac placerat vestibulum
            lectus mauris ultrices eros. Faucibus vitae aliquet nec ullamcorper
            sit amet risus nullam. Netus et malesuada fames ac. At urna
            condimentum mattis pellentesque id. Id donec ultrices tincidunt arcu
            non sodales neque sodales ut. Mattis molestie a iaculis at erat
            pellentesque adipiscing commodo. Tellus molestie nunc non blandit
            massa enim nec. Eget mauris pharetra et ultrices neque. Gravida in
            fermentum et sollicitudin. Urna id volutpat lacus laoreet non
            curabitur. Elementum nisi quis eleifend quam adipiscing vitae proin
            sagittis. Massa id neque aliquam vestibulum morbi blandit cursus. Eu
            tincidunt tortor aliquam nulla. Fames ac turpis egestas maecenas
            pharetra convallis. Malesuada pellentesque elit eget gravida cum
            sociis. Dui nunc mattis enim ut tellus elementum sagittis vitae. Sed
            tempus urna et pharetra pharetra massa massa.
          </p>
          <p>
            Adipiscing enim eu turpis egestas pretium aenean pharetra. Enim
            tortor at auctor urna nunc id. Ut morbi tincidunt augue interdum
            velit euismod in pellentesque. Egestas egestas fringilla phasellus
            faucibus scelerisque eleifend donec pretium vulputate. In mollis
            nunc sed id semper risus. At imperdiet dui accumsan sit. Mi quis
            hendrerit dolor magna eget. Consectetur lorem donec massa sapien
            faucibus et molestie ac feugiat. Phasellus faucibus scelerisque
            eleifend donec. Fringilla urna porttitor rhoncus dolor purus non
            enim praesent. Metus aliquam eleifend mi in nulla posuere
            sollicitudin aliquam ultrices. Euismod nisi porta lorem mollis
            aliquam ut porttitor. Maecenas sed enim ut sem. Ornare suspendisse
            sed nisi lacus sed. Aenean pharetra magna ac placerat vestibulum
            lectus mauris ultrices. Risus viverra adipiscing at in tellus.
            Egestas pretium aenean pharetra magna. Habitasse platea dictumst
            vestibulum rhoncus est pellentesque elit ullamcorper dignissim.
            Interdum posuere lorem ipsum dolor sit amet consectetur adipiscing.
          </p>
          <p>
            Amet est placerat in egestas erat imperdiet sed euismod. Leo duis ut
            diam quam nulla porttitor massa. Nibh ipsum consequat nisl vel
            pretium lectus quam. Congue quisque egestas diam in arcu cursus
            euismod quis. Risus in hendrerit gravida rutrum quisque non tellus.
            Auctor elit sed vulputate mi sit amet mauris commodo quis. Lectus
            mauris ultrices eros in cursus turpis massa tincidunt. Pretium nibh
            ipsum consequat nisl vel pretium lectus quam. Auctor augue mauris
            augue neque gravida in fermentum et. Sit amet cursus sit amet.
            Pellentesque massa placerat duis ultricies lacus sed. Non curabitur
            gravida arcu ac. Eu non diam phasellus vestibulum lorem sed risus
            ultricies tristique. Tortor aliquam nulla facilisi cras. Euismod
            lacinia at quis risus sed. Viverra adipiscing at in tellus integer.
            Nullam ac tortor vitae purus.
          </p>
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
      loggedInUser={loggedInUser}
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
            <th>QTY</th>
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
      <p>
        Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod
        tempor incididunt ut labore et dolore magna aliqua. Velit ut tortor
        pretium viverra. Ut venenatis tellus in metus. Nullam vehicula ipsum a
        arcu. Elit pellentesque habitant morbi tristique. In iaculis nunc sed
        augue lacus viverra vitae congue. Aliquet nibh praesent tristique magna
        sit amet purus gravida. Arcu felis bibendum ut tristique et egestas
        quis. Pellentesque nec nam aliquam sem et tortor consequat id. Vitae
        turpis massa sed elementum tempus egestas. Varius sit amet mattis
        vulputate enim nulla aliquet. Turpis tincidunt id aliquet risus feugiat
        in ante metus. Eget nulla facilisi etiam dignissim diam quis enim
        lobortis. Risus pretium quam vulputate dignissim suspendisse in est.
        Rhoncus est pellentesque elit ullamcorper dignissim cras tincidunt
        lobortis. Arcu bibendum at varius vel pharetra vel turpis. A condimentum
        vitae sapien pellentesque habitant morbi tristique senectus. Id faucibus
        nisl tincidunt eget nullam non nisi. Aenean pharetra magna ac placerat
        vestibulum lectus mauris ultrices.
      </p>
      <p>
        In nibh mauris cursus mattis molestie. Scelerisque in dictum non
        consectetur. Dictum at tempor commodo ullamcorper a lacus vestibulum sed
        arcu. Scelerisque fermentum dui faucibus in ornare quam. Volutpat
        maecenas volutpat blandit aliquam etiam erat. Lacus suspendisse faucibus
        interdum posuere lorem ipsum dolor sit amet. Egestas pretium aenean
        pharetra magna ac placerat vestibulum. Ipsum dolor sit amet consectetur
        adipiscing elit pellentesque habitant. Consequat id porta nibh venenatis
        cras sed felis eget velit. Egestas purus viverra accumsan in nisl nisi
        scelerisque eu. Mollis aliquam ut porttitor leo a diam sollicitudin
        tempor id. Etiam non quam lacus suspendisse faucibus interdum posuere.
        Orci eu lobortis elementum nibh tellus molestie.
      </p>
      <p>
        Laoreet sit amet cursus sit amet dictum. Amet tellus cras adipiscing
        enim eu turpis. Integer malesuada nunc vel risus commodo viverra. In
        dictum non consectetur a erat nam at. Et magnis dis parturient montes.
        Sed risus ultricies tristique nulla aliquet. Ullamcorper velit sed
        ullamcorper morbi tincidunt ornare massa eget. Vulputate sapien nec
        sagittis aliquam malesuada bibendum arcu vitae elementum. Turpis in eu
        mi bibendum neque egestas congue quisque. Egestas congue quisque egestas
        diam in arcu cursus. Nisi est sit amet facilisis magna etiam.
      </p>
      <p>
        Maecenas volutpat blandit aliquam etiam erat. Massa massa ultricies mi
        quis. Senectus et netus et malesuada fames ac turpis egestas.
        Pellentesque eu tincidunt tortor aliquam nulla facilisi cras. Malesuada
        proin libero nunc consequat interdum varius sit amet mattis. Massa
        tincidunt nunc pulvinar sapien et ligula ullamcorper malesuada proin.
        Blandit volutpat maecenas volutpat blandit. Fringilla urna porttitor
        rhoncus dolor purus non enim praesent. Non tellus orci ac auctor augue
        mauris augue. Dui accumsan sit amet nulla facilisi. Ornare suspendisse
        sed nisi lacus sed viverra tellus in. Posuere lorem ipsum dolor sit
        amet. Aliquam ut porttitor leo a diam sollicitudin tempor id. Tellus
        integer feugiat scelerisque varius morbi enim. Mattis ullamcorper velit
        sed ullamcorper morbi tincidunt ornare. Malesuada pellentesque elit eget
        gravida cum sociis natoque penatibus et. Habitasse platea dictumst
        quisque sagittis. Eget nunc scelerisque viverra mauris in aliquam sem
        fringilla. Diam ut venenatis tellus in metus vulputate. Leo integer
        malesuada nunc vel risus commodo viverra maecenas accumsan.
      </p>
      <p>
        Rhoncus urna neque viverra justo nec ultrices dui. In ante metus dictum
        at tempor commodo ullamcorper a. Justo laoreet sit amet cursus sit amet
        dictum sit. Feugiat nisl pretium fusce id velit ut. Faucibus ornare
        suspendisse sed nisi lacus sed viverra tellus. Sit amet aliquam id diam
        maecenas. Ut morbi tincidunt augue interdum velit euismod in
        pellentesque. Pretium quam vulputate dignissim suspendisse in est ante
        in nibh. Sed enim ut sem viverra aliquet eget sit amet tellus. Id diam
        vel quam elementum pulvinar etiam non. Vulputate sapien nec sagittis
        aliquam malesuada bibendum arcu vitae elementum. Ornare suspendisse sed
        nisi lacus sed viverra. Sed euismod nisi porta lorem mollis aliquam ut
        porttitor. Vitae purus faucibus ornare suspendisse. Laoreet id donec
        ultrices tincidunt. Sagittis orci a scelerisque purus. Dignissim enim
        sit amet venenatis urna cursus eget nunc scelerisque. Sit amet porttitor
        eget dolor morbi non arcu. Netus et malesuada fames ac turpis.
        Pellentesque diam volutpat commodo sed egestas egestas fringilla.
      </p>
      <p>
        Vitae tortor condimentum lacinia quis vel eros donec. Turpis egestas
        integer eget aliquet nibh. Quisque sagittis purus sit amet. Urna
        molestie at elementum eu facilisis sed. Fringilla est ullamcorper eget
        nulla facilisi etiam dignissim diam. Aenean et tortor at risus viverra
        adipiscing at in. Ut etiam sit amet nisl purus in mollis. Egestas sed
        sed risus pretium. Iaculis eu non diam phasellus vestibulum lorem sed.
        Tincidunt lobortis feugiat vivamus at augue eget arcu dictum varius.
        Malesuada fames ac turpis egestas maecenas pharetra. Nulla posuere
        sollicitudin aliquam ultrices sagittis orci. At in tellus integer
        feugiat. Ornare lectus sit amet est placerat. Orci a scelerisque purus
        semper eget duis at tellus at. Molestie a iaculis at erat. Tristique
        nulla aliquet enim tortor at auctor urna. Cursus metus aliquam eleifend
        mi in nulla posuere sollicitudin.
      </p>
      <p>
        Netus et malesuada fames ac turpis egestas integer eget. Sodales ut eu
        sem integer vitae. Eleifend mi in nulla posuere sollicitudin aliquam.
        Mauris in aliquam sem fringilla ut. Auctor urna nunc id cursus metus
        aliquam eleifend. Sit amet nulla facilisi morbi tempus iaculis urna. Sit
        amet purus gravida quis blandit turpis cursus in. In hendrerit gravida
        rutrum quisque non tellus orci. Sed cras ornare arcu dui vivamus arcu
        felis bibendum ut. Sit amet consectetur adipiscing elit. Non curabitur
        gravida arcu ac tortor dignissim convallis aenean et. Amet cursus sit
        amet dictum sit amet justo donec. Massa sed elementum tempus egestas sed
        sed risus pretium quam. Lectus nulla at volutpat diam ut venenatis.
        Porta non pulvinar neque laoreet suspendisse interdum consectetur. Amet
        nisl purus in mollis. Aliquet lectus proin nibh nisl condimentum id
        venenatis a condimentum. Fames ac turpis egestas maecenas pharetra.
      </p>
      <p>
        Velit ut tortor pretium viverra suspendisse potenti nullam ac tortor. Et
        malesuada fames ac turpis. Enim praesent elementum facilisis leo vel
        fringilla est ullamcorper. Ac placerat vestibulum lectus mauris ultrices
        eros. Faucibus vitae aliquet nec ullamcorper sit amet risus nullam.
        Netus et malesuada fames ac. At urna condimentum mattis pellentesque id.
        Id donec ultrices tincidunt arcu non sodales neque sodales ut. Mattis
        molestie a iaculis at erat pellentesque adipiscing commodo. Tellus
        molestie nunc non blandit massa enim nec. Eget mauris pharetra et
        ultrices neque. Gravida in fermentum et sollicitudin. Urna id volutpat
        lacus laoreet non curabitur. Elementum nisi quis eleifend quam
        adipiscing vitae proin sagittis. Massa id neque aliquam vestibulum morbi
        blandit cursus. Eu tincidunt tortor aliquam nulla. Fames ac turpis
        egestas maecenas pharetra convallis. Malesuada pellentesque elit eget
        gravida cum sociis. Dui nunc mattis enim ut tellus elementum sagittis
        vitae. Sed tempus urna et pharetra pharetra massa massa.
      </p>
      <p>
        Adipiscing enim eu turpis egestas pretium aenean pharetra. Enim tortor
        at auctor urna nunc id. Ut morbi tincidunt augue interdum velit euismod
        in pellentesque. Egestas egestas fringilla phasellus faucibus
        scelerisque eleifend donec pretium vulputate. In mollis nunc sed id
        semper risus. At imperdiet dui accumsan sit. Mi quis hendrerit dolor
        magna eget. Consectetur lorem donec massa sapien faucibus et molestie ac
        feugiat. Phasellus faucibus scelerisque eleifend donec. Fringilla urna
        porttitor rhoncus dolor purus non enim praesent. Metus aliquam eleifend
        mi in nulla posuere sollicitudin aliquam ultrices. Euismod nisi porta
        lorem mollis aliquam ut porttitor. Maecenas sed enim ut sem. Ornare
        suspendisse sed nisi lacus sed. Aenean pharetra magna ac placerat
        vestibulum lectus mauris ultrices. Risus viverra adipiscing at in
        tellus. Egestas pretium aenean pharetra magna. Habitasse platea dictumst
        vestibulum rhoncus est pellentesque elit ullamcorper dignissim. Interdum
        posuere lorem ipsum dolor sit amet consectetur adipiscing.
      </p>
      <p>
        Amet est placerat in egestas erat imperdiet sed euismod. Leo duis ut
        diam quam nulla porttitor massa. Nibh ipsum consequat nisl vel pretium
        lectus quam. Congue quisque egestas diam in arcu cursus euismod quis.
        Risus in hendrerit gravida rutrum quisque non tellus. Auctor elit sed
        vulputate mi sit amet mauris commodo quis. Lectus mauris ultrices eros
        in cursus turpis massa tincidunt. Pretium nibh ipsum consequat nisl vel
        pretium lectus quam. Auctor augue mauris augue neque gravida in
        fermentum et. Sit amet cursus sit amet. Pellentesque massa placerat duis
        ultricies lacus sed. Non curabitur gravida arcu ac. Eu non diam
        phasellus vestibulum lorem sed risus ultricies tristique. Tortor aliquam
        nulla facilisi cras. Euismod lacinia at quis risus sed. Viverra
        adipiscing at in tellus integer. Nullam ac tortor vitae purus.
      </p>
    </section>
    <footer>
      <p>This order has been approved by the below:</p>
    </footer>
  </>
);
