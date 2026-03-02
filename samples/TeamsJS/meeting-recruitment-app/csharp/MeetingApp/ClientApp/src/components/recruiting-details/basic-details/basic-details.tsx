import * as React from 'react';
import {
    Flex,
    Card,
    Button,
    Avatar,
    Text,
    ChatIcon,
    CallVideoIcon,
    CallIcon,
    EmailIcon,
    PaperclipIcon,
    PopupIcon,
    Dropdown
} from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"
import { getCandidateDetails } from "../services/recruiting-detail.service"
import { ICandidateDetails } from './basic-details.types';
import * as microsoftTeams from "@microsoft/teams-js";

export interface IBasicDetailsProps {
    setSelectedCandidateIndex: (index: number, email: string) => void,
    downloadFile: () => void,
}

const BasicDetails = (props: IBasicDetailsProps) => {
    const [candidateDetails, setCandidateDetails] = React.useState<ICandidateDetails[]>([]);
    const [selectedIndex, setSelectedIndex] = React.useState(0);
    const [candidateNames, setCandidateNames] = React.useState<any[]>([]);
    const [hostClientType, sethostClientType] = React.useState<any>('');

    const handleNameChange = (event: any, dropdownProps?: any) => {
        setSelectedIndex(dropdownProps.value.key);
        props.setSelectedCandidateIndex(dropdownProps.value.key, dropdownProps.value.email);
    }

    const startCall = () => {
        microsoftTeams.executeDeepLink("https://teams.microsoft.com/l/call/0/0?users="+candidateDetails[selectedIndex]?.email);
    }

    const startChat = () => {
        microsoftTeams.executeDeepLink("https://teams.microsoft.com/l/chat/0/0?users="+candidateDetails[selectedIndex]?.email);
    }

    React.useEffect(() => {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context) => {
            sethostClientType(context.hostClientType);
        });

        getCandidateDetails()
            .then((res) => {
                console.log(res)
                const data = res.data as ICandidateDetails[];
                setCandidateDetails(data);
                const names = data?.map((candidate, index) => {
                    return {
                        header: candidate.candidateName,
                        key: index,
                        email: candidate.email
                    }
                });
                setCandidateNames(names);
                props.setSelectedCandidateIndex(selectedIndex, data[selectedIndex]?.email);
            })
            .catch((ex) => {
                console.log(ex)
            });
    }, [])

    return (
        <Card fluid aria-roledescription="card with basic details" className="basic-details-card">
            <Card.Header>
                <Flex gap="gap.small" space="between">
                    <Flex gap="gap.small">
                        <Avatar
                            image="https://fabricweb.azureedge.net/fabric-website/assets/images/avatar/large/jenny.jpg"
                            label="Copy bandwidth"
                            name="Evie yundt"
                            status="unknown"
                        />
                        <Flex column>
                            <Dropdown
                                activeSelectedIndex={selectedIndex}
                                items={candidateNames}
                                onChange={handleNameChange}
                                inline
                                value={candidateDetails[selectedIndex]?.candidateName}
                            />
                            <Text content={candidateDetails[selectedIndex]?.role} size="small" />
                        </Flex>
                    </Flex>
                    <Flex >
                        <Button icon={<ChatIcon />} iconOnly text title="Message" onClick={startChat}/>
                        <Button icon={<CallVideoIcon />} iconOnly text title="Call" onClick={startCall}/>
                    </Flex>
                </Flex>
                <hr className="details-separator" />
            </Card.Header>
            <Card.Body>
                <Flex gap="gap.small">
                    <Flex column className={hostClientType == "web" || hostClientType == "desktop" ? "details details-border" : "detailsMobile details-border"}>
                        <Text content="Contact" weight="bold" />
                        <Flex column gap="gap.small">
                            <Flex >
                                <Button icon={<EmailIcon size="medium" />} iconOnly text title="Email" size="small" />
                                <Text content={candidateDetails[selectedIndex]?.email} size="small" />
                            </Flex>
                            <Flex>
                                <Button icon={<CallIcon size="medium" />} iconOnly text title="Call" size="small" onClick={startCall}/>
                                <Text content={candidateDetails[selectedIndex]?.mobile} size="small" />
                            </Flex>
                        </Flex>
                    </Flex>
                    <Flex column className="details details-border">
                        <Text content="Skills" weight="bold" />
                        <Text content={candidateDetails[selectedIndex]?.skills} size="small" />
                    </Flex>
                    <Flex column className="source-details details-border">
                        <Text content="Attachments" weight="bold" />
                        <Flex column gap="gap.small">
                            <Flex>
                                <Button
                                    icon={<PaperclipIcon />}
                                    primary
                                    text
                                    content={'Resume'}
                                    size="small"
                                    className="iconText"
                                    onClick={props.downloadFile} />
                            </Flex>
                            <Flex>
                                <Button
                                    icon={<PopupIcon />}
                                    primary
                                    text
                                    content={'portfolio.com'}
                                    size="small"
                                    className="iconText"
                                    onClick={() => {
                                        window.open(candidateDetails[selectedIndex].linkedInUrl)
                                    }} />
                            </Flex>
                        </Flex>
                    </Flex>
                    <Flex column className="source-details">
                        <Text content="Source" weight="bold" />
                        <Text content={candidateDetails[selectedIndex]?.source} size="small" />
                    </Flex>
                </Flex>
            </Card.Body>
        </Card>

    )
}

export default (BasicDetails);