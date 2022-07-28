import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';
import { Provider, teamsTheme } from '@fluentui/react-northstar'

ReactDOM.render(
  <React.StrictMode>
    <Provider theme={teamsTheme}>
      <App />
    </Provider>
  </React.StrictMode>,
  document.getElementById('root')
);