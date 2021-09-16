import React from "react";
import { Input, Button } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"
import * as microsoftTeams from "@microsoft/teams-js";

const AddQuestions = (): React.ReactElement => {
    const [question, setQuestion] = React.useState<string>('');

    React.useEffect(() => {
        microsoftTeams.initialize();
    }, [])

    const saveQuestion = () => {
        microsoftTeams.tasks.submitTask(question);
        return true;
    }

    return (
        <>
            <Input placeholder="Type your question" onChange={(e: any) => setQuestion(e.target.defaultValue)} />
            <Button content="Add" onClick={saveQuestion} />
        </>
    )
}

export default AddQuestions;