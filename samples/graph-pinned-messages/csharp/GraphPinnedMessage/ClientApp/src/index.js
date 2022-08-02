import { Provider, teamsTheme } from '@fluentui/react-northstar';
import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import { AppRoute } from './router/router';

ReactDOM.render(
  <Provider theme={teamsTheme}>
    <React.StrictMode>
      <AppRoute />
    </React.StrictMode>
  </Provider>,
  document.getElementById('root')
);