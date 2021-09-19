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
           <Flex column gap="gap.smaller">
             <TextArea placeholder="Add note" onChange={(event: any) => setNote(event.target.value)} />
             <Button content="Add" onClick={saveNote} />
            </Flex>
        </>
    )
}

export default AddNotes;