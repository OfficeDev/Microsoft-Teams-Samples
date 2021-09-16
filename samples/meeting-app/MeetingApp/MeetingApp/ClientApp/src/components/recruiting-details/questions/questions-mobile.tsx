import React from "react";
import { Flex, Card, Button, Text, AddIcon, TextArea } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"

const QuestionsMobile = (): React.ReactElement => {
    const [questionDetails, setQuestionDetails] = React.useState<any[]>([
        {
            key: "key1",
            question: "What are SDLC models available?",
            rating: 5,
            comments: {
                comment: "Comment 1",
                commentedBy: "User 1"
            },
            showAddComment: false
        },
        {
            key: "key2",
            question: "What are function points?",
            rating: 5,
            comments: {
                comment: "Comment 1",
                commentedBy: "User 1"
            },
            showAddComment: false
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
        console.log(localStorage.getItem("questionSet"));
    }, [])

    const setShowAddComment = (index: number, isShow: boolean) => {
        const currentQuestions = [...questionDetails];
        currentQuestions[index] = isShow ? { ...currentQuestions[index], showAddComment: true }
            : { ...currentQuestions[index], showAddComment: false };
        setQuestionDetails(currentQuestions);
    }

    return (
        <>
            {
                questionDetails.length > 0 && questionDetails.map((questionDetail, index) => {
                    return (
                        <Card fluid aria-roledescription="card with question details" className="questions-card-mobile">
                            <Card.Body>
                                <Flex gap="gap.small" column>
                                    <Text content={questionDetail.question} />
                                    <Flex gap="gap.small">
                                        {ratingsArray}
                                    </Flex>
                                    <Button
                                        text
                                        size="small"
                                        icon={<AddIcon size="small" />}
                                        content="Add comment"
                                        iconPosition="before"
                                        className="add-button"
                                        hidden={questionDetail.showAddComment}
                                        onClick={() => { setShowAddComment(index, true) }} />
                                    <Flex gap="gap.small" column hidden={!questionDetail.showAddComment}>
                                        <Text content="Comment" />
                                        <TextArea placeholder="Add comment here..." className="add-comment-textarea"/>
                                        <Flex gap="gap.smaller">
                                            <Button
                                                size="small"
                                                content="Discard"
                                                secondary
                                                onClick={() => { setShowAddComment(index, false) }} />
                                            <Button size="small" content="Save" primary />
                                        </Flex>
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

export default (QuestionsMobile);