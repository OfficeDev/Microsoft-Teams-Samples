// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import ReactDOM from 'react-dom';
//import './index.css';
import App from './components/App';
import { FluentProvider,   teamsLightTheme } from '@fluentui/react-components' //https://fluentsite.z22.web.core.windows.net/quick-start
import "./style.css";

ReactDOM.render(
    <FluentProvider >
        <App />
    </FluentProvider>, document.getElementById('root')
);
