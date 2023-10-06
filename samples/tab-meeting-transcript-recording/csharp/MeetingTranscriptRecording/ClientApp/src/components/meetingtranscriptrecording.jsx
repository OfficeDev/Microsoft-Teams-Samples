// <copyright file="MeetingTranscriptRecording.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useState, useEffect } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Button, Text, Card } from '@fluentui/react-components';
import { CardBody } from 'reactstrap';


const MeetingTranscriptRecording = () => {

    // Variable stores the username of users who login using Facebook or SSO.
    const [userName, setUserName] = useState("");

    // Declare new state variables that are required for a verified anonymous user or a normal user
    const [IsConsentButtonVisible, setIsConsentButtonVisible] = useState(false);

    useEffect(() => {
        microsoftTeams.app.initialize();
    }, [])

    // Tab sso authentication.
    const ssoAuthentication = () => {
        alert("Click");
        getClientSideToken()
            .then((clientSideToken) => {
                return getServerSideToken(clientSideToken);
            })
            .catch((error) => {
                if (error === "invalid_grant") {
                    // Display in-line button so user can consent
                    setIsConsentButtonVisible(true);
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
                        }
                        else {
                            let userDetails = JSON.parse(responseJson);
                            let userName = userDetails.user.displayName;
                            setUserName("Welcome: " + userName);
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
            <div>
                <Button appearance="primary" onClick={ssoAuthentication}>Sign-In</Button>
                <Text size={500} weight="semibold">{userName}</Text>
                {IsConsentButtonVisible &&
                    <>
                        <div id="divError">Please click on consent button</div>
                        <Button appearance="primary" onClick={requestConsent}>Consent</Button>
                    </>
                }
            </div>
            <div>
                <Card>
                    <Text weight='bold' as="h1">Capture Video</Text>
                    <CardBody>
                        <div>
                            <Text>Checks for permission to use media input</Text>
                            <Button>Capture video </Button>
                        </div>
                    </CardBody>
                </Card>
            </div>
        </div>

    );
};

export default MeetingTranscriptRecording;
