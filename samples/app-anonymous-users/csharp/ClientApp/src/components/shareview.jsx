// <copyright file="shareview.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useState, useEffect } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { HubConnectionBuilder } from '@microsoft/signalr';
import { Button } from '@fluentui/react-components';

const ShareView = () => {

    // Declare new state variables that are required to get and set the connection.
    const [connection, setConnection] = useState(null);

    // Declare new state variables that are required to get the counts of votes from anonymous and other users.
    const [aShowCount, aShowSetCount] = useState(0);

    const [uShowCount, uShowSetCount] = useState(0);

    // Declare new state variables that are required to disable the submit vote button.
    const [disabled, setDisabled] = useState(false)

    useEffect(() => {
        microsoftTeams.initialize();

    }, [])

    // Builds the SignalR connection, mapping it to /chatHub
    // Initializes a new instance of the HubConnectionBuilder class.
    useEffect(() => {
        const newConnection = new HubConnectionBuilder()
            .withUrl(`${window.location.origin}/chatHub`)
            .withAutomaticReconnect()
            .build();

        setConnection(newConnection);
    }, []);

     // Starts the SignalR connection
    useEffect(() => {
        if (connection) {
            connection.start()
                .then(result => {
                    connection.on("ReceiveMessage", (description: any, count: any) => {

                        if (description === "Anonymous") {
                            aShowSetCount(count);
                        } else {
                            uShowSetCount(count);
                        }
                    });
                })
                .catch(e => console.log('Connection failed: ', e));
        }
    }, [connection]);

    // Click submit vote button
    const submitVote = () => {
        if (connection) {
            microsoftTeams.app.getContext().then((context) => {

                setDisabled(true); // Disable Button
                // Once we call getContext API, we can recognize anonymous users by checking for the licenseType value like: context.user.licenseType === "Anonymous".
                if (context.user.licenseType === "Anonymous")
                {
                    // Update the state property for incremented count value.
                    let addAnonymousVal = aShowCount + 1;

                    // Sending the updated count to hub signal to show the latest data on stage view.
                    connection.send("SendMessage", "Anonymous", addAnonymousVal);
                }
                else {
                    // Update the state property for incremented count value.
                    let addUserVal = uShowCount + 1;

                    // Sending the updated count to hub signal to show the latest data on stage view.
                    connection.send("SendMessage", "User", addUserVal);
                }
            });
        }
        else {
            alert('No connection to server yet.');
        }
    }

    return (
        <div className="timerCount">
            <Button appearance="primary" onClick={submitVote} disabled={disabled} >Submit Vote</Button>
            <h1>Anonymous users voted: {aShowCount}</h1>
            <h1>Users voted: {uShowCount}</h1>
        </div>
    );
};

export default ShareView;
