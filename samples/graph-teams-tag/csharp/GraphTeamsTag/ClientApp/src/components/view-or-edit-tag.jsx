// <copyright file="view-tag.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect, useState } from "react";
import { Flex, FlexItem, ChevronStartIcon, Text, TrashCanIcon, Input, Button } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import DashboardState from "../models/dashboard-state";

// Display the tags information
const ViewOrEditTag = props => {
    const [temworkTagMembers, setTeamworkTagMembers] = useState([]);
    const [membersToAdd, setMembersToAdd] = useState([]);
    const [membersToRemove, setMembersToRemove] = useState([]);
    const [editedTagName, setEditedTagName] = useState("");
    const [isNoteDisabled, setIsNoteDisabled] = useState(true);

    useEffect(() => {
        microsoftTeams.app.initialize();
        initializeData();
    }, []);

    const initializeData = () => {
        setTeamworkTagMembers([{
            "id": "MjQzMmI1N2ItMGFiZC00M2RiLWFhN2ItMTZlYWRkMTE1ZDM0IyNlYjY1M2Y5Mi04MzczLTRkZTYtYmZlYy01YjRkMjE2YjZhZGUjI2QzYjJiM2ViLMW0N2YtNDViOS05NWYyLWIyZjJlZjYyGHVjZQ==",
            "displayName": "Adele Vance",
            "userId": "64af9a58-8030-46fb-8388-c2417b8b252a"
        },
        {
            "id": "MjQzMmI1N2ItMGFiZC00M2RiLWFhN2ItMTZlYWRkMTE1ZDM0IyNlYjY1M2Y5Mi04MzczLTRkZTYtYmZlYy01YjRkMjE2YjZhZGUjI2QzYjJiM2ViLMW0N2YtNDViOS05NWYyLWIyZjJlZjYyGHVjZQ==",
            "displayName": "Diego Siciliani",
            "userId": "298eb8ce-e0a6-4ba6-a6e9-461632ecb3fd"
        }]);
    }

    const onDisplayNameUpdate = (updatedText) => {
        if (updatedText.trim() !== "") {
            setEditedTagName(updatedText);
            setIsNoteDisabled(false);
        }
    }

    const onMemberRemove = (memberToRemove) => {
        var memberToRemoveIndex = temworkTagMembers.findIndex(member => member.userId === memberToRemove.userId);
        if (memberToRemoveIndex !== -1)  {
            temworkTagMembers.splice(memberToRemoveIndex, 1);
            let tempMembersToRemoveList = [...membersToRemove];
            tempMembersToRemoveList.push(memberToRemove);
            setMembersToRemove(tempMembersToRemoveList);
        }
    }

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
                        userId: people.objectId,
                        displayName: people.displayName
                    });
                    setIsNoteDisabled(false);
                }                
            });

            setMembersToAdd(members);
        });
    }

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
            {elements}
        </div>);
    }

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

    const renderTagInformation = () => {
        return (<Flex column gap="gap.medium">
            <Flex gap="gap.small" vAlign="center">
                <Text content={`Tag display name:`} weight="semibold" />
                {
                    props.dashboardState === DashboardState.View ?
                        <Text content={props.teamworkTag.displayName} /> :
                        <Input defaultValue={props.teamworkTag.displayName} onChange={(event, data) => { onDisplayNameUpdate(data.value) }} />
                }
            </Flex>

            <Flex gap="gap.small" vAlign="center">
                <Text content={`Tag desciption:`} weight="semibold" />
                <Text content={props.teamworkTag.description} />
            </Flex>

            <Flex gap="gap.small" vAlign="center">
                <Text content={`Tag members count:`} weight="semibold" />
                <Text content={props.teamworkTag.memberCount} />
            </Flex>
        </Flex>);
    }

    return (
        <Flex column gap="gap.medium">
            <Flex vAlign="center" gap="gap.medium" style={{ marginTop: "1rem" }}>
                <ChevronStartIcon className="manage-icons" onClick={props.onBackClick} />
                <Text size="large" content="Tag Details:" weight="semibold" />
            </Flex>
            <Text temporary content="(*To update any field make sure to click on update button)" />
            {renderTagInformation()}
            
            {renderTeamworkTagMemberList()}
            {renderMembersToAddList()}
            {renderMembersToRemoveList()}

            {props.dashboardState === DashboardState.Edit && <Flex>
                <FlexItem push>
                    <div>
                        {!isNoteDisabled && <Flex gap="gap.medium">
                            <Text content='Click on "Update" button to update the fields.' temporary error />
                            <Button content="Update" primary />
                        </Flex>}
                    </div>
                </FlexItem>
            </Flex>}
        </Flex>
    );
};

export default ViewOrEditTag;