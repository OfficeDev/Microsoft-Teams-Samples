import { Flex, Header } from '@fluentui/react-northstar';
import { useTeamsContext } from 'utils/TeamsProvider/hooks';
import { CreateDocumentButton } from 'components/CreateDocumentButton';
import styles from './TabContent.module.css';

/**
 * Content that is shown in the Meeting Tab
 * Includes the ability to open a Task Module to create a Document.
 *
 * @returns a component with a simple header and button to create a document
 */
export function TabContent() {
  const context = useTeamsContext();

  return (
    <Flex column className={styles.tabContent}>
      <Header
        as="h2"
        content="Meeting Signing, sharing to stage programmatically"
        description={{
          content: `FrameContext: ${context?.page.frameContext}`,
        }}
      />
      <CreateDocumentButton userIsAnonymous={false} />
    </Flex>
  );
}
