import React, { useState, useEffect, useRef } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Spinner, Button } from '@fluentui/react-components';

function RecordingTranscript() {

    // State variable for query parameter: subject
    const [querySubject, setQuerySubject] = useState(null);

    // State variable for query parameter: online meeting ID
    const [queryOnlineMeetingId, setQueryOnlineMeetingId] = useState(null);
 
    // State variable for query parameter: transcripts ID
    const [queryTranscriptsId, setQueryTranscriptsId] = useState(null);
 
    // State variable for query parameter: recording ID
    const [queryRecordingId, setQueryRecordingId] = useState(null);
 
    // State variable to control the loading of transcripts data
    const [loadTranscriptsData, setLoadTranscriptsData] = useState(null);

    // State variable to control the visibility of the send to magic notes button
    const [CanSendToMagicNotes, setCanSendToMagicNotes] = useState(false);

    const [loading, setLoading] = useState(false);

    const [loadingTranscripts, setLoadingTranscripts] = useState(false);

    const [loadingRecording, setLoadingRecording] = useState(false);

    const videoRef = useRef(null);

    // Initialize the component and extract query parameters when it mounts
    useEffect(() => {
        microsoftTeams.app.initialize().then(() => {
            extractQueryParameters();
        });
    }, [])

    // Effect Hook 1: Triggered when `queryOnlineMeetingId` or `queryTranscriptsId` or `queryRecordingId` change
    useEffect(() => {
        ssoAuthentication();
    }, [queryOnlineMeetingId, queryTranscriptsId, queryRecordingId])

    // Tab sso authentication.
    const ssoAuthentication = () => {
        getClientSideToken()
            .then((clientSideToken) => {
                    getMeetingRecordingValue(clientSideToken);
                    getMeetingTranscriptsValue(clientSideToken);
            })
            .catch((error) => {
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


    // Function to extract and set query parameters
    const extractQueryParameters = () => {
        setLoading(true);
        // Function to extract and display query parameters
        const queryParams = new URLSearchParams(window.location.search);
        // Access individual query parameters
        setQuerySubject(queryParams.get('subject'));
        setQueryOnlineMeetingId(queryParams.get('onlineMeetingId'));
        setQueryTranscriptsId(queryParams.get('transcriptsId'));
        setQueryRecordingId(queryParams.get('recordingId'));
        setLoading(false);
    };

    // Retrieve meeting transcripts using client-side token
    const getMeetingTranscriptsValue = (clientSideToken) => {
        if (queryOnlineMeetingId != null && queryTranscriptsId != null)
        microsoftTeams.app.getContext().then(() => {
            setLoadingTranscripts(true);
            fetch(`/getMeetingTranscripts?transcriptId=${queryTranscriptsId}&meetingId=${queryOnlineMeetingId}&ssoToken=${clientSideToken}`, {
                method: 'get',
                headers: {
                    'Accept': 'application/json; charset=utf-8',
                    'Content-Type': 'application/json;charset=UTF-8'
                }
            })
                .then(response => response.json())
                .then(data => {
                    setLoadingTranscripts(false);
                    const lines = data.split('\n');
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
                })
        });
    }

    // Retrieve meeting recording using client-side token
    const getMeetingRecordingValue = (clientSideToken) => {
        if (queryOnlineMeetingId != null && queryRecordingId != null)
        microsoftTeams.app.getContext().then(() => {
            setLoadingRecording(true)
            fetch(`/getMeetingRecordings?recordingId=${queryRecordingId}&meetingId=${queryOnlineMeetingId}&ssoToken=${clientSideToken}`, {
                method: 'get',
                headers: {
                    'Accept': 'application/json; charset=utf-8',
                    'Content-Type': 'application/json;charset=UTF-8'
                }
            })
            .then(response => response.blob())
            .then(blob => {
                setLoadingRecording(false)
                setCanSendToMagicNotes(true);
                const videoUrl = URL.createObjectURL(blob);
                videoRef.current.src = videoUrl;
            })
        });
    }

    // Send to Magic Notes
    const sendToMagicNotes = () => {
        alert('Sending to Magic Notes…');
    }

    return (

        <div>
            <div className="svgCalender">
                <svg className="calendarSVG" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M14 22H10C6.22876 22 4.34315 22 3.17157 20.8284C2 19.6569 2 17.7712 2 14V12C2 8.22876 2 6.34315 3.17157 5.17157C4.34315 4 6.22876 4 10 4H14C17.7712 4 19.6569 4 20.8284 5.17157C22 6.34315 22 8.22876 22 12V14C22 17.7712 22 19.6569 20.8284 20.8284C20.1752 21.4816 19.3001 21.7706 18 21.8985" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M7 4V2.5" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M17 4V2.5" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M21.5 9H16.625H10.75M2 9H5.875" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M18 17C18 17.5523 17.5523 18 17 18C16.4477 18 16 17.5523 16 17C16 16.4477 16.4477 16 17 16C17.5523 16 18 16.4477 18 17Z" fill="#1C274C"></path> <path d="M18 13C18 13.5523 17.5523 14 17 14C16.4477 14 16 13.5523 16 13C16 12.4477 16.4477 12 17 12C17.5523 12 18 12.4477 18 13Z" fill="#1C274C"></path> <path d="M13 17C13 17.5523 12.5523 18 12 18C11.4477 18 11 17.5523 11 17C11 16.4477 11.4477 16 12 16C12.5523 16 13 16.4477 13 17Z" fill="#1C274C"></path> <path d="M13 13C13 13.5523 12.5523 14 12 14C11.4477 14 11 13.5523 11 13C11 12.4477 11.4477 12 12 12C12.5523 12 13 12.4477 13 13Z" fill="#1C274C"></path> <path d="M8 17C8 17.5523 7.55228 18 7 18C6.44772 18 6 17.5523 6 17C6 16.4477 6.44772 16 7 16C7.55228 16 8 16.4477 8 17Z" fill="#1C274C"></path> <path d="M8 13C8 13.5523 7.55228 14 7 14C6.44772 14 6 13.5523 6 13C6 12.4477 6.44772 12 7 12C7.55228 12 8 12.4477 8 13Z" fill="#1C274C"></path> </g></svg>
            </div>
            <div>
                <h4 className="txtSubject">{querySubject}</h4>
            </div>
            <div class="mainRecordTrans">
                <div className="divRecording">
                {loadingRecording ? (
                            <div style={{ paddingTop: '20%' }}>
                                <Spinner label="Loading Recordings..." size="small" />
                            </div>
                        ) : (
                    <div>
                        <video ref={videoRef} className="videoPlay" controls />
                        {CanSendToMagicNotes && (
                            <div className="magicNotesButtonRow">
                                <Button
                                    appearance="primary"
                                    aria-label="Send to Magic Notes"
                                    onClick={sendToMagicNotes}
                                >
                                    Send to Magic Notes
                                </Button>
                            </div>
                        )}
                    </div>
                )}
                </div>
                <div className="divTranscripts">
                    <h4>Transcripts</h4>
                    {loadingTranscripts ? (
                                <div className='container'>
                                    <Spinner label="Loading Transcript..." size="small" />
                                </div>
                            ) :
                    <p style={{ whiteSpace: 'pre-wrap' }} dangerouslySetInnerHTML={{ __html: loadTranscriptsData }} />}
                </div>
            </div>
        </div>
    );
}

export default RecordingTranscript;