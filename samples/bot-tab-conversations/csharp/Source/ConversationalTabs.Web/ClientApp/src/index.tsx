import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter } from 'react-router-dom';
import * as microsoftTeams from '@microsoft/teams-js';
import { Query, QueryClient, QueryClientProvider } from 'react-query';
import App from './App';
import { TeamsProvider } from 'utils/TeamsProvider';
import './index.css';
import reportWebVitals from './reportWebVitals';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      /* React Query will refetch if you refocus on a window.
        Due to Teams using iframes to host tabs, if a user clicks on Teams UI and then the app, that counts as a refocus.
        This can lead to a bad experience where:
          1. The user sees a spinner, then a request to consent
          2. The user clicks on the tab to start the consent
          3. However, this click counts as a window focus, leading to react query to refetch
          4. Leading to a user seeing a spinner followed by the same consent ask
        To prevent this we will only refetch on window focus only if we have previously got the data successfully. */
      refetchOnWindowFocus: (query: Query) => query.state.status === 'success',
    },
  },
});

ReactDOM.render(
  <React.StrictMode>
    <TeamsProvider microsoftTeams={microsoftTeams}>
      <BrowserRouter>
        <QueryClientProvider client={queryClient}>
          <App />
        </QueryClientProvider>
      </BrowserRouter>
    </TeamsProvider>
  </React.StrictMode>,
  document.getElementById('root'),
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
