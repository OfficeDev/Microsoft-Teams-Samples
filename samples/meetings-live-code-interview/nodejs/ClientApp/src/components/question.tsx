import React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { IQuestionDetails } from '../types/question';
import { Flex, Text } from '@fluentui/react-northstar'
import Editor from '@monaco-editor/react';
import io from "socket.io-client";
import "./tab.css"

const Question = (props: any) => {
    const params = props.match.params;
    const questionNumber = params['srno'];
    const [data, setData] = React.useState();
    const socket = io();

    React.useEffect(() => {
        microsoftTeams.initialize();

    }, [])

    const debounce = (callback: any, lim: any) => {
        let timer: any;
        return (...args: any) => {
            clearTimeout(timer);
            timer = setTimeout(() => {
                callback.apply(this, args);
            }, lim)
        }
    }

    // Handle editor value change. 
    const handleEditor = debounce((value: any) => {
        socket.on('connection', () => {
        });
        socket.emit('message', value);
    }, 2000);

    // get latest state of editor.
    socket.on("message", data => {
        setData(data);
    });

    return (
        <>
            {IQuestionDetails.questions ? IQuestionDetails.questions.map((question) => {
                if (question.srNo == questionNumber) {
                    return <>
                        <Flex gap="gap.small">
                            <Text content={question.srNo} weight="bold" />
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