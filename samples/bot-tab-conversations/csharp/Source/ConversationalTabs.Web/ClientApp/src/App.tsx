import { Flex, FlexItem } from '@fluentui/react-northstar';
import { Route, Routes } from 'react-router-dom';
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

function App() {
  return (
    <Flex column>
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
            <Route
              path="support-department/:entityId/inquiry/:subEntityId"
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
