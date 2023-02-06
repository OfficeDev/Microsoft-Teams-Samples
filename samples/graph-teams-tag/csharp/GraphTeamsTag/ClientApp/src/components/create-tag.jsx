// <copyright file="create-tag.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect, useState } from "react";
import { Text, Flex, FlexItem, Button, Input, TextArea, TrashCanIcon } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import "../style/style.css";
import axios from "axios";

// This page allow user to create new tag in task module.
const CreateTag = props => {
    const [tagName, setTagName] = useState("");
    const [tagDescription, setTagDescription] = useState("");
    const [membersToAdd, setMembersToAdd] = useState([]);

    // Initialize the SDK.
    useEffect(() => {
        microsoftTeams.app.initialize();
    }, []);

    // Handler when user updates the name.
    const onTagNameChange = (data) => {
        setTagName(data.value.trim());
    }

    // Handler when user updates the description.
    const onTagDescriptionChange = (data) => {
        setTagDescription(data.value.trim());
    }

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

    // Handler when user selects members.
    const onUpdateMembers = () => {
        let exisitingMembersId = membersToAdd.map(member => member.userId);
        microsoftTeams.people.selectPeople({ setSelected: exisitingMembersId }).then((peoples) => {
            let members = [];
            peoples.forEach((people) => {
                members.push({
                    id: "",
                    userId: people.objectId,
                    displayName: people.displayName
                });
            });
            setMembersToAdd(members);
        });
    }

    // Handler when user removes members.
    const onRemoveMember = (memberToUpdate) => {
        let tempMemberToAddList = [...membersToAdd]
        let memberIndex = tempMemberToAddList.findIndex(member => member.userId === memberToUpdate.userId);
        tempMemberToAddList.splice(memberIndex, 1);

        setMembersToAdd(tempMemberToAddList);
    }

    // Handler when user click on create button.
    const onCreateTagButtonClick = () => {
        microsoftTeams.app.getContext().then(async (context) => {
            if (tagName !== "" && tagDescription !== "") {
                var membersToBeAdded = addSelfIfNotAdded(context.user.id);

                var createNewTag = {
                    id: "",
                    displayName: tagName,
                    description: tagDescription,
                    membersToBeAdded: membersToBeAdded,
                    membersToBeDeleted: []
                }

                var response = await axios.post(`api/teamtag/${context.team.groupId}`, createNewTag);
                if (response.status === 201) {
                    microsoftTeams.dialog.submit("Created successfully!");
                }
            }
        });
    }

    // Render list of selected members to be added in tag.
    const renderMembersToAddList = () => {
        var elements = [];
        membersToAdd.map((teamworkTagMember, index) => {
            elements.push(<Flex className="tag-container" vAlign="center">
                <Text content={teamworkTagMember.displayName} />
                <Flex.Item push>
                    <Flex gap="gap.large">
                        <TrashCanIcon className="manage-icons" onClick={() => { onRemoveMember(teamworkTagMember) }} />
                    </Flex>
                </Flex.Item>
            </Flex>);
        });

        if (elements.length > 0) {
            return (<div>

                {elements}
            </div>);
        }

        return <></>;
    }

    return (
        <Flex className="container" vAlign="center" gap="gap.medium" column>
            <Text content="Create tag" size="large" weight="semibold" />

            <Flex column gap="gap.small" className="input-container">
                <Flex vAlign="center" gap="gap.medium">
                    <Text className="input-fields-label" content="Tag Name:" />
                    <Input fluid onChange={(e, data) => { onTagNameChange(data) } }/>
                </Flex>
                <Flex vAlign="center" gap="gap.medium">
                    <Text className="input-fields-label" content="Tag Description:" />
                    <TextArea fluid onChange={(e, data) => { onTagDescriptionChange(data) } }/>
                </Flex>
                <Flex vAlign="center">
                    <Text content="Members to be added" weight="semibold" style={{ marginTop: "1rem" }} />
                    <FlexItem push>
                        <Button content="Select members" onClick={onUpdateMembers} />
                    </FlexItem>
                </Flex>
                <Text temporary content="You will be added by default"/>
                {renderMembersToAddList()}
            </Flex>

            <Flex>
                <FlexItem push>
                    <Button primary content="Create" onClick={onCreateTagButtonClick }/>
                </FlexItem>
            </Flex>
        </Flex>
    )
}

export default CreateTag;