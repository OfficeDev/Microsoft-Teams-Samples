// <copyright file="create-tag.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect, useState } from "react";
import { Text, Flex, FlexItem, Button, Input, TextArea, TrashCanIcon } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import "../style/style.css";

// This page allow user to create new tag in task module.
const CreateTag = props => {
    const [tagName, setTagName] = useState("");
    const [tagDescription, setTagDescription] = useState("");
    const [membersToAdd, setMembersToAdd] = useState([]);

    useEffect(() => {
        microsoftTeams.app.initialize();
    }, []);

    const onUpdateMembers = () => {
        let exisitingMembersId = membersToAdd.map(member => member.userId);
        microsoftTeams.people.selectPeople({ setSelected: exisitingMembersId }).then((peoples) => {
            let members = [];
            peoples.forEach((people) => {
                members.push({
                    userId: people.objectId,
                    displayName: people.displayName
                });
            });
            setMembersToAdd(members);
        });
    }

    const onRemoveMember = (memberToUpdate) => {
        let tempMemberToAddList = [...membersToAdd]
        let memberIndex = tempMemberToAddList.findIndex(member => member.userId === memberToUpdate.userId);
        tempMemberToAddList.splice(memberIndex, 1);

        setMembersToAdd(tempMemberToAddList);
    }

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
                    <Text content="Tag Name:" />
                    <Input />
                </Flex>
                <Flex vAlign="center" gap="gap.medium">
                    <Text fluid content="Tag Description:" />
                    <TextArea />
                </Flex>
                <Flex vAlign="center">
                    <Text content="Members to be added" weight="semibold" style={{ marginTop: "1rem" }} />
                    <FlexItem push>
                        <Button content="Select members" onClick={onUpdateMembers} />
                    </FlexItem>
                </Flex>
                {renderMembersToAddList()}
            </Flex>

            <Flex>
                <FlexItem push>
                    <Button primary content="Create" />
                </FlexItem>
            </Flex>
        </Flex>
    )
}

export default CreateTag;