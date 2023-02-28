// <copyright file="shareview.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useState, useEffect } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { HubConnectionBuilder } from '@microsoft/signalr';
import { Button, Text } from '@fluentui/react-components';

const ShareView = () => {

    // Declare new state variables that are required to get and set the connection.
    const [connection, setConnection] = useState(null);

    // Declare new state variables that are required to get the counts of votes from anonymous and other users.
    const [aShowCount, aShowSetCount] = useState(0);

    const [uShowCount, uShowSetCount] = useState(0);

    // Declare new state variables that are required to disable the submit vote button.
    const [isVoteBtnDisabled, setIsVoteBtnDisabled] = useState(false);

    // Variable stores the username of users who login using Facebook or SSO.
    const [userName, setUserName] = useState("");

    // Declare new state variables that are required to enabled the Facebook button after login
    const [IsFacebookButtonEnabled, setIsFacebookButtonEnabled] = useState(true);

    // Declare new state variables that are required for login success load the submit vote button
    const [enableVoteDiv, setEnableVoteDiv] = useState(false);

    // Declare new state variables that are required for a verified anonymous user or a normal user
    const [IsAnonymousUser, setIsAnonymousUser] = useState(false);

    // Declare new state variables that are required for a verified anonymous user or a normal user
    const [IsConsentButtonVisible, setIsConsentButtonVisible] = useState(false);

    // Declare new state variables that are required disable the authentication button after login
    const [ssoAuthenticationButtonVisible, setIsSsoAuthenticationButtonVisible] = useState(true);

    useEffect(() => {
        microsoftTeams.app.initialize();
        verifyAnonymousUser();
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

    // Once we call getContext API, we can recognize anonymous users by checking for the licenseType value like: context.user.licenseType === "Anonymous".
    const verifyAnonymousUser = () => {
          microsoftTeams.app.getContext().then((context) => {
            if (context.user.licenseType === "Anonymous") {
                setIsAnonymousUser(true);
            }
        });
    }

    // Click submit vote button
    const submitVote = () => {

        if (connection) {
            setIsVoteBtnDisabled(true); // Disable Button
            if (IsAnonymousUser) {
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
        }
        else {
            alert('No connection to server yet.');
        }
    }

    // Initiate facebook login.
    const facebookLogin = () => {
        fbAuthentication() // This method get a client-side token for Facebook
            .then((result) => {
                return getFacebookProfileName(result.idToken); // This method get a face book user profile details.
            })
            .catch((error) => {
                console.log(error);
            });
    }

    // Get client side token for facebook.
    const fbAuthentication = () => {
        var facebookAppId = process.env.REACT_APP_FACEBOOK_APP_ID;
        let redirectUri = window.location.origin + "/facebook-auth-end";
        let state = Math.random().toString(36).substring(2, 7);
        localStorage.setItem("simple.state", state);

        return new Promise((resolve, reject) => {
            microsoftTeams.authentication.authenticate({
                url: `https://www.facebook.com/v12.0/dialog/oauth?client_id=${facebookAppId}&redirect_uri=${redirectUri}&state=${state}`,
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
    const getFacebookProfileName = (clientSideToken) => {
        return new Promise((resolve, reject) => {
            microsoftTeams.app.getContext().then((context) => {
                fetch('/getFacebookLoginUserInfo', {
                    method: 'POST',
                    body: JSON.stringify(clientSideToken), 
                    headers: {
                        'Accept': 'application/json; charset=utf-8',
                        'Content-Type': 'application/json;charset=UTF-8'
                    }
                })
                .then((response) => {
                    if (response.ok) {
                        return response.text();
                    } else {
                        reject(response.error);
                    }
                })
                .then((responseJson) => {
                    if (responseJson !== "") {
                        let jsonResult = JSON.parse(responseJson);
                        let facebookProfile = JSON.parse(jsonResult);

                        // This variables will load the values to the labels
                        setUserName("Welcome: " + facebookProfile.name);
                        setIsFacebookButtonEnabled(false);
                        setEnableVoteDiv(true);
                    }
                });
            });
        });
    }

    // Tab sso authentication.
    const ssoAuthentication = () => {
        getClientSideToken()
            .then((clientSideToken) => {
                return getServerSideToken(clientSideToken);
            })
            .catch((error) => {
                if (error === "invalid_grant") {
                    // Display in-line button so user can consent
                    setIsConsentButtonVisible(true);
                    setIsSsoAuthenticationButtonVisible(false);
                }
            });
    }

    // Get client side token.
    const getClientSideToken = () => {
        return new Promise((resolve, reject) => {
            microsoftTeams.authentication.getAuthToken()
                .then((result) => {
                    resolve(result);
                })
                .catch((error) => {
                    reject("Error getting token: " + error);
                });
        });
    }

    // Get server side token and user profile.
    const getServerSideToken = (clientSideToken) => {
        return new Promise((resolve, reject) => {
            microsoftTeams.app.getContext().then((context) => {
                 fetch('/GetLoginUserInformation', {
                    method: 'get',
                    headers: {
                        "Content-Type": "application/text",
                        "Authorization": "Bearer " + clientSideToken
                    },
                    cache: 'default'
                })
                .then((response) => {
                    if (response.ok) {
                        return response.text();
                    } else {
                        reject(response.error);
                    }
                })
                .then((responseJson) => {
                    if (responseJson === "") {
                        setIsConsentButtonVisible(true);
                        setIsSsoAuthenticationButtonVisible(false);
                    }
                    else {
                        let userDetails = JSON.parse(responseJson);
                        let userName = userDetails.user.displayName;
                        setUserName("Welcome: " + userName);

                        setIsSsoAuthenticationButtonVisible(false);
                        setEnableVoteDiv(true);
                    }
                });
            });
        });
    }

    // Request consent on implicit grant error.
    const requestConsent = () => {
        getToken()
            .then(data => {
                setIsConsentButtonVisible(false);
                getClientSideToken()
                    .then((clientSideToken) => {
                        return getServerSideToken(clientSideToken);
                    });
            });
    }
    // Get token for multi tenant.
    const getToken = () => {
        return new Promise((resolve, reject) => {
            microsoftTeams.authentication.authenticate({
                url: window.location.origin + "/auth-start",
                width: 600,
                height: 535
            })
                .then((result) => {
                    resolve(result);
                })
                .catch((reason) => {
                    reject(reason);
                });
        });
    }

    return (
        <div className="timerCount">
            {IsAnonymousUser
                ? 
                   <div className="btnlogin">
                    {IsFacebookButtonEnabled &&
                            <Button appearance="primary" onClick={facebookLogin}>Sign-In</Button>
                        }
                        <Text size={500} weight="semibold">{userName}</Text>
                   </div>
                 : <div className="btnlogin">
                    {ssoAuthenticationButtonVisible &&
                        <Button appearance="primary" onClick={ssoAuthentication}>Sign-In</Button>
                    }
                    <Text size={500} weight="semibold">{userName}</Text>
                    {IsConsentButtonVisible &&
                        <>
                            <div id="divError">Please click on consent button</div>
                            <Button appearance="primary" onClick={requestConsent}>Consent</Button>
                        </>
                    }
                 </div>
            }
            {enableVoteDiv &&  // If the login is successful, only the submitted vote button will be visible. 
                <div>
                    <Button appearance="primary" onClick={submitVote} disabled={isVoteBtnDisabled} >Submit Vote</Button>
                    <br />
                    <Text size={500} weight="semibold">Anonymous users voted : {aShowCount}</Text>
                    <br />
                    <Text size={500} weight="semibold">Users voted : {uShowCount}</Text>
                </div>
            }
        </div>
    );
};

export default ShareView;
