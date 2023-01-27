// <copyright file="app-in-meeting.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { Component } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { Button } from '@fluentui/react-components';

class AppInMeeting extends Component {
    constructor(props) {
        super(props)
        this.state = {
            errorCode: "",
            result: ""
        }
    }

    //Allows us to execute the React code when the component is already placed in the DOM.
    componentDidMount() {
        microsoftTeams.app.initialize();
    }

    // First callback
    // function with two
    // arguments error and data
    callback = (errcode, bln) => {
        if (errcode) {
            this.setState({ errorCode: JSON.stringify(errcode) })
        }
        else {
            this.setState({ result: JSON.stringify(bln) })
        }
    }

    /// <summary>
    /// This method getIncomingClientAudioState returns the current state of client audio.
    /// The incoming audio is muted if the result is true and unmuted if the result is false.
    /// </summary>
    ClientAudioState = () => {
        microsoftTeams.meeting.getIncomingClientAudioState(this.callback);
    }

    /// <summary>
    /// This method toggleIncomingClientAudio which toggles mute/unmute to client audio.
    /// Setting for the meeting user from mute to unmute or vice-versa.
    /// </summary>
    toggleState = () => {
        microsoftTeams.meeting.toggleIncomingClientAudio(this.callback);
    }

    // Share the content to meeting stage view.
    shareSpecificPart = () => {
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

    render() {
        return (
            <>
                <div>
                    <div className="tag-container">
                        <h3>Share To Stage View</h3>
                        <Button appearance="primary" onClick={this.shareSpecificPart} >Share</Button>
                    </div>
                </div>
                <div>
                    <div className="tag-container">
                        <h3>Mute/Unmute Audio Call </h3>
                        <Button appearance="primary" onClick={this.toggleState} >Mute/Un-Mute</Button>
                        <li className="break"> Mute State: <b>{this.state.result}</b></li>
                    </div>
                </div>
            </>
        )
    }
}
export default AppInMeeting