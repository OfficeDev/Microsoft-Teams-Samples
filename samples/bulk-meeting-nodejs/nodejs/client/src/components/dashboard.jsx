// <copyright file="dashboard.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { Component } from "react";
import { Text, Flex, FlexItem, Button } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import DashboardState from "../models/dashboard-state";
import axios from "axios";
import "../style/style.css";
import "../style/table.css";

// Dashboard
class Dashboard extends Component {
    constructor(props) {
        super(props);

        this.state = {
            userId: "",
            meetingList: [],
            dashboardState: DashboardState.Default,
            teamsContext: {},
        }
    }

    componentDidMount() {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.getContext().then((context) => {
                this.setState({ teamsContext: context });
                this.initializeData(context.user.id);
            })
        });
    }

    /**
    * Initialize the the list of meeting.
    * @param {any} userId Id of user.
    */
    initializeData = async (userId) => {
        var response = await axios.get(`/api/meeting/list?userId=${userId}`);
        if (response.status === 200) {
            this.setState({ meetingList: response.data });
            return response.data;
        }
    }

    // when user click on create new meeting button.
    onCreateMeeting = () => {
        microsoftTeams.dialog.open({
            title: "Create Meeting/Events",
            url: `${window.location.origin}/fileupload`,
            size: {
                height: 450,
                width: 700,
            }
        }, (dialogResponse) => {
            if (dialogResponse.result) {
                this.setState({
                    dashboardState: DashboardState.Default,
                });
            }
        });
    }

    // Renders list of meeetingd available for current user.
    renderMeetingList = () => {
        if (this.state.meetingList) {
            var elements = [];
            this.state.meetingList.forEach(item => {
                elements.push(<div>
                    <table className="table">
                        <tr>
                            <th>Topic Name</th>
                            <th>Tutor Name</th>
                            <th>Date</th>
                            <th>Participant Email</th>
                        </tr>
                        <tr>
                            <td>{item.subject}</td>
                            <td>{item.contentType}</td>
                            <td>{item.dateTime}</td>
                            <td>{item.emailAddress}</td>
                        </tr>
                    </table>
                </div>);
            });
            
            if (elements.length > 0) {
                this.setState({ meetingList: elements });
            }
        }
    }

    render() {
        return (<Flex className="container" column >
            <Flex vAlign="center"   >
                <Text content="Create Meeting/Events" size="larger" weight="semibold" />
                <FlexItem push>
                    <Button primary content="Create Meeting" onClick={this.onCreateMeeting} />
                </FlexItem>
            </Flex>
            <Flex>
                <Text size="large" content="Teams created list" style={{ marginTop: "1rem" }} />
                {this.state.meetingList}
            </Flex>
        </Flex>)
    }
}

export default Dashboard;