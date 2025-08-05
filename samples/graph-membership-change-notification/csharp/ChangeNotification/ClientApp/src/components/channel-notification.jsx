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
            pageId: "",
            channelId: "",
            memberList: [],
            memberListLoading: false
        }
    }

    componentDidMount() {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.getContext().then((context) => {
                this.setState({ teamId: context.channel.ownerGroupId });
                this.setState({ channelId: context.channel.id });
                var url = window.location.href;
                var pageid = url.match(/\d+$/)[0];
                this.setState({ pageId: pageid });
                this.initializeData(context.channel.ownerGroupId, context.channel.id);
                // Automatically fetch member list on startup
                this.fetchMemberList(context.channel.ownerGroupId, context.channel.id);
            })
        });
    }

    ///Summary///
    ///Passing {teamId} and {PageId} to team controller for change notification subscription.
    ///Summary///
    initializeData = async (teamId, channelId) => {
        var response = await axios.post(`/api/Notifications/${teamId}/${this.state.pageId}/${channelId}`);

        try {
            if (response.status === 200) {
                var responseData = response.data;
                console.log(responseData.item);

                if (responseData) {
                    debugger;
                    var elements = [];
                    responseData.forEach(item => {
                        elements.push(<div>
                            {(() => {
                                debugger;
                                if (item.changeType === 'created' && item.displayName !== null) {
                                    return (
                                        <div>
                                            <p><b>Description:</b> Channel is shared with a Team</p>
                                            <p><b>Event Type:</b> <span className="statusColor"><b>{item.changeType}</b></span></p>
                                            <p><b>Team Name:</b> {item.displayName}</p>
                                        </div>
                                    );
                                }

                                if (item.changeType === 'deleted' && item.displayName !== null) {
                                    return (
                                        <div>
                                            <p><b>Description:</b> Channel is unshared from a Team</p>
                                            <p><b>Event Type:</b> <span className="deleteStatus"><b>{item.changeType}</b></span></p>
                                            <p><b>Team Name:</b> {item.displayName}</p>
                                        </div>
                                    );
                                }
                                if (item.changeType === 'updated') {
                                    return (<div><p><b>Description  : </b> User membership updated</p>
                                        <p><b>Status         : </b><span className="statusColor"> {item.changeType}</span></p>
                                    </div>);
                                }

                                if (item.changeType === 'created') {
                                    return (<div><p><b>Description  : </b> New user added</p>
                                        <p><b>Status         : </b><span className="statusColor"> {item.changeType}</span></p>
                                    </div>);
                                }

                                if (item.changeType === 'deleted') {
                                    return (<div><p><b>Description  : </b> User have been removed</p>
                                        <p><b>Status         : </b><span className="deleteStatus"> {item.changeType}</span></p>
                                         <p><b>Access Status:</b> {item.userHaveAccess ? 'User still has access' : 'User no longer has access'}</p>
                                    </div>);
                                }
                            })()}
                            
                            {/* Add member list update information */}
                            {item.memberListUpdated !== undefined && (
                                <div>
                                    <p><b>Member List Updated:</b> <span className={item.memberListUpdated ? "statusColor" : "deleteStatus"}><b>{item.memberListUpdated ? 'Yes' : 'No'}</b></span></p>
                                    
                                    {item.memberListUpdated && item.currentMemberCount !== undefined && (
                                        <p><b>Current Member Count:</b> {item.currentMemberCount}</p>
                                    )}
                                    
                                    {!item.memberListUpdated && item.memberListSkipReason && (
                                        <p><b>Skip Reason:</b> {item.memberListSkipReason}</p>
                                    )}
                                    
                                    {item.memberListUpdateError && (
                                        <p><b>Update Error:</b> <span className="deleteStatus">{item.memberListUpdateError}</span></p>
                                    )}
                                </div>
                            )}
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

    // Fetch member list from API
    fetchMemberList = async (teamId, channelId) => {
        if (!teamId || !channelId) {
            console.error('Team/Channel context not available for member list fetch');
            return;
        }

        this.setState({ memberListLoading: true });

        try {
            const response = await axios.get(`/api/Notifications/members/${teamId}/${channelId}`);
            if (response.status === 200) {
                this.setState({ 
                    memberList: response.data.members || [],
                    memberListLoading: false 
                });
            }
        } catch (error) {
            console.error('Error fetching member list:', error);
            this.setState({ 
                memberList: [],
                memberListLoading: false 
            });
        }
    }

    welcomeMessage = () => {
        return (
            <div>
                <h3 className="headcolor">Channel Notifications</h3>
                <h4>Welcome to Channel Notification Tab</h4>
                <p>This Tab has successfully configured, you will get notifications of member added/removed/updated and when the channel is shared/unshared with a team, in this tab.</p>
                <p>The member list is automatically updated when notifications are received (except when removed users still have access).</p>
            </div>
        );
    }

    renderMemberList = () => {
        return (
            <div style={{ marginBottom: '20px' }}>
                <h4>Channel Member List</h4>
                <button 
                    onClick={() => this.fetchMemberList(this.state.teamId, this.state.channelId)} 
                    style={{
                        backgroundColor: '#5b5fc7',
                        color: 'white',
                        padding: '8px 16px',
                        border: 'none',
                        borderRadius: '4px',
                        cursor: 'pointer',
                        marginRight: '10px'
                    }}
                >
                    Refresh Member List
                </button>
                
                <div style={{
                    marginTop: '10px',
                    padding: '10px',
                    backgroundColor: '#f8f9fa',
                    borderRadius: '4px'
                }}>
                    {this.state.memberListLoading ? (
                        <p>Loading member list...</p>
                    ) : (
                        <div>
                            <h5>Channel Members ({this.state.memberList.length})</h5>
                            {this.state.memberList.length === 0 ? (
                                <p>No members found or loading initial member list...</p>
                            ) : (
                                <ul style={{ paddingLeft: '20px' }}>
                                    {this.state.memberList.map((member, index) => (
                                        <li key={index} style={{ margin: '5px 0' }}>
                                            {member.displayName || member.email || member.id || 'Unknown Member'}
                                        </li>
                                    ))}
                                </ul>
                            )}
                        </div>
                    )}
                </div>
            </div>
        );
    }

    render() {
        return (
            <div className="tag-container">
                <div>
                    {this.welcomeMessage()}
                    <hr></hr>
                    {this.renderMemberList()}
                    <hr></hr>
                    <h4>Notifications</h4>
                    {this.state.changeNotifications}
                </div>
            </div>
        )
    }
}


export default ChangeNotificationChannel;