// <copyright file="channel-notification.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { Component } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import axios from "axios";
import moment from 'moment'
import "../style/style.css";

class ChangeNotificationChannel extends Component {
    constructor(props) {
        super(props);

        this.state = {
            teamId: "",
            changeNotifications: [],
            teamsContext: {}
        }
    }

    /// </summary>
    /// ComponentDidMount function call On Page Load
    /// </summary>
    componentDidMount() {
        microsoftTeams.app.initialize().then(() => {
            console.log("Teams SDK initialized");
            microsoftTeams.app.getContext().then((context) => {
                this.setState({ teamId: context.channel.ownerGroupId});
                this.initializeData(context.channel.ownerGroupId);
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
        await axios.post(`/api/changeNotification?teamId=${teamId}`);
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
                    {(() => {
                        if (item.changeType === 'updated') {
                            return (
                                <div>
                                    <p><b>Description:</b> Users membership updated</p>
                                    <p><b>Event Type:</b> <span className="statusColor"><b>{item.changeType}</b></span></p>
                                </div>
                            );
                        }

                        if (item.changeType === 'created') {
                            return (
                                <div>
                                    <p><b>Description:</b> New user has been Added</p>
                                    <p><b>Event Type:</b> <span className="statusColor"><b>{item.changeType}</b></span></p>
                                </div>
                            );
                        }

                        if (item.changeType === 'deleted') {
                            return (
                                <div>
                                    <p><b>Description:</b> User has been removed</p>
                                    <p><b>Event Type:</b> <span className="deleteStatus"><b>{item.changeType}</b></span></p>
                                    {item.hasUserAccess !== undefined && 
                                        <p><b>Access Status:</b> {item.hasUserAccess ? 'User still has access' : 'User no longer has access'}</p>
                                    }
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
            this.setState({ changeNotifications: elements.reverse() });
        }
    }

    /// </summary>
    /// Sends welcome Message for change notification subscription.
    /// </summary>
    welcomeMessage = () => {
        return (
            <div>
                <h3 className="headcolor">Membership Change Notifications</h3>
                <h4>Welcome to Membership Change Notification Tab</h4>
                <p>This Tab has successfully configured, you will get notifications of direct/indirect membership change (add/update/remove) in this tab.</p>
            </div>
        );
    }

    render() {
        return (
            <div className="tag-container">
                <div>
                    {this.welcomeMessage()}
                    <hr />
                    {this.state.changeNotifications}
                </div>
            </div>
        )
    }
}

export default ChangeNotificationChannel;