import { Flex, Card, Avatar, Text, Header, Label } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"

const BasicDetailsMobile = () => {
    return (
        <Card fluid aria-roledescription="card with basic details" className="basic-details-card-mobile">
            <Card.Header>
                <Text content="Candidate Details" weight="bold" />
            </Card.Header>
            <Card.Body>
                <Flex gap="gap.small" padding="padding.medium" column className="basicDetails">
                    <Flex gap="gap.small">
                        <Avatar
                            image="https://fabricweb.azureedge.net/fabric-website/assets/images/avatar/large/jenny.jpg"
                            label="Copy bandwidth"
                            name="Evie yundt"
                            status="unknown"
                        />
                        <Flex column>
                            <Text content="Aaron Brooker" weight="bold" />
                            <Text content="Software Engineer" size="small" />
                        </Flex>
                    </Flex>
                    <Flex>
                        <Text content="Experience: "/>
                        <Text content="4 yrs 8 mos"/>
                    </Flex>
                    <Flex>
                        <Text content="Education: "/>
                        <Text content="B Tech"/>
                    </Flex>
                    <Flex>
                        <Header as="h5" content="Skills" className="subHeaders"/>
                    </Flex>
                    <Flex>
                        <Header as="h5" content="Links" className="subHeaders" />
                    </Flex>
                </Flex>
            </Card.Body>
        </Card>
    )
}

export default (BasicDetailsMobile);