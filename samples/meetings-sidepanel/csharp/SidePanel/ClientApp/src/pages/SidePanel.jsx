/*!
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

import { useEffect, useState } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { SharedMap } from "fluid-framework";
import { LiveShareHost } from "@microsoft/teams-js";
import { LiveShareClient } from "@microsoft/live-share";
import { setMeetingContext, addAgendaTask, postAgenda } from "./services/agendaAPIHelper"

let containerValue;

const SidePanel = (props) => {

const agendaValueKey = "editor-value-key";
const [appTheme, setAppTheme] = useState("");
const [isFluidReady, setIsFluidReady] = useState(false);
const [notification, setNotification] = useState({ show: false, message: '', type: '' });

// Helper function to show notifications
const showNotification = (message, type = 'info') => {
    setNotification({ show: true, message, type });
    setTimeout(() => {
        setNotification({ show: false, message: '', type: '' });
    }, 5000); // Auto-hide after 5 seconds
};

    useEffect(() => {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.getContext().then((context) => {
                // Applying default theme from app context property
                switch (context.app.theme) {
                    case 'dark':
                        setAppTheme('theme-dark');
                        break;
                    case 'default':
                        setAppTheme('theme-light');
                        break;
                    case 'contrast':
                        setAppTheme('theme-contrast');
                        break;
                    default:
                        return setAppTheme('theme-light');
                }

                var userId = context.user.id;
                var meetingId = context.meeting?.id;
                var tenantId = context.user.tenant.id;
                var conversationId = context.chat?.id || context.channel?.id || context.meeting?.conversationId;

                // Try to initialize conversation context from Teams SDK
                if (conversationId) {
                    console.log('Initializing conversation from Teams context:', conversationId);
                    fetch(`${window.location.origin}/Home/InitializeConversation`, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({
                            conversationId: conversationId,
                            serviceUrl: 'https://smba.trafficmanager.net/amer/' // Default service URL
                        })
                    }).then(response => {
                        if (response.ok) {
                            console.log('Conversation initialized successfully');
                        }
                    }).catch(err => console.error('Error initializing conversation:', err));
                }

                agendaListPopulate();

                if (meetingId) {
                    setMeetingContext(userId, meetingId, tenantId).then((result) => {
                        if (result.data == true) {
                            document.getElementById("agendaButtonDiv").style.display = "block";
                            document.getElementById("publishAgendaButton").style.display = "block";
                        }
                        else {
                            document.getElementById("agendaButtonDiv").style.display = "none";
                            document.getElementById("publishAgendaButton").style.display = "none";
                        }
                    }).catch(err => {
                        console.error('Error checking organizer role:', err);
                        // Show agenda button anyway if role check fails
                        document.getElementById("agendaButtonDiv").style.display = "block";
                        document.getElementById("publishAgendaButton").style.display = "block";
                    });
                }
            });

            // Handle app theme when 'Teams' theme changes
            microsoftTeams.app.registerOnThemeChangeHandler(function (theme) {
                switch (theme) {
                    case 'dark':
                        setAppTheme('theme-dark');
                        break;
                    case 'default':
                        setAppTheme('theme-light');
                        break;
                    case 'contrast':
                        setAppTheme('theme-contrast');
                        break;
                    default:
                        return setAppTheme('theme-light');
                }
            });
        });
    }, []);

    // Initial setup for using fluid container.
    useEffect(() => {
        (async function () {

            let connection;
            window.localStorage.debug = "fluid:*";
            
            try {
                await microsoftTeams.app.initialize();
                const host = LiveShareHost.create();

                // Define Fluid document schema and create container
                const client = new LiveShareClient(host);
                const containerSchema = {
                    initialObjects: { editorMap: SharedMap }
                };

                function onContainerFirstCreated(container) {
                    // Set initial state of the editorMap.
                    var initalArray = ["Approve 5% dividend payment to shareholders.", "Increase research budget by 10%.", "Continue with WFH for next 3 months."];
                    container.initialObjects.editorMap.set(agendaValueKey, initalArray);
                }

                // Joining the container with default schema defined.
                const { container } = await client.joinContainer(containerSchema, onContainerFirstCreated);
                containerValue = container;
                containerValue.initialObjects.editorMap.on("valueChanged", updateEditorState);
                
                // Mark Fluid as ready
                setIsFluidReady(true);
                console.log('Fluid container initialized successfully');
                
                // Update the agenda list display
                agendaListPopulate();
            } catch (error) {
                console.error('Error initializing Fluid container:', error);
                setIsFluidReady(false);
                // Still show default agenda even if Fluid fails
                agendaListPopulate();
            }
        })();


    }, []);


    function agendaListPopulate() {
        var agendaValue;
        if (containerValue == null || containerValue == "") {
            agendaValue = ["Approve 5% dividend payment to shareholders.", "Increase research budget by 10%.", "Continue with WFH for next 3 months."];
        }
        else {
            agendaValue = containerValue.initialObjects.editorMap.get(agendaValueKey);
        }

        var divStart = "<ol type=\"1\">";
        agendaValue.forEach(x => {
            divStart += "<li>" + x + "</li>";
        });
        divStart += "</ol>";
        document.getElementById("agendaList").innerHTML = divStart;
    }

    function showAgendaInput() {
        document.getElementById("agendaInputDiv").style.display = "block";
        document.getElementById("agendaButtonDiv").style.display = "none";
        document.getElementById("agendaInput").focus();
    }

    function addAgenda() {
        document.getElementById("agendaInputDiv").style.display = "none";
        document.getElementById("agendaButtonDiv").style.display = "block";
        var newAgendaItem = document.getElementById('agendaInput').value;
        
        if (!newAgendaItem || newAgendaItem.trim() === '') {
            showNotification('Please enter an agenda item.', 'warning');
            document.getElementById("agendaInputDiv").style.display = "block";
            document.getElementById("agendaButtonDiv").style.display = "none";
            document.getElementById("agendaInput").focus();
            return;
        }

        let taskInfo = {
            title: newAgendaItem
        };

        // API call to save agenda.
        addAgendaTask(taskInfo)
            .then(() => {
                showNotification('Agenda item added successfully!', 'success');
            })
            .catch(err => {
                console.error('Error adding agenda task:', err);
                showNotification('Failed to save agenda item.', 'error');
            });

        // Update Fluid container if initialized
        if (containerValue && containerValue.initialObjects && containerValue.initialObjects.editorMap) {
            var editorMap = containerValue.initialObjects.editorMap;
            var agendas = editorMap.get(agendaValueKey) || [];
            agendas.push(newAgendaItem);
            editorMap.set(agendaValueKey, agendas);
        } else {
            // Fallback: if Fluid is not ready, just update locally
            console.warn('Fluid container not ready, agenda will be added on next sync');
            agendaListPopulate();
        }

        // Clear input
        document.getElementById('agendaInput').value = '';
    }

    // This method is called to publish the agenda in meeting chat.
    function publishAgenda() {
        // Check if container is initialized
        if (!containerValue || !containerValue.initialObjects || !containerValue.initialObjects.editorMap) {
            showNotification('Loading agenda data, please wait a moment and try again.', 'warning');
            return;
        }

        const agendaValue = containerValue.initialObjects.editorMap.get(agendaValueKey);
        
        postAgenda(agendaValue)
            .then((response) => {
                showNotification('Agenda published successfully to meeting chat!', 'success');
            })
            .catch((error) => {
                console.error('Error publishing agenda:', error);
                if (error.response && error.response.data) {
                    const errorData = error.response.data;
                    if (errorData.message) {
                        showNotification(errorData.message, 'error');
                    } else if (typeof errorData === 'string') {
                        showNotification(errorData, 'error');
                    } else {
                        showNotification('Error publishing agenda. Please send a message in the chat first to initialize the bot.', 'error');
                    }
                } else {
                    showNotification('Error publishing agenda. Please make sure you are in a meeting and try again.', 'error');
                }
            });
    }

    // This method is called whenever the shared state is updated.
    const updateEditorState = () => {
        const agendaValue = containerValue.initialObjects.editorMap.get(agendaValueKey);
        agendaListPopulate();
    };

    return (
        <>
            <div className={appTheme}>
                <div className="agendaTitle">
                    Agenda
                </div>
                
                {/* Notification Banner */}
                {notification.show && (
                    <div style={{
                        padding: '12px',
                        marginBottom: '10px',
                        borderRadius: '4px',
                        backgroundColor: 
                            notification.type === 'success' ? '#dff6dd' :
                            notification.type === 'error' ? '#fde7e9' :
                            notification.type === 'warning' ? '#fff4ce' :
                            '#e1f5fe',
                        color: 
                            notification.type === 'success' ? '#107c10' :
                            notification.type === 'error' ? '#d13438' :
                            notification.type === 'warning' ? '#8a6d3b' :
                            '#0277bd',
                        border: `1px solid ${
                            notification.type === 'success' ? '#107c10' :
                            notification.type === 'error' ? '#d13438' :
                            notification.type === 'warning' ? '#f0ad4e' :
                            '#0277bd'
                        }`,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between'
                    }}>
                        <span>
                            {notification.type === 'success' && '? '}
                            {notification.type === 'error' && '? '}
                            {notification.type === 'warning' && '? '}
                            {notification.type === 'info' && '? '}
                            {notification.message}
                        </span>
                        <button 
                            onClick={() => setNotification({ show: false, message: '', type: '' })}
                            style={{
                                background: 'none',
                                border: 'none',
                                fontSize: '18px',
                                cursor: 'pointer',
                                padding: '0 5px',
                                color: 'inherit'
                            }}
                        >
                            ×
                        </button>
                    </div>
                )}

                {/* Loading Indicator */}
                {!isFluidReady && (
                    <div style={{ padding: '10px', backgroundColor: '#fff4ce', borderRadius: '4px', marginBottom: '10px', border: '1px solid #f0ad4e' }}>
                        ? Loading collaboration features...
                    </div>
                )}
                
                <div id="agendaButtonDiv">
                    <button id="agendaButton" onClick={showAgendaInput}>Add New Agenda Item</button>
                </div>
                <div id="agendaInputDiv" style={{ display: 'none' }}>
                    <input type="text" id="agendaInput" /><br />
                    <button id="addAgendaButton" onClick={addAgenda}>Add</button>
                </div>
                <div id="list">
                    <ol type="1" id="agendaList">
                    </ol>
                </div>
                <button 
                    id="publishAgendaButton" 
                    onClick={publishAgenda}
                    title={!isFluidReady ? "Please wait for collaboration features to load" : "Publish agenda to meeting chat"}
                >
                    Publish Agenda {!isFluidReady && '(Loading...)'}
                </button>
            </div>
        </>
    );
};

export default SidePanel;