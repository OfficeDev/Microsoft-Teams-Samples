import React from "react";
import { Flex, Card, Button, Text, AddIcon, TextArea } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"
import { getQuestions } from "../services/recruiting-detail.service";
import * as microsoftTeams from "@microsoft/teams-js";
import { IQuestionDetails } from "../../../types/recruitment.types";

export interface IQuestionProps {
    questionsSet: IQuestionDetails[],
    setRating: (event: any) => void,
    saveComment: (rowKey: string) => void
    setShowAddComment: (rowKey: string, isShow: boolean) => void
}

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
                                    <Text content={questionDetail.question} />
                                    <Flex gap="gap.small">
                                        {getRatings(questionDetail.rowKey, questionDetail.rating!)}
                                    </Flex>

                                    <Flex>
                                        <Button
                                            text
                                            size="small"
                                            icon={<AddIcon size="small" />}
                                            content="Add comment"
                                            iconPosition="before"
                                            className="add-button"
                                            hidden={questionDetail.showAddComment}
                                            onClick={() => { props.setShowAddComment(questionDetail.rowKey, true) }} />
                                    </Flex>

                                    <Flex gap="gap.small" column hidden={!questionDetail.showAddComment}>
                                        <Text content="Comment" />
                                        <Text size="smaller" content={questionDetail.comment} hidden={questionDetail.comment == ''} />
                                        <TextArea
                                            placeholder="Add comment here..."
                                            className="add-comment-textarea"
                                            id={questionDetail.rowKey + `_textarea`}
                                            hidden={questionDetail.showAddComment && questionDetail.comment != ''} />
                                        <Flex gap="gap.smaller" hidden={questionDetail.comment != ''}>
                                            <Button
                                                size="small"
                                                content="Discard"
                                                secondary
                                                onClick={() => {
                                                    props.setShowAddComment(questionDetail.rowKey, false);
                                                }} />
                                            <Button size="small" content="Save" primary onClick={() => props.saveComment(questionDetail.rowKey)} />
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