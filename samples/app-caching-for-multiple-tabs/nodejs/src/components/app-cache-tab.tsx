// <copyright file="app-cache-tab.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { useState } from "react";
import React from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import "../components/index.css";

/// </summary>
///  logitem to show the activity log
/// </summary>
function logItem(action: string, actionColor: string, message: string) {
    return ("<span style='font-weight:bold;color:" +
        actionColor +
        "'>" +
        action +
        "</span> " +
        message +
        "</br>");
}

const AppCacheTabInner = (props: {entityId: string, items: string[], appTheme: string}) => {
    const {entityId, items, appTheme} = props;
    return (
        <div className={appTheme} >
            <div className={`tab-entity-${entityId}`}>
                <h3>{entityId}</h3>
                {items.map((item) => {
                    return <div dangerouslySetInnerHTML={{ __html: item }} />;
                })}
            </div>
        </div>
    );
};

const AppCacheTab = (props: {entityId: string}) => {

    const {entityId} = props;
    const [items, setItems] = useState<string[]>([]);
    const [appTheme, setAppTheme] = useState('theme-light');

    React.useEffect(() => {
        const app = microsoftTeams.app;
        if (entityId) {
            setItems((Items) => [...Items, logItem("AppCacheTab mounting", "blue", `started for entity ${entityId}`)]);
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
            });
        }

        return () => {
            console.log("useEffect cleanup - AppCacheTab");
            setItems((Items) => [...Items, logItem("AppCacheTab unmounting", "purple", `started for entity ${entityId}`)]);
        }
    }, [entityId]);

    return appTheme && entityId ? <AppCacheTabInner entityId={entityId} items={items} appTheme={appTheme} /> : <div className="loading" />;
};

export default AppCacheTab;
