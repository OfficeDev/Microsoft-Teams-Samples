import React from "react";
import { Flex, Button, Dropdown } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"
import * as microsoftTeams from "@microsoft/teams-js";

const AddQuestions = (): React.ReactElement => {
    const [question, setQuestion] = React.useState<any>();
    const inputQuestions = [
        "What are SDLC processes"
    ]
    React.useEffect(() => {
        microsoftTeams.initialize();
    }, [])

    const saveQuestion = () => {
        microsoftTeams.tasks.submitTask(question);
        return true;
    }

    return (
        <>
           <Flex column gap="gap.smaller">
             <Dropdown clearable items={inputQuestions} placeholder="Select question" onChange={(event, option): void => {setQuestion(option.value)}} />
             <Button content="Add" onClick={saveQuestion} />
            </Flex>
        </>
    )
}

export default AddQuestions;