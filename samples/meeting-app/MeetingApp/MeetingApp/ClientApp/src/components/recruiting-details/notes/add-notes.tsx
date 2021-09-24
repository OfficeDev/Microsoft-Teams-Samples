import React from "react";
import { Flex, Button, Text, TextArea } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"
import * as microsoftTeams from "@microsoft/teams-js";

const AddNotes = (): React.ReactElement => {
    const [note, setNote] = React.useState<string>('');

    React.useEffect(() => {
        microsoftTeams.initialize();
    }, [])

    const saveNote = () => {
        microsoftTeams.tasks.submitTask(note);
        return true;
    }

    return (
        <>
            <Flex column gap="gap.small" padding="padding.medium">
                <Text content="Please add your notes here" />
                <TextArea placeholder="Add note" className="editTextArea"
                    onChange={(event: any) => setNote(event.target.value)} />
                <Flex>
                    <Button primary content="Add note" onClick={saveNote} />
                </Flex>
            </Flex>
        </>
    )
}

export default AddNotes;