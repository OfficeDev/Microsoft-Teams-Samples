/* eslint-disable array-callback-return */
// <copyright file="MeetingTranscriptRecording.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useState, useEffect } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Button, Text, Card, Spinner } from '@fluentui/react-components';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { CardBody } from 'reactstrap';

const MeetingTranscriptRecording = () => {

    // Declare new state variables that are required to get and set the connection.
    const [connection, setConnection] = useState(null);

    // Define an array state variable with an initial value
    const [cardData, setData] = useState([]);

    // Define a state variable to manage the visibility of the consent button
    const [IsConsentButtonVisible, setIsConsentButtonVisible] = useState(false);

    // Define a state variable to manage the visibility of the login button
    const [IsLoginVisible, setIsLoginVisible] = useState(true);

    // Define a state variable to manage the visibility of the card visible
    const [IsCardVisible, setIsCardVisible] = useState(false);

    const [loading, setLoading] = useState(false);

    const [loginAdminAccount, setloginAdminAccount] = useState(false);

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
                    connection.on("ReceiveMessage", (description: any, cardData: any) => {
                        if (description === "TranscriptRecording") {
                            setData(cardData);
                        }
                    });
                })
                .catch(e => console.log('Connection failed: ', e));
        }
    }, [connection]);

    // Tab sso authentication.
    const ssoAuthentication = () => {
        setLoading(true);
        getClientSideToken()
            .then((clientSideToken) => {
                return getServerSideToken(clientSideToken);
            })
            .catch((error) => {
                if (error === "invalid_grant") {
                    // Display an in-line button so the user can consent
                    setIsConsentButtonVisible(true);
                }
            });
    }

    // Get client-side token.
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

    // Get server-side token and user profile.
    const getServerSideToken = (clientSideToken) => {
        return new Promise((resolve, reject) => {
            microsoftTeams.app.getContext().then((context) => {
                fetch('/GetEventInformation', {
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
                        }
                    })
                    .then((responseJson) => {
                        if (responseJson === "" || responseJson === "[]" || responseJson === "{}") {
                            setIsConsentButtonVisible(true);
                            setIsLoginVisible(false);
                            setLoading(false);
                        }
                        else {
                            if (responseJson !== undefined) {
                                setIsLoginVisible(false);
                                setIsCardVisible(true);
                                let userDetails = JSON.parse(responseJson);
                                setData(userDetails);
                                setLoading(false);
                                Object.keys(userDetails).map((key) => {
                                    const element = userDetails[key]
                                    meetingTranscriptsIdRecordingId(clientSideToken, element.onlineMeetingId);
                                })
                            }
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

    const meetingTranscriptsIdRecordingId = (clientSideToken, onlineMeetingId) => {
        microsoftTeams.app.getContext().then((context) => {
            fetch('/getMeetingTranscriptsIdRecordingId', {
                method: 'POST',
                body: JSON.stringify({ 'MeetingId': onlineMeetingId }),
                headers: {
                    'Accept': 'application/json; charset=utf-8',
                    'Content-Type': 'application/json;charset=UTF-8',
                    'Authorization': "Bearer " + clientSideToken
                }
            })
                .then((response) => {
                    // Check if the response is OK
                    if (response.ok) {
                        return response.text();
                    }
                })
                .then((responseJson) => {
                    if (responseJson === "" || responseJson === "[]" || responseJson === "{}") {
                        setIsConsentButtonVisible(true);
                        setIsLoginVisible(false);
                        setLoading(false);
                    }
                    else {
                        if (responseJson !== undefined) {
                            setIsLoginVisible(false);
                            setIsCardVisible(true);
                            let userDetails = JSON.parse(responseJson);
                            setData(userDetails);
                            setLoading(false);
                        }
                    }
                });
        });

    }

    // Get token for multi-tenant.
    const getToken = () => {
        return new Promise((resolve, reject) => {
            microsoftTeams.authentication.authenticate({
                url: window.location.origin + "/auth-start",
                width: 600,
                height: 535
            })
                .then((result) => {
                    resolve(result);
                    setLoading(true);
                })
                .catch((reason) => {
                    setloginAdminAccount(true);
                    setIsConsentButtonVisible(false);
                    setIsLoginVisible(false);
                    setLoading(false);
                });
        });
    }

    // Open stage view
    const fetchrecordingtranscript = (subject, onlineMeetingId, transcriptsId, recordingId) => () => {
        var submitHandler = function (err, result) { console.log("Err: ".concat(err, "; Result:  + ").concat(result)); };
        let taskInfo = {
            title: null,
            height: null,
            width: null,
            url: null,
            card: null,
            fallbackUrl: null,
            completionBotId: null,
        };
        taskInfo.url = `${window.location.origin}/RecordingTranscript?subject=${subject}&onlineMeetingId=${onlineMeetingId}&transcriptsId=${transcriptsId}&recordingId=${recordingId}`;
        taskInfo.title = "Recording and Transcript Form";
        taskInfo.height = 510;
        taskInfo.width = 1300;
        submitHandler = (err, result) => {
            console.log(`Submit handler - err: ${err}`);
        };
        microsoftTeams.tasks.startTask(taskInfo, submitHandler);
    }

    return (
        <div className="">
            <div className="btnLogin">
                {IsLoginVisible &&
                    <>
                        <Button appearance="primary" onClick={ssoAuthentication}>Sign-In</Button>
                    </>
                }
                {IsConsentButtonVisible &&
                    <>
                        <div id="divError">Please click on consent button</div>
                        <Button appearance="primary" onClick={requestConsent}>Consent</Button>
                    </>
                }

                {loginAdminAccount &&
                    <>
                        <h3>Please login with admin account.</h3>
                    </>
                }
            </div>
            <div className="mainCard">
                {loading &&
                    <>
                        <div className="loadingIcon">
                            <Spinner label="Loading meetings, fetching Transcript and Recordings..." size="large" />
                        </div>
                    </>
                }
                {IsCardVisible &&
                    <>
                        {Object.keys(cardData).map((key) => {
                            const element = cardData[key]
                            return (
                                <div className="divMainCard">
                                    <Card>
                                        <CardBody className="main1Card">
                                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                                <div>
                                                    <svg className="calendarSVG" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M14 22H10C6.22876 22 4.34315 22 3.17157 20.8284C2 19.6569 2 17.7712 2 14V12C2 8.22876 2 6.34315 3.17157 5.17157C4.34315 4 6.22876 4 10 4H14C17.7712 4 19.6569 4 20.8284 5.17157C22 6.34315 22 8.22876 22 12V14C22 17.7712 22 19.6569 20.8284 20.8284C20.1752 21.4816 19.3001 21.7706 18 21.8985" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M7 4V2.5" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M17 4V2.5" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M21.5 9H16.625H10.75M2 9H5.875" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M18 17C18 17.5523 17.5523 18 17 18C16.4477 18 16 17.5523 16 17C16 16.4477 16.4477 16 17 16C17.5523 16 18 16.4477 18 17Z" fill="#1C274C"></path> <path d="M18 13C18 13.5523 17.5523 14 17 14C16.4477 14 16 13.5523 16 13C16 12.4477 16.4477 12 17 12C17.5523 12 18 12.4477 18 13Z" fill="#1C274C"></path> <path d="M13 17C13 17.5523 12.5523 18 12 18C11.4477 18 11 17.5523 11 17C11 16.4477 11.4477 16 12 16C12.5523 16 13 16.4477 13 17Z" fill="#1C274C"></path> <path d="M13 13C13 13.5523 12.5523 14 12 14C11.4477 14 11 13.5523 11 13C11 12.4477 11.4477 12 12 12C12.5523 12 13 12.4477 13 13Z" fill="#1C274C"></path> <path d="M8 17C8 17.5523 7.55228 18 7 18C6.44772 18 6 17.5523 6 17C6 16.4477 6.44772 16 7 16C7.55228 16 8 16.4477 8 17Z" fill="#1C274C"></path> <path d="M8 13C8 13.5523 7.55228 14 7 14C6.44772 14 6 13.5523 6 13C6 12.4477 6.44772 12 7 12C7.55228 12 8 12.4477 8 13Z" fill="#1C274C"></path> </g></svg>
                                                    <Text className="txtMeetingTitle" weight='bold' as="h1">{element.subject}</Text>
                                                </div>
                                                {element.signalRCondition ?
                                                    <svg className="calendarSVG" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 256 256"><rect width="256" height="256" fill="none"></rect><path d="M207.8,112a79.7,79.7,0,0,0-79.2-80H128a79.9,79.9,0,0,0-79.8,80c0,34.3-7.1,53.7-13,63.9a16.2,16.2,0,0,0-.1,16.1A15.9,15.9,0,0,0,49,200H88a40,40,0,0,0,80,0h39a15.9,15.9,0,0,0,13.9-8,16.2,16.2,0,0,0-.1-16.1C214.9,165.7,207.8,146.3,207.8,112ZM128,224a24.1,24.1,0,0,1-24-24h48A24.1,24.1,0,0,1,128,224ZM224.9,73.3a9.3,9.3,0,0,1-3.5.8,7.9,7.9,0,0,1-7.2-4.5,97,97,0,0,0-35-38.8,8,8,0,0,1,8.5-13.6,111.7,111.7,0,0,1,40.8,45.4A8,8,0,0,1,224.9,73.3Zm-190.3.8a9.3,9.3,0,0,1-3.5-.8,8,8,0,0,1-3.6-10.7A111.7,111.7,0,0,1,68.3,17.2a8,8,0,0,1,8.5,13.6,97,97,0,0,0-35,38.8A7.9,7.9,0,0,1,34.6,74.1Z" fill="#5b5fc7"></path></svg>
                                                    :
                                                    <div></div>
                                                }
                                            </div>
                                            <div>
                                                <Text className="meetingDate">{element.start} - {element.end} </Text>
                                            </div>
                                            <div>
                                                <Text className="organizerName">{element.organizer}</Text>
                                            </div>
                                            <div className="btnCard">
                                                <Button appearance="primary" disabled={!element.condition} onClick={fetchrecordingtranscript(element.subject, element.onlineMeetingId, element.transcriptsId, element.recordingId)}>Fetch Recording & Transcript</Button>
                                            </div>
                                        </CardBody>
                                    </Card>
                                </div>
                            );
                        })}
                    </>
                }
            </div>
        </div>
    );
};

export default MeetingTranscriptRecording;
