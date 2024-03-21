// <copyright file="app-in-meeting.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect, useState } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { Button } from '@fluentui/react-components';

const AppInMeeting = () => {

    // Theme change variables
    const [appTheme, setAppTheme] = useState("");

    // Allows us to execute the React code when the component is already placed in the DOM.
    useEffect(() => {
        (async function () {
            await microsoftTeams.app.initialize();
        })();
    }, []);

    componentDidMount = () => {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.getContext().then((context) => {

                // Applying default theme from app context property
                switch (context.app.theme) {
                    case 'dark':
                        setAppTheme("theme-dark");
                        break;
                    case 'contrast':
                        setAppTheme("theme-contrast");
                        break;
                    case 'default':
                        setAppTheme("theme-light");
                        break;
                }
            }).bind(this);
        });
    }

    componentDidUpdate = () => {
        // handle theme when Teams theme changes to dark,light and contrast.
        microsoftTeams.app.registerOnThemeChangeHandler(function (theme) {
            switch (theme) {
                case 'dark':
                    setAppTheme("theme-dark");
                    break;
                case 'contrast':
                    setAppTheme("theme-contrast");
                    break;
                case 'default':
                    setAppTheme("theme-light");
                    break;
            }
        }.bind(this));
    }

    // Share the content to meeting stage view.
    const shareSpecificPart = () => {
        var appContentUrl = "";
        microsoftTeams.app.getContext().then((context) => {
            appContentUrl = `${window.location.origin}/shareview?meetingId=${context.meeting.id}`;
            microsoftTeams.meeting.shareAppContentToStage((err, result) => {
                if (result) {
                    // handle success
                    console.log("Shared successfully!");
                }

                if (err) {
                    // handle error
                    alert('Error while sharing stage view' + JSON.stringify(err))
                }
            }, appContentUrl);
        });
    }

    return (
        <div className={appTheme}>
            <div className="share-specific-part-button">
                <div className="tag-container">
                    <h3>Share To Stage View</h3>
                    <Button appearance="primary" onClick={shareSpecificPart} >Share</Button>
                </div>
            </div>
        </div>
    );
};

export default AppInMeeting;
