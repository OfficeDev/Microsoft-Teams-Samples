import * as React from 'react';
import { Flex, Card, Button, Avatar, Text, ChatIcon, CallVideoIcon, CallIcon, EmailIcon, PaperclipIcon, PopupIcon } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"
import { getCandidateDetails } from "../services/recruiting-detail.service"
import { ICandidateDetails } from './basic-details.types';

const BasicDetails = () => {
    const [candidateDetails, setCandidateDetails] = React.useState<ICandidateDetails>();
    React.useEffect(() => {
        getCandidateDetails()
            .then((res) => {
                console.log(res)
                const data = res.data as ICandidateDetails;
                setCandidateDetails(data);
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
                            <Text content={candidateDetails?.candidateName} weight="bold" />
                            <Text content={candidateDetails?.role} size="small" />
                        </Flex>
                    </Flex>
                    <Flex >
                        <Button icon={<ChatIcon />} iconOnly text title="Favourite" />
                        <Button icon={<CallVideoIcon />} iconOnly text title="Download" />
                    </Flex>
                </Flex>
                <hr className="details-separator" />
            </Card.Header>
            <Card.Body>
                <Flex gap="gap.small">
                    <Flex column className="details details-border">
                        <Text content="Contact" weight="bold" />
                        <Flex column gap="gap.small">
                            <Flex >
                                <Button icon={<EmailIcon size="medium" />} iconOnly text title="Email" size="small" />
                                <Text content={candidateDetails?.email} size="small" />
                            </Flex>
                            <Flex>
                                <Button icon={<CallIcon size="medium" />} iconOnly text title="Call" size="small" />
                                <Text content={candidateDetails?.mobile} size="small" />
                            </Flex>
                        </Flex>
                    </Flex>
                    <Flex column className="details details-border">
                        <Text content="Skills" weight="bold" />
                        <Text content={candidateDetails?.skills} size="small" />
                    </Flex>
                    <Flex column className="source-details details-border">
                        <Text content="Attachments" weight="bold" />
                        <Flex column gap="gap.small">
                            <Flex>
                                <Button icon={<PaperclipIcon size="medium" />} iconOnly text title="Email" size="small" />
                                <Text content={'Resume'} size="small" className="iconText"/>
                            </Flex>
                            <Flex>
                                <Button icon={<PopupIcon size="medium" />} iconOnly text title="Open link" size="small" />
                                <Text content={'portfolio.com'} size="small" className="iconText"/>
                            </Flex>
                        </Flex>
                    </Flex>
                    <Flex column className="source-details">
                        <Text content="Source" weight="bold" />
                        <Text content={candidateDetails?.source} size="small" />
                    </Flex>
                </Flex>
            </Card.Body>
        </Card>

    )
}

export default (BasicDetails);