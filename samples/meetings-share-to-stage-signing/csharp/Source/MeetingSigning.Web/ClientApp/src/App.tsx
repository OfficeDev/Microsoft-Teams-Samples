import { Outlet, Route, Routes } from 'react-router-dom';
import { Flex, FlexItem } from '@fluentui/react-northstar';
import { useDefaultColorScheme } from 'hooks';
import Home from 'routes/Home';
import { DocumentStage } from 'components/DocumentStage';
import Configure from 'routes/Configure';
import { AuthStartAad, AuthStartMsa } from 'routes/AuthStart';
import { AuthEndAad, AuthEndMsa } from 'routes/AuthEnd';

export default function App() {
  const colorScheme = useDefaultColorScheme();
  const appInlineStyles = { backgroundColor: colorScheme.background };

  return (
    <Flex column styles={appInlineStyles}>
      <FlexItem>
        <Routes>
          <Route path="/" element={<Outlet />}>
            <Route index element={<Home />} />
            <Route path="configure" element={<Configure />} />
            <Route path="stage/:documentId" element={<DocumentStage />} />
            <Route path="auth-start/aad" element={<AuthStartAad />} />
            <Route path="auth-start/msa" element={<AuthStartMsa />} />
            <Route path="auth-end/aad" element={<AuthEndAad />} />
            <Route path="auth-end/msa" element={<AuthEndMsa />} />
          </Route>
        </Routes>
      </FlexItem>
    </Flex>
  );
}
