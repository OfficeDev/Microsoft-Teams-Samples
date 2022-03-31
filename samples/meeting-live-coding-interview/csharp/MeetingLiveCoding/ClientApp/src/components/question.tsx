import React, { useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { IQuestionDetails } from '../types/question';
import { Flex, Text } from '@fluentui/react-northstar'
import Editor from '@monaco-editor/react';
import { getLatestEditorValue } from "./services/getLatestEditorValue"
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

const Question =(props :any)=>
{
    const params = props.match.params;
    const baseUrl = window.location.origin;
    const questionNumber = params['questionId'];
    const [data, setData] = React.useState();
    const [meetingId, setMeetingId] = React.useState();
    const [connection, setConnection] = useState<null | HubConnection>(null);
    React.useEffect(() => {
        microsoftTeams.initialize();
        
    }, [])

    React.useEffect(() => {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context: any) => {
            setMeetingId(context.meetingId)
            getLatestEditorValue(questionNumber, context.meetingId).then((res: any) => {
                if(res.data.value != null && res.data.value != "")
                setData(res.data.value);
            })
        })

        // create a new signalr connection
        const connect = new HubConnectionBuilder()
            .withUrl(`${window.location.origin}/chatHub`)
            .withAutomaticReconnect()
            .build();

        setConnection(connect);
    }, []);

    // subscribe to a signalr connection
    React.useEffect(() => {
        if (connection) {
            connection
                .start()
                .then(() => {
                    connection.on("ReceiveMessage", (user: any, description: any, questionId:any, meetingId:any, baseUrl: any) => {
                        setData(description);
                    });
                })
                .catch((error) => console.log(error));
        }
    }, [connection]);

    // Emit action to send message to server.
    const emitMessageAction = (handleEditorChange: any, time: any) => {
        let timer: any;
        return (...argument: any) => {
            clearTimeout(timer);
            timer = setTimeout(() => {
                handleEditorChange.apply(this, argument);
            }, time)
        }
    }

     // Handle editor value change. 
    const handleEditor = emitMessageAction(async (value: any) => {
        if (connection) await connection.send("SendMessage", "EditorCode", value, questionNumber, meetingId, baseUrl);
    }, 2000);
    
    return (
        <>
            {IQuestionDetails.questions ? IQuestionDetails.questions.map((question) => {
                if (question.questionId == questionNumber){
                    return <>
                        <Flex gap="gap.small">
                            <Flex column>
                                <Flex> <Text className="text-ui" content="Question :" size="small" weight="bold" /> <Text className="text-ui" content={question.question} size="small" /></Flex>
                                <Flex> <Text className="text-ui" content="Language :" size="small" weight="bold" /> <Text className="text-ui" content={question.language} size="small" /></Flex>
                                <Flex><Text className="text-ui" content="Expected output :" size="small" weight="bold" /><Text className="text-ui" content={question.expectedOuput} size="small" /></Flex>
                            </Flex>
                        </Flex>
  
                        <Editor
                            height="80vh"
                            defaultLanguage={question.language}
                            defaultValue={question.defaultValue}
                            value={data}
                            onChange={handleEditor}
                        />
                    </>
                }
            }) : null}
        </>
    )
}

export default (Question)