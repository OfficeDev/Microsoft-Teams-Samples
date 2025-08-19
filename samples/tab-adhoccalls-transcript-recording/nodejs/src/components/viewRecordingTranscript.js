import React, { useState, useEffect, useRef } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Button, Spinner } from '@fluentui/react-components';
import io from "socket.io-client";

/**
 * RecordingTranscript Component
 * 
 * This React component provides a user interface for viewing Microsoft Teams call recordings
 * and transcripts. It establishes a WebSocket connection to receive real-time updates about
 * call recordings and transcripts, and displays them to the user.
 * 
 * Features:
 * - Real-time transcript display with speaker identification
 * - Video recording playback
 * - Automatic subscription to recording and transcript notifications
 * - Loading states and error handling
 * - Responsive UI with status messages
 */
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

    /**
     * Effect Hook: Initialize WebSocket Connection
     * 
     * Creates a new Socket.IO connection when the component mounts.
     * This connection is used to receive real-time transcript and recording data.
     */
    useEffect(() => {
        setSocket(io());     
    }, []);



    /**
     * Effect Hook: WebSocket Event Listeners
     * 
     * Sets up event listeners for WebSocket communication to handle:
     * 1. Connection events
     * 2. Transcript data reception and formatting
     * 3. Recording data reception and video blob creation
     */
    useEffect(() => {
        if (!socket) return;
            socket.on('connection', () => {
                socket.connect();
            });

            // receive a message from the server
        socket.on("transcript", data => {
            console.log("Transcript received:", data);

            const regex = /<v\s+([^>]+)>(.*?)<\/v>/g;
            let match;
            const formattedLines = [];

            while ((match = regex.exec(data)) !== null) {
                const speaker = match[1].trim();
                const text = match[2].trim();
                formattedLines.push(`<b>${speaker}</b> : ${text}`);
            }

            // Deduplicate with Set
            setLoadTranscriptsData(prev => {
                const existingLines = prev
                    ? prev.split("<br/>").map(line => line.trim()).filter(l => l.length > 0)
                    : [];

                const newUniqueLines = [...new Set([...existingLines, ...formattedLines])];

                return newUniqueLines.join("<br/>");
            });

            setHasData(true);
            setLoadingTranscripts(false);
        });

        socket.on('recordingAvailable', async ({ callId, recordingId, url, token }) => {
            console.log(`Recording available: CallId=${callId}, RecordingId=${recordingId}`);

        try {
            const response = await fetch(url, {
                headers: { Authorization: `Bearer ${token}` }
            });

            if (!response.ok) {
                throw new Error(`Failed to fetch recording: ${response.statusText}`);
            }
            const blob = await response.blob();
            const videoUrl = URL.createObjectURL(blob);
            if (videoRef.current) {
                if (videoRef.current.src?.startsWith('blob:')) {
                    URL.revokeObjectURL(videoRef.current.src);
                }
                videoRef.current.src = videoUrl;
                videoRef.current.load();
            }
            setHasData(true);
            setHasRecordingData(true);
            setLoadingRecording(false);

        } catch (err) {
            console.error('Error fetching video:', err);
            setLoadingRecording(false);
        }
     });
    
    }, [socket]);

    /**
     * Effect Hook: Automatic Subscription Creation
     * 
     * When the component mounts and Teams context is available, this effect:
     * 1. Creates subscriptions for both transcript and recording notifications
     * 2. Handles the responses from both subscription requests
     * 3. Updates the UI with subscription status messages
     * 
     * The subscriptions enable the server to receive webhook notifications
     * when new recordings or transcripts become available.
     */
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

    /**
     * Component Render Method
     * 
     * Renders the user interface with the following sections:
     * 1. Subscription status message (success/error notification)
     * 2. Welcome message (shown when no data has been received)
     * 3. Recording and transcript display areas (shown when data is available)
     * 
     * The UI adapts based on the current state of data availability and loading status.
     */
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
            
            {!hasData && (
                <div>
                     <p className="welcome-message">
                        Welcome to Adhoc Calls Transcript Recording  Join the group call, then click "Start Recording" or "Start Transcript" to capture video and text. After the call ends, wait a few seconds for the recording and transcript to be ready.
                        </p>
                </div>
            )}
            
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
