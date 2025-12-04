import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';
import { FluentProvider, teamsDarkTheme } from '@fluentui/react-components'

ReactDOM.render(
  <React.StrictMode>
    <FluentProvider theme={teamsDarkTheme}>
      <App />
    </FluentProvider>
  </React.StrictMode>,
  document.getElementById('root')
);