import React from "react";
import { Flex, Button, Dropdown, Text, TextArea } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"
//import * as microsoftTeams from "@microsoft/teams-js";
import { app, dialog } from "@microsoft/teams-js";
const EditQuestion = (props: any): React.ReactElement => {
    const [question, setQuestion] = React.useState<any>('');
    React.useEffect(() => {
        app.initialize();
        const search = props.location.search;
        const editText = new URLSearchParams(search).get('editText');
        setQuestion(editText);
    }, [])

    const saveQuestion = () => {
        dialog.url.submit(question.trim());
        return true;
    }

    return (
        <>
            {question != '' &&
                <Flex column gap="gap.medium" padding="padding.medium">
                    <Text content="Please edit the below question" />
                    <TextArea fluid defaultValue={question} className="editTextArea"
                        onChange={(event: any) => {
                            setQuestion(event.target.value)
                        }} />
                    <Flex>
                        <Button primary content="Update" onClick={saveQuestion} />
                    </Flex>
                </Flex>
            }
        </>
    )
}

export default EditQuestion;