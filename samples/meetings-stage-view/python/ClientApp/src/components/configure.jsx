// <copyright file="configure.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";

const Configure = props => {
    useEffect(() => {
        const initializeTeams = async () => {
            try {
                await microsoftTeams.app.initialize();

                microsoftTeams.pages.config.setValidityState(true);
                microsoftTeams.pages.config.registerOnSaveHandler(function (saveEvent) {
                    const baseUrl = window.location.origin;
                    
                    const configData = {
                        entityId: "App in Meeting Demo",
                        contentUrl: `${baseUrl}/appInMeeting`,
                        suggestedTabName: "App in meeting",
                        websiteUrl: `${baseUrl}/appInMeeting`,
                    };
                    
                    microsoftTeams.pages.config.setConfig(configData);
                    saveEvent.notifySuccess();
                });
                
                microsoftTeams.app.notifyAppLoaded();
                microsoftTeams.app.notifySuccess();
                
            } catch (error) {
                console.error("Error initializing Teams SDK:", error);
            }
        };

        initializeTeams();
    }, []);

    const onClick = () => {
        microsoftTeams.pages.config.setValidityState(true);
    }

    const containerStyle = {
        padding: '20px',
        fontFamily: 'Segoe UI, Tahoma, Geneva, Verdana, sans-serif'
    };

    return (
        <div style={containerStyle}>
            <h2>App Configuration</h2>
            <p>Configure your meeting app settings:</p>
            
            <div>
                <input 
                    type="radio" 
                    name="notificationType" 
                    value="Create" 
                    onClick={onClick} 
                    defaultChecked
                    id="createOption"
                />
                <label htmlFor="createOption" style={{marginLeft: '8px'}}>
                    Add App in a meeting
                </label>
            </div>
        </div>
    );
};

export default Configure;