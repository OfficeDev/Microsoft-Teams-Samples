// <copyright file="app-in-meeting.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import $ from "jquery";
import Doing from "./doing";
import Todo from "./todo";
import Done from "./done";

// Handles redirection after successful/failure sign in attempt.
const AppInMeeting = props => {

    useEffect(() => {
        microsoftTeams.app.initialize();
        microsoftTeams.app.getContext().then((context) => {
            if (context.page.frameContext === "sidePanel") {
                // Adding and removing classes based on screen width, to show app in stage view and in side panel
                $("#todo, #doing, #done").addClass("grid-item-sidepanel");
                $("#todo, #doing, #done").removeClass("grid-item");
                $("#boardDiv").addClass("chat-window-sidepanel");
                $("#boardDiv").removeClass("chat-window");
            }
            else {
                // Adding and removing classes based on screen width, to show app in stage view and in side panel
                $("#todo, #doing, #done").addClass("grid-item");
                $("#todo, #doing, #done").removeClass("grid-item-sidepanel");
                $("#boardDiv").addClass("chat-window");
                $("#boardDiv").removeClass("chat-window-sidepanel");
            }
        });
    }, []);

    // Share the content to meeting stage view.
    const shareSpecificPart = (partName) => {
        var appContentUrl = "";
        microsoftTeams.app.getContext().then((context) => {
          appContentUrl = partName == 'todo' ? `${window.location.origin}/todoView?meetingId=${context.meeting.id}` : partName == 'doing' ? `${window.location.origin}/doingView?meetingId=${context.meeting.id}` : `${window.location.origin}/doneView?meetingId=${context.meeting.id}`;
          microsoftTeams.meeting.shareAppContentToStage((err, result) => {
            if (result) {
              // handle success
              console.log(result);
            }
            if (err) {
              // handle error
              alert(JSON.stringify(err))
            }
          }, appContentUrl);
        });
      };

    return (
        <div id="chatSection" className="chatSection">
            <div className="label">
                Sprint Status
            </div>
            <div id="boardDiv" className="chat-window">
                <div className="part-container">
                    <Todo shareSpecificPart={shareSpecificPart}/>
                </div>
                <div className="part-container">
                    <Doing shareSpecificPart={shareSpecificPart}/>
                </div>
                <div className="part-container">
                    <Done shareSpecificPart={shareSpecificPart}/>
                </div>
            </div>
        </div>
    );
};

export default AppInMeeting;