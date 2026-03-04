// <copyright file="app-cache-tab.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { useState } from "react";
import React from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import "../components/index.css";

/// </summary>
///  appendLog to show the activity log
/// </summary>
function appendLog(message: string, color: string, addNewLine: boolean = true): string {
    return ("<span style='font-weight:bold;color:" +
        color +
        "'>" +
        message +
        "</span> " +
        "</br>" +
        (addNewLine ? "<br/>" : ""));
}

const AppCacheTabInner = (props: {entityId: string, displayLogs: string[], appTheme: string}) => {
    const {entityId, displayLogs, appTheme} = props;
    return (
        <div className={appTheme} >
            <div className={`tab-entity-${entityId}`}>
                <h3>{entityId}</h3>
                {displayLogs.map((item) => {
                    return <div dangerouslySetInnerHTML={{ __html: item }} />;
                })}
            </div>
        </div>
    );
};

const AppCacheTab = (props: {entityId: string}) => {

    const {entityId} = props;
    const [displayLogs, setDisplayLogs] = useState<string[]>([]);
    const [appTheme, setAppTheme] = useState('theme-light');

    React.useEffect(() => {
        const app = microsoftTeams.app;
        if (entityId) {
            app.getContext().then((context: any) => {
                // Get default theme from app context and set app-theme
                let defaultTheme = context.app.theme;
                switch (defaultTheme) {
                    case 'dark':
                        setAppTheme("theme-dark");
                        break;
                    default:
                        setAppTheme('theme-light');
                }

                // Handle app theme when 'Teams' theme changes
                microsoftTeams.app.registerOnThemeChangeHandler(function (theme) {
                    switch (theme) {
                        case 'dark':
                            setAppTheme('theme-dark');
                            break;
                        case 'default':
                            setAppTheme('theme-light');
                            break;
                        case 'contrast':
                            setAppTheme('theme-contrast');
                            break;
                        default:
                            return setAppTheme('theme-dark');
                    }
                });

                app.notifySuccess();
                setDisplayLogs((displayLogs) => [...displayLogs, appendLog(`Tab ${entityId} mounted`, entityId)]);
            });
        }

        return () => {
            console.log("useEffect cleanup - AppCacheTab");
            if (entityId) {
                setDisplayLogs((displayLogs) => [...displayLogs, appendLog(`Tab ${entityId} unmounted`, entityId, true)]);
            }
        }
    }, [entityId]);

    return appTheme && entityId ? <AppCacheTabInner entityId={entityId} displayLogs={displayLogs} appTheme={appTheme} /> : <div className="loading" />;
};

export default AppCacheTab;
