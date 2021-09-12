import { Flex, Card, Button, Text, AddIcon, RadioGroup } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"

const QuestionsMobile = (): React.ReactElement => {
    const questionDetails = [
        {
            key: "key1",
            question: "Question 1",
            rating: 5,
            comments: {
                comment: "Comment 1",
                commentedBy: "User 1"
            }
        },
        {
            key: "key2",
            question: "Question 2",
            rating: 5,
            comments: {
                comment: "Comment 1",
                commentedBy: "User 1"
            }
        }
    ]
    return (
        <>
            {
                questionDetails.length > 0 && questionDetails.map((questionDetail, index) => {
                    return (
                        <Card fluid aria-roledescription="card with question details" className="questions-card-mobile">
                            <Card.Body>
                                <Flex gap="gap.small" column>
                                    <Text content={questionDetail.question} />
                                    <Flex>
                                        <Button size="small" circular content="1" />
                                        <RadioGroup
                                            checkedValue={checkedValue}
                                            items={items}
                                            vertical={vertical}
                                            onCheckedValueChange={(e, data) => setCheckedValue(data.value)}
                                        />
                                    </Flex>
                                    <Button
                                        text
                                        size="small"
                                        icon={<AddIcon size="small" />}
                                        content="Add comment"
                                        iconPosition="before"
                                        className="add-button" />
                                </Flex>
                            </Card.Body>
                        </Card>
                    )
                })
            }
        </>
    )
}

export default (QuestionsMobile);