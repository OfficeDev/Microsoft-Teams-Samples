import { Flex, Card, Button, Text, AddIcon } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"

const BasicDetailsMobile = () => {
    return (
        <Card fluid aria-roledescription="card with basic details" className="notes-card">
            <Card.Header>
                <Flex gap="gap.small" space="between">
                <Text content="Notes" weight="bold" />
                    <Flex >
                        <Button size="small" icon={<AddIcon  size="small"/>} content="Add a note" iconPosition="before" />
                    </Flex>
                </Flex>
                <hr className="details-separator" />
            </Card.Header>
            <Card.Body>
            </Card.Body>
        </Card>

    )
}

export default (BasicDetailsMobile);