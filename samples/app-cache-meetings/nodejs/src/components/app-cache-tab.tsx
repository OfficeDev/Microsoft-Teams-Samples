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
    readyToUnload: () => void) => {
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
    const [title] = useState("App Cache Testing Sample");
    const [initState] = useState(true);

    React.useEffect(() => {
        if (!initState) {
            return;
        }

        microsoftTeams.app.initialize().then(() => {

            try {
                microsoftTeams.teamsCore.registerBeforeUnloadHandler((readyToUnload) => {
                    const result = beforeUnloadHandler(readyToUnload);
                    return result;
                });

                microsoftTeams.teamsCore.registerOnLoadHandler((data) => {
                    loadHandler(setItems, data);
                    console.log(data.contentUrl, data.entityId);
                });

                const newItem = logItem("Handlers", "orange", "Registered load and before unload handlers. Ready for app caching.");
                setItems((Items) => [...Items, newItem]);

            }
            catch (error) {
                console.log(error, "could not registered handlers");
            }
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
