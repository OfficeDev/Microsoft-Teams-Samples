// <copyright file="index.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import * as ReactDOM from "react-dom";
import { Provider, teamsTheme } from '@fluentui/react-northstar';
import App from "./app";
import { NotificationProvider } from "./providers/notification-provider";

ReactDOM.render(
    <React.StrictMode>
        <Provider theme={teamsTheme}>         
            <App />         
        </Provider>
    </React.StrictMode>,
    document.getElementById('root')
);