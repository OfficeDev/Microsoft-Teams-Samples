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

    // Allows us to execute the React code when the component is already placed in the DOM.
    componentDidMount() {
        microsoftTeams.app.initialize();
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
                <div className="share-specific-part-button">
                    <div className="tag-container">
                        <h3>Share To Stage View</h3>
                        <Button appearance="primary" onClick={this.shareSpecificPart} >Share</Button>
                    </div>
                </div>
            </>
        )
    }
}
export default AppInMeeting