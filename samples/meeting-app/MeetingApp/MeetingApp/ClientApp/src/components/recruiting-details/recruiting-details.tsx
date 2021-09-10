import React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Flex, Card, Button, Avatar, Text, ChatIcon, CallVideoIcon } from '@fluentui/react-northstar'
import "../recruiting-details/recruiting-details.css"
import BasicDetails from "./basic-details/basic-details"
import Timeline from "./basic-details/timeline"
import Notes from "./basic-details/notes"

const RecruitingDetails = () => {
    return (
        <Flex gap="gap.small" padding="padding.medium" className="container">
            <Flex column gap="gap.small" padding="padding.medium" className="detailsContainer">
                <BasicDetails/>
                <Timeline/>
                <Notes/>
            </Flex>
            <Flex gap="gap.small" padding="padding.medium" className="questionsContainer">
                Questions
            </Flex>
        </Flex>
    )
}

export default (RecruitingDetails);