// <copyright file="app-cache-tab.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { useState } from "react";
import React from "react";
import * as microsoftTeams from "@microsoft/teams-js";

/// </summary>
/// function logitem
/// </summary>
function logItem(action: string, actionColor: string, message: string) {
    const newItem =
        "<span style='font-weight:bold;color:" +
        actionColor +
        "'>" +
        action +
        "</span> " +
        message +
        "</br>";
    return newItem;
}

/// </summary>
/// In beforeUnloadHandler using setItems and readyToUnload callback function
/// </summary>
const beforeUnloadHandler = (
    setItems: React.Dispatch<React.SetStateAction<string[]>>,
    readyToUnload: () => void) => {

    let newItem = logItem("OnBeforeUnload", "purple", "Started");
    setItems((Items) => [...Items, newItem]);

    // dispose resources and cleanup
    newItem = logItem("OnBeforeUnload", "purple", "Completed");
    setItems((Items) => [...Items, newItem]);

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
    logItem("OnLoad", "blue", "Started for " + data.entityId);

    let newItem = logItem("OnLoad", "blue", "Completed for " + data.entityId);
    setItems((Items) => [...Items, newItem]);

    microsoftTeams.app.notifySuccess();
};

const AppCacheTab = () => {
    const [items, setItems] = useState<string[]>([]);
    const [title, setTitle] = useState("App Cache Sample");
    const [initState] = useState(true);

    React.useEffect(() => {
        if (!initState) {
            return;
        }

        let app = microsoftTeams.app;
        app.initialize().then(app.getContext).then((context) => {
            app.notifySuccess();

            // check condition of framecontext to sidepanel
            if (context.page.frameContext === "sidePanel") {
                const loadContext = logItem("Success", "green", "Loaded Teams context");
                setItems((Items) => [...Items, loadContext]);

                const newLogItem = logItem("FrameContext", "orange", "Frame context is " + context.page.frameContext);
                setItems((Items) => [...Items, newLogItem]);

                microsoftTeams.teamsCore.registerBeforeUnloadHandler((readyToUnload) => {
                    const result = beforeUnloadHandler(setItems, readyToUnload);
                    return result;
                });

                microsoftTeams.teamsCore.registerOnLoadHandler((data) => {
                    loadHandler(setItems, data);
                    setTitle("Entity Id : " + data.entityId);
                    console.log(data.contentUrl, data.entityId);
                });

                const newItem = logItem("Handlers", "orange", "Registered load and before unload handlers. Ready for app caching.");
                setItems((Items) => [...Items, newItem]);
            }

        }).catch(function (error) {
            console.log(error, "could not register handlers");
        });

        return () => {
            console.log("useEffect cleanup - Tab");
        };

    }, [initState]);

    const jsx = initState ? (
        <div>
            <h3>{title}</h3>
            {items.map((item) => {
                return <div dangerouslySetInnerHTML={{ __html: item }} />;
            })}
        </div>
    ) : (
        <div style={{ color: "white" }}>loading</div>
    );
    return jsx;
};

export default AppCacheTab;
