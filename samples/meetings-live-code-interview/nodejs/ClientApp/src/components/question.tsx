import React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { IQuestionDetails } from '../types/question';
import { Flex, Text } from '@fluentui/react-northstar'
import Editor from '@monaco-editor/react';
import io from "socket.io-client";

const Question =(props :any)=>
{
    const params = props.match.params;
    const questionNumber = params['srno'];
    const [data, setData] = React.useState();
    //const socket = io("http://localhost:3978");
    React.useEffect(() => {
        microsoftTeams.initialize();
        
    }, [])

    const handleEditor = (value: any)=>{
        // socket.on('connection', () => {
        //     socket.connect();
        //     socket.emit('message', value);
        // }); 
    }

    // socket.on("message", data => {
    //     setData(data);
    //   });
    
    return (
        <>
            {IQuestionDetails.questions ? IQuestionDetails.questions.map((question) => {
                if(question.srNo === questionNumber){
                    return <>
                        <Flex gap="gap.small">
                            <Text content={question.srNo} weight="bold" />
                            <Flex column>
                                <Text content={"Question: " + question.question} weight="bold" />
                                <Text content={"Language: " + question.language} size="small" />
                            </Flex>
                        </Flex>
  
                        <Editor
                            height="70vh"
                            defaultLanguage={question.language}
                            defaultValue={data}
                            onChange={handleEditor}
                        />
                    </>
                }
            }) : null}
        </>
    )
}

export default (Question)