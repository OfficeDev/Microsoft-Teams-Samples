import React, { useState, useEffect, useRef } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Button, Spinner } from '@fluentui/react-components';
import io from "socket.io-client";

/**
 * RecordingTranscript Component
 * 
 * This React component provides a user interface for viewing Microsoft Teams call recordings
 * and transcripts. It fetches existing recording and transcript data from Microsoft Graph API
 * via Socket.IO communication and displays them to the user.
 * 
 * Features:
 * - Fetch existing transcripts and recordings via Microsoft Graph API
 * - Real-time transcript display with speaker identification
 * - Video recording playback
 * - Loading states and error handling
 * - Responsive UI with status messages

 */
function RecordingTranscript() {

    const [fetchMessage, setFetchMessage] = useState(""); // Store fetch operation status
    const [allTranscripts, setAllTranscripts] = useState([]); // Store all transcript entries
    const [allRecordings, setAllRecordings] = useState([]); // Store all recording entries
    const [isLoadingAllData, setIsLoadingAllData] = useState(false);
    const [isLoadingTranscripts, setIsLoadingTranscripts] = useState(false);
    const [isLoadingRecordings, setIsLoadingRecordings] = useState(false);
    const [transcriptsLoaded, setTranscriptsLoaded] = useState(false);
    const [recordingsLoaded, setRecordingsLoaded] = useState(false);
    
    // Pagination state
    const [transcriptsNextLink, setTranscriptsNextLink] = useState(null);
    const [recordingsNextLink, setRecordingsNextLink] = useState(null);
    const [hasMoreTranscripts, setHasMoreTranscripts] = useState(false);
    const [hasMoreRecordings, setHasMoreRecordings] = useState(false);
    const [isLoadingNext, setIsLoadingNext] = useState(false);
    const [currentPage, setCurrentPage] = useState(1);

    // Declare new state variables that are required to get and set the connection.
    const [socket, setSocket] = useState(io());

    /**
     * Effect Hook: Initialize WebSocket Connection
     * 
     * Creates a new Socket.IO connection when the component mounts.
     * This connection is used to receive fetched transcript and recording data from the API server.
     */
    useEffect(() => {
        setSocket(io());     
    }, []);



    /**
     * Effect Hook: WebSocket Event Listeners
     * 
     * Sets up event listeners for WebSocket communication to handle:
     * 1. Connection events
     * 2. Transcript data reception from Microsoft Graph API (with pagination)
     * 3. Recording data reception and video blob creation from Microsoft Graph API (with pagination)
     * 4. Next page data handling for pagination
     */
    useEffect(() => {
        if (!socket) return;
            socket.on('connection', () => {
                socket.connect();
            });

        // Listen for transcripts data (initial load and pagination)
        socket.on('transcriptsData', (data) => {
            console.log('Transcripts received:', data);
            
            if (data.isNextPage) {
                // Append to existing transcripts for next page
                setAllTranscripts(prev => [...prev, ...data.transcripts]);
            } else {
                // Replace for initial load
                setAllTranscripts(data.transcripts);
            }
            
            setTranscriptsNextLink(data.nextLink);
            setHasMoreTranscripts(data.hasNext);
            setIsLoadingTranscripts(false);
            setTranscriptsLoaded(true);
        });

        // Listen for recordings data (initial load and pagination)
        socket.on('recordingsData', async (data) => {
            console.log('Recordings received:', data);
            setIsLoadingRecordings(true);
            
            // Process recordings to create video URLs
            const processedRecordings = [];
            for (const recording of data.recordings) {
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
            
            if (data.isNextPage) {
                // Append to existing recordings for next page
                setAllRecordings(prev => [...prev, ...processedRecordings]);
            } else {
                // Replace for initial load
                setAllRecordings(processedRecordings);
            }
            
            setRecordingsNextLink(data.nextLink);
            setHasMoreRecordings(data.hasNext);
            setIsLoadingRecordings(false);
            setRecordingsLoaded(true);
        });

        // Listen for next page transcripts data
        socket.on('nextTranscriptsData', (data) => {
            console.log('Next transcripts received:', data);
            setAllTranscripts(prev => [...prev, ...data.transcripts]);
            setTranscriptsNextLink(data.nextLink);
            setHasMoreTranscripts(data.hasNext);
        });

        // Listen for next page recordings data
        socket.on('nextRecordingsData', async (data) => {
            console.log('Next recordings received:', data);
            
            // Process recordings to create video URLs
            const processedRecordings = [];
            for (const recording of data.recordings) {
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
                    processedRecordings.push(recording);
                }
            }
            
            setAllRecordings(prev => [...prev, ...processedRecordings]);
            setRecordingsNextLink(data.nextLink);
            setHasMoreRecordings(data.hasNext);
            setIsLoadingNext(false);
        });
    
    }, [socket]);

    /**
     * Function to fetch initial transcript and recording data (first 10 items)
     */
    const fetchAllData = async () => {
        setIsLoadingAllData(true);
        setIsLoadingTranscripts(true);
        setIsLoadingRecordings(true);
        setTranscriptsLoaded(false);
        setRecordingsLoaded(false);
        setCurrentPage(1);
        
        // Reset pagination state
        setTranscriptsNextLink(null);
        setRecordingsNextLink(null);
        setHasMoreTranscripts(false);
        setHasMoreRecordings(false);
        
        try {
            const response = await fetch('/fetchingTranscriptsandRecordings', {
                method: 'POST',
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ isNextPage: false })
            });
            
            if (response.ok) {
                setFetchMessage("Successfully fetched first 10 transcript and recording items from Microsoft Graph API");
            } else {
                setFetchMessage("Error fetching data from Microsoft Graph API");
            }
        } catch (err) {
            console.error(err);
            setFetchMessage("Error fetching data: " + err.message);
            setIsLoadingAllData(false);
            setIsLoadingTranscripts(false);
            setIsLoadingRecordings(false);
        }
    };

    /**
     * Function to fetch next page of transcripts and recordings (10 items per page)
     */
    const fetchNextPage = async () => {
        if (!hasMoreTranscripts && !hasMoreRecordings) {
            setFetchMessage("No more data to load");
            return;
        }

        setIsLoadingNext(true);
        
        try {
            const response = await fetch('/fetchNextPage', {
                method: 'POST',
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    transcriptsNextLink: transcriptsNextLink,
                    recordingsNextLink: recordingsNextLink
                })
            });
            
            if (response.ok) {
                const data = await response.json();
                setCurrentPage(prev => prev + 1);
                setFetchMessage(`Successfully loaded ${data.transcripts.count} more transcripts and ${data.recordings.count} more recordings`);
                
                // Hide success message after 2 seconds
                setTimeout(() => {
                    setFetchMessage("");
                }, 2000);
            } else {
                setFetchMessage("Error fetching next page from Microsoft Graph API");
                setIsLoadingNext(false);
            }
        } catch (err) {
            console.error(err);
            setFetchMessage("Error fetching next page: " + err.message);
            setIsLoadingNext(false);
        }
    };

    // Effect to update overall loading state
    useEffect(() => {
        if (transcriptsLoaded && recordingsLoaded) {
            setIsLoadingAllData(false);
            // Hide success message after data is fully loaded
            if (fetchMessage === "Successfully fetched all transcript and recording data from Microsoft Graph API") {
                setTimeout(() => {
                    setFetchMessage("");
                }, 2000); // Hide after 2 seconds
            }
        }
    }, [transcriptsLoaded, recordingsLoaded, fetchMessage]);

    /**
     * Effect Hook: Automatic Data Fetching
     * 
     * When the component mounts and Teams context is available, this effect:
     * 1. Automatically fetches existing transcript and recording data from Microsoft Graph API
     * 2. Handles the responses from the API requests
     * 3. Updates the UI with fetch operation status messages
     * 
     * This provides immediate access to available call recordings and transcripts
     * without requiring user interaction to see existing data.
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
     * 1. Fetch operation status message (success/error notification)
     * 2. Fetch data button to manually retrieve all existing data from Microsoft Graph API
     * 3. Welcome message (shown when no data has been retrieved)
     * 4. Recording and transcript display areas (shown when data is available)
     * 5. Paginated lists of all available recordings and transcripts
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

            {fetchMessage && (
                <div style={{ 
                    marginTop: "10px", 
                    marginBottom: "20px",
                    padding: "12px 16px",
                    borderRadius: "8px",
                    border: fetchMessage.includes("Error") ? "1px solid #ff4444" : "1px solid #28a745",
                    backgroundColor: fetchMessage.includes("Error") ? "#ffebee" : "#e8f5e9",
                    color: fetchMessage.includes("Error") ? "#c62828" : "#2e7d32",
                    fontWeight: "500",
                    fontSize: "14px",
                    boxShadow: "0 2px 4px rgba(0,0,0,0.1)"
                }}>
                    {fetchMessage}
                </div>
            )}
            
            {!allRecordings.length && !allTranscripts.length && !isLoadingAllData && (
                <div>
                     <p className="welcome-message">
                        Welcome to Adhoc Calls Transcript & Recording Viewer! This application retrieves and displays existing call recordings and transcripts from Microsoft Teams. Click "Fetch Recording & Transcript" to view available call data from your Teams meetings.
                        </p>
                </div>
            )}

            {/* Recording & Transcript Pairs Section */}
            {(allRecordings.length > 0 || allTranscripts.length > 0) && (
                <div style={{ marginBottom: '30px' }}>
                    {(() => {
                        // Create proper pairs by matching callId
                        const allCallIds = new Set([
                            ...allRecordings.map(r => r.callId),
                            ...allTranscripts.map(t => t.callId)
                        ]);
                        
                        const callIdsArray = Array.from(allCallIds);
                        
                        return (
                            <>
                                <h3 style={{ color: '#6264a7', borderBottom: '2px solid #6264a7', paddingBottom: '10px', marginBottom: '20px' }}>
                                    Recordings & Transcripts
                                </h3>
                                <div style={{ marginTop: '20px' }}>
                                    {callIdsArray.map((callId, index) => {
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
                                                border: '1px solid #e9ecef'
                                            }}>
                                                <em>No recording available for this recording</em>
                                            </div>
                                        )}
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
                        })}
                                </div>
                            </>
                        );
                    })()}
                    
                    {/* Pagination Section */}
                    {(allRecordings.length > 0 || allTranscripts.length > 0) && (hasMoreTranscripts || hasMoreRecordings) && (
                        <div style={{ 
                            marginTop: '20px', 
                            textAlign: 'center',
                            borderTop: '1px solid #e1e1e1',
                            paddingTop: '20px'
                        }}>
                            <Button 
                                onClick={fetchNextPage} 
                                disabled={isLoadingNext || (!hasMoreTranscripts && !hasMoreRecordings)}
                                style={{ 
                                    backgroundColor: '#6264a7', 
                                    color: 'white',
                                    padding: '10px 20px',
                                    fontSize: '14px'
                                }}
                            >
                                {isLoadingNext ? 'Loading Next...' : 'Next'}
                            </Button>
                            {isLoadingNext && <Spinner size="small" style={{ marginLeft: '10px' }} />}
                            
                            <div style={{ 
                                marginTop: '10px', 
                                fontSize: '12px', 
                                color: '#666',
                                fontStyle: 'italic'
                            }}>
                                Page {currentPage} • Showing {allTranscripts.length} transcripts, {allRecordings.length} recordings
                                {(hasMoreTranscripts || hasMoreRecordings) && " • More available"}
                            </div>
                        </div>
                    )}
                </div>
            )}
        </div>
    );
}

export default RecordingTranscript;
