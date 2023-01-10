// <copyright file="app-in-meeting.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";

// Handles redirection after successful/failure sign in attempt.
const AppInMeeting = props => {

    const RaiseHandAPI = () => {
        alert("test");
        microsoftTeams.meeting.registerRaiseHandStateChangedHandler(
            eventData => {
                alert(JSON.stringify(eventData));
                alert(JSON.stringify(eventData.error));

                if (eventData.error) {
                    // handle error 
                    setApiResult(
                        `error: ${JSON.stringify(eventData.error)}`
                    );
                }
                setApiResult(
                    `raiseHandState: ${JSON.stringify(
                        eventData.raiseHandState

                    )}`
                );
            }
        );
    }

    return (
        <div id="chatSection" className="chatSection">
            <div className="label">
                Sprint Status
            </div>
            <div id="boardDiv" className="chat-window">
                <button onClick={RaiseHandAPI}>Raise Hand API</button>
            </div>
        </div>
    );
};

export default AppInMeeting;