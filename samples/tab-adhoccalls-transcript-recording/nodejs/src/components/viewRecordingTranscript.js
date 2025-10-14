/**
 * RecordingTranscript Component
 * 
 * Manages fetching, processing, and displaying Teams meeting recordings and transcripts.
 * Uses Socket.IO for real-time updates and Microsoft Graph API for data retrieval.
 */
import React, { useState, useEffect } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Button, Spinner } from '@fluentui/react-components';
import io from "socket.io-client";

// State Hooks
function RecordingTranscript() {
    const [fetchMessage, setFetchMessage] = useState("");
    const [allTranscripts, setAllTranscripts] = useState([]);
    const [allRecordings, setAllRecordings] = useState([]);
    const [isLoadingAllData, setIsLoadingAllData] = useState(false);
    const [isLoadingTranscripts, setIsLoadingTranscripts] = useState(false);
    const [isLoadingRecordings, setIsLoadingRecordings] = useState(false);
    const [transcriptsLoaded, setTranscriptsLoaded] = useState(false);
    const [recordingsLoaded, setRecordingsLoaded] = useState(false);
    const [transcriptsNextLink, setTranscriptsNextLink] = useState(null);
    const [recordingsNextLink, setRecordingsNextLink] = useState(null);
    const [hasMoreTranscripts, setHasMoreTranscripts] = useState(false);
    const [hasMoreRecordings, setHasMoreRecordings] = useState(false);
    const [isLoadingNext, setIsLoadingNext] = useState(false);
    const [currentPage, setCurrentPage] = useState(1);
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
    * processRecordings
    * 
    * Downloads each recording as a blob and creates a video URL for rendering.
    * @param {Array} recordings - List of recording objects with URL and token
    * @returns {Promise<Array>} Processed recordings with videoUrl added
    */
    const processRecordings = async (recordings) => {
        const processedRecordings = [];
        for (const recording of recordings) {
            try {
                const response = await fetch(recording.url, {
                    headers: { Authorization: `Bearer ${recording.token}` }
                });
                if (response.ok) {
                    const blob = await response.blob();
                    processedRecordings.push({ ...recording, videoUrl: URL.createObjectURL(blob) });
                }
            } catch (err) {
                console.error('Error processing recording:', err);
                processedRecordings.push(recording);
            }
        }
        return processedRecordings;
    };

    /**
    * Effect Hook: Setup Socket.IO Event Handlers
    * 
    * Subscribes to events from the server for transcripts and recordings data.
    * Handles initial data and pagination updates.
    */
    useEffect(() => {
        if (!socket) return;

        socket.on('connection', () => socket.connect());

        socket.on('transcriptsData', (data) => {
            setAllTranscripts(data.isNextPage ? prev => [...prev, ...data.transcripts] : data.transcripts);
            setTranscriptsNextLink(data.nextLink);
            setHasMoreTranscripts(data.hasNext);
            setIsLoadingTranscripts(false);
            setTranscriptsLoaded(true);
        });

        socket.on('recordingsData', async (data) => {
            setIsLoadingRecordings(true);
            const processedRecordings = await processRecordings(data.recordings);
            setAllRecordings(data.isNextPage ? prev => [...prev, ...processedRecordings] : processedRecordings);
            setRecordingsNextLink(data.nextLink);
            setHasMoreRecordings(data.hasNext);
            setIsLoadingRecordings(false);
            setRecordingsLoaded(true);
        });

        socket.on('nextTranscriptsData', (data) => {
            setAllTranscripts(prev => [...prev, ...data.transcripts]);
            setTranscriptsNextLink(data.nextLink);
            setHasMoreTranscripts(data.hasNext);
        });

        socket.on('nextRecordingsData', async (data) => {
            const processedRecordings = await processRecordings(data.recordings);
            setAllRecordings(prev => [...prev, ...processedRecordings]);
            setRecordingsNextLink(data.nextLink);
            setHasMoreRecordings(data.hasNext);
            setIsLoadingNext(false);
        });
    }, [socket]);

    /**
     * fetchAllData
     * 
     * Initiates fetching of all transcripts and recordings from the server.
     * Resets pagination and loading states.
     */
    const fetchAllData = async () => {
        setIsLoadingAllData(true);
        setIsLoadingTranscripts(true);
        setIsLoadingRecordings(true);
        setTranscriptsLoaded(false);
        setRecordingsLoaded(false);
        setCurrentPage(1);
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

            setFetchMessage(response.ok ? "Successfully fetched data from Microsoft Graph API" : "Error fetching data from Microsoft Graph API");
        } catch (err) {
            console.error(err);
            setFetchMessage("Error fetching data: " + err.message);
            setIsLoadingAllData(false);
            setIsLoadingTranscripts(false);
            setIsLoadingRecordings(false);
        }
    };

    /**
     * fetchNextPage
     * 
     * Fetches the next page of transcripts and recordings if available.
     * Updates pagination state and loading indicators.
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
                setTimeout(() => setFetchMessage(""), 2000);
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

    /**
     * Effect Hook: Handle Overall Loading State
     * 
     * Monitors if both transcripts and recordings are loaded to disable global loading indicator.
     */
    useEffect(() => {
        if (transcriptsLoaded && recordingsLoaded) {
            setIsLoadingAllData(false);
            if (fetchMessage === "Successfully fetched all transcript and recording data from Microsoft Graph API") {
                setTimeout(() => setFetchMessage(""), 2000);
            }
        }
    }, [transcriptsLoaded, recordingsLoaded, fetchMessage]);

    /**
     * Effect Hook: Fetch Data on Teams Context Initialization
     */
    useEffect(() => {
        microsoftTeams.app.getContext().then(() => {
            fetchAllData();
        });
    }, []);

    /**
    * renderRecordingContent
    * 
    * Returns JSX to render a recording video or placeholder based on loading state.
    */
    const renderRecordingContent = (recording, isLoading) => {
        if (recording?.videoUrl) {
            return <video src={recording.videoUrl} controls style={{ width: '100%', maxHeight: '250px', borderRadius: '4px' }} />;
        }
        const message = recording ? "Processing video..." : isLoading ? "Loading recordings..." : "No recording available";
        return (
            <div style={{ height: '250px', display: 'flex', alignItems: 'center', justifyContent: 'center', backgroundColor: '#f8f9fa', borderRadius: '4px', color: '#6c757d', fontStyle: 'italic', border: '1px solid #e9ecef' }}>
                {(recording && !recording.videoUrl) || isLoading ? <Spinner label={message} size="medium" /> : <em>{message}</em>}
            </div>
        );
    };

    /**
    * renderTranscriptContent
    * 
    * Returns JSX to render a transcript or placeholder based on loading state.
    */
    const renderTranscriptContent = (transcript, isLoading, loaded) => {
        if (transcript) {
            return (
                <div style={{ backgroundColor: 'white', padding: '15px', borderRadius: '4px', fontSize: '14px', lineHeight: '1.6', marginTop: '10px', border: '1px solid #e9ecef', maxHeight: '250px', overflow: 'auto' }}>
                    {transcript.formattedContent ? (
                        <div style={{ margin: '0' }} dangerouslySetInnerHTML={{ __html: transcript.formattedContent }} />
                    ) : transcript.content ? (
                        <pre style={{ whiteSpace: 'pre-wrap', margin: '0', fontFamily: 'inherit', fontSize: '14px', color: '#333' }}>{transcript.content}</pre>
                    ) : loaded ? (
                        <div style={{ height: '100px', display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#6c757d', fontStyle: 'italic' }}>No transcript content available</div>
                    ) : (
                        <div style={{ height: '100px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}><Spinner label="Processing transcript..." size="small" /></div>
                    )}
                </div>
            );
        }
        const message = isLoading && !loaded ? "Loading transcripts..." : "No transcript available";
        return (
            <div style={{ height: '250px', display: 'flex', alignItems: 'center', justifyContent: 'center', backgroundColor: '#f8f9fa', borderRadius: '4px', color: '#6c757d', fontStyle: 'italic', marginTop: '10px', border: '1px solid #e9ecef' }}>
                {isLoading && !loaded ? <Spinner label={message} size="medium" /> : <em>{message}</em>}
            </div>
        );
    };

    return (
        <div style={{ padding: '20px' }}>
            <div style={{ marginBottom: '20px' }}>
                <Button onClick={fetchAllData} disabled={isLoadingAllData} style={{ backgroundColor: '#6264a7', color: 'white', marginRight: '10px' }}>
                    {isLoadingAllData ? 'Loading...' : 'Fetch Recording & Transcript'}
                </Button>
                {isLoadingAllData && <Spinner size="small" style={{ marginLeft: '10px' }} />}
            </div>

            {fetchMessage && (
                <div style={{
                    marginTop: "10px", marginBottom: "20px", padding: "12px 16px", borderRadius: "8px",
                    border: fetchMessage.includes("Error") ? "1px solid #ff4444" : "1px solid #28a745",
                    backgroundColor: fetchMessage.includes("Error") ? "#ffebee" : "#e8f5e9",
                    color: fetchMessage.includes("Error") ? "#c62828" : "#2e7d32",
                    fontWeight: "500", fontSize: "14px", boxShadow: "0 2px 4px rgba(0,0,0,0.1)"
                }}>
                    {fetchMessage}
                </div>
            )}

            {!allRecordings.length && !allTranscripts.length && !isLoadingAllData && (
                <p className="welcome-message">
                    Welcome to Adhoc Calls Transcript & Recording Viewer! Click "Fetch Recording & Transcript" to view available call data from your Teams meetings.
                </p>
            )}

            {(allRecordings.length > 0 || allTranscripts.length > 0) && (
                <div style={{ marginBottom: '30px' }}>
                    <h3 style={{ color: '#6264a7', borderBottom: '2px solid #6264a7', paddingBottom: '10px', marginBottom: '20px' }}>
                        Recordings & Transcripts
                    </h3>
                    <div style={{ marginTop: '20px' }}>
                        {Array.from(new Set([...allRecordings.map(r => r.callId), ...allTranscripts.map(t => t.callId)])).map((callId, index) => {
                            const recording = allRecordings.find(r => r.callId === callId);
                            const transcript = allTranscripts.find(t => t.callId === callId);

                            return (
                                <div key={`pair-${callId}`} style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '20px', marginBottom: '30px', minHeight: '300px' }}>
                                    <div style={{ border: '1px solid #e1e1e1', borderRadius: '8px', padding: '15px', backgroundColor: '#f9f9f9' }}>
                                        <h4 style={{ marginTop: '0', color: '#333' }}>Recording {index + 1}</h4>
                                        {renderRecordingContent(recording, isLoadingRecordings)}
                                    </div>
                                    <div style={{ border: '1px solid #e1e1e1', borderRadius: '8px', padding: '15px', backgroundColor: '#f9f9f9' }}>
                                        <h4 style={{ marginTop: '0', color: '#333' }}>Transcript {index + 1}</h4>
                                        {renderTranscriptContent(transcript, isLoadingTranscripts, transcriptsLoaded)}
                                    </div>
                                </div>
                            );
                        })}
                    </div>

                    {(hasMoreTranscripts || hasMoreRecordings) && (
                        <div style={{ marginTop: '20px', textAlign: 'center', borderTop: '1px solid #e1e1e1', paddingTop: '20px' }}>
                            <Button onClick={fetchNextPage} disabled={isLoadingNext || (!hasMoreTranscripts && !hasMoreRecordings)} style={{ backgroundColor: '#6264a7', color: 'white', padding: '10px 20px', fontSize: '14px' }}>
                                {isLoadingNext ? 'Loading Next...' : 'Next'}
                            </Button>
                            {isLoadingNext && <Spinner size="small" style={{ marginLeft: '10px' }} />}
                            <div style={{ marginTop: '10px', fontSize: '12px', color: '#666', fontStyle: 'italic' }}>
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
