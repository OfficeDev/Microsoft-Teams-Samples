import { Flex } from '@fluentui/react-northstar';
import { Outlet } from 'react-router-dom';
import styles from './Container.module.css';

function Container() {

  return (
    <Flex column className={styles.container}>
      <Outlet />
    </Flex>
  );
}

export { Container };
