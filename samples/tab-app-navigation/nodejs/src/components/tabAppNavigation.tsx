// <copyright file="tabAppNavigation.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { pages } from "@microsoft/teams-js";
import './index.css';

function TabAppNavigation() {
    let app = microsoftTeams.app;
    let externalAppId = "<<External-App-Id>>"; // Optional - you can get the external appId from teams admin portal.


    React.useEffect(() => {
        app.initialize().then(app.getContext).then((context: any) => {
            app.notifySuccess();
        });
    });

    // Navigation between tabs within an app.
    const navigateBetweenTabs = () => {
        app.initialize().then(app.getContext).then((context: any) => {
            if (pages.currentApp.isSupported()) {
                const navPromise = pages.currentApp.navigateTo({ pageId: "tab_One", subPageId: "" });
                navPromise.
                    then((result) => console.log("Navigation Successfull", result)).
                    catch((error) => console.log("Navigation Failed", error));
            }
            else {
                const navPromise = pages.navigateToApp({
                    appId: externalAppId, pageId: "tab_one" 
                });
                navPromise.
                    then((result) => console.log("Navigation Successfull", result)).
                    catch((error) => console.log("Navigation Failed", error));
            }
        });
    }

    return (
        <div className="container">
            <h2>Tab App Navigation - Navigate Between Tabs </h2>
            <br></br>
            <button onClick={navigateBetweenTabs}>Navigate Between Tabs</button>
        </div>
    );
};

export default TabAppNavigation;
