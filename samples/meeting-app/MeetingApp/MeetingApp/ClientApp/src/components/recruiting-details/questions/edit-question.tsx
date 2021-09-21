import React from "react";
import { Flex, Button, Dropdown, Input } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"
import * as microsoftTeams from "@microsoft/teams-js";

const EditQuestion = (props: any): React.ReactElement => {
    const [question, setQuestion] = React.useState<any>('');
    React.useEffect(() => {
        microsoftTeams.initialize();
        const search = props.location.search;
        const editText = new URLSearchParams(search).get('editText');
        if (editText != null)
            setQuestion(editText);
    }, [])

    const saveQuestion = () => {
        microsoftTeams.tasks.submitTask(question);
        return true;
    }

    return (
        <>
            <Flex column gap="gap.smaller" padding="padding.medium">
                <Input fluid defaultValue={question} onChange={(event: any) => { setQuestion(event.target.defaultValue) }} />
                <Button content="Add" onClick={saveQuestion} />
            </Flex>
        </>
    )
}

export default EditQuestion;