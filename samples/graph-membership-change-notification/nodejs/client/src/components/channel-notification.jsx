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
            channelId: "",
            changeNotifications: [],
            teamsContext: {},
            membersList: [],
            memberListError: null,
            isLoadingMembers: false
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
                this.setState({ channelId: context.channel.id });
                this.initializeData(context.channel.ownerGroupId, context.channel.id);
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
    initializeData = async (teamId, channelId) => {
        await axios.post(`/api/changeNotification?teamId=${teamId}&channelId=${channelId}`);
        await this.fetchNotifications();
        await this.fetchMembersList(teamId, channelId);
    }

    fetchMembersList = async (teamId, channelId) => {
        if (!teamId || !channelId) {
            console.log("Missing teamId or channelId for member list fetch");
            return;
        }

        this.setState({ isLoadingMembers: true, memberListError: null });
        
        try {
            const response = await axios.get(`/api/members/${teamId}/${channelId}`);
            if (response.data && response.data.members) {
                this.setState({ 
                    membersList: response.data.members,
                    isLoadingMembers: false 
                });
            }
        } catch (error) {
            console.error("Error fetching members list:", error);
            this.setState({ 
                memberListError: error.response?.data?.message || "Failed to load members",
                isLoadingMembers: false 
            });
        }
    }

    fetchNotifications = async () => {
        try {
            const response = await axios.post('/api/notifications');
            if (response.data) {
                this.processNotificationData(response.data);
                console.log(response.data);
                
                // If any notifications indicate member list updates, refresh the member list
                const hasUpdates = response.data.some(item => item.memberListUpdated);
                if (hasUpdates && this.state.teamId && this.state.channelId) {
                    await this.fetchMembersList(this.state.teamId, this.state.channelId);
                }
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
                        if (item.changeType ==='created' && item.displayName !== null) {
                            return (
                                <div>
                                    <p><b>Description:</b> Channel is shared with a Team</p>
                                    <p><b>Event Type:</b> <span className="statusColor"><b>{item.changeType}</b></span></p>
                                    <p><b>Team Name:</b> {item.displayName}</p>
                                    {item.memberListUpdated !== undefined && (
                                        <p><b>Member List:</b> {item.memberListUpdated ? 
                                            `Updated (${item.currentMemberCount || 'Unknown'} members)` : 
                                            'Not updated'
                                        }</p>
                                    )}
                                </div>
                            );
                        }

                        if (item.changeType ==='deleted' && item.displayName !== null) {
                            return (
                                <div>
                                    <p><b>Description:</b> Channel is unshared from a Team</p>
                                    <p><b>Event Type:</b> <span className="deleteStatus"><b>{item.changeType}</b></span></p>
                                    <p><b>Team Name:</b> {item.displayName}</p>
                                    {item.memberListUpdated !== undefined && (
                                        <p><b>Member List:</b> {item.memberListUpdated ? 
                                            `Updated (${item.currentMemberCount || 'Unknown'} members)` : 
                                            'Not updated'
                                        }</p>
                                    )}
                                </div>
                            );
                        }

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
                                    {item.memberListUpdated !== undefined && (
                                        <p><b>Member List:</b> {item.memberListUpdated ? 
                                            `Updated (${item.currentMemberCount || 'Unknown'} members)` : 
                                            'Not updated'
                                        }</p>
                                    )}
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
                                    {item.memberListUpdated !== undefined && (
                                        <p><b>Member List:</b> {item.memberListUpdated ? 
                                            `Updated (${item.currentMemberCount || 'Unknown'} members)` : 
                                            `Not updated ${item.memberListSkipReason ? `(${item.memberListSkipReason})` : ''}`
                                        }</p>
                                    )}
                                    {item.memberListUpdateError && 
                                        <p><b>Update Error:</b> <span className="error">{item.memberListUpdateError}</span></p>
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
                <p>This Tab has successfully configured, you will get notifications of direct/indirect membership change (add/update/remove) in this tab and when the channel is shared/unshared with a team.</p>
            </div>
        );
    }

    /// </summary>
    /// Renders the member list section
    /// </summary>
    renderMembersList = () => {
        const { membersList, isLoadingMembers, memberListError, teamId, channelId } = this.state;

        return (
            <div className="members-section">
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <h4 className="headcolor">Current Channel Members</h4>
                    <button 
                        onClick={() => this.fetchMembersList(teamId, channelId)}
                        disabled={isLoadingMembers}
                        style={{
                            padding: '5px 10px',
                            backgroundColor: '#6264a7',
                            color: 'white',
                            border: 'none',
                            borderRadius: '3px',
                            cursor: isLoadingMembers ? 'not-allowed' : 'pointer'
                        }}
                    >
                        {isLoadingMembers ? 'Loading...' : 'Refresh'}
                    </button>
                </div>
                
                {memberListError && (
                    <p style={{ color: 'red' }}>Error: {memberListError}</p>
                )}
                
                {isLoadingMembers && (
                    <p>Loading members...</p>
                )}
                
                {!isLoadingMembers && !memberListError && (
                    <div>
                        <p><strong>Total Members: {membersList.length}</strong></p>
                        {membersList.length > 0 ? (
                            <div style={{ maxHeight: '200px', overflowY: 'auto', border: '1px solid #ccc', padding: '10px' }}>
                                {membersList.map((member, index) => (
                                    <div key={member.id || index} style={{ marginBottom: '8px', paddingBottom: '8px', borderBottom: '1px solid #eee' }}>
                                        <div><strong>{member.displayName || 'Unknown'}</strong></div>
                                        <div style={{ fontSize: '0.9em', color: '#666' }}>
                                            {member.roles && member.roles.length > 0 && (
                                                <span>Roles: {member.roles.join(', ')}</span>
                                            )}
                                        </div>
                                    </div>
                                ))}
                            </div>
                        ) : (
                            <p>No members found</p>
                        )}
                    </div>
                )}
            </div>
        );
    }

    render() {
        return (
            <div className="tag-container">
                <div>
                    {this.welcomeMessage()}
                    <hr />
                    {this.renderMembersList()}
                    <hr />
                    <h4 className="headcolor">Recent Notifications</h4>
                    {this.state.changeNotifications}
                </div>
            </div>
        )
    }
}

export default ChangeNotificationChannel;