// <copyright file="app-cache-tab.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { useState } from "react";
import React from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import axios from 'axios';
import '../index.css';
const querystring = require('querystring');

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
};

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
    const [items, setItems] = useState<string[]>([]);
    const [title, setTitle] = useState("App Caching Sample");
    const [searchText] = useState('Angular');
    const [apiData, setApiData] = useState<string[]>([]);

    React.useEffect(() => {
        let app = microsoftTeams.app;
        app.initialize().then(app.getContext).then((context: any) => {
            app.notifySuccess();

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

                // Fetch NPM packages using npm api
                const fetchNPMApiData = async (searchText_param: any) => {
                    const result = await axios.get(`https://registry.npmjs.com/-/v1/search?${querystring.stringify({ text: searchText_param, size: 8 })}`);

                    if (result) {
                        app.notifyAppLoaded(); //stop loading when api data is fetched
                        const npmPackages = result.data.objects;
                        Object.values(npmPackages).map((value, index) => {
                            setApiData((apiData) => [...apiData, (npmPackages[index].package.name)]);
                        });
                    }
                }

                fetchNPMApiData(searchText);
            }

        }).catch(function (error: any) {
            console.log(error, "Could not register handlers.");
        });

        return () => {
            console.log("useEffect cleanup - Tab");
        };

    }, []);

    return (
        <div>
            <h3>{title}</h3>
            {items.map((item) => {
                return <div dangerouslySetInnerHTML={{ __html: item }} />;
            })}
            <div className="packages">
                <h3>Most NPM package's download</h3>

                {apiData.map((item: any) => {
                    return <li dangerouslySetInnerHTML={{ __html: item }} />;
                })}
            </div>
        </div>
    );
};

export default AppCacheTab;
