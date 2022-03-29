import React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { IQuestionDetails } from '../types/question';
import { Flex, Text } from '@fluentui/react-northstar'
import Editor from '@monaco-editor/react';
import io from "socket.io-client";
import {getLatestEditorValue} from "./services/getLatestEditorValue"
import "./tab.css"

const Question = (props: any) => {
    const params = props.match.params;
    const questionNumber = params['questionId'];
    const [data, setData] = React.useState();
    const [meetingId, setMeetingId] = React.useState();
    const [socket, setSocket] = React.useState(io());

    React.useEffect(() => {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context:any) =>{
            setMeetingId(context.meetingId)
            getLatestEditorValue(questionNumber,context.meetingId).then((res:any) =>{
                setData(res.data.value);
            })
        })
        setSocket(io());
        
    }, [])

    // subscribe to the socket event
    React.useEffect(() => {
    if (!socket) return;
    socket.on('connection', () => {
        socket.connect();
    });

    // get latest state of editor.
    socket.on("message", data => {
        setData(data);
    });
 
  }, [socket]);

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
    const handleEditor = emitMessageAction((value: any) => {
        socket.emit('message', value,questionNumber,meetingId);
    }, 2000);

    return (
        <>
            {IQuestionDetails.questions ? IQuestionDetails.questions.map((question) => {
                if (question.questionId == questionNumber) {
                    return <>
                        <Flex gap="gap.small">
                            <Text content={question.questionId} weight="bold" />
                            <Flex column>
                                <Text className="text-ui" content={"Question: " + question.question} weight="bold" />
                                <Text className="text-ui" content={"Language: " + question.language} size="small" />
                                <Text className="text-ui" content={"Expected output: " + question.expectedOuput} size="small" />
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