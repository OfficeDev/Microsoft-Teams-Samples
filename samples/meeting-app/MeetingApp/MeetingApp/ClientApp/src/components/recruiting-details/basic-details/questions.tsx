import React from "react";
import { Flex, Card, Button, Text, Header, AddIcon } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"
import * as microsoftTeams from "@microsoft/teams-js";
import { IQuestionSet } from "./basic-details.types";
import { saveQuestions } from "../services/recruiting-detail.service";

const Questions = (): React.ReactElement => {
    const [questionDetails, setQuestionDetails] = React.useState<any[]>([
        {
            key: 1,
            question: "What are SDLC models available?",
        },
        {
            key: 2,
            question: "What are function points?",
        }
    ]);

    const [ratingsArray, setRatingsArray] = React.useState<any[]>([]);

    const addQuestionsTaskModule = () => {
        let taskInfo = {
            title: "Questions",
            height: 250,
            width: 250,
            url: `${window.location.origin}/questions`,
        };

        microsoftTeams.tasks.startTask(taskInfo, (err, question) => {
            const currentQuestions = [...questionDetails]
            currentQuestions.push({key: currentQuestions.length + 1, question: question})
            setQuestionDetails(currentQuestions);
           
        });
    };

    React.useEffect((): any => {
        const prevItems = [];
        for (var i = 1; i <= 5; i++) {
            prevItems.push(
                <Button size="small" circular content={i} />
            )
        }
        setRatingsArray(prevItems);
        microsoftTeams.initialize();
        microsoftTeams.getContext((context) => {
            const questDetails: IQuestionSet[] = [];
            questionDetails.map((questionDetail, index) => {
                questDetails.push({
                    meetingId: context.meetingId,
                    question: questionDetail,
                    setBy: context.userPrincipalName,
                    isDelete: 0
                });
            })

            saveQuestions(questDetails[0]);
        })
    }, [])

    return (
        <>
            <Flex gap="gap.smaller">
                <Header as="h4" content="Questions" className="questionsHeader"/>
                <AddIcon onClick={addQuestionsTaskModule} title="Add new questions"/>
            </Flex>
            <Text content="Questions added here will appear in meeting with candidate and can help you rate at the point of time" />
            {
                questionDetails.map((questionDetail, index) => {
                    return (
                        <>
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
                        </>
                    )
                })
            }
        </>
    )
}

export default (Questions);