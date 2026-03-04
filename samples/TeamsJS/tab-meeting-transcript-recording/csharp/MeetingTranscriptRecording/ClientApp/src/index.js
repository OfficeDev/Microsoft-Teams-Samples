import { FluentProvider, teamsLightTheme } from '@fluentui/react-components';
import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import { AppRoute } from './router/router';
import * as microsoftTeams from "@microsoft/teams-js";

microsoftTeams.app.initialize().then(() => {
    const rootElement = document.getElementById('root');
    
    if (rootElement) {
        const root = ReactDOM.createRoot(rootElement);
        root.render(
            <FluentProvider theme={teamsLightTheme}>
                <AppRoute />
            </FluentProvider>
        );
    } else {
        console.error("Root element not found");
    }
});
