/*!
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

import { useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { SharedMap } from "fluid-framework";
import { LiveShareClient } from "@microsoft/live-share";
import { setMeetingContext, addAgendaTask, postAgenda } from "./services/agendaAPIHelper"

let containerValue;

const SidePanel = (props) => {

    const agendaValueKey = "editor-value-key";

    useEffect(() => {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.getContext().then((context) => {
                var userId = context.user.id;
                var meetingId = context.meeting.id;
                var tenantId = context.user.tenant.id;
                agendaListPopulate();

                setMeetingContext(userId, meetingId, tenantId).then((result) => {
                    if (result.data == true) {
                        document.getElementById("agendaButtonDiv").style.display = "block";
                        document.getElementById("publishAgendaButton").style.display = "block";
                    }
                    else {
                        document.getElementById("agendaButtonDiv").style.display = "none";
                        document.getElementById("publishAgendaButton").style.display = "none";
                    }
                })
            })
        });
    }, []);

    // Initial setup for using fluid container.
    useEffect(() => {
        microsoftTeams.app.initialize();
        (async function () {

            let connection;
            window.localStorage.debug = "fluid:*";
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
        let taskInfo = {
            title: newAgendaItem
        };

        // API call to save agenda.
        addAgendaTask(taskInfo);

        var editorMap = containerValue.initialObjects.editorMap;
        var agendas = editorMap.get(agendaValueKey);
        agendas.push(newAgendaItem);
        editorMap.set(agendaValueKey, agendas);
    }

    // This method is called to publish the agenda in meeting chat.
    function publishAgenda() {
        const agendaValue = containerValue.initialObjects.editorMap.get(agendaValueKey);
        postAgenda();
    }

    // This method is called whenever the shared state is updated.
    const updateEditorState = () => {
        const agendaValue = containerValue.initialObjects.editorMap.get(agendaValueKey);
        agendaListPopulate();
    };

    return (
        <>
            <div className="agendaTitle">
                Agenda
            </div>
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
            <button id="publishAgendaButton" onClick={publishAgenda}>Publish Agenda</button>
        </>
    );
};

export default SidePanel;