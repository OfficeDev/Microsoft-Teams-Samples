// <copyright file="create-event.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect, useState } from "react";
import { Text, Flex, FlexItem, Button, Input, TextArea, TrashCanIcon } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import "../style/style.css";
import axios from "axios";

// This page allow user to create new tag in task module.
const CreateEvent = props => {
    const [TopicName, settopicname] = useState("");
    const [startDate, setStartDate] = useState("");
    const [EndDate, setEndDate] = useState("");
    const [Participants, SetParticipants] = useState("");
    const [membersToAdd, setMembersToAdd] = useState([]);

    // Initialize the SDK.
    useEffect(() => {
        microsoftTeams.app.initialize();
    }, []);

    // Handler when user updates the name.
    const onTopicNameChange = (data) => {
        settopicname(data.value.trim());
    }
    // startDate

    const onStartDateChange = (data) => {
        setStartDate(data.value.trim());
    }
    // EndDate

    const onEndDateChange = (data) => {
        setEndDate(data.value.trim());
    }
    // Participants
    /**
     * Add current user if he is not already in members list.
     * @param {any} id Id of current user to be added.
     */
    const addSelfIfNotAdded = (id) => {
        if (membersToAdd.findIndex(member => member.userId === id) === -1) {
            let tempMemberList = [...membersToAdd];
            tempMemberList.push({
                id: "",
                userId: id,
                displayName: ""
            });

            return tempMemberList;
        }

        return membersToAdd;
    }

    
    // Handler when user removes members.
    const onRemoveMember = (memberToUpdate) => {
        let tempMemberToAddList = [...membersToAdd]
        let memberIndex = tempMemberToAddList.findIndex(member => member.userId === memberToUpdate.userId);
        tempMemberToAddList.splice(memberIndex, 1);

        setMembersToAdd(tempMemberToAddList);
    }

    // Handler when user click on create button.
    const onCreateEventButtonClick = () => {
        microsoftTeams.app.getContext().then(async (context) => {
            if (TopicName !== "" && startDate !== "") {
                var membersToBeAdded = addSelfIfNotAdded(context.user.id);

                var createEvent = {
                    topicName: TopicName,
                    trainerName:"d",
                    startdate: startDate,
                    enddate: EndDate,
                    timing: "",
                    participants: Participants
                }

                var response = await axios.post(`api/meetings`, createEvent);
                if (response.status === 201) {
                    microsoftTeams.dialog.submit("Created successfully!");
                }
            }
        });
    }
    return (
        <Flex className="container" vAlign="center" gap="gap.medium" column>
            <Text content="Create event" size="large" weight="semibold" />

            <Flex column gap="gap.small" className="input-container">
                <Flex vAlign="center" gap="gap.medium">
                    <Text className="input-fields-label" content="Topic Name:" />
                    <Input fluid onChange={(e, data) => { onTopicNameChange(data) }} />
                </Flex>
               
                <Flex vAlign="center" gap="gap.medium">
                    <Text className="input-fields-label" content="Start Date Time:" />
                    <Input fluid onChange={(e, data) => { onStartDateChange(data) }} />
                </Flex>
                <Flex vAlign="center" gap="gap.medium">
                    <Text className="input-fields-label" content="End Date Time:" />
                    <Input fluid onChange={(e, data) => { onEndDateChange(data) }} />
                </Flex>                             
            </Flex>
            <Flex>
                <FlexItem push>
                    <Button primary content="Create" onClick={onCreateEventButtonClick} />
                </FlexItem>
            </Flex>
        </Flex>
    )
}

export default CreateEvent;