// <copyright file="app-in-meeting.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useState } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { Button } from '@fluentui/react-components';
import "../index.css";

const AppInMeeting = () => {
    let app = microsoftTeams.app;
    app.initialize();

    const [appTheme, setAppTheme] = useState("");
    const [result, setResult] = useState(false);

    // First callback
    // function with two
    // arguments error and data
    const callback = (errcode, bln) => {
        if (errcode) {
            setResult(JSON.stringify(errcode))
        }
        else {
            setResult(JSON.stringify(bln))
        }
    }

    /// <summary>
    /// This method getIncomingClientAudioState returns the current state of client audio.
    /// The incoming audio is muted if the result is true and unmuted if the result is false.
    /// </summary>
    const ClientAudioState = () => {
        microsoftTeams.meeting.getIncomingClientAudioState(callback);
    }

    /// <summary>
    /// This method toggleIncomingClientAudio which toggles mute/unmute to client audio.
    /// Setting for the meeting user from mute to unmute or vice-versa.
    /// </summary>
    const toggleState = () => {
        microsoftTeams.meeting.toggleIncomingClientAudio(callback);
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
                    alert(+JSON.stringify(err))
                }
            }, appContentUrl);
        });
    };

    React.useEffect(() => {
        app.initialize().then(app.getContext).then((context) => {
            app.notifySuccess();

            // Applying default theme from app context property
            switch (context.app.theme) {
                case 'dark':
                    setAppTheme("theme-dark");
                    break;
                case 'contrast':
                    setAppTheme("theme-contrast");
                    break;
                case 'default':
                    setAppTheme('theme-light');
                    break;
                default:
                    return setAppTheme('theme-dark');
            }

            // Handle app theme when 'Teams' theme changes
            microsoftTeams.app.registerOnThemeChangeHandler(function (theme) {
                switch (theme) {
                    case 'dark':
                        setAppTheme('theme-dark');
                        break;
                    case 'default':
                        setAppTheme('theme-light');
                        break;
                    case 'contrast':
                        setAppTheme('theme-contrast');
                        break;
                    default:
                        return setAppTheme('theme-light');
                }
            });
        }).catch(function (error) {
            console.log(error, "Could not register handlers.");
        });
    }, []);

    return (
        <div className={appTheme}>
            <div>
                <h3>Share To Stage View</h3>
                <Button appearance="primary" onClick={shareSpecificPart} >Share</Button>
            </div>
            <div>
                <h3>Mute/Unmute Audio Call </h3>
                <Button appearance="primary" onClick={toggleState} >Mute/Un-Mute</Button>
                <li className="break"> Mute State: <b>{result}</b></li>
            </div>
        </div>
    )
}

export default AppInMeeting