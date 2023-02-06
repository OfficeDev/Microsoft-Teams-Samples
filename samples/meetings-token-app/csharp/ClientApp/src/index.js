// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import ReactDOM from 'react-dom';
//import './index.css';
import App from './Components/App';
import { Provider } from '@fluentui/react-northstar' //https://fluentsite.z22.web.core.windows.net/quick-start
import "./style.css";

ReactDOM.render(
    <Provider >
        <App />
    </Provider>, document.getElementById('root')
);