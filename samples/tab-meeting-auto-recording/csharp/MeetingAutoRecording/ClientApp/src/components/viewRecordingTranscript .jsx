import React, { useState, useEffect, useRef } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Spinner } from '@fluentui/react-components';

function RecordingTranscript() {

    // State variable for query parameter: subject
    const [querySubject, setQuerySubject] = useState();

    // State variable for query parameter: online meeting ID
    const [queryOnlineMeetingId, setQueryOnlineMeetingId] = useState();

    // State variable for query parameter: transcripts ID
    const [queryTranscriptsId, setQueryTranscriptsId] = useState();

    // State variable for query parameter: recording ID
    const [queryRecordingId, setQueryRecordingId] = useState();

    // State variable to control the loading of transcripts data
    const [loadTranscriptsData, setLoadTranscriptsData] = useState();

    // State variable to manage loading state for a general operation.
    const [loading, setLoading] = useState(false);

    // State variable to manage loading state specifically for transcripts.
    const [loadingTranscripts, setLoadingTranscripts] = useState(false);

    // State variable to manage loading state specifically for recordings.
    const [loadingRecording, setLoadingRecording] = useState(false);

    // A ref to store a reference to a video element, allowing direct manipulation in the DOM.
    const videoRef = useRef(null);

    // Initialize the component and extract query parameters when it mounts
    useEffect(() => {
        microsoftTeams.app.initialize().then(() => {
            extractQueryParameters();
        });
    }, [])

    // Effect Hook 1: Triggered when `queryOnlineMeetingId` or `queryTranscriptsId` change
    useEffect(() => {
        // Perform Single Sign-On (SSO) authentication for meeting transcripts
        ssoAuthenticationMeetingTranscripts();
    }, [queryOnlineMeetingId, queryTranscriptsId])


    // Effect Hook 2: Triggered when `queryOnlineMeetingId` or `queryRecordingId` change
    useEffect(() => {
        // Perform Single Sign-On (SSO) authentication for meeting recording
        ssoAuthenticationMeetingRecording();
    }, [queryOnlineMeetingId, queryRecordingId])


    // Function for Single Sign-On (SSO) authentication related to meeting transcripts
    const ssoAuthenticationMeetingTranscripts = () => {
        setLoadingTranscripts(true);
        // Obtain a client-side token for authentication
        getClientSideToken()
            .then((clientSideToken) => {
                // Retrieve meeting transcripts using the obtained client-side token
                return getMeetingTranscriptsValue(clientSideToken);
            })
            .catch((error) => {
                // Handle any errors that occur during the authentication process
                // This section can include error handling logic or error reporting
            });
    }

    // Function for obtaining a client-side token using Microsoft Teams authentication
    const getClientSideToken = () => {
        return new Promise((resolve, reject) => {
            // Use the Microsoft Teams SDK to request an authentication token
            microsoftTeams.authentication.getAuthToken()
                .then((result) => {
                    // Resolve the promise with the obtained token
                    resolve(result);
                })
                .catch((error) => {
                    // Reject the promise with an error message if token retrieval fails
                    reject("Error getting token: " + error);
                });
        });
    }

    // Function to extract and set query parameters
    const extractQueryParameters = () => {
        setLoading(true);
        // Function to extract and display query parameters
        const queryParams = new URLSearchParams(window.location.search);

        // Set the 'querySubject' state variable with the value from the 'subject' query parameter
        setQuerySubject(queryParams.get('subject'));

        // Set the 'queryOnlineMeetingId' state variable with the value from the 'onlineMeetingId' query parameter
        setQueryOnlineMeetingId(queryParams.get('onlineMeetingId'));

        // Set the 'queryTranscriptsId' state variable with the value from the 'transcriptsId' query parameter
        setQueryTranscriptsId(queryParams.get('transcriptsId'));

        // Set the 'queryRecordingId' state variable with the value from the 'recordingId' query parameter
        setQueryRecordingId(queryParams.get('recordingId'));

        setLoading(false);


    };

    // Function for Single Sign-On (SSO) authentication related to meeting recording
    const ssoAuthenticationMeetingRecording = () => {
        setLoadingRecording(true);
        // Obtain a client-side token for authentication
        getClientSideToken()
            .then((clientSideToken) => {
                // Retrieve meeting recording using the obtained client-side token
                return getMeetingRecordingValue(clientSideToken);
            })
            .catch((error) => {
                // Handle any errors that occur during the authentication process
                // This section can include error handling logic or error reporting
            });
    }

    // Function for fetching meeting transcripts using a client-side token
    const getMeetingTranscriptsValue = (clientSideToken) => {
        // Check if both 'queryOnlineMeetingId' and 'queryTranscriptsId' are not null
        if (queryOnlineMeetingId != null && queryTranscriptsId != null) {
            // Retrieve the context from Microsoft Teams
            microsoftTeams.app.getContext().then(() => {
                // Make a POST request to the '/getMeetingTranscripts' endpoint
                fetch('/getMeetingTranscripts', {
                    method: 'POST',
                    body: JSON.stringify({ 'meetingId': queryOnlineMeetingId, 'transcriptsId': queryTranscriptsId }),
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
                        // Check if the response JSON is not empty
                        if (responseJson !== "") {
                            // Parse the JSON response and update the 'loadTranscriptsData' state
                            let jsonResult = JSON.parse(responseJson);

                            const lines = jsonResult.split('\n');
                            const formattedLines = [];
                            for (let i = 0; i < lines.length; i++) {
                                const line = lines[i];
                                const match = line.match(/<v\s([^>]*)>(.*?)<\/v>/);
                                if (match) {
                                    const speaker = match[1];
                                    const text = match[2];
                                    formattedLines.push(`<b>${speaker}</b> : ${text}`);
                                }
                            }
                            const formattedOutput = formattedLines.join('<br/>')

                            setLoadTranscriptsData(formattedOutput);

                            setLoadingTranscripts(false);
                        }
                    });
            });
        }
    }

    // Function for fetching meeting recordings using a client-side token
    const getMeetingRecordingValue = (clientSideToken) => {
        // Check if both 'queryOnlineMeetingId' and 'queryRecordingId' are not null
        if (queryOnlineMeetingId != null && queryRecordingId != null) {
            // Retrieve the context from Microsoft Teams
            microsoftTeams.app.getContext().then(() => {
                // Make a POST request to the '/getMeetingRecording' endpoint
                fetch('/getMeetingRecording', {
                    method: 'POST',
                    body: JSON.stringify({ 'meetingId': queryOnlineMeetingId, 'recordingId': queryRecordingId }),
                    headers: {
                        'Accept': 'application/json; charset=utf-8',
                        'Content-Type': 'application/json;charset=UTF-8',
                        'Authorization': "Bearer " + clientSideToken
                    }
                })
                    .then(response => response.blob())
                    .then(blob => {
                        setLoadingRecording(false);
                        // Create a URL for the video blob and set it as the source of 'videoRef'
                        const videoUrl = URL.createObjectURL(blob);
                        videoRef.current.src = videoUrl;
                    })
            });
        }
    }

    return (
        <div>
            {loading ? (
                <div className="loadingIcon">
                    <Spinner label="Loading meetings, fetching Transcript and Recordings..." size="large" />
                </div>
            ) : (
                <div>
                    <div className="svgCalender">
                        <svg className="calendarSVG" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M14 22H10C6.22876 22 4.34315 22 3.17157 20.8284C2 19.6569 2 17.7712 2 14V12C2 8.22876 2 6.34315 3.17157 5.17157C4.34315 4 6.22876 4 10 4H14C17.7712 4 19.6569 4 20.8284 5.17157C22 6.34315 22 8.22876 22 12V14C22 17.7712 22 19.6569 20.8284 20.8284C20.1752 21.4816 19.3001 21.7706 18 21.8985" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M7 4V2.5" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M17 4V2.5" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M21.5 9H16.625H10.75M2 9H5.875" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M18 17C18 17.5523 17.5523 18 17 18C16.4477 18 16 17.5523 16 17C16 16.4477 16.4477 16 17 16C17.5523 16 18 16.4477 18 17Z" fill="#1C274C"></path> <path d="M18 13C18 13.5523 17.5523 14 17 14C16.4477 14 16 13.5523 16 13C16 12.4477 16.4477 12 17 12C17.5523 12 18 12.4477 18 13Z" fill="#1C274C"></path> <path d="M13 17C13 17.5523 12.5523 18 12 18C11.4477 18 11 17.5523 11 17C11 16.4477 11.4477 16 12 16C12.5523 16 13 16.4477 13 17Z" fill="#1C274C"></path> <path d="M13 13C13 13.5523 12.5523 14 12 14C11.4477 14 11 13.5523 11 13C11 12.4477 11.4477 12 12 12C12.5523 12 13 12.4477 13 13Z" fill="#1C274C"></path> <path d="M8 17C8 17.5523 7.55228 18 7 18C6.44772 18 6 17.5523 6 17C6 16.4477 6.44772 16 7 16C7.55228 16 8 16.4477 8 17Z" fill="#1C274C"></path> <path d="M8 13C8 13.5523 7.55228 14 7 14C6.44772 14 6 13.5523 6 13C6 12.4477 6.44772 12 7 12C7.55228 12 8 12.4477 8 13Z" fill="#1C274C"></path> </g></svg>
                    </div>
                    <div>
                        <h4 className="txtSubject">{querySubject}</h4>
                    </div>
                    <div class="mainRecordTrans">
                        {loadingRecording ? (
                            <div className="loadingIconRecordings">
                                    <Spinner label="Loading Recordings..." size="small" />
                            </div>
                        ) : (
                            <div className="divRecording">
                                <video ref={videoRef} className="videoPlay" controls />
                            </div>
                        )}
                        <div className="divTranscripts">
                            <h4>Transcripts</h4>
                            {loadingTranscripts ? (
                                <div>
                                        <Spinner label="Loading Transcript..." size="small" />
                                </div>
                            ) : (
                                <p style={{ whiteSpace: 'pre-wrap' }} dangerouslySetInnerHTML={{ __html: loadTranscriptsData }} />
                            )}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default RecordingTranscript;
