import React from "react";
import { Flex, Card, Button, Text, AddIcon, TextArea } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"
import { IQuestionDetails } from "../../../types/recruitment.types";

export interface IQuestionProps {
    questionsSet: IQuestionDetails[],
    setRating: (event: any) => void,
    saveComment: (rowKey: string) => void
    setShowAddComment: (rowKey: string, isShow: boolean) => void
}

// Component for Questions section in mobile for feedback by interviewer
const QuestionsMobile = (props: IQuestionProps): React.ReactElement => {
    const getRatings = (questionId: string, currentRating: number) => {
        const prevItems = [];
        for (var i = 1; i <= 5; i++) {
            prevItems.push(
                <Button
                    size="small"
                    circular
                    content={i}
                    id={questionId}
                    onClick={props.setRating}
                    className={currentRating >= i ? 'selectedBtn' : 'defaultBtn'} />
            )
        }
        return prevItems;
    }

    return (
        <>
            {
                props.questionsSet.length && props.questionsSet.map((questionDetail, index) => {
                    return (
                        <Card fluid aria-roledescription="card with question details" className="questions-card-mobile">
                            <Card.Body>
                                <Flex gap="gap.small" column>
                                    <Text content={questionDetail.Question} />
                                    <Flex gap="gap.small">
                                        {getRatings(questionDetail.RowKey, questionDetail.Rating!)}
                                    </Flex>

                                    <Flex>
                                        <Button
                                        text
                                        size="small"
                                        icon={<AddIcon size="small" />}
                                        content="Add comment"
                                        iconPosition="before"
                                        className="add-button"
                                        hidden={questionDetail.ShowAddComment}
                                        onClick={() => { props.setShowAddComment(questionDetail.RowKey, true) }} />
                                    </Flex>

                                    <Flex gap="gap.small" column hidden={!questionDetail.ShowAddComment}>
                                        <Text content="Comment" />
                                        <Text size="smaller" content={questionDetail.Comment} hidden={questionDetail.Comment == ''} />
                                        <TextArea
                                            placeholder="Add comment here..."
                                            className="add-comment-textarea"
                                            id={questionDetail.RowKey + `_textarea`}
                                            hidden={questionDetail.ShowAddComment && questionDetail.Comment != ''} />
                                        <Flex gap="gap.smaller" hidden={questionDetail.Comment != ''}>
                                            <Button
                                                size="small"
                                                content="Discard"
                                                secondary
                                                onClick={() => {
                                                    props.setShowAddComment(questionDetail.RowKey, false);
                                                }} />
                                            <Button size="small" content="Save" primary onClick={() => props.saveComment(questionDetail.RowKey)} />
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