import { Outlet, Route, Routes } from 'react-router-dom';
import { Flex, FlexItem } from '@fluentui/react-northstar';
import { useDefaultColorScheme } from 'hooks';
import { AuthProvider } from './utils/AuthProvider';
import Home from 'routes/Home';
import { DocumentStage } from 'components/DocumentStage';
import Configure from 'routes/Configure';
import AuthStart from 'routes/AuthStart';
import AuthEnd from 'routes/AuthEnd';

export default function App() {
  const colorScheme = useDefaultColorScheme();
  const appInlineStyles = { backgroundColor: colorScheme.background };

  return (
    <AuthProvider>
      <Flex column={true} styles={appInlineStyles}>
        <FlexItem>
          <Routes>
            <Route path="/" element={<Outlet />}>
              <Route index element={<Home />} />
              <Route path="configure" element={<Configure />} />
              <Route path="stage/:documentId" element={<DocumentStage />} />
              <Route path="auth-start" element={<AuthStart />} />
              <Route path="auth-end" element={<AuthEnd />} />
            </Route>
          </Routes>
        </FlexItem>
      </Flex>
    </AuthProvider>
  );
}
