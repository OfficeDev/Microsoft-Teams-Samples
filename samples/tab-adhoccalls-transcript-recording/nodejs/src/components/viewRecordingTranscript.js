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

    const [subscriptionMessage, setSubscriptionMessage] = useState(""); // 🔹 Store subscription status
    const [allTranscripts, setAllTranscripts] = useState([]); // Store all transcript entries
    const [allRecordings, setAllRecordings] = useState([]); // Store all recording entries
    const [isLoadingAllData, setIsLoadingAllData] = useState(false);
    const [isLoadingTranscripts, setIsLoadingTranscripts] = useState(false);
    const [isLoadingRecordings, setIsLoadingRecordings] = useState(false);
    const [transcriptsLoaded, setTranscriptsLoaded] = useState(false);
    const [recordingsLoaded, setRecordingsLoaded] = useState(false);

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

        // Listen for all transcripts data
        socket.on('allTranscriptsData', (data) => {
            console.log('All transcripts received:', data);
            
            // Remove duplicates based on callId and id combination
            const uniqueTranscripts = data.filter((transcript, index, self) => 
                index === self.findIndex(t => t.callId === transcript.callId && t.id === transcript.id)
            );
            
            setAllTranscripts(uniqueTranscripts);
            setIsLoadingTranscripts(false);
            setTranscriptsLoaded(true);
        });

        // Listen for all recordings data  
        socket.on('allRecordingsData', async (data) => {
            console.log('All recordings received:', data);
            setIsLoadingRecordings(true);
            
            // Remove duplicates based on callId and recordingId combination
            const uniqueRecordings = data.filter((recording, index, self) => 
                index === self.findIndex(r => r.callId === recording.callId && r.recordingId === recording.recordingId)
            );
            
            // Process recordings to create video URLs
            const processedRecordings = [];
            for (const recording of uniqueRecordings) {
                try {
                    const response = await fetch(recording.url, {
                        headers: { Authorization: `Bearer ${recording.token}` }
                    });

                    if (response.ok) {
                        const blob = await response.blob();
                        const videoUrl = URL.createObjectURL(blob);
                        processedRecordings.push({
                            ...recording,
                            videoUrl: videoUrl
                        });
                    }
                } catch (err) {
                    console.error('Error processing recording:', err);
                    // Add without video URL if failed
                    processedRecordings.push(recording);
                }
            }
            
            setAllRecordings(processedRecordings);
            setIsLoadingRecordings(false);
            setRecordingsLoaded(true);
        });
    
    }, [socket]);

    /**
     * Function to fetch all available transcript and recording data
     */
    const fetchAllData = async () => {
        setIsLoadingAllData(true);
        setIsLoadingTranscripts(true);
        setIsLoadingRecordings(true);
        setTranscriptsLoaded(false);
        setRecordingsLoaded(false);
        
        try {
            const response = await fetch('/createAdhocCallTranscriptSubscription', {
                method: 'POST',
                headers: { "Content-Type": "application/json" }
            });
            
            if (response.ok) {
                setSubscriptionMessage("Successfully fetched all transcript and recording data");
            } else {
                setSubscriptionMessage("Error fetching data");
            }
        } catch (err) {
            console.error(err);
            setSubscriptionMessage("Error fetching data: " + err.message);
            setIsLoadingAllData(false);
            setIsLoadingTranscripts(false);
            setIsLoadingRecordings(false);
        }
    };

    // Effect to update overall loading state
    useEffect(() => {
        if (transcriptsLoaded && recordingsLoaded) {
            setIsLoadingAllData(false);
        }
    }, [transcriptsLoaded, recordingsLoaded]);

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
            // Automatically fetch all data on component mount
            fetchAllData();
        });
    }, []);

    /**
     * Component Render Method
     * 
     * Renders the user interface with the following sections:
     * 1. Subscription status message (success/error notification)
     * 2. Fetch data button to manually retrieve all data
     * 3. Welcome message (shown when no data has been received)
     * 4. Recording and transcript display areas (shown when data is available)
     * 5. Lists of all available recordings and transcripts
     * 
     * The UI adapts based on the current state of data availability and loading status.
     */
    return (
        <div style={{ padding: '20px' }}>
            <div style={{ marginBottom: '20px' }}>
                <Button 
                    onClick={fetchAllData} 
                    disabled={isLoadingAllData}
                    style={{ 
                        backgroundColor: '#6264a7', 
                        color: 'white',
                        marginRight: '10px'
                    }}
                >
                    {isLoadingAllData ? 'Loading...' : 'Fetch Recording & Transcript'}
                </Button>
                {isLoadingAllData && <Spinner size="small" style={{ marginLeft: '10px' }} />}
            </div>

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
            
            {!allRecordings.length && !allTranscripts.length && !isLoadingAllData && (
                <div>
                     <p className="welcome-message">
                        Welcome to Adhoc Calls Transcript Recording! Join the group call, then click "Start Recording" or "Start Transcript" to capture video and text. After the call ends, wait a few seconds for the recording and transcript to be ready.
                        </p>
                </div>
            )}

            {/* Recording & Transcript Pairs Section */}
            {(allRecordings.length > 0 || allTranscripts.length > 0) && (
                <div style={{ marginBottom: '30px' }}>
                    <h3 style={{ color: '#6264a7', borderBottom: '2px solid #6264a7', paddingBottom: '10px' }}>
                        Recordings & Transcripts ({Math.max(allRecordings.length, allTranscripts.length)} items)
                    </h3>
                    <div style={{ marginTop: '20px' }}>
                        {(() => {
                            // Create proper pairs by matching callId
                            const allCallIds = new Set([
                                ...allRecordings.map(r => r.callId),
                                ...allTranscripts.map(t => t.callId)
                            ]);
                            
                            return Array.from(allCallIds).map((callId, index) => {
                                const recording = allRecordings.find(r => r.callId === callId);
                                const transcript = allTranscripts.find(t => t.callId === callId);
                                
                                // Debug logging
                                console.log(`Rendering pair ${index + 1} for callId ${callId}:`, {
                                    hasRecording: !!recording,
                                    hasTranscript: !!transcript,
                                    recordingId: recording?.recordingId,
                                    transcriptId: transcript?.id,
                                    transcriptContent: transcript?.formattedContent ? 'Has formatted content' : transcript?.content ? 'Has raw content' : 'No content',
                                    transcriptsLoaded,
                                    isLoadingTranscripts
                                });
                                
                                return (
                                <div key={`pair-${callId}`} className="recording-transcript-pair" style={{
                                    display: 'grid',
                                    gridTemplateColumns: '1fr 1fr',
                                    gap: '20px',
                                    marginBottom: '30px',
                                    minHeight: '300px'
                                }}>
                                    {/* Left Side - Recording */}
                                    <div style={{
                                        border: '1px solid #e1e1e1',
                                        borderRadius: '8px',
                                        padding: '15px',
                                        backgroundColor: '#f9f9f9'
                                    }}>
                                        <h4 style={{ marginTop: '0', color: '#333' }}>Recording {index + 1}</h4>
                                        {recording ? (
                                            recording.videoUrl ? (
                                                <video 
                                                    src={recording.videoUrl} 
                                                    controls 
                                                    style={{ width: '100%', maxHeight: '250px', borderRadius: '4px' }}
                                                />
                                            ) : (
                                                <div style={{ 
                                                    height: '250px', 
                                                    display: 'flex', 
                                                    alignItems: 'center', 
                                                    justifyContent: 'center',
                                                    backgroundColor: '#e9ecef',
                                                    borderRadius: '4px',
                                                    color: '#6c757d'
                                                }}>
                                                    <Spinner label="Processing video..." size="medium" />
                                                </div>
                                            )
                                        ) : isLoadingRecordings ? (
                                            <div style={{ 
                                                height: '250px', 
                                                display: 'flex', 
                                                alignItems: 'center', 
                                                justifyContent: 'center',
                                                backgroundColor: '#f8f9fa',
                                                borderRadius: '4px',
                                                color: '#6c757d',
                                                fontStyle: 'italic'
                                            }}>
                                                <Spinner label="Loading recordings..." size="medium" />
                                            </div>
                                        ) : null}
                                    </div>

                                    {/* Right Side - Transcript */}
                                    <div style={{
                                        border: '1px solid #e1e1e1',
                                        borderRadius: '8px',
                                        padding: '15px',
                                        backgroundColor: '#f9f9f9'
                                    }}>
                                        <h4 style={{ marginTop: '0', color: '#333' }}>Transcript {index + 1}</h4>
                                        {transcript ? (
                                            <div style={{ 
                                                backgroundColor: 'white', 
                                                padding: '15px', 
                                                borderRadius: '4px',
                                                fontSize: '14px',
                                                lineHeight: '1.6',
                                                marginTop: '10px',
                                                border: '1px solid #e9ecef',
                                                maxHeight: '250px',
                                                overflow: 'auto'
                                            }}>
                                                {transcript.formattedContent ? (
                                                    <div style={{ margin: '0' }} 
                                                         dangerouslySetInnerHTML={{ __html: transcript.formattedContent }} />
                                                ) : transcript.content ? (
                                                    <pre style={{ 
                                                        whiteSpace: 'pre-wrap', 
                                                        margin: '0', 
                                                        fontFamily: 'inherit',
                                                        fontSize: '14px',
                                                        color: '#333'
                                                    }}>{transcript.content}</pre>
                                                ) : transcriptsLoaded ? (
                                                    <div style={{ 
                                                        height: '100px',
                                                        display: 'flex', 
                                                        alignItems: 'center', 
                                                        justifyContent: 'center',
                                                        color: '#6c757d',
                                                        fontStyle: 'italic'
                                                    }}>
                                                        No transcript content available
                                                    </div>
                                                ) : (
                                                    <div style={{ 
                                                        height: '100px',
                                                        display: 'flex', 
                                                        alignItems: 'center', 
                                                        justifyContent: 'center'
                                                    }}>
                                                        <Spinner label="Processing transcript..." size="small" />
                                                    </div>
                                                )}
                                            </div>
                                        ) : isLoadingTranscripts && !transcriptsLoaded ? (
                                            <div style={{ 
                                                height: '250px', 
                                                display: 'flex', 
                                                alignItems: 'center', 
                                                justifyContent: 'center',
                                                backgroundColor: '#f8f9fa',
                                                borderRadius: '4px',
                                                color: '#6c757d',
                                                fontStyle: 'italic',
                                                marginTop: '10px',
                                                border: '1px solid #e9ecef'
                                            }}>
                                                <Spinner label="Loading transcripts..." size="medium" />
                                            </div>
                                        ) : (
                                            <div style={{ 
                                                height: '250px', 
                                                display: 'flex', 
                                                alignItems: 'center', 
                                                justifyContent: 'center',
                                                backgroundColor: '#f8f9fa',
                                                borderRadius: '4px',
                                                color: '#6c757d',
                                                fontStyle: 'italic',
                                                marginTop: '10px',
                                                border: '1px solid #e9ecef'
                                            }}>
                                                <em>No transcript available for this recording</em>
                                            </div>
                                        )}
                                    </div>
                                </div>
                            );
                        })})()}
                    </div>
                </div>
            )}
        </div>
    );
}

export default RecordingTranscript;
