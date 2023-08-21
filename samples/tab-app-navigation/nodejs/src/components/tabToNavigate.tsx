// <copyright file="tabToNavigate.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React from "react";
import * as microsoftTeams from "@microsoft/teams-js";

function TabToNavigation() {
    let app = microsoftTeams.app;

    React.useEffect(() => {
        app.initialize().then(app.getContext).then((context: any) => {
            app.notifySuccess();
        });
    });

    return (
        <div>
            <h3>Hello and Welcome !!</h3>
        </div>
    );
};

export default TabToNavigation;
