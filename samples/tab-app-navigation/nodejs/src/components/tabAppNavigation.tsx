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
    let appId = "82a4d78a-68ed-4987-8527-0c255f1ce84c";
    
    React.useEffect(() => {
        app.initialize().then(app.getContext).then((context: any) => {
            app.notifySuccess();
        });
    });

    // Navigation between tabs within an app.
    const navigateBetweenTabs = () => {
        app.initialize().then(app.getContext).then((context: any) => {
            if (pages.currentApp.isSupported()) {
                const navPromise = pages.currentApp.navigateTo({ pageId: context.page.id, subPageId: context.page.subPageId });
                navPromise.
                    then((result) => console.log("Navigation Successfull", result)).
                    catch((error) => console.log("Navigation Failed", error));
            }
            else {
                const navPromise = pages.navigateToApp({
                    appId: appId, pageId: context.page.id
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
                    catch((error) => { console.log("error",error) });
            }
            else {
                console.log("Capability is not supported");
                const navPromise = pages.navigateToApp({ appId: appId, pageId: context.page.id, subPageId: context.page.subPageId });
                navPromise.
                    then((result) => { console.log("Navigation Successfull",result) }).
                    catch((error) => { console.log("error",error) });
            }
        });
    }

    // Object onfocus forward and backward with boolean value 
    const onFoucs = {
        forward: true,
        backward: false
    }

    // The returnFocus() function accepts a Boolean indicating the direction to advance focus within the host app
    // Boolean 'true' for forward and it highlights the search bar.
    const returnFocus_Forward = () => {
        pages.returnFocus(onFoucs.forward);
    }

    // Boolean 'false' for backwards highlights the app bar.
    const returnFocus_backward = () => {
        pages.returnFocus(onFoucs.backward);
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
        <div>
            <h3>Welcome To Tab App Navigation</h3>
            <br></br>
            <button onClick={navigateBetweenTabs}>Navigate Between Tabs</button>
            <br></br><br></br>
            <button onClick={onNavigateToDefaultTab}>Navigate To Default Page</button>
            <br></br><br></br>
            <button onClick={returnFocus_Forward}>Return Focus Forward</button>
            <br></br><br></br>
            <button onClick={returnFocus_backward}>Return Focus Backward</button>
            <br></br><br></br>
            <button onClick={backButtonNavigation}>Back Button Navigation</button>
        </div>
    );
};

export default TabAppNavigation;
