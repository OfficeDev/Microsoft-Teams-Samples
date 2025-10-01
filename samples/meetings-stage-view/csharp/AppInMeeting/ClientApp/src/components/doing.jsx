// <copyright file="doing.tsx" company="Microsoft Corporation">
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

// Return in-progress meeting data.
const Doing = props => {
    const editorValueKey = "meeting-doing";
    const [meetingDataArray, setMeetingDataArray] = useState([]);

    useEffect(() => {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.getContext().then((context) => {
                $.ajax({
                    url: "api/meeting/getMeetingData?meetingId=" + context.meeting.id + "&status=doing",
                    type: "GET",
                    success: function (response) {
                        console.log(response);
                        if (response) {
                            setMeetingDataArray(response);
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
            window.localStorage.debug = "fluid:*";
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
    const saveMeetingData = (meeting, meetingId) => {
        $.ajax({
            url: `api/meeting/saveMeetingData?meetingId=${meetingId}&status=doing`,
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
    const updateState = (meeting, meetingId) => {
        var editorMap = containerValue.initialObjects.editorMap;

        let details = [...meetingDataArray];
        details.push(meeting);

        saveMeetingData(meeting, meetingId);

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
                    updateState(taskDetails.result, context.meeting.id);
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
                    <div className="description" title={meeting.taskDescription}>{meeting.taskDescription}</div>
                    <div className="userName">{meeting.userName}</div>
                </div>);
            })
        }

        return elements;
    }

    return (
        <>
            <div className="label">
                Doing
            </div>
            <button onClick={() => { openTaskModule('doing') }} className="add-card-button">
                <img className="add-icon" src="/add_icon.svg" title="Click to continue existing conversation" />
            </button>
            <br />
            <button onClick={() => { props.shareSpecificAppContent('doing') }} className="share-specific-part-button">
                Share Doing
            </button>
            <div id="doing">
                {getMeetingDataList()}
            </div>
        </>
    );
};

export default Doing;