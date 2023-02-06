// <copyright file="dashboard.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { Component } from "react";
import { Text, Flex, FlexItem, Button, TrashCanIcon, EditIcon, EyeFriendlierIcon, Loader } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import axios from "axios";
import ViewEditTag from "./view-edit-tag";
import DashboardState from "../models/dashboard-state";

import "../style/style.css";

var accessToken;
var consentLink;
var idToken;

// Dashboard where user can manage the tags
class Dashboard extends Component {
    constructor(props) {
        super(props);

        this.state = {
            teamworkTags: [],
            appId: "",
            context: undefined,
            dashboardState: DashboardState.Default,
            selectedTeamworkTag: {},
            teamsContext: {},
            isLoading: true,
            isError: false
        }
    }

    componentDidMount() {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.getContext().then((context) => {
                this.setState({ teamsContext: context });
            }).then(() => {
                // Fetch id token
                microsoftTeams.authentication.getAuthToken().then((result) => {
                    console.log(this.state.teamsContext);
                    accessToken = result;
                    this.ssoLoginSuccess(result);
                }).catch((error) => {
                    if (error.response.status == 500) {
                        this.setState({ isLoading: false, isError: true, dashboardState: DashboardState.Error });
                    }
                    else {
                        this.ssoLoginFailure(error)
                    }
                });
            })
        });
    }

    // Success callback for getAuthtoken method
        ssoLoginSuccess = async (result) => {
            accessToken = result;
            this.exchangeClientTokenForServerToken(result);
    }

    // Failure callback for getAuthtoken method
    ssoLoginFailure(error) {
        alert("SSO failed: ", error);
    }

    // Exchange client token with server token and fetch the pinned message details.
    exchangeClientTokenForServerToken = async (token) => {
        var id = this.state.teamsContext.team.groupId;
        idToken = token;
        var appDataResponse = await axios.get(`/api/teamtag/getAppData`);
        consentLink = `https://login.microsoftonline.com/common/adminconsent?client_id=${appDataResponse.data}`;
        this.setState({ appId: appDataResponse.data });
        await axios.get(`/api/teamtag/list?ssoToken=${token}&teamId=${id}`).then((response) => {
            if (response.status === 200) {
                this.setState({ teamworkTags: response.data, isLoading: false });
                return response.data;
            }

        }).catch((error) => {
            if (error.response.status == 500) {
                this.setState({ isLoading: false, isError: true, dashboardState: DashboardState.Error });
            }
            console.log("error is", error);
        });
    }

    /**
     * Initialize the the list of tags.
     * @param {any} teamId Id of team.
     */
    initializeData = async (teamId) => {
        this.setState({ isLoading: true });
        var response = await axios.get(`/api/teamtag/${teamId}/list`);

        if (response.status === 200) {
            this.setState({ teamworkTags: response.data, isLoading: false });
            return response.data;
        }
    }

    // Handler when user click on create new tag button.
    onCreateNewTagClick = () => {
        microsoftTeams.dialog.open({
            title: "Create new Tag",
            url: `${window.location.origin}/create-new-tag`,
            size: {
                height: 450,
                width: 500,
            }
        }, (dialogResponse) => {
            if (dialogResponse.result) {
                this.setState({
                    dashboardState: DashboardState.Default,
                    selectedTeamworkTag: {}
                });

                this.initializeData(this.state.teamsContext.team.groupId);
            }
        });
    }

    // Handler when tag is updated.
    onTeamworkTagUpdate = async () => {
        let tempSelectedTeamworkTag = this.state.selectedTeamworkTag;
        this.setState({ dashboardState: DashboardState.Default, selectedTeamworkTag: {} });

        var updatedTagsList = await this.initializeData(this.state.teamsContext.team.groupId);

        var findSelectedTeamworkTag = updatedTagsList.find(tag => tag.id === tempSelectedTeamworkTag.id);

        if (findSelectedTeamworkTag) {
            this.setState({ dashboardState: DashboardState.Edit, selectedTeamworkTag: findSelectedTeamworkTag });
        }
    }

    // Handler when user clicks on back icon.
    onBackClick = () => {
        this.setState({
            dashboardState: DashboardState.Default,
            selectedTeamworkTag: {}
        });
        this.initializeData(this.state.teamsContext.team.groupId);
    }

    // Handler when user clicks on view icon.
    onViewClick = (teamworkTag) => {
        this.setState({
            dashboardState: DashboardState.View,
            selectedTeamworkTag: teamworkTag
        });
    }

    // Handler when user clicks on edit icon.
    onEditClick = (teamworkTag) => {
        this.setState({
            dashboardState: DashboardState.Edit,
            selectedTeamworkTag: teamworkTag
        });
    }

    // Handler when user clicks on delete icon.
    onDeleteTagClick = async (teamworkTag) => {
        this.setState({ isLoading: true });
        var response = await axios.delete(`api/teamtag?ssoToken=${idToken}&teamId=${this.state.teamsContext.team.groupId}&tagId=${teamworkTag.id}`);

        if (response.status === 204) {
            await this.initializeData(this.state.teamsContext.team.groupId);
        }
        this.setState({ isLoading: false });
    }

    // Renders the elements based on dashboard state.
    renderBasedOnDashboardState = () => {
        switch (this.state.dashboardState) {
            case DashboardState.View:
                return <ViewEditTag isLoading={this.state.isLoading} onBackClick={this.onBackClick} teamworkTag={this.state.selectedTeamworkTag} dashboardState={DashboardState.View} onTeamworkTagUpdate={this.onTeamworkTagUpdate} />
                break;
            case DashboardState.Edit:
                return <ViewEditTag isLoading={this.state.isLoading} onBackClick={this.onBackClick} teamworkTag={this.state.selectedTeamworkTag} dashboardState={DashboardState.Edit} onTeamworkTagUpdate={this.onTeamworkTagUpdate} />
                break;
            case DashboardState.Error:
                return <Flex><Text content="Error occured admin consent required for application permissions." /><a href={consentLink} target="_blank" rel="noopener noreferrer">Click here to grant consent.</a></Flex>
            default:
                return (<Flex column>
                    <Text size="large" content="Tags created for current team" style={{ marginTop: "1rem" }} />
                    {this.renderTeamworkTagList()}
                </Flex>)
        }
    }

    // Renders list of tags available for current team.
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
                    <Text content={`Members count: ${teamworkTag.membersCount}`} />
                </Flex.Item>

                <Flex.Item size="size.quarter">
                    <Flex gap="gap.large">
                        <EyeFriendlierIcon className="manage-icons" onClick={() => { this.onViewClick(teamworkTag) }} />
                        <EditIcon className="manage-icons"  onClick={() => { this.onEditClick(teamworkTag) }}/>
                        <TrashCanIcon className="manage-icons" onClick={() => { this.onDeleteTagClick(teamworkTag) } }/>
                    </Flex>
                </Flex.Item>
            </Flex>);
        });

        return elements;
    }

    render() {
        return (<Flex className="container" column >
            <Flex vAlign="center"   >
                <Text content="Manage Teams Tag" size="larger" weight="semibold" />
                <FlexItem push>
                    <Button primary content="Create new Tag" onClick={this.onCreateNewTagClick}/>
                </FlexItem>
            </Flex>

            {this.state.isLoading ? <Loader /> : this.renderBasedOnDashboardState()}
        </Flex>)
    }
}

export default Dashboard;