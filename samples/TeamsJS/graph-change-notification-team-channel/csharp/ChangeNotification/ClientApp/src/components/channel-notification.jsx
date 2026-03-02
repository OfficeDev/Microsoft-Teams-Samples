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
            teamsContext: {},
            pageId: ""
        }
    }

    componentDidMount() {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.getContext().then((context) => {
                this.setState({ teamId: context.team.groupId });
                var url = window.location.href;
                var pageid = url.match(/\d+$/)[0];
                this.setState({ pageId: pageid });
                this.initializeData(context.team.groupId);
            })
        });
    }

    ///Summary///
    ///Passing {teamId} and {PageId} to team controller for change notification subscription.
    ///Summary///
    initializeData = async (teamId) => {
        var response = await axios.post(`/api/Notifications/${teamId}/${this.state.pageId}`);

        try {
            if (response.status === 200) {
                var responseData = response.data;

                if (responseData) {
                    var elements = [];

                    responseData.forEach(item => {
                        elements.push(<div>
                            <p><b>Channel Name :</b> {item.displayName}</p>
                            {(() => {
                                if (item.changeType === 'updated') {
                                    return (<div><p><b>Description  : </b> Channel has Renamed</p>
                                        <p><b>Status         : </b><span className="statusColor"> {item.changeType}</span></p>
                                    </div>);
                                }

                                if (item.changeType === 'created') {
                                    return (<div><p><b>Description  : </b> New Channel has Created</p>
                                        <p><b>Status         : </b><span className="statusColor"> {item.changeType}</span></p>
                                    </div>);
                                }

                                if (item.changeType === 'deleted') {
                                    return (<div><p><b>Description  : </b> Channel has deleted</p>
                                        <p><b>Status         : </b><span className="deleteStatus"> {item.changeType}</span></p>
                                    </div>);
                                }
                            })()}
                            <p><b>Date         :</b> {moment(item.createdDate).format('LLL')} <b>
                                <span className="headcolor">{moment(item.createdDate).fromNow()}</span></b></p>
                            <hr></hr>
                        </div>);
                    });

                    if (elements.length > 0) {
                        this.setState({ changeNotifications: elements.reverse() });
                    }
                }
            }
        } catch (e) {
            console.log("error", e);
        }
    }

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