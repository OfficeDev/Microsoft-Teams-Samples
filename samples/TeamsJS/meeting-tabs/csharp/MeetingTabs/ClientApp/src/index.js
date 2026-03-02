// <copyright file="index.js" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { FluentProvider, teamsLightTheme } from '@fluentui/react-components';
import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import { AppRoute } from './router/router';

ReactDOM.render(
    <FluentProvider theme={teamsLightTheme}>
        <AppRoute />
    </FluentProvider>, document.getElementById('root')
);
