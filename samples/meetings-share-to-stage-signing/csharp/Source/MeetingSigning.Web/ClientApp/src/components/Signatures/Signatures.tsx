import { Form, FormInput } from '@fluentui/react-northstar';
import * as microsoftTeams from '@microsoft/teams-js';
import { TaskModuleDimension } from '@microsoft/teams-js';
import * as ACData from 'adaptivecards-templating';
import { SignatureConfirmationCard } from 'adaptive-cards';
import signatureApi from 'api/signatureApi';
import { useApi } from 'hooks';
import { Signature } from 'models';
import './Signature.css';

type SignatureInputProps = {
  documentId: string;
  loggedInAadId: string;
  clickable: boolean;
  signature: Signature;
};

/**
 * Single Signature input, includes the label and the input field.
 * The input field will be clickable, if the logged in user is the Signer.
 * Clicking on the input box will open a Task Dialog confirming the signature.
 *
 * @param documentId used in the call to the Sign API
 * @param loggedInAadId ID of the logged in user, used in logic to allow signing or not
 * @param clickable Boolean field to disable signing in specific scenarios. e.g. in the Sidepanel
 * @param signature Signature details
 */
function SignatureInput({
  documentId,
  loggedInAadId,
  clickable,
  signature,
}: SignatureInputProps) {
  const postSignDocumentApi = useApi(signatureApi.postSignDocument);

  const isSignatureForLoggedInPerson: boolean =
    signature.signer.userId === loggedInAadId;

  const signatureConfirmationTaskModule = () => {
    const template = new ACData.Template(SignatureConfirmationCard);
    const card = template.expand({
      $root: {
        name: signature.signer.name,
      },
    });

    const signatureConfirmationSubmitHandler = async (
      error: string,
      result: string,
    ) => {
      if (error !== null) {
        console.log(`Signature Confirmation handler - error: '${error}'`);
      } else if (result !== undefined) {
        const signatureSigned = { ...signature };
        const resultParsed = JSON.parse(result);
        signatureSigned.text = resultParsed.confirmation;

        await postSignDocumentApi.request(documentId, signatureSigned);
      }
    };
    microsoftTeams.tasks.startTask(
      {
        width: TaskModuleDimension.Medium,
        card: JSON.stringify(card),
      },
      signatureConfirmationSubmitHandler,
    );
  };

  return (
    <>
      <FormInput
        label={
          (postSignDocumentApi.data && postSignDocumentApi.data.signer.name) ||
          signature.signer.name
        }
        value={
          (postSignDocumentApi.data && postSignDocumentApi.data.text) ||
          signature.text
        }
        placeholder={
          isSignatureForLoggedInPerson ? 'Click to sign!' : undefined
        }
        inline
        required={isSignatureForLoggedInPerson}
        disabled={
          !isSignatureForLoggedInPerson ||
          !clickable ||
          (postSignDocumentApi.data && postSignDocumentApi.data.isSigned) ||
          signature.isSigned
        }
        error={postSignDocumentApi.error !== undefined}
        errorMessage={postSignDocumentApi.error}
        showSuccessIndicator={false}
        onClick={() => signatureConfirmationTaskModule()}
        input={{
          readOnly: true,
        }}
        className="signature-input"
      />
    </>
  );
}

export type SignatureListProps = {
  documentId: string;
  loggedInAadId: string;
  clickable: boolean;
  signatures: Signature[];
};

/**
 * List of Signature fields
 *
 * @param documentId used in the call to the Sign API
 * @param loggedInAadId ID of the logged in user, used in logic to allow signing or not
 * @param clickable Boolean field to disable signing in specific scenarios. e.g. in the Sidepanel
 * @param signatures Details for all the relevant Signature
 */
export function SignatureList({
  documentId,
  loggedInAadId,
  clickable,
  signatures,
}: SignatureListProps) {
  return (
    <Form className="signature-list">
      {signatures &&
        signatures.length > 0 &&
        signatures.map((s, index) => (
          <SignatureInput
            documentId={documentId}
            loggedInAadId={loggedInAadId}
            clickable={clickable}
            signature={s}
            key={index}
          />
        ))}
    </Form>
  );
}
