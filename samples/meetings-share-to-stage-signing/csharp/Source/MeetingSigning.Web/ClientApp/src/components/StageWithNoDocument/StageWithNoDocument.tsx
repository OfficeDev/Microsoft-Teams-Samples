import { Flex, Button, Header } from '@fluentui/react-northstar';
import styles from './StageWithNoDocument.module.css';

export function StageWithNoDocument() {
  return (
    <>
      <Flex column className={styles.stageWithNoDocument}>
        <Header
          as="h1"
          content="That sharing button is not supported in this app"
        />
        <p>
          To share a document please select the{' '}
          <Button content="Share to meeting" /> on a Document from the panel on
          the side.
        </p>
      </Flex>
    </>
  );
}
