// <copyright file="configure.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { Component } from "react";
import { Text, Flex, FlexItem, Button, TrashCanIcon, EditIcon, EyeFriendlierIcon } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import "../style/style.css";
import ViewTag from "./view-tag";
import DashboardState from "../models/dashboard-state";

// Dashboard where user can manage the tags
class Dashboard extends Component {
    constructor(props) {
        super(props);

        this.state = {
            teamworkTags: [],
            dashboardState: DashboardState.Default,
            selectedTeamworkTag: {}
        }
    }

    componentDidMount() {
        this.initializeData();
    }

    initializeData = async () => {
        this.setState({
            teamworkTags: [
                {
                    "id": "MjQzMmI1N2ItMGFiZC00M2RiLWFhN2ItMTZlYWRkMTE1ZDM0IyM3ZDg4M2Q4Yi1hMTc5LTRkZDctOTNiMy1hOGQzZGUxYTIxMmUjI3RhY29VSjN2RGk==",
                    "teamId": "53c53217-fe77-4383-bc5a-ed4937a1aecd",
                    "displayName": "Finance",
                    "description": "Finance Team for Mach 8 Project",
                    "memberCount": 2,
                    "tagType": "standard"
                },
                {
                    "id": "MjQzMmI1N2ItMGFiZC00M2RiLWFhN2ItMTZlYWRkMTE1ZDM0IyNlYjY1M2Y5Mi04MzczLTRkZTYtYmZlYy01YjRkMjE2YjZhZGUjIzk3ZjYyMzQ0LTU3ZGMtNDA5Yy04OGFkLWM0YWYxNDE1OGZmNQ==",
                    "teamId": "53c53217-fe77-4383-bc5a-ed4937a1aecd",
                    "displayName": "Legal",
                    "description": "Legal experts, ask us your legal questions",
                    "memberCount": 4,
                    "tagType": "standard"
                }
            ]
        })
    }

    onBackClick = () => {
        this.setState({
            dashboardState: DashboardState.Default,
            selectedTeamworkTag: {}
        });
    }

    onViewClick = (teamworkTag) => {
        this.setState({
            dashboardState: DashboardState.View,
            selectedTeamworkTag: teamworkTag
        });
    }

    onEditClick = (teamworkTag) => {
        this.setState({
            dashboardState: DashboardState.Edit,
            selectedTeamworkTag: teamworkTag
        });
    }

    renderBasedOnDashboardState = () => {
        switch (this.state.dashboardState) {
            case DashboardState.View:
                return <ViewTag onBackClick={this.onBackClick} teamworkTag={this.state.selectedTeamworkTag} dashboardState={DashboardState.View} />
                break;
            case DashboardState.Edit:
                return <ViewTag onBackClick={this.onBackClick} teamworkTag={this.state.selectedTeamworkTag} dashboardState={DashboardState.Edit} />
                break;
            default:
                return (<Flex column>
                    <Text size="large" content="Tags created for current team" style={{ marginTop: "1rem" }} />
                    {this.renderTeamworkTagList()}
                </Flex>)
        }
    }

    renderTeamworkTagList = () => {
        var elements = [];
        this.state.teamworkTags.map((teamworkTag, index) => {
            elements.push(<Flex className="tag-container" vAlign="center">
                <Flex.Item size="size.half">
                    <Flex column>
                        <Text content={teamworkTag.displayName} weight="semibold" />
                        <Text truncated content={teamworkTag.description} />
                    </Flex>
                </Flex.Item>

                <Flex.Item size="size.quarter">
                    <Text content={`Members count: ${teamworkTag.memberCount}`} />
                </Flex.Item>

                <Flex.Item size="size.quarter">
                    <Flex gap="gap.large">
                        <EyeFriendlierIcon className="manage-icons" onClick={() => { this.onViewClick(teamworkTag) }} />
                        <EditIcon className="manage-icons"  onClick={() => { this.onEditClick(teamworkTag) }}/>
                        <TrashCanIcon className="manage-icons" />
                    </Flex>
                </Flex.Item>
            </Flex>);
        });

        return elements;
    }

    render() {
        return (<Flex className="container" column >
            <Flex vAlign="center"   >
                <Text content="Team Tag Management" size="larger" weight="semibold" />
                <FlexItem push>
                    <Button primary content="Create new Tag" />
                </FlexItem>
            </Flex>

            {this.renderBasedOnDashboardState()}
        </Flex>)
    }
}

export default Dashboard;