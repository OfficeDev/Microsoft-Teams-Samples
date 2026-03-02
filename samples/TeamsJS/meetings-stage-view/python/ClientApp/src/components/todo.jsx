// <copyright file="todo.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect, useState } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import $ from "jquery";
import { LiveShareClient } from "@microsoft/live-share";
import { LiveShareHost } from "@microsoft/teams-js";
import { SharedMap } from "fluid-framework";

let containerValue;

// Return todo meeting data.
const Todo = props => {
    const editorValueKey = "meeting-details-todo";

    const [meetingDataArray, setMeetingDataArray] = useState([]);

    useEffect(() => {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.getContext().then((context) => {
                $.ajax({
                    url: "/getMeetingData?meetingId=" + context.meeting.id + "&status=todo",
                    type: "GET",
                    success: function (response) {
                        if (response.data) {
                            setMeetingDataArray(response.data);
                        }
                    },
                    error: function (xhr, textStatus, errorThrown) {
                        console.log("textStatus: " + textStatus + ", errorThrown:" + errorThrown);
                    },
                });

                if (context.page.frameContext == "sidePanel") {
                    $(".share-specific-part-button").show();
                }
                else {
                    $(".share-specific-part-button").hide();
                }
            });
        });
    }, []);

    // Initial setup for using fluid container.
    useEffect(() => {
        (async function () {
            await microsoftTeams.app.initialize();
            // Define Fluid document schema and create container
            const host = LiveShareHost.create();
            const client = new LiveShareClient(host);

            const containerSchema = {
                initialObjects: { editorMap: SharedMap }
            };

            function onContainerFirstCreated(container) {
                // Set initial state of the editorMap.
                container.initialObjects.editorMap.set(editorValueKey, meetingDataArray);
            }

            try {
                // Joining the container with default schema defined.
                const { container } = await client.joinContainer(containerSchema, onContainerFirstCreated);
                containerValue = container;
                containerValue.initialObjects.editorMap.on("valueChanged", updateEditorState);
            }
            catch (err) {
                console.log(err)
            };
        })();
    }, []);

    // This method is called whenever the shared state is updated.
    const updateEditorState = () => {
        const editorValue = containerValue.initialObjects.editorMap.get(editorValueKey);
        if (editorValue) {
            setMeetingDataArray(editorValue);
        }
    };

    // Save meeting data.
    const saveMeetingData = (meeting) => {
        $.ajax({
            url: "/saveMeetingData",
            type: "POST",
            data: JSON.stringify(meeting),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                console.log("POST Call success: saveMeetingData");
            },
            error: function (xhr, textStatus, errorThrown) {
                console.log("textStatus: " + textStatus + ", errorThrown:" + errorThrown);
            },
        });
    }

    // Update meeting data state.
    const updateState = (meetingData) => {
        var editorMap = containerValue.initialObjects.editorMap;

        let details = [...meetingDataArray];
        details.push(meetingData);

        saveMeetingData(meetingData);

        editorMap.set(editorValueKey, details);
    }


    // Task module to collect details from the meeting participants.
    const openTaskModule = (meetingStatus) => {
        let taskInfo = {
            title: "Status Details",
            size: {
                height: 250,
                width: 250,
            },
            url: `${window.location.origin}/home`,
        };

        microsoftTeams.app.getContext().then((context) => {
            // Invoking task module to collect status details from participants.
            microsoftTeams.dialog.url.open(taskInfo, (taskDetails) => {
                if (taskDetails.result?.taskDescription) {
                    let meeting = {
                        meetingId: context.meeting.id,
                        status: meetingStatus,
                        message: {
                            taskDescription: taskDetails.result.taskDescription,
                            userName: taskDetails.result.userName,
                        }
                    }

                    updateState(meeting);
                }
            });
        });
    };

    // Return element list of meeting data.
    const getMeetingDataList = () => {
        let elements = [];
        if (meetingDataArray) {
            meetingDataArray.forEach((meeting) => {
                elements.push(<div className="details">
                    <div className="description" title={meeting.message.taskDescription}>{meeting.message.taskDescription}</div>
                    <div className="userName">{meeting.message.userName}</div>
                </div>);
            })
        }

        return elements;
    }

    return (
        <>
            <div className="label">
                To Do
            </div>
            <button onClick={() => { openTaskModule('todo') }} className="add-card-button">
                <img className="add-icon" src="/add_icon.svg" title="Click to continue existing conversation" />
            </button>
            <br />
            <button onClick={() => { props.shareSpecificAppContent('todo') }} className="share-specific-part-button">
                Share To Do
            </button>
            <div id="todo">
                {getMeetingDataList()}
            </div>
        </>
    );
};

export default Todo;