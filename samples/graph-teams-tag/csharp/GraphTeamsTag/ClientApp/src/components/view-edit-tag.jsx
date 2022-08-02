// <copyright file="view-edit-tag.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect, useState } from "react";
import { Flex, FlexItem, ChevronStartIcon, Text, TextArea, TrashCanIcon, Input, Button, Loader } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import DashboardState from "../models/dashboard-state";
import axios from "axios";

// Display the tags information
const ViewEditTag = props => {
    const [temworkTagMembers, setTeamworkTagMembers] = useState([]);
    const [membersToAdd, setMembersToAdd] = useState([]);
    const [membersToRemove, setMembersToRemove] = useState([]);
    const [editedTagName, setEditedTagName] = useState("");
    const [editedDescription, setEditedDescription] = useState("");
    const [isNoteDisabled, setIsNoteDisabled] = useState(true);
    const [teamId, setTeamId] = useState("");
    const [isUpdateButtonLoading, setIsUpdateButtonLoading] = useState(false);
    const [isMemberLoading, setIsMemberLoading] = useState(true);

    useEffect(() => {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.getContext().then((context) => {
                initializeData(context.team.groupId);
                setTeamId(context.team.groupId);
            });
        })
    }, []);

    // Gets the members of selected tags.
    const initializeData = async (teamId) => {
        setIsMemberLoading(true)
        var response = await axios.get(`api/teamtag/${teamId}/tag/${props.teamworkTag.id}/members`)

        if (response.status == 200) {
            setTeamworkTagMembers(response.data);
        }

        setIsMemberLoading(false);
    }

    // Handler when display name is updated.
    const onDisplayNameUpdate = (updatedText) => {
        setEditedTagName(updatedText.trim());
        setIsNoteDisabled(false);
    }

    // Handler when description is updated.
    const onDescriptionUpdate = (updatedText) => {
        setEditedDescription(updatedText.trim());
        setIsNoteDisabled(false);
    }

    // Handler when user remove member.
    const onMemberRemove = (memberToRemove) => {
        var memberToRemoveIndex = temworkTagMembers.findIndex(member => member.userId === memberToRemove.userId);
        if (memberToRemoveIndex !== -1)  {
            temworkTagMembers.splice(memberToRemoveIndex, 1);
            let tempMembersToRemoveList = [...membersToRemove];
            tempMembersToRemoveList.push(memberToRemove);
            setMembersToRemove(tempMembersToRemoveList);
        }
    }

    // Handler when user selects to add new member.
    const onUpdateMembers = () => {
        let existingMembers = membersToAdd.map(member => member.userId);
        microsoftTeams.people.selectPeople({ setSelected: existingMembers }).then((peoples) => {
            let members = [];
            peoples.forEach((people) => {
                
                let memberPresentInRemoveListIndex = membersToRemove.findIndex(member => member.userId === people.objectId);

                if (memberPresentInRemoveListIndex !== -1 ) {
                    let tempMemberToRemoveList = [...membersToRemove];
                    tempMemberToRemoveList.splice(memberPresentInRemoveListIndex, 1);
                    setMembersToRemove(tempMemberToRemoveList);
                }

                if(temworkTagMembers.findIndex(member => member.userId === people.objectId) === -1)
                {
                    members.push({
                        id: "",
                        userId: people.objectId,
                        displayName: people.displayName
                    });

                    setIsNoteDisabled(false);
                }
            });

            setMembersToAdd(members);
        });
    }

    // Handler when user click on update button.
    const onUpdateButtonClick = async () => {
        setIsUpdateButtonLoading(true);
        var updateTagDto = {
            id: props.teamworkTag.id,
            displayName: editedTagName === "" ? props.teamworkTag.displayName : editedTagName,
            description: editedDescription === "" ? props.teamworkTag.description : editedDescription,
            MembersToBeAdded: membersToAdd,
            MembersToBeDeleted: membersToRemove,
        };

        var response = await axios.patch(`api/teamtag/${teamId}/update`, updateTagDto);

        if (response.status === 204) {
            props.onTeamworkTagUpdate();
        }
        setIsUpdateButtonLoading(false);
    }

    // Checks if the same member is in both top be added and to be deleted list, if true then removes it.
    const commonAddedRemovedMemberUpdate = (memberToUpdate, isMemberFromAddedList) => {
        if (isMemberFromAddedList) {
            let tempMemberToAddList = [...membersToAdd]
            let memberIndex = tempMemberToAddList.findIndex(member => member.userId === memberToUpdate.userId);
            tempMemberToAddList.splice(memberIndex, 1);
            
            setMembersToAdd(tempMemberToAddList);
            setIsNoteDisabled(false);
        }
        else {
            let tempMemberToRemoveList = [...membersToRemove]
            let memberIndex = tempMemberToRemoveList.findIndex(member => member.userId === memberToUpdate.userId);
            tempMemberToRemoveList.splice(memberIndex, 1);
            
            setMembersToRemove(tempMemberToRemoveList);
            setIsNoteDisabled(false);
        }
    }

    // Render members of tag.
    const renderTeamworkTagMemberList = () => {
        var elements = [];
        temworkTagMembers.map((teamworkTagMember, index) => {
            elements.push(<Flex className="tag-container" vAlign="center">
                <Text content={teamworkTagMember.displayName} />
                <Flex.Item push>
                    <Flex gap="gap.large">
                        {props.dashboardState === DashboardState.Edit && <TrashCanIcon className="manage-icons" onClick={() => { onMemberRemove(teamworkTagMember)}} />}
                    </Flex>
                </Flex.Item>
            </Flex>);
        });

        return (<div>
            <Flex vAlign="center">
                <Text size="large" content="Members" style={{ marginTop: "1rem" }} />
                {props.dashboardState === DashboardState.Edit && <FlexItem push>
                    <Button content="Select members to add" text onClick={onUpdateMembers} />
                </FlexItem>}
            </Flex>
            {isMemberLoading ? <Loader />: elements}
        </div>);
    }

    // Render members to add list.
    const renderMembersToAddList = () => {
        var elements = [];
        membersToAdd.map((teamworkTagMember, index) => {
            elements.push(<Flex className="tag-container" vAlign="center">
                <Text content={teamworkTagMember.displayName} />
                <Flex.Item push>
                    <Flex gap="gap.large">
                        {props.dashboardState === DashboardState.Edit && <TrashCanIcon className="manage-icons" onClick={() => { commonAddedRemovedMemberUpdate(teamworkTagMember, true)}} />}
                    </Flex>
                </Flex.Item>
            </Flex>);
        });

        if (elements.length > 0) {
            return (<div>
                <Flex vAlign="center">
                    <Text size="large" content="Members to be added" style={{ marginTop: "1rem" }} />
                </Flex>
                {elements}
            </div>);
        }

        return <></>;
    }

    // Render members to removed list.
    const renderMembersToRemoveList = () => {
        var elements = [];
        membersToRemove.map((teamworkTagMember, index) => {
            elements.push(<Flex className="tag-container" vAlign="center">
                <Text content={teamworkTagMember.displayName} />
                <Flex.Item push>
                    <Flex gap="gap.large">
                        {props.dashboardState === DashboardState.Edit && <TrashCanIcon className="manage-icons" onClick={() => { commonAddedRemovedMemberUpdate(teamworkTagMember, false)}} />}
                    </Flex>
                </Flex.Item>
            </Flex>);
        });

        if (elements.length > 0) {
            return (<div>
                <Flex vAlign="center">
                    <Text size="large" content="Members to be removed" style={{ marginTop: "1rem" }} />
                </Flex>
                {elements}
            </div>);
        }

        return <></>;
    }

    // Render tag information.
    const renderTagInformation = () => {
        return (<Flex column gap="gap.medium">
            <Flex gap="gap.small" vAlign="center">
                <Text className={props.dashboardState === DashboardState.View ? "text-fields-label" : "input-fields-label"} content={`Tag display name:`} weight="semibold" />
                {
                    props.dashboardState === DashboardState.View ?
                        <Text content={props.teamworkTag.displayName} /> :
                        <Input fluid defaultValue={props.teamworkTag.displayName} onChange={(event, data) => { onDisplayNameUpdate(data.value) }} />
                }
            </Flex>

            <Flex gap="gap.small" vAlign="center">
                <Text className={props.dashboardState === DashboardState.View ? "text-fields-label" : "input-fields-label"} content={`Tag description:`} weight="semibold" />
                {
                    props.dashboardState === DashboardState.View ?
                        <Text content={props.teamworkTag.description} /> :
                        <TextArea fluid className="input-fields" defaultValue={props.teamworkTag.description} onChange={(event, data) => { onDescriptionUpdate(data.value) }} />
                }
            </Flex>

            <Flex gap="gap.small" vAlign="center">
                <Text content={`Tag members count:`} weight="semibold" />
                <Text content={props.teamworkTag.membersCount} />
            </Flex>
        </Flex>);
    }

    const renderBasedOnLoadingState = () => {
        var elements = [];
        if (props.isLoading) {
            return <Loader />
        }
        else {
            elements.push(renderTeamworkTagMemberList());
            elements.push(renderMembersToAddList());
            elements.push(renderMembersToRemoveList());

            return elements;
        }
    }

    return (
        <Flex column gap="gap.medium">
            <Flex vAlign="center" gap="gap.medium" style={{ marginTop: "1rem" }}>
                <ChevronStartIcon className="manage-icons" onClick={props.onBackClick} />
                <Text size="large" content="Tag Details:" weight="semibold" />
            </Flex>
            <Text temporary content="(*To update any field make sure to click on update button)" />
            {renderTagInformation()}

            {renderBasedOnLoadingState()}

            {props.dashboardState === DashboardState.Edit && <Flex>
                <FlexItem push>
                    <div>
                        {!isNoteDisabled && <Flex gap="gap.medium">
                            <Text content='Click on "Update" button to update the fields.' temporary error />
                            <Button content="Update" primary disabled={isUpdateButtonLoading} isUpdateButtonLoading={isUpdateButtonLoading} onClick={onUpdateButtonClick} />
                        </Flex>}
                    </div>
                </FlexItem>
            </Flex>}
        </Flex>
    );
};

export default ViewEditTag;