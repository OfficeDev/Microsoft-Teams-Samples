import { Alert, Button, Flex } from '@fluentui/react-northstar';
import { useState } from 'react';
import { useMutation } from 'react-query';
import { TaskInfo } from '@microsoft/teams-js';
import * as microsoftTeams from '@microsoft/teams-js';
import * as ACData from 'adaptivecards-templating';
import { CreateDocumentCard } from 'adaptive-cards';
import { createDocument } from 'api/documentApi';
import {
  ApiErrorCode,
  Document,
  DocumentInput,
  DocumentType,
  User,
} from 'models';
import { apiRetryQuery, isApiErrorCode } from 'utils/UtilsFunctions';
import { ConsentRequest } from 'components/ConsentRequest';

type Choice = {
  name: string;
  value: string;
};

export type CreateDocumentButtonProps = {
  userIsAnonymous: boolean;
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const createTaskInfo = (card: any): TaskInfo => {
  return {
    card: JSON.stringify(card),
  };
};

const createDocumentTypeArray = () => {
  const documents: Choice[] = Object.entries(DocumentType).map(
    ([value, name]) => {
      return { name, value } as Choice;
    },
  );

  return documents;
};

const createUserArray = (
  commaArrayOfUsers?: string,
  isEmail?: boolean,
): User[] => {
  if (!commaArrayOfUsers) {
    return [];
  }

  return commaArrayOfUsers.split(',').map((u: string) => {
    return {
      userId: isEmail ? undefined : u,
      email: isEmail ? u : undefined,
    } as User;
  });
};

/**
 * Content that is shown in the Meeting Tab
 * Includes the ability to open a Task Module to create a Document.
 *
 * @returns a component with a simple header and button to create a document
 */
export function CreateDocumentButton({
  userIsAnonymous,
}: CreateDocumentButtonProps) {
  const [userHasConsented, setUserHasConsented] = useState<boolean>(false);
  const [documentInput, setDocumentInput] = useState<DocumentInput | undefined>(
    undefined,
  );

  const createDocumentMutation = useMutation<Document, Error, DocumentInput>(
    ['createDocument', userIsAnonymous],
    (documentInput: DocumentInput) =>
      createDocument(documentInput, userIsAnonymous),
    {
      retry: (failureCount: number, error: Error) =>
        apiRetryQuery(
          failureCount,
          error,
          userHasConsented,
          setUserHasConsented,
        ),
    },
  );

  const createDocumentsTaskModule = () => {
    const template = new ACData.Template(CreateDocumentCard);
    const documentsCard = template.expand({
      $root: {
        title: 'Select the documents that needs to be reviewed in the meeting',
        error: 'At least one document is required',
        choices: createDocumentTypeArray(),
        successButtonText: 'Next',
        id: 'documents',
      },
    });

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const createDocumentsSubmitHandler = (error: string, resultObj: any) => {
      if (error !== null) {
        console.log(`Document handler - error: '${error}'`);
      } else if (resultObj !== undefined) {
        let result = resultObj;

        // There is a bug for task module cards for Anonymous Users that returns a stringified
        // object to the callback instead of the object. So if the result is a string parse it
        // to an object.
        if (typeof result === 'string') {
          try {
            result = JSON.parse(result);
          } catch (error: any) {
            console.error(
              `Failed to parse task module result, string may not be a valid object: ${error}`,
            );
          }
        }

        const documents: string[] = result.documentsValue.split(',');

        const aadViewers: User[] = createUserArray(result.viewersValue);
        const anonymousViewers: User[] = createUserArray(
          result.anonymousViewers,
          true,
        );
        // Push anonymous viewers to the Viewers array
        const viewers: User[] = aadViewers.concat(anonymousViewers);

        const aadSigners: User[] = createUserArray(result.signersValue);
        const anonymousSigners: User[] = createUserArray(
          result.anonymousSigners,
          true,
        );
        const signers: User[] = aadSigners.concat(anonymousSigners);

        documents.forEach(async (d: string) => {
          const documentInput: DocumentInput = {
            documentType: DocumentType[d as keyof typeof DocumentType],
            viewers: viewers,
            signers: signers,
          };

          setDocumentInput(documentInput);
          createDocumentMutation.mutate(documentInput);
        });
      }
    };

    // tasks.startTasks is deprecated, but the 2.0 of SDK's dialog.open does not support opening adaptive cards yet.
    microsoftTeams.tasks.startTask(
      createTaskInfo(documentsCard),
      createDocumentsSubmitHandler,
    );
  };

  const consentCallback = (error?: string, result?: string) => {
    if (error) {
      console.log(`Error: ${error}`);
    }
    if (result) {
      setUserHasConsented(true);
      if (documentInput !== undefined) {
        createDocumentMutation.mutate(documentInput);
      }
    }
  };

  const displayConsentRequest =
    isApiErrorCode(
      ApiErrorCode.AuthConsentRequired,
      createDocumentMutation.error,
    ) && !userHasConsented;

  return (
    <Flex column>
      {createDocumentMutation.isError && displayConsentRequest && (
        <ConsentRequest callback={consentCallback} />
      )}

      {!displayConsentRequest && (
        <Button
          content="Create Documents"
          onClick={() => createDocumentsTaskModule()}
          primary
          loading={createDocumentMutation.isLoading}
        />
      )}
      {createDocumentMutation.isError && !displayConsentRequest && (
        <Alert
          header="Error"
          content={
            createDocumentMutation.error?.message ??
            'Something went wrong while creating your document'
          }
          danger
          visible
        />
      )}

      {createDocumentMutation.data && (
        <Alert header="Success" content="Document Created" success visible />
      )}
    </Flex>
  );
}
