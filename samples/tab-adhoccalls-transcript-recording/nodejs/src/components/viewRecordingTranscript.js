import React, { useState, useEffect, useRef } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Button, Spinner } from '@fluentui/react-components';
import io from "socket.io-client";

function RecordingTranscript() {

    const [loadTranscriptsData, setLoadTranscriptsData] = useState(null);
    const [loadingTranscripts, setLoadingTranscripts] = useState(false);
    const [loadingRecording, setLoadingRecording] = useState(false);
    const [subscriptionMessage, setSubscriptionMessage] = useState(""); // 🔹 Store subscription status
    const [hasData, setHasData] = useState(false); // Track if any data (transcript or recording) has been received
    const [hasRecordingData, setHasRecordingData] = useState(false); // Track if recording data has been received
    const videoRef = useRef(null);

    // Declare new state variables that are required to get and set the connection.
    const [socket, setSocket] = useState(io());

    // Builds the socket connection, mapping it to /io
    useEffect(() => {
        setSocket(io());     
    }, []);

    // subscribe to the socket event
    useEffect(() => {
        if (!socket) return;
            socket.on('connection', () => {
                socket.connect();
            });

            // receive a message from the server
        socket.on("transcript", data => {
            console.log("Transcript received:", data);
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
            setHasData(true); // Mark that we have received data
            setLoadingTranscripts(false); // stop spinner
        });

        socket.on('recording', ({ callId, recordingId, videoData }) => {
            console.log(`Received recording: CallId=${callId}, RecordingId=${recordingId}`);
            console.log(`Video data length: ${videoData ? videoData.length : 'undefined'}`);

            if (!videoData) {
                console.error('No video data received');
                return;
            }

            try {
                // Convert Base64 to byte array
                const byteCharacters = atob(videoData);
                const byteNumbers = new Array(byteCharacters.length);
                for (let i = 0; i < byteCharacters.length; i++) {
                    byteNumbers[i] = byteCharacters.charCodeAt(i);
                }
                const byteArray = new Uint8Array(byteNumbers);
                const blob = new Blob([byteArray], { type: 'video/mp4' });
                const videoUrl = URL.createObjectURL(blob);
                if (videoRef.current) {
                    if (videoRef.current.src && videoRef.current.src.startsWith('blob:')) {
                        URL.revokeObjectURL(videoRef.current.src);
                    }
                    videoRef.current.src = videoUrl;
                    videoRef.current.load();
                }
                setHasData(true); // Mark that we have received data
                setHasRecordingData(true); // Mark that we have recording data
                setLoadingRecording(false);
                
            } catch (error) {
                console.error('Error processing video data:', error);
                setLoadingRecording(false);
            }
    });
    
    }, [socket]);

    // Automatically create BOTH subscriptions when component mounts
    useEffect(() => {
        microsoftTeams.app.getContext().then(() => {
            Promise.all([
                fetch('/createAdhocCallTranscriptSubscription', {
                    method: 'POST',
                    headers: { "Content-Type": "application/json" }
                }),
                fetch('/createAdhocCallRecordingSubscription', {
                    method: 'POST',
                    headers: { "Content-Type": "application/json" }
                })
            ])
            .then(async ([transcriptRes, recordingRes]) => {
                const transcriptData = await transcriptRes.json();
                const recordingData = await recordingRes.json();

                let message = "";
                if (transcriptData.status) {
                    message += `Transcript: ${transcriptData.message}  `;
                }
                if (recordingData.status) {
                    message += `Recording: ${recordingData.message}`;
                }
                setSubscriptionMessage(message.trim());
            })
            .catch(err => {
                console.error(err);
                setSubscriptionMessage("Error creating subscriptions: " + err.message);
            });
        });
    }, []);

    return (
        <div>
            {subscriptionMessage && (
                <div style={{ 
                    marginTop: "10px", 
                    marginBottom: "20px",
                    padding: "12px 16px",
                    borderRadius: "8px",
                    border: subscriptionMessage.includes("Error") ? "1px solid #ff4444" : "1px solid #28a745",
                    backgroundColor: subscriptionMessage.includes("Error") ? "#ffebee" : "#e8f5e9",
                    color: subscriptionMessage.includes("Error") ? "#c62828" : "#2e7d32",
                    fontWeight: "500",
                    fontSize: "14px",
                    boxShadow: "0 2px 4px rgba(0,0,0,0.1)"
                }}>
                    {subscriptionMessage}
                </div>
            )}
            
            {/* Show welcome message only when no data has been received */}
            {!hasData && (
                <div>
                     <p className="welcome-message">Welcome to Adhoc Calls Transcript Recording Join the meeting, then click "Start Recording" or "Start Transcript" to capture video and text. 
                     After the meeting ends, wait a few seconds for the recording and transcript to be ready.
                        </p>
                </div>
            )}

            {/* Show video and transcript sections only when data has been received */}
            {hasData && (
                <div className="mainRecordTrans">
                    <div className="divRecording">
                        {!hasRecordingData ? (
                                <div style={{ paddingTop: '20%' }}>
                                    <Spinner label="Waiting for Recording..." size="small" />
                                </div>
                            ) : (
                                loadingRecording ? (
                                    <div style={{ paddingTop: '20%' }}>
                                        <Spinner label="Loading Recordings..." size="small" />
                                    </div>
                                ) :
                                <video ref={videoRef} className="videoPlay" controls />
                            )}
                    </div>

                    <div className="divTranscripts">
                        <h4>Transcripts</h4>
                        {!loadTranscriptsData ? (
                                    <div className='container'>
                                        <Spinner label="Waiting for Transcript..." size="small" />
                                    </div>
                                ) : (
                                    loadingTranscripts ? (
                                        <div className='container'>
                                            <Spinner label="Loading Transcript..." size="small" />
                                        </div>
                                    ) :
                                    <p style={{ whiteSpace: 'pre-wrap' }} dangerouslySetInnerHTML={{ __html: loadTranscriptsData }} />
                                )}
                    </div>
                </div>
            )}
        </div>
    );
}

export default RecordingTranscript;
