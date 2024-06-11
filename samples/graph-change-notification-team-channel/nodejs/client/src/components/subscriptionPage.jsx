// <copyright file="channel-notification.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useState, useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import axios from "axios";
import moment from 'moment'
import "../style/style.css";

const Subscriptions = () => {

    // let notifications = [];
    const [chatId, setChatId] = useState("");
    const [userId, setUserId] = useState("");
    const [notifications, setNotifications] = useState([]);

    const [subStatusForChat, setSubStatusForChat] = useState(false);
    const [subStatusForAnyChat, setSubStatusForAnyChat] = useState(false);
    const [subStatusNotifyOnUser, setSubStatusNotifyOnUser] = useState(false);
    const [subStatusUserLevelChats, setSubStatusUserLevelChats] = useState(false);
    const [subStatusMePath, setSubStatusMePath] = useState(false);
    const [subStatusUserLevelChatsOnNotifiy, setSubStatusUserLevelChatsOnNotifiy] = useState(false);
    const [subStatusTeamsAppInstalled, setSubStatusTeamsAppInstalled] = useState(false);

    useEffect(() => {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.getContext().then((context) => {
                setChatId(context.chat.id);
                setUserId(context.user.id);
                getExistingSubscription(context.chat.id,context.user.id);
            });

            // Set Status base on existing subscription
            const getExistingSubscription = async (chatId,userId) => {
                var response = await axios.get(`/api/changeNotification/checkExistingSubsription`);

                if (response.data != "") {
                    switch (response.data) {
                        
                        case `/chats/${chatId}`:
                            setSubStatusForChat(true);
                            setSubStatusForAnyChat(false);
                            setSubStatusNotifyOnUser(false);
                            setSubStatusUserLevelChats(false);
                            setSubStatusMePath(false);
                            setSubStatusUserLevelChatsOnNotifiy(false);
                            break;
                        case `/chats`:
                            setSubStatusForAnyChat(true);
                            setSubStatusForChat(false);
                            setSubStatusNotifyOnUser(false);
                            setSubStatusUserLevelChats(false);
                            setSubStatusMePath(false);
                            setSubStatusUserLevelChatsOnNotifiy(false);
                            break;
                        case `/chats/${chatId}?notifyOnUserSpecificProperties=${true}`:
                            setSubStatusNotifyOnUser(true);
                            setSubStatusForAnyChat(false);
                            setSubStatusForChat(false);
                            setSubStatusUserLevelChats(false);
                            setSubStatusMePath(false);
                            setSubStatusUserLevelChatsOnNotifiy(false);
                            break;
                        case `/users/${chatId}/chats`:
                            setSubStatusUserLevelChats(true);
                            setSubStatusNotifyOnUser(false);
                            setSubStatusForAnyChat(false);
                            setSubStatusForChat(false);
                            setSubStatusMePath(false);
                            setSubStatusUserLevelChatsOnNotifiy(false);
                            break;
                        case `/me/chats`:
                            setSubStatusMePath(true);
                            setSubStatusUserLevelChats(false);
                            setSubStatusNotifyOnUser(false);
                            setSubStatusForAnyChat(false);
                            setSubStatusForChat(false);
                            setSubStatusUserLevelChatsOnNotifiy(false);
                            break;
                        case `/users/${userId}/chats?notifyOnUserSpecificProperties=${true}`:
                            setSubStatusUserLevelChatsOnNotifiy(true);
                            setSubStatusMePath(false);
                            setSubStatusUserLevelChats(false);
                            setSubStatusNotifyOnUser(false);
                            setSubStatusForAnyChat(false);
                            setSubStatusForChat(false);
                            break;
                    }
                }
            }

            // Call function when page loads
            //getExistingSubscription();
        });
    }, []);

    const refreshData = async () => {
        var response = await axios.post('/api/notifications');

        if (response.data.length > 0) {
            try {
                if (response.data) {
                    setNotifications(([]) => [...[], []]);
                    let responseData = response.data;
                    var elements = [];
                    elements.push(<table id="notifications">
                        <tr>
                            <th>Group Chat Name</th>
                            <th>Change Type </th>
                            <th>Last Updated</th>
                            <th>Is Hidden</th>
                            <th>Created Date </th>
                        </tr>
                        <tbody>
                            {responseData.map((item, index) => (
                                <tr key={index}>
                                    <td>{item.displayName}</td>
                                    <td>{item.changeType}</td>
                                    <td><span className="statusColor"><b>{moment(item.lastUpdate).fromNow()}</b></span></td>
                                    <td>{item.isHidden.toString()}</td>
                                    <td>{moment(item.createdDate).format('LLL')} <span className="headColor"> - {moment(item.createdDate).fromNow()} </span></td>
                                </tr>
                            ))}
                        </tbody>
                    </table >);

                    // Set Notification list        
                    setNotifications((Items) => [...Items, elements])
                }
            }
            catch (e) {
                console.log("error", e);
            }
        }
    }

    const chatSubscription = async () => {
        await axios.post(`/api/changeNotification/chat?chatId=${chatId}`);

        // response data from graph API
        var response = await axios.post('/api/notifications');
        console.log("response--->", response);
        if (response.status == "200") {
            setSubStatusForChat(true);
            setSubStatusForAnyChat(false);
            setSubStatusNotifyOnUser(false);
            setSubStatusUserLevelChats(false);
            setSubStatusMePath(false);
            setSubStatusUserLevelChatsOnNotifiy(false);
        }
    }

    const anychatSubscription = async () => {
        await axios.post(`/api/changeNotification/anychat?chatId=${chatId}`);

        // response data from graph API
        var response = await axios.post('/api/notifications');
        console.log("repsonse--->", response.status);
        if (response.status == "200") {
            setSubStatusForAnyChat(true);
            setSubStatusForChat(false);
            setSubStatusNotifyOnUser(false);
            setSubStatusUserLevelChats(false);
            setSubStatusMePath(false);
            setSubStatusUserLevelChatsOnNotifiy(false);
        }
    }

    const notifyOnUserSpecificProperties = async () => {
        await axios.post(`/api/changeNotification/notifyOnUser?chatId=${chatId}`);

        // response data from graph API
        var response = await axios.post('/api/notifications');
        console.log("repsonse--->", response.status);
        if (response.status == "200") {
            setSubStatusNotifyOnUser(true);
            setSubStatusForAnyChat(false);
            setSubStatusForChat(false);
            setSubStatusUserLevelChats(false);
            setSubStatusMePath(false);
            setSubStatusUserLevelChatsOnNotifiy(false);
        }
    }

    const userLevelChats = async () => {
        await axios.post(`/api/changeNotification/userLevelChats?userId=${userId}`);

        // response data from graph API
        var response = await axios.post('/api/notifications');
        console.log("repsonse--->", response.status);
        if (response.status == "200") {
            setSubStatusUserLevelChats(true);
            setSubStatusNotifyOnUser(false);
            setSubStatusForAnyChat(false);
            setSubStatusForChat(false);
            setSubStatusMePath(false);
            setSubStatusUserLevelChatsOnNotifiy(false);
        }
    }

    const userLevelUsingMePath = async () => {
        await axios.post(`/api/changeNotification/userLevelMePath?userId=${userId}`);

        // response data from graph API
        var response = await axios.post('/api/notifications');
        console.log("repsonse--->", response.status);
        if (response.status == "200") {
            setSubStatusMePath(true);
            setSubStatusUserLevelChats(false);
            setSubStatusNotifyOnUser(false);
            setSubStatusForAnyChat(false);
            setSubStatusForChat(false);
            setSubStatusUserLevelChatsOnNotifiy(false);
        }
    }

    const userLevelChatsUsingNotifyOnUserSpecificProperties = async () => {
        await axios.post(`/api/changeNotification/UserLevelChatsUsingNotifyOnUser?userId=${userId}`);

        // response data from graph API
        var response = await axios.post('/api/notifications');
        console.log("repsonse--->", response.status);
        if (response.status == "200") {
            setSubStatusUserLevelChatsOnNotifiy(true);
            setSubStatusMePath(false);
            setSubStatusUserLevelChats(false);
            setSubStatusNotifyOnUser(false);
            setSubStatusForAnyChat(false);
            setSubStatusForChat(false);
        }
    }

    const anyChatWhereTeamsAppIsInstalled = async () => {
        await axios.post(`/api/changeNotification/TeamsAppIsInstalled?userId=${userId}`);

        // response data from graph API
        var response = await axios.post('/api/notifications');
        console.log("repsonse--->", response.status);
        if (response.status == "200") {
            setSubStatusTeamsAppInstalled(true);
        }
    }

    return (
        <div className="tag-container">
            <div className="header">
                <h2>Graph API Subscriptions</h2>
                <p>Subscribe below Graph API's To Get Notifications </p>
            </div>
            <table id="subscriptions">
                <tr>
                    <th>Subscription Name</th>
                    <th>Resource Type</th>
                    <th>Subscribe</th>
                    <th>Status</th>
                </tr>
                <tr>
                    <td>Subscribe to specific chat using chat-id</td>
                    <td>"resource": "/chats/id"</td>
                    <td><button type="button" className="primary" onClick={chatSubscription}>Subscribe</button></td>
                    <td><span class="label" style={{ backgroundColor: subStatusForChat ? "#04AA6D" : "#808080" }}>Active</span></td>
                </tr>
                <tr>
                    <td>Subscribe to any chat at tenant level</td>
                    <td>"resource": "/chats"</td>
                    <td><button type="button" className="primary" onClick={anychatSubscription}>Subscribe</button></td>
                    <td><span class="label" style={{ backgroundColor: subStatusForAnyChat ? "#04AA6D" : "#808080" }}>Active</span></td>
                </tr>
                <tr>
                    <td>Subscribe to particular chat using the notifyOnUserSpecificProperties query parameter</td>
                    <td>"resource": "/chats/id?notifyOnUserSpecificProperties=true"</td>
                    <td><button type="button" className="primary" onClick={notifyOnUserSpecificProperties}>Subscribe</button></td>
                    <td><span class="label" style={{ backgroundColor: subStatusNotifyOnUser ? "#04AA6D" : "#808080" }}>Active</span></td>
                </tr>
                <tr>
                    <td>Subscribe to changes in user-level chats</td>
                    <td>"resource": "/users/userid/chats"</td>
                    <td> <button type="button" className="primary" onClick={userLevelChats}>Subscribe</button></td>
                    <td><span class="label" style={{ backgroundColor: subStatusUserLevelChats ? "#04AA6D" : "#808080" }}>Active</span></td>
                </tr>
                <tr>
                    <td>Subscribe to changes in user-level chats using the me path</td>
                    <td>"resource": "/me/chats"</td>
                    <td> <button type="button" className="primary" onClick={userLevelUsingMePath}>Subscribe</button></td>
                    <td><span class="label" style={{ backgroundColor: subStatusMePath ? "#04AA6D" : "#808080" }}>Active</span></td>
                </tr>
                <tr>
                    <td>Subscribe to changes in user-level chats using the notifyOnUserSpecificProperties query parameter</td>
                    <td>"resource": "/users/user-Id/chats?notifyOnUserSpecificProperties=true"</td>
                    <td> <button type="button" className="primary" onClick={userLevelChatsUsingNotifyOnUserSpecificProperties}>Subscribe</button></td>
                    <td><span class="label" style={{ backgroundColor: subStatusUserLevelChatsOnNotifiy ? "#04AA6D" : "#808080" }}>Active</span></td>
                </tr>
            </table>
            <hr></hr>
            <div>
                <h4> Notifications - <button onClick={refreshData}> Refresh</button></h4>
                <hr></hr>
                {notifications}
            </div>
        </div>
    )
}

export default Subscriptions;