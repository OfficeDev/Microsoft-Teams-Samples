import { Alert, Button, Flex, Header } from '@fluentui/react-northstar';
import * as microsoftTeams from '@microsoft/teams-js';
import { TaskInfo } from '@microsoft/teams-js';
import * as ACData from 'adaptivecards-templating';
import { CreateDocumentCard } from 'adaptive-cards';
import documentApi from 'api/documentApi';
import { useTeamsContext } from 'utils/TeamsProvider/hooks';
import { useApi } from 'hooks';
import { DocumentInput, DocumentType, User } from 'models';
import styles from './TabContent.module.css';

type Choice = {
  name: string;
  value: string;
};

/**
 * Content that is shown in the Meeting Tab
 * Includes the ability to open a Task Module to create a Document.
 *
 * @returns a component with a simple header and button to create a document
 */
export function TabContent() {
  const context = useTeamsContext();
  const createDocumentApi = useApi(documentApi.createDocument);

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
    const createDocumentsSubmitHandler = (error: string, result: any) => {
      if (error !== null) {
        console.log(`Document handler - error: '${error}'`);
      } else if (result !== undefined) {
        const documents: string[] = result.documentsValue.split(',');
        const viewers: User[] = result.viewersValue
          .split(',')
          .map((s: string) => {
            return { userId: s, name: '' };
          });
        const signers: User[] = result.signersValue
          .split(',')
          .map((s: string) => {
            return { userId: s, name: '' };
          });

        documents.forEach(async (d: string) => {
          const documentInput: DocumentInput = {
            documentType: DocumentType[d as keyof typeof DocumentType],
            viewers: viewers,
            signers: signers,
          };

          await createDocumentApi.request(documentInput);
        });
      }
    };

    microsoftTeams.tasks.startTask(
      createTaskInfo(documentsCard),
      createDocumentsSubmitHandler,
    );
  };

  return (
    <Flex column={true} className={styles.tabContent}>
      <Header
        as="h2"
        content="Meeting Signing, sharing to stage programmatically"
        description={{
          content: `FrameContext: ${context?.frameContext}`,
        }}
      />
      <Button
        content="Create Documents"
        onClick={() => createDocumentsTaskModule()}
        primary
      />
      {createDocumentApi.error && (
        <Alert
          header="Error"
          content={createDocumentApi.error}
          danger
          visible
        />
      )}
      {createDocumentApi.data && (
        <Alert header="Success" content="Document Created" success visible />
      )}
    </Flex>
  );
}
