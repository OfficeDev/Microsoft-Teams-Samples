// <copyright file="dashboard.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { Component } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import axios from "axios";
import "../style/style.css";
import "../style/card.css";

// Dashboard where user can manage the tags
class Dashboard extends Component {
    constructor(props) {
        super(props);

        this.state = {
            teamId: "",
            notifications: [],
            teamsContext: {},
        }
    }

    componentDidMount() {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.getContext().then((context) => {
                this.setState({ teamId: context.team.groupId });
                this.setState({ teamName: context.team.displayName })
                this.initializeData(context.team.groupId);
            })
        });
    }

    initializeData = async (teamId) => {
        var response = await axios.post(`/api/Notifications/${teamId}`);
        if (response.status === 200) {
            var responseData = response.data;
            if (responseData) {
                var elements = [];
                responseData.forEach(item => {
                    elements.push(<div>
                        <p><b>Channel Name :</b> {item.displayName}</p>
                        <p><b>Status       :</b> Channel has {item.changeType}</p>
                    </div>);
                });
                if (elements.length > 0) {
                    this.setState({ notifications: elements });
                }
            }
        }
    }

    welcomeMessage = () => {
        return (
            <div>
                <h4>Welcome to Channel Notification Tab</h4>
                <p>This Tab has successfully configured, you will get notifications of channels delete/edit/create in this team</p>
            </div>
        );
    }

    render() {
        return (
            <div className="tag-container">
                <h3 className="headcolor">Channel Notifications</h3>
                <div>
                    {this.welcomeMessage()}
                    <hr></hr>
                    {this.state.notifications}
                </div>
            </div>
        )
    }
}

export default Dashboard;