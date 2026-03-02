// <copyright file="configure.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { useEffect, useState } from "react";
import * as microsoftTeams from "@microsoft/teams-js";

// Configure page.
const Configure = props => {
    const [ entity, setEntity] = useState("");

    const updateSelection = (event) => {
        const selectedValue = event.target.value;
        if (selectedValue) {
            setEntity(selectedValue);
            microsoftTeams.pages.config.setConfig({
                entityId: selectedValue,
                contentUrl: `${window.location.origin}/appCacheTab?entityId=${selectedValue}`,
                suggestedDisplayName: `${selectedValue}-Tab`,
                websiteUrl: `${window.location.origin}/appCacheTab?entityId=${selectedValue}`,
            });
            microsoftTeams.pages.config.setValidityState(true);
        }
    };

    useEffect(() => {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.notifySuccess();

            microsoftTeams.pages.config.registerOnSaveHandler(function (saveEvent) {
                saveEvent.notifySuccess();
            });
        });
    }, []);
    return (
        <div>
            <h2>Select a page for your channel</h2>
            <select onChange={updateSelection} value={entity}>
                <option value="">Select</option>
                <option value="red">Red</option>
                <option value="green">Green</option>
                <option value="blue">Blue</option>
                <option value="yellow">Yellow</option>
            </select>
            <p>Please click save button to proceed.</p>
        </div>
    );
};

export default Configure;