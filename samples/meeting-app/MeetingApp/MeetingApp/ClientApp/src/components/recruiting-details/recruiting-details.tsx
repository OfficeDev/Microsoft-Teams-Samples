import React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Flex, Menu } from '@fluentui/react-northstar'
import "../recruiting-details/recruiting-details.css"
import BasicDetails from "./basic-details/basic-details"
import Timeline from "./basic-details/timeline"
import Notes from "./basic-details/notes"
import QuestionsMobile from './questions/questions-mobile';
import BasicDetailsMobile from './basic-details/basic-details-mobile';

const RecruitingDetails = () => {
    const mobileMenuItems = [
        {
            key: 'overview',
            content: 'Overview',
        },
        {
            key: 'questions',
            content: 'Questions',
        }
    ];

    const [activeMobileMenu, setActiveMobileMenu] = React.useState(0);

    return (
        <>
            {/* Content for stage view */}
            <Flex gap="gap.small" padding="padding.medium" className="container">
                <Flex column gap="gap.small" padding="padding.medium" className="detailsContainer">
                    <BasicDetails />
                    <Timeline />
                    <Notes />
                </Flex>
                <Flex gap="gap.small" padding="padding.medium" className="questionsContainer">
                    Questions
                </Flex>
            </Flex>

            {/* Content for sidepanel/mobile view */}
            <Flex gap="gap.small" padding="padding.medium" className="container-mobile" column>
                <Menu 
                  defaultActiveIndex={0} 
                  items={mobileMenuItems} 
                  underlined 
                  onItemClick={(event: any, options: any) => setActiveMobileMenu(options.index)} 
                  className="menu-item"
                  primary/>
                <Flex column gap="gap.small">
                    {!activeMobileMenu && <BasicDetailsMobile />}
                    {activeMobileMenu && <QuestionsMobile />}
                </Flex>
            </Flex>
        </>
    )
}

export default (RecruitingDetails);