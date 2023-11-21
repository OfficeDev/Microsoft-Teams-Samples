// <copyright file="tabTwo.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { pages } from "@microsoft/teams-js";

function TabTwo() {
    let app = microsoftTeams.app;

    React.useEffect(() => {
        app.initialize().then(app.getContext).then((context: any) => {
            app.notifySuccess();
        });
    });

    // Navigation between tabs within an app.
    const navigateBetweenTabs = () => {
        app.initialize().then(app.getContext).then((context: any) => {
            if (pages.currentApp.isSupported()) {
                const navPromise = pages.currentApp.navigateTo({ pageId: "tab_three", subPageId: "" });
                navPromise.
                    then((result) => console.log("Navigation Successfull", result)).
                    catch((error) => console.log("Navigation Failed", error));
            }
            else {
                alert("Not Supported");
                const navPromise = pages.navigateToApp({
                    appId: "", pageId: "tab_three"
                });
                navPromise.
                    then((result) => console.log("Navigation Successfull", result)).
                    catch((error) => console.log("Navigation Failed", error));
            }
        });
    }

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
                alert("Capability is not supported");
                const navPromise = pages.navigateToApp({ appId: "", pageId: "default_tab", subPageId: "" });
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
            <h2>Tab Two</h2>
            <button onClick={navigateBetweenTabs}>Tab-2 To Tab-3</button>
            <br></br><br></br>
            <button onClick={onNavigateToDefaultTab}>Default Tab</button>
            <br></br><br></br>
            <button onClick={backButtonNavigation}>Back Button Navigation</button>
        </div>
    );
};

export default TabTwo;
