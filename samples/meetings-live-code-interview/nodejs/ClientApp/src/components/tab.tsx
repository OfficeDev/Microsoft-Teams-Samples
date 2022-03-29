import React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Flex, Button, Text, Card, CardBody, CardHeader } from '@fluentui/react-northstar'
import "./tab.css"
import { IQuestionDetails } from '../types/question';

const Tab = () => {
    const [frameContext, setframeContext] = React.useState<any>('');
    React.useEffect(() => {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context) => {
            setframeContext(context.frameContext);
        });
    }, [])

    // Share specific content 
    const shareSpecificPart = (partName: number) => {
        var appContentUrl = "";
        appContentUrl = `${window.location.origin}/question/${partName}`;
        microsoftTeams.meeting.shareAppContentToStage((error, result) => {
            if (result) {
                // handle success
                console.log("success")
            }

            if (error) {
                // handle error
                console.log(JSON.stringify(error))
            }
        }, appContentUrl);
    };

    return (
        <>
            <div hidden={frameContext !== "sidePanel"}>
                {IQuestionDetails.questions ? IQuestionDetails.questions.map((question) => {
                    return <Card className="card">
                        <CardHeader>
                            <Flex gap="gap.small">
                                <Text content={question.questionId} weight="bold" />
                                <Flex column>
                                    <Text className="text-ui" content={"Question: " + question.question} weight="bold" />
                                    <Text className="text-ui" content={"Language: " + question.language} size="small" />
                                </Flex>
                            </Flex>
                        </CardHeader>
                        <CardBody>
                            <Button className="send-btn" content="Share" onClick={() => shareSpecificPart(question.questionId)} />
                        </CardBody>
                    </Card>
                }) : <div> No content to show</div>}
            </div>
        </>
    )
}

export default (Tab);