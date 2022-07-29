// <copyright file="view-tag.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect, useState } from "react";
import { Flex, FlexItem, ChevronStartIcon, Text, TrashCanIcon, Input, Button } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import DashboardState from "../models/dashboard-state";

// Display the tags information
const ViewTag = props => {
    const [temworkTagMembers, setTeamworkTagMembers] = useState([]);
    const [editedTagName, setEditedTagName] = useState("");
    const [membersToBeAdded, setMembersToBeAdded] = useState([]);
    const [membersToBeDeleted, setMembersToBeDeleted] = useState([]);

    useEffect(() => {
        initializeData();
    }, []);

    const initializeData = () => {
        setTeamworkTagMembers([{
            "id": "MjQzMmI1N2ItMGFiZC00M2RiLWFhN2ItMTZlYWRkMTE1ZDM0IyNlYjY1M2Y5Mi04MzczLTRkZTYtYmZlYy01YjRkMjE2YjZhZGUjI2QzYjJiM2ViLWM0N2YtNDViOS05NWYyLWIyZjJlZjYyMTVjZQ==",
            "displayName": "Grady Archie",
            "tenantId": "18be64a9-c73a-4862-bccc-76c31ef09b9d",
            "userId": "92f6952f-61ca-4a94-8910-508a240bc167"
        },
        {
            "id": "MjQzMmI1N2ItMGFiZC00M2RiLWFhN2ItMTZlYWRkMTE1ZDM0IyNlYjY1M2Y5Mi04MzczLTRkZTYtYmZlYy01YjRkMjE2YjZhZGUjI2QzYjJiM2ViLMW0N2YtNDViOS05NWYyLWIyZjJlZjYyGHVjZQ==",
            "displayName": "Lee Gu",
            "tenantId": "18be64a9-c73a-4862-bccc-76c31ef09b9d",
            "userId": "945fe02a-5dc1-4d28-bf5c-30a6147fe842"
        }]);
    }

    const renderTeamworkTagMemberList = () => {
        var elements = [];
        temworkTagMembers.map((teamworkTagMember, index) => {
            elements.push(<Flex className="tag-container" vAlign="center">
                <Text content={teamworkTagMember.displayName} />
                <Flex.Item push>
                    <Flex gap="gap.large">
                        <TrashCanIcon className="manage-icons" />
                    </Flex>
                </Flex.Item>
            </Flex>);
        });

        return elements;
    }

    const renderTagInformation = () => {
        return (<Flex column gap="gap.medium">
            <Flex gap="gap.small" vAlign="center">
                <Text content={`Tag display name:`} weight="semibold" />
                {
                    props.dashboardState === DashboardState.View ?
                        <Text content={props.teamworkTag.displayName} /> :
                        <Input defaultValue={props.teamworkTag.displayName} />
                }
            </Flex>

            <Flex gap="gap.small" vAlign="center">
                <Text content={`Tag desciption:`} weight="semibold" />
                <Text content={props.teamworkTag.description} />
            </Flex>

            <Flex gap="gap.small" vAlign="center">
                <Text content={`Tag dmember count:`} weight="semibold" />
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
            {renderTagInformation()}
            
            <Flex vAlign="center">
                <Text size="large" content="Members" style={{ marginTop: "1rem" }} />
                {props.dashboardState === DashboardState.Edit && <FlexItem push>
                    <Button content="Add members"/>
                </FlexItem>}
            </Flex>
            {renderTeamworkTagMemberList()}
            <Flex>
                <FlexItem push>
                    <Button content="Update" primary />
                </FlexItem>
            </Flex>
        </Flex>
    );
};

export default ViewTag;