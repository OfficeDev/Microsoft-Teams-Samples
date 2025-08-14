// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import ReactDOM from 'react-dom/client';
//import './index.css';
import App from './components/App';
import { FluentProvider,   teamsLightTheme } from '@fluentui/react-components' //https://fluentsite.z22.web.core.windows.net/quick-start
import "./style.css";

const root = ReactDOM.createRoot(document.getElementById("root") as HTMLElement);  // ✅ Use createRoot
root.render(
    <FluentProvider theme={teamsLightTheme}>
        <App />
    </FluentProvider>
);
