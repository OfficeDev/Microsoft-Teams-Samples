import * as React from 'react';
import { Flex, Card, Button, Avatar, Text, ChatIcon, CallVideoIcon, CallIcon, EmailIcon} from '@fluentui/react-northstar'
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
                <Flex gap="gap.small" padding="padding.medium">
                    <Flex column className="details">
                        <Text content="Contact" weight="bold" />
                        <Flex>
                            <Button icon={<EmailIcon />} iconOnly text title="Email" size="small"/>
                            <Text content={candidateDetails?.email} size="small" />
                        </Flex>
                        <Flex>
                            <Button icon={<CallIcon />} iconOnly text title="Call" size="small" />
                            <Text content={candidateDetails?.mobile} size="small" />
                        </Flex>
                    </Flex>
                    <Flex column className="details">
                        <Text content="Skills" weight="bold" />
                        <Text content={candidateDetails?.skills} size="small" />
                    </Flex>
                    <Flex column className="details">
                        <Text content="Attachments" weight="bold" />
                        <Text content="email" size="small" />
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