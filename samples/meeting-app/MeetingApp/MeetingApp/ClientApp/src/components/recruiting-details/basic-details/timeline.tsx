import { Flex, Card, Button, Avatar, Text, ChatIcon, CallVideoIcon } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"

const Timeline = () => {
    return (
        <Card fluid aria-roledescription="card with basic details" className="timeline-card">
            <Card.Header>
                <Flex gap="gap.small">
                    <Text content="Timeline" weight="bold" />
                </Flex>
                <hr className="details-separator" />
            </Card.Header>
            <Card.Body>
                
            </Card.Body>
        </Card>

    )
}

export default (Timeline);