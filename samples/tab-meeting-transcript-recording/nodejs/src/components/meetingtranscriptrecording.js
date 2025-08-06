import React, { useState, useEffect } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Button, Text, Card, Spinner } from '@fluentui/react-components';
import { CardBody } from 'reactstrap';
import io from "socket.io-client";

const MeetingTranscriptRecording = () => {
    // ONLINE MEETINGS STATE
    const [meetingData, setMeetingData] = useState([]);
    // ADHOC CALLS STATE
    const [adhocData, setAdhocData] = useState([]);
    // Common UI states
    const [IsConsentButtonVisible, setIsConsentButtonVisible] = useState(false);
    const [IsLoginVisible, setIsLoginVisible] = useState(true);
    const [IsCardVisible, setIsCardVisible] = useState(false);
    const [loading, setLoading] = useState(false);
    const [loginAdminAccount, setloginAdminAccount] = useState(false);
    const [socket, setSocket] = useState(io());
    const [eventUpdated, setEventUpdated] = useState(false);

    // Interval to get updated data
    useEffect(() => {
        const intervalId = setInterval(getUpdatedData, 60000);
        return () => clearInterval(intervalId);
    }, []);

    // Build/rebuild socket connection on mount
    useEffect(() => setSocket(io()), []);

    // Socket subscription — both meeting and adhoc updates
    useEffect(() => {
        if (!socket) return;
        socket.on('connection', () => socket.connect());
        socket.on("message", data => {
            // Data could be either meeting or adhoc: detect and set separately
            if (Array.isArray(data) && data.length > 0 && data[0]?.onlineMeetingId) {
                setMeetingData(data);
            } else {
                setAdhocData(data);
            }
        });
    }, [socket]);

    // Online Meetings: fetch updates
    const getUpdatedData = () => {
        return new Promise((resolve, reject) => {
            microsoftTeams.app.getContext().then((context) => {
                fetch('/getUpdatedEvents', {
                    method: 'get',
                    headers: { "Content-Type": "application/text" },
                    cache: 'default'
                })
                    .then(response => response.json())
                    .then(data => {
                        if (data.eventDetails?.length) setMeetingData(data.eventDetails);
                        if (data.eventUpdated) {
                            setEventUpdated(true);
                            ssoAuthentication();
                        }
                    });
            });
        });
    };

    // Authentication and set up both types
    const ssoAuthentication = () => {
        if (!eventUpdated) setLoading(true);
        setIsLoginVisible(false);
        getClientSideToken()
            .then(clientSideToken => {
                // Fetch both meetings and adhoc data after authentication
                getMeetingServerSide(clientSideToken);
                getAdhocServerSide(clientSideToken);
                createMeetingSubscription(clientSideToken);
                createAdhocSubscription(clientSideToken);
            })
            .catch(error => {
                if (error === "invalid_grant") setIsConsentButtonVisible(true);
            });
    };

    const getClientSideToken = () => {
        return new Promise((resolve, reject) => {
            microsoftTeams.authentication.getAuthToken()
                .then(result => resolve(result))
                .catch(error => reject("Error getting token: " + error));
        });
    };

    // Fetch ONLINE MEETING data (sets Meeting cards)
    const getMeetingServerSide = (clientSideToken) => {
        microsoftTeams.app.getContext().then(() => {
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
                    if (data.error === "consent_required") {
                        setIsConsentButtonVisible(true);
                        setIsLoginVisible(false);
                        setLoading(false);
                    } else {
                        setMeetingData(data);
                        setIsLoginVisible(false);
                        setLoading(false);
                        setIsCardVisible(true);
                        setEventUpdated(false);
                        data.forEach(event => getMeetingTranscriptsIdRecordingId(clientSideToken, event.joinUrl));
                    }
                });
        });
    };

    // Fetch ADHOC CALL data (sets Adhoc cards)
    const getAdhocServerSide = (clientSideToken) => {
        microsoftTeams.app.getContext().then(() => {
            fetch('/getAdhocCalls?ssoToken=' + clientSideToken, {
                method: 'get',
                headers: {
                    "Content-Type": "application/text",
                    "Authorization": "Bearer " + clientSideToken
                },
                cache: 'default'
            })
                .then(response => response.json())
                .then(data => {
                    if (data.error === "consent_required") {
                        setIsConsentButtonVisible(true);
                        setIsLoginVisible(false);
                        setLoading(false);
                    } else {
                        setAdhocData(data);
                        setIsCardVisible(true);
                        setLoading(false);
                        setEventUpdated(false);
                        data.forEach(call => getAdhocTranscriptsIdRecordingId(clientSideToken, call.callId));
                    }
                });
        });
    };

    // Populate Online Meeting transcript/recording IDs
    const getMeetingTranscriptsIdRecordingId = (clientSideToken, joinUrl) => {
        fetch(`/getMeetingTranscriptsIdRecordingId?joinUrl=${encodeURIComponent(joinUrl)}&ssoToken=${clientSideToken}`, {
            method: 'get',
            headers: {
                "Content-Type": "application/text",
                "Authorization": "Bearer " + clientSideToken
            },
            cache: 'default'
        })
            .then(response => response.json())
            .then(data => { if (Array.isArray(data)) setMeetingData(data); });
    };

    // Populate Adhoc Call transcript/recording IDs
    const getAdhocTranscriptsIdRecordingId = (clientSideToken, callId) => {
        fetch(`/getAdhocCallTranscriptsIdRecordingId?callId=${encodeURIComponent(callId)}&ssoToken=${clientSideToken}`, {
            method: 'get',
            headers: {
                "Content-Type": "application/text",
                "Authorization": "Bearer " + clientSideToken
            },
            cache: 'default'
        })
            .then(response => response.json())
            .then(data => { if (Array.isArray(data)) setAdhocData(data); });
    };

    // Create subscriptions for new Online Meetings
    const createMeetingSubscription = (clientSideToken) => {
        fetch('/createsubscription?ssoToken=' + clientSideToken, {
            method: 'post',
            headers: {
                "Content-Type": "application/text",
                "Authorization": "Bearer " + clientSideToken
            },
            cache: 'default'
        }).then(response => response.json())
            .then(data => {
                if (data.error === "consent_required") {
                    setIsConsentButtonVisible(true);
                    setIsLoginVisible(false);
                    setLoading(false);
                }
            });
    };

    // Create subscriptions for new Adhoc Calls
    const createAdhocSubscription = (clientSideToken) => {
        // For each adhoc call, create a subscription
        adhocData.forEach(call => {
            fetch(`/createAdhocSubscription?callId=${call.callId}&ssoToken=${clientSideToken}`, {
                method: 'post',
                headers: {
                    "Content-Type": "application/text",
                    "Authorization": "Bearer " + clientSideToken
                },
                cache: 'default'
            }).then(response => response.json())
                .then(data => {
                    if (data.error === "consent_required") {
                        setIsConsentButtonVisible(true);
                        setIsLoginVisible(false);
                        setLoading(false);
                    }
                });
        });
    };

    // Consent flow
    const requestConsent = () => {
        getToken()
            .then(() => {
                setIsConsentButtonVisible(false);
                getClientSideToken()
                    .then(clientSideToken => {
                        // Replicate full data flow after consent
                        getMeetingServerSide(clientSideToken);
                        getAdhocServerSide(clientSideToken);
                    });
            });
    };

    const getToken = () => {
        return new Promise((resolve, reject) => {
            microsoftTeams.authentication.authenticate({
                url: window.location.origin + "/auth-start",
                width: 600,
                height: 535
            })
                .then(result => { resolve(result); setLoading(true); })
                .catch(reason => {
                    reject(reason);
                    setloginAdminAccount(true);
                    setIsConsentButtonVisible(false);
                    setIsLoginVisible(false);
                    setLoading(false);
                });
        });
    };

    // StageView launchers (each type fetches its own IDs)
    const fetchRecordingTranscriptMeeting = (subject, meetingId, transcriptsId, recordingId) => () => {
        openTaskModule("Recording and Transcript", `${window.location.origin}/RecordingTranscript?subject=${subject}&onlineMeetingId=${meetingId}&transcriptsId=${transcriptsId}&recordingId=${recordingId}`);
    };
    const fetchRecordingTranscriptAdhoc = (subject, callId, transcriptsId, recordingId) => () => {
        openTaskModule("Recording and Transcript", `${window.location.origin}/RecordingTranscript?subject=${subject}&callId=${callId}&transcriptsId=${transcriptsId}&recordingId=${recordingId}`);
    };
    function openTaskModule(title, url) {
        const maxWidth = 1200;
        const maxHeight = 1000;
        let taskInfo = {
            title,
            height: Math.min(window.innerHeight - 500, maxHeight),
            width: Math.min(window.innerWidth - 1000, maxWidth),
            url,
            completionBotId: null,
        };
        microsoftTeams.dialog.url.open(taskInfo, (err, result) => {
            if (err) console.log(`Submit handler - err: ${err}`);
        });
    }

    return (
        <div className="">
            <div className="btnLogin">
                {IsLoginVisible &&
                    <Button appearance="primary" onClick={ssoAuthentication}>Sign-In</Button>
                }
                {IsConsentButtonVisible &&
                    <>
                        <div id="divError">Please click on consent button</div>
                        <Button appearance="primary" onClick={requestConsent}>Consent</Button>
                    </>
                }
                {loginAdminAccount && <h3>Please login with admin account.</h3>}
            </div>
            <div className="cardColumns">
                {/* ONLINE MEETINGS */}
                <div className="column">
                    <div className="mainCard">
                        <h3>Online Meetings</h3>
                        {loading && <div className="loadingIcon"><Spinner label="Loading meetings, fetching Transcript and Recordings..." size="large" /></div>}
                        {IsCardVisible && meetingData.length > 0 && meetingData.map((element, index) => (
                            <div key={index} className="divMainCard">
                                <Card>
                                    <CardBody className="main1Card">
                                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                            <div>
                                                <Text className="txtMeetingTitle" weight='bold' as="h1">{element.subject}</Text>
                                            </div>
                                            {element.notify ?
                                                <svg className="calendarSVG">{/* icon here */}</svg>
                                                : <div></div>
                                            }
                                        </div>
                                        <div>
                                            <Text className="meetingDate">{element.start} - {element.end} </Text>
                                        </div>
                                        <div>
                                            <Text className="organizerName">{element.organizer}</Text>
                                        </div>
                                        <div className="btnCard">
                                            <Button appearance="primary" disabled={!element.condition} onClick={fetchRecordingTranscriptMeeting(element.subject, element.onlineMeetingId, element.transcriptsId, element.recordingId)}>Fetch Recording & Transcript</Button>
                                        </div>
                                    </CardBody>
                                </Card>
                            </div>
                        ))}
                    </div>
                </div>
                {/* ADHOC CALLS */}
                <div className="column">
                    <div className="mainCard">
                        <h3>Adhoc Calls</h3>
                        {loading && <div className="loadingIcon"><Spinner label="Loading adhoc calls, fetching Transcript and Recordings..." size="large" /></div>}
                        {IsCardVisible && adhocData.length > 0 && adhocData.map((element, index) => (
                            <div key={index} className="divMainCard">
                                <Card>
                                    <CardBody className="main1Card">
                                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                            <div>
                                                <Text className="txtMeetingTitle" weight='bold' as="h1">{element.subject}</Text>
                                            </div>
                                            {element.notify ?
                                                <svg className="calendarSVG">{/* icon here */}</svg>
                                                : <div></div>
                                            }
                                        </div>
                                        <div>
                                            <Text className="meetingDate">{element.start} - {element.end} </Text>
                                        </div>
                                        <div>
                                            <Text className="organizerName">{element.organizer}</Text>
                                        </div>
                                        <div className="btnCard">
                                            <Button appearance="primary" disabled={!element.condition} onClick={fetchRecordingTranscriptAdhoc(element.subject, element.callId, element.transcriptsId, element.recordingId)}>Fetch Recording & Transcript</Button>
                                        </div>
                                    </CardBody>
                                </Card>
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default MeetingTranscriptRecording;
