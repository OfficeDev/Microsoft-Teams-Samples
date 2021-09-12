import { Flex, Card, Button, Avatar, Text, ChatIcon, CallVideoIcon } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"

const BasicDetails = () => {
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
                            <Text content="Aaron Brooker" weight="bold" />
                            <Text content="Software Engineer | 4yrs 8 mos" size="small" />
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
                        <Text content="email" size="small" />
                    </Flex>
                    <Flex column className="details">
                        <Text content="Skills" weight="bold" />
                        <Text content="email" size="small" />
                    </Flex>
                    <Flex column className="details">
                        <Text content="Attachments" weight="bold" />
                        <Text content="email" size="small" />
                    </Flex>
                    <Flex column className="source-details">
                        <Text content="Source" weight="bold" />
                        <Text content="email" size="small" />
                    </Flex>
                </Flex>
            </Card.Body>
        </Card>

    )
}

export default (BasicDetails);