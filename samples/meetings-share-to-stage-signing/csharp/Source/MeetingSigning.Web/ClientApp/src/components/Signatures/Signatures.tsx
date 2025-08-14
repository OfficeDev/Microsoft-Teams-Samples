import { Form, FormInput } from '@fluentui/react-northstar';
import { DialogDimension } from '@microsoft/teams-js';
import * as microsoftTeams from '@microsoft/teams-js';
import * as ACData from 'adaptivecards-templating';
import { SignatureConfirmationCard } from 'adaptive-cards';
import { useMutation } from 'react-query';
import { postSignDocument, SignDocumentModel } from 'api/signatureApi';
import { Signature, User } from 'models';
import { useUserIsAnonymous } from 'utils/TeamsProvider/hooks';
import './Signature.css';

type SignatureInputProps = {
  documentId: string;
  loggedInUser: User;
  clickable: boolean;
  signature: Signature;
};

/**
 * Single Signature input, includes the label and the input field.
 * The input field will be clickable, if the logged in user is the Signer.
 * Clicking on the input box will open a Task Dialog confirming the signature.
 *
 * @param documentId used in the call to the Sign API
 * @param loggedInUser ID of the logged in user, used in logic to allow signing or not
 * @param clickable Boolean field to disable signing in specific scenarios. e.g. in the Sidepanel
 * @param signature Signature details
 */
function SignatureInput({
  documentId,
  loggedInUser,
  clickable,
  signature,
}: SignatureInputProps) {
  const userIsAnonymous = useUserIsAnonymous();

  // We are using https://react-query.tanstack.com/ for handling the calls to our APIs.
  // To post a call we set-up a mutation, which we then call further down when we want
  // to make the call to the API.
  const signDocumentMutation = useMutation<Signature, Error, SignDocumentModel>(
    (model: SignDocumentModel) => postSignDocument(model, userIsAnonymous),
  );

  const isSignatureForLoggedInPerson: boolean =
    loggedInUser &&
    (signature.signer.userId === loggedInUser.userId ||
      signature.signer.email === loggedInUser.email);

  const signatureConfirmationTaskModule = () => {
    const template = new ACData.Template(SignatureConfirmationCard);
    const card = template.expand({
      $root: {
        name: signature.signer.name,
      },
    });

    const signatureConfirmationSubmitHandler = async (
      error: string,
      result: string | object,
    ) => {
      if (error !== null) {
        console.log(`Signature Confirmation handler - error: '${error}'`);
      } else if (result !== undefined) {
        const signatureSigned = { ...signature };
        const resultParsed =
          typeof result === 'object' ? result : JSON.parse(result);
        signatureSigned.text = resultParsed.confirmation;

        signDocumentMutation.mutate({
          documentId: documentId,
          signature: signatureSigned,
        });
      }
    };

    // tasks.startTasks is deprecated, but the 2.0 of SDK's dialog.open does not support opening adaptive cards yet.
    microsoftTeams.tasks.startTask(
      {
        width: DialogDimension.Medium,
        card: JSON.stringify(card),
      },
      signatureConfirmationSubmitHandler,
    );
  };

  return (
    <>
      <FormInput
        label={
          (signDocumentMutation.data &&
            signDocumentMutation.data.signer.name) ||
          signature.signer.name
        }
        value={
          (signDocumentMutation.data && signDocumentMutation.data.text) ||
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
          (signDocumentMutation.data && signDocumentMutation.data.isSigned) ||
          signature.isSigned
        }
        error={signDocumentMutation.isError}
        errorMessage={signDocumentMutation.error}
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
  loggedInUser: User;
  clickable: boolean;
  signatures: Signature[];
};

/**
 * List of Signature fields
 *
 * @param documentId used in the call to the Sign API
 * @param loggedInUser ID of the logged in user, used in logic to allow signing or not
 * @param clickable Boolean field to disable signing in specific scenarios. e.g. in the Sidepanel
 * @param signatures Details for all the relevant Signature
 */
export function SignatureList({
  documentId,
  loggedInUser,
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
            loggedInUser={loggedInUser}
            clickable={clickable}
            signature={s}
            key={index}
          />
        ))}
    </Form>
  );
}
