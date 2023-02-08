// <copyright file="shareview.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useState, useEffect, useRef } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { HubConnectionBuilder } from '@microsoft/signalr';
import { Button } from '@fluentui/react-components';

const ShareView = () => {

    // Declare a new state variable
    const [connection, setConnection] = useState(null);

    const [aShowCount, aShowSetCount] = useState(0);

    const [uShowCount, uShowSetCount] = useState(0);

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

    const verifyAnonymousUser = () => {
        if (connection) {
            microsoftTeams.app.getContext().then((context) => {
                setDisabled(true); // Disable Button
                // Apps receive the following info for an anonymous user when they call the getContext API from the shared app stage. 
                // You can recognize anonymous users by checking for a userLicenseType value of Anonymous.
                if (context.user.licenseType === "Anonymous")
                {
                    // Update state with incremented value
                    let addAnonymousVal = aShowCount + 1;
                    connection.send("SendMessage", "Anonymous", addAnonymousVal);
                }
                else {
                    // Update state with incremented value
                    let addUserVal = uShowCount + 1;
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
            <Button appearance="primary" onClick={verifyAnonymousUser} disabled={disabled} >Submit Vote</Button>
            <h1>Anonymous users voted: {aShowCount}</h1>
            <h1>User voted: {uShowCount}</h1>
        </div>
    );
};

export default ShareView;
