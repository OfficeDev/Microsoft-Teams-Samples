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
            ch_notifications: [],
            teamsContext: {}
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
            })
        });
    }

    /// </summary>
    /// <param name="teamId"></param>
    /// </summary>
    initializeData = async (teamId) => {
        await axios.post(`/api/changeNotification?teamId=${teamId}`);
        var response = await axios.post('/api/notifications');

        try {
            if (response.data) {
                let responseData = response.data;

                var elements = [];

                responseData.forEach(item => {
                    elements.push(<div>
                        <p><b>Channel Name :</b> {item.displayName}</p>

                        {(() => {
                            if (item.changeType === 'updated') {
                                return (<div><p><b>Description : </b> When channel has Renamed  </p>
                                    <p><b>Event Type : </b><span className="statusColor"><b> {item.changeType}</b></span></p>
                                </div>);
                            }

                            if (item.changeType === 'created') {
                                return (<div><p><b>Description  : </b> New channel has Created</p>
                                    <p><b>Event Type : </b><span className="statusColor"><b> {item.changeType}</b></span></p>
                                </div>);
                            }

                            if (item.changeType === 'deleted') {
                                return (<div><p><b>Description  : </b> When channel has deleted</p>
                                    <p><b>Event Type : </b><span className="deleteStatus"> <b>{item.changeType}</b></span></p>
                                </div>);
                            }
                        })()}

                        <p><b>Date :</b> {moment(item.createdDate).format('LLL')} <b>
                            <span className="headColor">{moment(item.createdDate).fromNow()}</span>
                        </b></p>
                        <hr></hr>
                    </div>);
                });

                if (elements.length > 0) {
                    this.setState({ changeNotifications: elements.reverse() });
                }
            }
        } catch (e) {
            console.log("error", e);
        }
    }

    /// </summary>
    /// Sends welcome Message for change notification subscription.
    /// </summary>
    welcomeMessage = () => {
        return (
            <div>
                <h3 className="headcolor">Channel Notifications</h3>
                <h4>Welcome to Channel Notification Tab</h4>
                <p>This Tab has successfully configured, you will get notifications of channel delete/edit/create in this tab.</p>
            </div>
        );
    }

    render() {
        return (
            <div className="tag-container">
                <div>
                    {this.welcomeMessage()}
                    <hr></hr>
                    {this.state.changeNotifications}
                </div>
            </div>
        )
    }
}

export default ChangeNotificationChannel;