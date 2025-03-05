// <copyright file="router.js" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import {
    BrowserRouter,
    Route,
    Routes
} from 'react-router-dom';
import * as microsoftTeams from "@microsoft/teams-js";
import Configure from "../components/configure";
import AppCacheTab from "../components/app-cache-tab";
import Index from "../components/index";

export const AppRoute = () => {
    const [entityId, setEntityId] = React.useState<string>("");

    React.useEffect(() => {
        // Initialize the Microsoft Teams SDK
        const app = microsoftTeams.app;

        app.initialize().then(app.getContext).then((context: any) => {

            // Check if the framecontext is a cacheable one
            if (context.page.frameContext === "sidePanel" || context.page.frameContext === "content") {

                microsoftTeams.teamsCore.registerBeforeUnloadHandler((readyToUnload: any) => {
                    readyToUnload();
                    console.log("sending readyToUnload to TEAMS");
                    return true;
                });

                microsoftTeams.teamsCore.registerOnLoadHandler((data: any) => {
                    if (data.entityId !== entityId) {
                        console.log("Load handler sending new entityId to TEAMS " + data.entityId);
                        setEntityId(data.entityId);
                    }
                });
            }
        }).catch(function (error: any) {
            console.error(error, "Could not initialize TeamsJS SDK.");
        });
        
        if (!entityId) {
            const params = new URLSearchParams(window.location.search);
            const entityId = params.get("entityId");
            if (entityId) {
                setEntityId(entityId);
            }
        }
        return () => {
            console.log("useEffect cleanup - Tab");
        };
    }, []);

    return (
        <React.Fragment>
            <BrowserRouter>
                <Routes>
                    <Route path="/" element={<Index />} />
                    <Route path="/configure" element={<Configure />}/>
                    <Route path="/appCacheTab" element={<AppCacheTab entityId={entityId} />}/>
                </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};