// <copyright file="tabThree.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { pages } from "@microsoft/teams-js";

function TabThree() {
    let app = microsoftTeams.app;
    let externalAppId = "<<External-App-Id>>"; // Optional - you can get the external appId from teams admin portal.

    React.useEffect(() => {
        app.initialize().then(app.getContext).then((context: any) => {
            app.notifySuccess();
        });
    });

    // Navigate to default tab
    const onNavigateToDefaultTab = () => {
        app.initialize().then(app.getContext).then((context) => {
            if (pages.currentApp.isSupported()) {
                const navPromise = pages.currentApp.navigateToDefaultPage();
                navPromise.
                    then((result) => { console.log("This is Default Page") }).
                    catch((error) => { console.log("error", error) });
            }
            else {
                console.log("Capability is not supported");
                const navPromise = pages.navigateToApp({ appId: externalAppId, pageId: "default_tab", subPageId: "" });
                navPromise.
                    then((result) => { console.log("Navigation Successfull", result) }).
                    catch((error) => { console.log("error", error) });
            }
        });
    }

    // Back button navigation
    const backButtonNavigation = () => {
        if (pages.backStack.isSupported()) {
            pages.backStack.navigateBack();
        }
        else {
            console.log("Capability is not supported")
        }
    }

    return (
        <div className="container">
            <h2>Tab Three</h2>
            <button onClick={onNavigateToDefaultTab}>Default Tab</button>
            <br></br><br></br>
            <button onClick={backButtonNavigation}>Back Button Navigation</button>
        </div>
    );
};

export default TabThree;
