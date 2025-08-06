import React, { useState, useEffect, useRef } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Spinner } from '@fluentui/react-components';

function RecordingTranscript() {
    // Common query params (applies to both meeting and adhoc call)
    const [querySubject, setQuerySubject] = useState(null);

    // Online meeting params (for backwards compatibility)
    const [queryOnlineMeetingId, setQueryOnlineMeetingId] = useState(null);

    // Adhoc call params
    const [queryCallId, setQueryCallId] = useState(null);

    // Shared
    const [queryTranscriptsId, setQueryTranscriptsId] = useState(null);
    const [queryRecordingId, setQueryRecordingId] = useState(null);

    // Loading, UI, and result state
    const [loadTranscriptsData, setLoadTranscriptsData] = useState(null);
    const [loading, setLoading] = useState(false);
    const [loadingTranscripts, setLoadingTranscripts] = useState(false);
    const [loadingRecording, setLoadingRecording] = useState(false);
    const videoRef = useRef(null);

    // Component mount: read params (runs once)
    useEffect(() => {
        microsoftTeams.app.initialize().then(() => {
            extractQueryParameters();
        });
    }, []);

    // When params change (runs when params extracted)
    useEffect(() => {
        if (!queryTranscriptsId && !queryRecordingId) return;
        ssoAuthentication();
    }, [queryOnlineMeetingId, queryCallId, queryTranscriptsId, queryRecordingId]);

    // Extracts params from URL and sets state
    const extractQueryParameters = () => {
        setLoading(true);
        const queryParams = new URLSearchParams(window.location.search);
        setQuerySubject(queryParams.get('subject'));

        // Adhoc call query
        setQueryCallId(queryParams.get('callId'));

        // Online meeting query
        setQueryOnlineMeetingId(queryParams.get('onlineMeetingId'));

        // Shared
        setQueryTranscriptsId(queryParams.get('transcriptsId'));
        setQueryRecordingId(queryParams.get('recordingId'));
        setLoading(false);
    };

    // Perform SSO then fetch transcript & recording
    const ssoAuthentication = () => {
        getClientSideToken()
            .then((clientSideToken) => {
                // If "adhoc call" style
                if (queryCallId) {
                    getAdhocRecordingValue(clientSideToken);
                    getAdhocTranscriptsValue(clientSideToken);
                } else {
                    getMeetingRecordingValue(clientSideToken);
                    getMeetingTranscriptsValue(clientSideToken);
                }
            })
            .catch(() => { });
    };

    // Get client side token.
    const getClientSideToken = () => {
        return new Promise((resolve, reject) => {
            microsoftTeams.authentication.getAuthToken()
                .then(resolve)
                .catch(error => reject("Error getting token: " + error));
        });
    };

    // -- ONLINE MEETING REQUESTS --
    const getMeetingTranscriptsValue = (clientSideToken) => {
        if (queryOnlineMeetingId && queryTranscriptsId)
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
                        formatTranscriptData(data);
                    });
            });
    };

    const getMeetingRecordingValue = (clientSideToken) => {
        if (queryOnlineMeetingId && queryRecordingId)
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
                        setLoadingRecording(false);
                        const videoUrl = URL.createObjectURL(blob);
                        videoRef.current.src = videoUrl;
                    });
            });
    };

    // -- ADHOC CALL REQUESTS --
    const getAdhocTranscriptsValue = (clientSideToken) => {
        if (queryCallId && queryTranscriptsId)
            microsoftTeams.app.getContext().then(() => {
                setLoadingTranscripts(true);
                fetch(`/getAdhocCallTranscript?transcriptId=${queryTranscriptsId}&callId=${queryCallId}&ssoToken=${clientSideToken}`, {
                    method: 'get',
                    headers: {
                        'Accept': 'application/json; charset=utf-8',
                        'Content-Type': 'application/json;charset=UTF-8'
                    }
                })
                    .then(response => response.json())
                    .then(data => {
                        setLoadingTranscripts(false);
                        formatTranscriptData(data);
                    });
            });
    };

    const getAdhocRecordingValue = (clientSideToken) => {
        if (queryCallId && queryRecordingId)
            microsoftTeams.app.getContext().then(() => {
                setLoadingRecording(true);
                fetch(`/getAdhocCallRecording?recordingId=${queryRecordingId}&callId=${queryCallId}&ssoToken=${clientSideToken}`, {
                    method: 'get',
                    headers: {
                        'Accept': 'application/json; charset=utf-8',
                        'Content-Type': 'application/json;charset=UTF-8'
                    }
                })
                    .then(response => response.blob())
                    .then(blob => {
                        setLoadingRecording(false);
                        const videoUrl = URL.createObjectURL(blob);
                        videoRef.current.src = videoUrl;
                    });
            });
    };

    // *** Parse VTT transcript data for display
    const formatTranscriptData = (data) => {
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
        const formattedOutput = formattedLines.join('<br/>');
        setLoadTranscriptsData(formattedOutput);
    };

    // ===== RENDER =====
    return (
        <div>
            <div className="svgCalender">
                <svg className="calendarSVG" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M14 22H10C6.22876 22 4.34315 22 3.17157 20.8284C2 19.6569 2 17.7712 2 14V12C2 8.22876 2 6.34315 3.17157 5.17157C4.34315 4 6.22876 4 10 4H14C17.7712 4 19.6569 4 20.8284 5.17157C22 6.34315 22 8.22876 22 12V14C22 17.7712 22 19.6569 20.8284 20.8284C20.1752 21.4816 19.3001 21.7706 18 21.8985" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M7 4V2.5" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M17 4V2.5" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M21.5 9H16.625H10.75M2 9H5.875" stroke="#1C274C" stroke-width="1.5" stroke-linecap="round"></path> <path d="M18 17C18 17.5523 17.5523 18 17 18C16.4477 18 16 17.5523 16 17C16 16.4477 16.4477 16 17 16C17.5523 16 18 16.4477 18 17Z" fill="#1C274C"></path> <path d="M18 13C18 13.5523 17.5523 14 17 14C16.4477 14 16 13.5523 16 13C16 12.4477 16.4477 12 17 12C17.5523 12 18 12.4477 18 13Z" fill="#1C274C"></path> <path d="M13 17C13 17.5523 12.5523 18 12 18C11.4477 18 11 17.5523 11 17C11 16.4477 11.4477 16 12 16C12.5523 16 13 16.4477 13 17Z" fill="#1C274C"></path> <path d="M13 13C13 13.5523 12.5523 14 12 14C11.4477 14 11 13.5523 11 13C11 12.4477 11.4477 12 12 12C12.5523 12 13 12.4477 13 13Z" fill="#1C274C"></path> <path d="M8 17C8 17.5523 7.55228 18 7 18C6.44772 18 6 17.5523 6 17C6 16.4477 6.44772 16 7 16C7.55228 16 8 16.4477 8 17Z" fill="#1C274C"></path> <path d="M8 13C8 13.5523 7.55228 14 7 14C6.44772 14 6 13.5523 6 13C6 12.4477 6.44772 12 7 12C7.55228 12 8 12.4477 8 13Z" fill="#1C274C"></path> </g></svg>

            </div>
            <div>
                <h4 className="txtSubject">{querySubject}</h4>
            </div>
            <div className="mainRecordTrans">
                <div className="divRecording">
                    {loadingRecording ? (
                        <div style={{ paddingTop: '20%' }}>
                            <Spinner label="Loading Recordings..." size="small" />
                        </div>
                    ) :
                        <video ref={videoRef} className="videoPlay" controls />}
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
