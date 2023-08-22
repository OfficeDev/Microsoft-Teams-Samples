// <copyright file="tabToNavigate.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { pages } from "@microsoft/teams-js";

function TabToNavigation() {
    let app = microsoftTeams.app;

    React.useEffect(() => {
        app.initialize().then(app.getContext).then((context: any) => {
            app.notifySuccess();
        });
    });

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
            <h3>Hello and Welcome !!</h3>
            <br></br>
            <button onClick={backButtonNavigation}>Back To Main Tab</button>
        </div>
    );
};

export default TabToNavigation;
