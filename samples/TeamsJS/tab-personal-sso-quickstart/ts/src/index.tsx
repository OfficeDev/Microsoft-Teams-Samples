// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import './index.css';
import App from './components/App';
import { FluentProvider, teamsLightTheme } from '@fluentui/react-components'; 
import ReactDOM from "react-dom/client";

const root = ReactDOM.createRoot(document.getElementById("root")!)
root.render(<App />);

