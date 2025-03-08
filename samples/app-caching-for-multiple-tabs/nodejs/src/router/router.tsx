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
    const [appInitialized, setAppInitialized] = React.useState(false);

    React.useEffect(() => {
        if (!entityId && window.location.pathname === "/appCacheTab") {
            const params = new URLSearchParams(window.location.search);
            const routeEntityId = params.get("entityId");
            if (routeEntityId) {
                setEntityId(routeEntityId);
            }
        }
    }, [entityId]);

    React.useEffect(() => {
        // Initialize the Microsoft Teams SDK
        const app = microsoftTeams.app;

        app.initialize().then(() => {

            // Check if the framecontext is a cacheable one
            if (window.location.pathname === "/appCacheTab") {

                microsoftTeams.teamsCore.registerBeforeUnloadHandler((readyToUnload: any) => {
                    readyToUnload();
                    console.log("sending readyToUnload to TEAMS");
                    return true;
                });

                microsoftTeams.teamsCore.registerOnLoadHandler((data: any) => {
                    if (data.entityId) {
                        console.log("Load handler sending new entityId to TEAMS " + data.entityId);
                        setEntityId(data.entityId);
                    }
                });
            }

            setAppInitialized(true);
        }).catch(function (error: any) {
            console.error(error, "Could not initialize TeamsJS SDK.");
        });

        return () => {
            console.log("useEffect cleanup - Tab");
        };
    }, []);

    return (
        <React.Fragment>
            {appInitialized ? (
                <BrowserRouter>
                    <Routes>
                        <Route path="/" element={<Index />} />
                        <Route path="/configure" element={<Configure />}/>
                        <Route path="/appCacheTab" element={<AppCacheTab entityId={entityId} />}/>
                    </Routes>
                </BrowserRouter>) : null
            }
        </React.Fragment>
    );
};