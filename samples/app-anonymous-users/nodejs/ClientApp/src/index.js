// <copyright file="index.js" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { FluentProvider, teamsLightTheme } from '@fluentui/react-components';
import React from "react";
// import ReactDOM from "react-dom";
import { createRoot } from "react-dom/client";
import './index.css';
import { AppRoute } from './App';

const root = createRoot(document.getElementById("root"));
root.render(
    <FluentProvider theme={teamsLightTheme}>
        <AppRoute />
    </FluentProvider>
);