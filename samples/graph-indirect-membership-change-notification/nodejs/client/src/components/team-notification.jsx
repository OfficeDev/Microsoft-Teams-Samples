// <copyright file="team-notification.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { Component } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import axios from "axios";
import moment from 'moment'
import "../style/style.css";

class TeamChangeNotification extends Component {
    constructor(props) {
        super(props);

        this.state = {
            teamId: "",
            notifications: [],
            teamsContext: {},
            pollingInterval: null
        }
    }

    /// </summary>
    /// ComponentDidMount function call On Page Load
    /// </summary>
    componentDidMount() {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.getContext().then((context) => {
                this.setState({ teamId: context.team.groupId });
                this.initializeData(context.team.groupId);
            }).catch(err => {
                console.error("Failed to get context:", err);
            });
        }).catch(err => {
            console.error("Teams SDK failed to initialize:", err);
        });
    }
    /// </summary>
    /// <param name="teamId"></param>
    /// </summary>
    initializeData = async (teamId) => {
        await axios.post(`/api/changeNotification/team?teamId=${teamId}`);
        await this.fetchNotifications();
    }

    fetchNotifications = async () => {
        try {
            const response = await axios.post('/api/notifications');
            if (response.data) {
                this.processNotificationData(response.data);
            }
        } catch (e) {
            console.error("Error fetching notifications:", e);
        }
    }

    processNotificationData = (responseData) => {
        const elements = [];

        responseData.forEach(item => {
            elements.push(
                <div key={item.createdDate}>
                    <p><b>Team Name:</b> {item.displayName}</p>
                    {(() => {
                        if (item.changeType === 'updated') {
                            return (
                                <div>
                                    <p><b>Description:</b> Team name has been Renamed</p>
                                    <p><b>Event Type:</b> <span className="statusColor"><b>{item.changeType}</b></span></p>
                                </div>
                            );
                        }

                        if (item.changeType === 'deleted') {
                            return (
                                <div>
                                    <p><b>Description:</b> Team has been deleted</p>
                                    <p><b>Event Type:</b> <span className="deleteStatus"><b>{item.changeType}</b></span></p>
                                </div>
                            );
                        }
                    })()}
                    <p>
                        <b>Date:</b> {moment(item.createdDate).format('LLL')} 
                        <b><span className="headColor">{moment(item.createdDate).fromNow()}</span></b>
                    </p>
                    <hr />
                </div>
            );
        });

        if (elements.length > 0) {
            this.setState({ notifications: elements.reverse() });
        }
    }

    /// </summary>
    /// Sends welcome Message for change notification subscription.
    /// </summary>
    welcomeMessage = () => {
        return (
            <div>
                <h3 className="headcolor">Team Notifications</h3>
                <h4>Welcome to Teams Notification Tab</h4>
                <p>This Tab has successfully configured, you will get notifications of team delete/edit in this tab.</p>
            </div>
        );
    }

    render() {
        return (
            <div className="tag-container">
                <div>
                    {this.welcomeMessage()}
                    <hr />
                    {this.state.notifications}
                </div>
            </div>
        )
    }
}

export default TeamChangeNotification;