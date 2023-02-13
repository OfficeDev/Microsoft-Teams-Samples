// <copyright file="shareview.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useState, useEffect } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { HubConnectionBuilder } from '@microsoft/signalr';
import { Button, Image, Text } from '@fluentui/react-components';
import $ from "jquery";

const ShareView = () => {

    // Declare new state variables that are required to get and set the connection.
    const [connection, setConnection] = useState(null);

    // Declare new state variables that are required to get the counts of votes from anonymous and other users.
    const [aShowCount, aShowSetCount] = useState(0);

    const [uShowCount, uShowSetCount] = useState(0);

    // Declare new state variables that are required to disable the submit vote button.
    const [disabled, setDisabled] = useState(false);

    // Declare new state variables that are required to assgin facebook user details
    const [facebookUserImage, setfacebookUserImage] = useState("");

    const [facebookUserName, setfacebookUserName] = useState("");

    const [disabledfacebookBtn, setdisabledfacebookBtn] = useState(true);

    const [isLoggedIn, isSetLoggedIn] = useState(false);


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
                if (context.user.licenseType === "Anonymous") {
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

    // Initiate facebook login.
    const facebookLogin = () => {
        fbAuthentication()
            .then((result) => {
                return getServerSideTokenFb(result.idToken);
            })
            .catch((error) => {
                console.log(error);
            });
    }

    // Get client side token for facebook.
    const fbAuthentication = () => {
        var facebookAppId = '{{Facebook App Id}}';
        let redirectUri = window.location.origin + "/facebook-auth-end";

        return new Promise((resolve, reject) => {
            microsoftTeams.authentication.authenticate({
                url: `https://www.facebook.com/v12.0/dialog/oauth?client_id=${facebookAppId}&redirect_uri=${redirectUri}&state=1234535`,
                width: 600,
                height: 535
            })
                .then((result) => {
                    let data = localStorage.getItem(result);
                    let tokenDetails = JSON.parse(data);
                    localStorage.removeItem(result);
                    resolve(tokenDetails);
                })
                .catch((reason) => {
                    reject(reason);
                })
        });
    }

    // Get face book user profile details.
    const getServerSideTokenFb = (clientSideToken) => {
        return new Promise(() => {
            microsoftTeams.app.getContext().then(() => {
                $.ajax({
                    type: 'POST',
                    url: '/getFbAccessToken',
                    dataType: 'json',
                    data: {
                        'accessToken': clientSideToken,
                    },
                    success: function (responseJson) {
                        let facebookProfile = JSON.parse(responseJson);
                        let facebookName = facebookProfile.name;
                        setfacebookUserName("Welcome: " + facebookName);
                        setfacebookUserImage(facebookProfile.picture);
                        setdisabledfacebookBtn(false);
                        isSetLoggedIn(true);
                    },
                    error: function (textStatus, errorThrown) {
                        console.log("textStatus: " + textStatus + ", errorThrown:" + errorThrown);
                    }
                });
            });
        });
    }

    return (
        <div className="timerCount">
            <div className="btnlogin">
                {disabledfacebookBtn &&
                    <Button appearance="primary" onClick={facebookLogin}>Login with Facebook</Button>
                }
                <Text size={500} weight="semibold">{facebookUserName}</Text>
                <br />
                {facebookUserImage &&
                    <Image width="100" height="100" src={facebookUserImage} />
                }
            </div>

            <div>
                {isLoggedIn &&
                    <div>
                        <Button appearance="primary" onClick={submitVote} disabled={disabled} >Submit Vote</Button>
                        <br />
                        <Text size={500} weight="semibold">Anonymous users voted : {aShowCount}</Text>
                        <br />
                        <Text size={500} weight="semibold">Users voted : {uShowCount}</Text>
                    </div>
                }
            </div>
        </div>
    );
};

export default ShareView;
