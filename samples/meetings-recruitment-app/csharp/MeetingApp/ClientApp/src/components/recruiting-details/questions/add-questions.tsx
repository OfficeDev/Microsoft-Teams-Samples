import React from "react";
import { Flex, Button, Checkbox, Text, formTextAreaClassName } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"
import * as microsoftTeams from "@microsoft/teams-js";

const AddQuestions = (props: any): React.ReactElement => {
    const [questions, setQuestions] = React.useState<any[]>([
        {
            key: 1,
            value: "What are SDLC processes?",
            checked: false
        },
        {
            key: 2,
            value: "What are function poniters?",
            checked: false
        },
        {
            key: 3,
            value: "What is dependency injection?",
            checked: false
        },
        {
            key: 4,
            value: "What is Encapsulation?",
            checked: false
        },
        {
            key: 5,
            value: "What is recursion?",
            checked: false
        }
    ]);
  
    React.useEffect(() => {
        microsoftTeams.initialize();
    }, [])

    const saveQuestion = () => {
        microsoftTeams.tasks.submitTask(JSON.stringify(questions));
        return true;
    }

    const checkUncheck = (props: any, index: number) => {
       const currentQuest = [...questions];
       const questToUpdate = currentQuest.find(quest => quest.key == index + 1);
        questToUpdate.checked = props.checked;
        setQuestions(currentQuest);
    }

    return (
        <>
            <Flex column gap="gap.smaller" padding="padding.medium">
                <Text content="Please select questions for meeting"/>
                {
                     questions.map((question, index) => {
                        return (
                            <Checkbox label={question.value} defaultValue={question.value} onChange={(event, props) => {checkUncheck(props, index)}}/>
                        )
                    })
                }
                <Flex>
                 <Button primary content="Save selection" onClick={saveQuestion} />
                </Flex>
            </Flex>
        </>
    )
}

export default AddQuestions;