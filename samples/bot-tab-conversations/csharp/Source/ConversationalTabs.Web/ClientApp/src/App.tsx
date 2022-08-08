import { Flex, FlexItem } from '@fluentui/react-northstar';
import { useEffect } from 'react';
import { Route, Routes, useLocation } from 'react-router-dom';
import {
  Admin,
  AuthEnd,
  AuthStart,
  Configure,
  Container,
  Home,
  InquirySubEntityTab,
  PersonalTab,
  SupportDepartmentChannelTab,
} from 'routes';
import styles from './App.module.css';

const titles = [
  {
    path: '/admin',
    title: 'External Admin Service | '
  },
];

function App() {
  const location = useLocation();
  useEffect(() => {
    document.title = (titles.find(item => item.path === location.pathname)?.title ?? '') + 'Tab Conversations Proof-of-Concept'
  }, [location]);

  return (
    <Flex column className={styles.app}>
      <FlexItem>
        <Routes>
          <Route path="/" element={<Container />}>
            <Route index element={<Home />} />
            <Route path="configure" element={<Configure />} />
            <Route path="personal" element={<PersonalTab />} />
            <Route
              path="support-department/:entityId"
              element={<SupportDepartmentChannelTab />}
            />
            {/* Sub entities can be sourced from either support-department or personal */}
            <Route
              path=":source/:entityId/inquiry/:subEntityId"
              element={<InquirySubEntityTab />}
            />
            <Route path="admin" element={<Admin />} />
            <Route path="auth-start" element={<AuthStart />} />
            <Route path="auth-end" element={<AuthEnd />} />
          </Route>
        </Routes>
      </FlexItem>
    </Flex>
  );
}

export default App;
