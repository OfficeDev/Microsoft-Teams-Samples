// <copyright file="MeetingTranscriptRecording.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useState, useEffect } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Button, Text, Card, Spinner } from '@fluentui/react-components';
import { CardBody } from 'reactstrap';

const MeetingTranscriptRecording = () => {

    // Define an array state variable with an initial value
    const [cardData, setCardData] = useState([]);

   // Define a state variable to manage the visibility of the consent button
    const [IsConsentButtonVisible, setIsConsentButtonVisible] = useState(false);

    // Define a state variable to manage the visibility of the login button
    const [IsLoginVisible, setIsLoginVisible] = useState(true);

    // Define a state variable to manage the visibility of the card visible
    const [IsCardVisible, setIsCardVisible] = useState(false);

    const [loading, setLoading] = useState(false);

    const [loginAdminAccount, setloginAdminAccount] = useState(false);

    useEffect(() => {
        // Set up an interval to call refreshData every 1000 milliseconds (1 second)
        const intervalId = setInterval(getUpdatedData, 60000);
        // Clear the interval when the component unmounts
        return () => {
          clearInterval(intervalId);
        };
      }, []);

    const getUpdatedData = () => {
        return new Promise((resolve, reject) => {
            setLoading(true);
            microsoftTeams.app.getContext().then((context) => {
                fetch('/getUpdatedEvents', {
                    method: 'get',
                    headers: {
                        "Content-Type": "application/text"
                    },
                    cache: 'default'
                })
                .then(response => response.json())
                .then(data => {
                        if (data.eventDetails.length !== 0) {
                          setCardData(data.eventDetails);
                          setLoading(false);
                        } if (data.eventUpdated) {
                            ssoAuthentication();
                        }
                    })
            });
        });
    }

    // Tab sso authentication.
    const ssoAuthentication = () => {
        setLoading(true);
        setIsLoginVisible(false);
        getClientSideToken()
            .then((clientSideToken) => {
                getServerSideToken(clientSideToken);
                createSubscription(clientSideToken);
            })
            .catch((error) => {
                if (error === "invalid_grant") {
                    // Display an in-line button so the user can consent
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
                fetch('/GetLoginUserInformation?ssoToken=' + clientSideToken, {
                    method: 'get',
                    headers: {
                        "Content-Type": "application/text",
                        "Authorization": "Bearer " + clientSideToken
                    },
                    cache: 'default'
                })
                .then(response => response.json())
                .then(data => {
                        if (data.error == "consent_required") {
                            setIsConsentButtonVisible(true);
                            setIsLoginVisible(false);
                            setLoading(false);
                        }
                        if (data) {
                          setCardData(data);
                          setIsLoginVisible(false);
                          setLoading(false);
                          setIsCardVisible(true);
                        } else {
                            setLoading(false);
                            reject(response.error);
                            setIsConsentButtonVisible(true);
                            setIsLoginVisible(false);
                        }
                    })
            });
        });
    }

    const createSubscription = (clientSideToken) => {
        return new Promise((resolve, reject) => {
            microsoftTeams.app.getContext().then((context) => {
                fetch('/createsubscription?ssoToken=' + clientSideToken, {
                    method: 'post',
                    headers: {
                        "Content-Type": "application/text",
                        "Authorization": "Bearer " + clientSideToken
                    },
                    cache: 'default'
                })
                .then(response => response.json())
                .then(data => {
                        if (data.error == "consent_required") {
                            setIsConsentButtonVisible(true);
                            setIsLoginVisible(false);
                            setLoading(false);
                        }
                        if (data) {
                            debugger;
                            return;
                        } else {
                            return;
                        }
                    })
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
                    setLoading(true);
                })
                .catch((reason) => {
                    reject(reason);
                    setloginAdminAccount(true);
                    setIsConsentButtonVisible(false);
                    setIsLoginVisible(false);
                    setLoading(false);
                });
        });
    }

    // Open stage view
    const fetchRecordingTranscript = (subject, onlineMeetingId, transcriptsId, recordingId) => () => {
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
                        {cardData.length > 0 && cardData.map((element, index) => {
                            return (
                                <div key={index} className="divMainCard">
                                    <Card>
                                        <CardBody className="main1Card">
                                            <div>
                                                <svg className="calendarSVG" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M14 22H10C6.22876 22 4.34315 22 3.17157 20.8284C2 19.6569 2 17.7712 2 14V12C2 8.22876 2 6.34315 3.17157 5.17157C4.34315 4 6.22876 4 10 4H14C17.7712 4 19.6569 4 20.8284 5.17157C22 6.34315 22 8.22876 22 12V14C22 17.7712 22 19.6569 20.8284 20.8284C20.1752 21.4816 19.3001 21.7706 18 21.8985" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M7 4V2.5" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M17 4V2.5" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M21.5 9H16.625H10.75M2 9H5.875" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M18 17C18 17.5523 17.5523 18 17 18C16.4477 18 16 17.5523 16 17C16 16.4477 16.4477 16 17 16C17.5523 16 18 16.4477 18 17Z" fill="#1C274C"></path> <path d="M18 13C18 13.5523 17.5523 14 17 14C16.4477 14 16 13.5523 16 13C16 12.4477 16.4477 12 17 12C17.5523 12 18 12.4477 18 13Z" fill="#1C274C"></path> <path d="M13 17C13 17.5523 12.5523 18 12 18C11.4477 18 11 17.5523 11 17C11 16.4477 11.4477 16 12 16C12.5523 16 13 16.4477 13 17Z" fill="#1C274C"></path> <path d="M13 13C13 13.5523 12.5523 14 12 14C11.4477 14 11 13.5523 11 13C11 12.4477 11.4477 12 12 12C12.5523 12 13 12.4477 13 13Z" fill="#1C274C"></path> <path d="M8 17C8 17.5523 7.55228 18 7 18C6.44772 18 6 17.5523 6 17C6 16.4477 6.44772 16 7 16C7.55228 16 8 16.4477 8 17Z" fill="#1C274C"></path> <path d="M8 13C8 13.5523 7.55228 14 7 14C6.44772 14 6 13.5523 6 13C6 12.4477 6.44772 12 7 12C7.55228 12 8 12.4477 8 13Z" fill="#1C274C"></path> </g></svg>
                                                <Text className="txtMeetingTitle" weight='bold' as="h1">{element.subject}</Text>
                                            </div>
                                            <div>
                                                <Text className="meetingDate">{element.start} - {element.end} </Text>
                                            </div>
                                            <div>
                                                <Text className="organizerName">{element.organizer}</Text>
                                            </div>
                                            <div className="btnCard">
                                                <Button appearance="primary" disabled={!element.condition} onClick={fetchRecordingTranscript(element.subject, element.onlineMeetingId, element.transcriptsId, element.recordingId)}>Fetch Recording & Transcript</Button>
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