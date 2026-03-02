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

/// </summary>
/// In beforeUnloadHandler using setItems and readyToUnload callback function
/// </summary>
const beforeUnloadHandler = (
    setItems: React.Dispatch<React.SetStateAction<string[]>>,
    readyToUnload: () => void) => {
    setItems((Items) => [...Items, logItem("OnBeforeUnload", "purple", "Started")]);

    // Dispose resources and cleanup
    // Write your custom code to perform resource cleanup.
    setItems((Items) => [...Items, logItem("OnBeforeUnload", "purple", "Dispose resources and cleanup")]);
    setItems((Items) => [...Items, logItem("OnBeforeUnload", "purple", "Completed")]);
    console.log("sending readyToUnload to TEAMS");
    readyToUnload();

    return true;
};

/// </summary>
/// loadHandler using setItems to set values
/// </summary>
const loadHandler = (
    setItems: React.Dispatch<React.SetStateAction<string[]>>,
    data: microsoftTeams.LoadContext) => {
    setItems((Items) => [...Items, logItem("OnLoad", "blue", "Started for " + data.entityId)]);

    // Use the entityId, contentUrl to route to the correct page within your App and then invoke notifySuccess
    setItems((Items) => [...Items, logItem("OnLoad", "blue", "Route to specific page")]);
    setItems((Items) => [...Items, logItem("OnLoad", "blue", "Completed for " + data.entityId)]);
    microsoftTeams.app.notifySuccess();
};

const AppCacheTab = () => {
    let app = microsoftTeams.app;
    const [items, setItems] = useState<string[]>([]);
    const [title, setTitle] = useState("App Caching Sample");
    const [appTheme, setAppTheme] = useState("");

    React.useEffect(() => {
        app.initialize().then(app.getContext).then((context: any) => {

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

            // Check if the framecontext is set to sidepanel
            if (context.page.frameContext === "sidePanel") {
                const loadContext = logItem("Success", "green", "Loaded Teams context");
                setItems((Items) => [...Items, loadContext]);

                const newLogItem = logItem("FrameContext", "orange", "Frame context is " + context.page.frameContext);
                setItems((Items) => [...Items, newLogItem]);

                microsoftTeams.teamsCore.registerBeforeUnloadHandler((readyToUnload: any) => {
                    const result = beforeUnloadHandler(setItems, readyToUnload);

                    return result;
                });

                microsoftTeams.teamsCore.registerOnLoadHandler((data: any) => {
                    loadHandler(setItems, data);
                    setTitle("Entity Id : " + data.entityId);
                    console.log(data.contentUrl, data.entityId);
                });

                const newItem = logItem("Handlers", "orange", "Registered load and before unload handlers. Ready for app caching.");
                setItems((Items) => [...Items, newItem]);
            }

            // Notify success only after all handlers have been registered
            app.notifySuccess();

        }).catch(function (error: any) {
            console.log(error, "Could not register handlers.");
        });

        return () => {
            console.log("useEffect cleanup - Tab");
        };

    }, []);

    return (
        <div className={appTheme}>
            <h3>{title}</h3>
            {items.map((item) => {
                return <div dangerouslySetInnerHTML={{ __html: item }} />;
            })}
        </div>
    );
};

export default AppCacheTab;
