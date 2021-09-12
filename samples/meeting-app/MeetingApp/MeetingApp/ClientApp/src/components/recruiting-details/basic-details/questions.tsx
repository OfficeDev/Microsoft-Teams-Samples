import React from "react";
import { Flex, Card, Button, Text, AddIcon, TextArea } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"

const Questions = (): React.ReactElement => {
    const [questionDetails, setQuestionDetails] = React.useState<any[]>([
        {
            key: "key1",
            question: "Question 1",
        },
        {
            key: "key2",
            question: "Question 2",
        }
    ]);
    const [ratingsArray, setRatingsArray] = React.useState<any[]>([]);

    React.useEffect(() => {
        const prevItems = [];
        for (var i = 1; i <= 5; i++) {
            prevItems.push(
                <Button size="small" circular content={i} />
            )
        }
        console.log(prevItems);
        setRatingsArray(prevItems);
    }, [])

    return (
        <>
            {
                questionDetails.length > 0 && questionDetails.map((questionDetail, index) => {
                    return (
                        <Card fluid aria-roledescription="card with question details" className="questionsCard">
                            <Card.Body>
                                <Flex gap="gap.smaller" column>
                                    <Text content={questionDetail.question} />
                                    <Flex gap="gap.small">
                                        {ratingsArray}
                                    </Flex>
                                </Flex>
                            </Card.Body>
                        </Card>
                    )
                })
            }
        </>
    )
}

export default (Questions);