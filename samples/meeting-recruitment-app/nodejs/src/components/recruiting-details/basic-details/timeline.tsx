import * as React from 'react';
import { Flex, Card, Text, NotesIcon } from '@fluentui/react-northstar'
import * as microsoftTeams from "@microsoft/teams-js";
import "../../recruiting-details/recruiting-details.css"

const Timeline = () => {
    const [hostClientType, sethostClientType] = React.useState<any>('');

    React.useEffect(() => {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context) => {
            sethostClientType(context.hostClientType);
        });
    }, []);

    return (
        <Card fluid aria-roledescription="card with basic details" className="timeline-card">
            <Card.Header>
                <Flex gap="gap.small">
                    <Text content="Timeline" weight="bold" />
                </Flex>
                <hr className="details-separator" />
            </Card.Header>
            <Card.Body>
                <Flex gap="gap.small" className="timeline" column>
                    <Flex gap="gap.small" className={hostClientType == "web" || hostClientType == "desktop" ? "timelineContainer" : "timelineContainerMobile"}>
                        <Flex column className="timelineDetail">
                            <Text content='26 Nov, 2020' weight="bold" />
                            <Text content='14:00' />
                        </Flex>
                        <Flex column className="timelineDetail">
                            <Text content='Stage' />
                            <Text content='Shortlisted' weight="bold" />
                        </Flex>
                        <Flex column className="timelineDetail">
                            <Text content='Hiring Team' />
                            <Text content='Daniela' weight="bold" />
                        </Flex>
                        <Flex column className="timelineDetail">
                            <Text content='Result' />
                            <Text content='Shortlisted' weight="bold" />
                        </Flex>
                        <Flex>
                            <NotesIcon />
                        </Flex>
                    </Flex>
                    <Flex gap="gap.small" className={hostClientType == "web" || hostClientType == "desktop" ? "timelineContainer" : "timelineContainerMobile"}>
                        <Flex column className="timelineDetail">
                            <Text content='26 Nov, 2020' weight="bold" />
                            <Text content='14:00' />
                        </Flex>
                        <Flex column className="timelineDetail">
                            <Text content='Stage' />
                            <Text content='Round 1' weight="bold" />
                        </Flex>
                        <Flex column className="timelineDetail">
                            <Text content='Hiring Team' />
                            <Text content='Ray' weight="bold" />
                        </Flex>
                        <Flex column className="timelineDetail">
                            <Text content='Result' />
                            <Text content='Hire' weight="bold" />
                        </Flex>
                        <Flex>
                            <NotesIcon />
                        </Flex>
                    </Flex>
                </Flex>
            </Card.Body>
        </Card>
    )
}

export default (Timeline);