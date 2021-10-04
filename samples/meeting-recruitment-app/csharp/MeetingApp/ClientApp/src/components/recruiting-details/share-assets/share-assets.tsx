import React from "react";
import { Flex, Button, Header, TextArea, Checkbox } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"
import * as microsoftTeams from "@microsoft/teams-js";

const ShareAssets = (): React.ReactElement => {
    const [note, setNote] = React.useState<string>('');

    React.useEffect(() => {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context) => {
            console.log(context)
        })
    }, [])

    const saveNote = () => {
        microsoftTeams.tasks.submitTask(note);
        return true;
    }

    return (
        <>
            <Flex column gap="gap.smaller" padding="padding.medium" className="shareAssetsTaskModule">
                <Flex column>
                    <Header as="h5" content={'Select the files to include'} />
                    <Checkbox label="FAQs.pdf" />
                    <Checkbox label="Document checklist.docx" />
                    <Checkbox label="Standard terms and conditions" />
                </Flex>
                <Flex column>
                    <Header as="h5" content={'Notes'} />
                    <TextArea
                        fluid
                        placeholder="Add note"
                        onChange={(event: any) => setNote(event.target.value)}
                        className="shareAssetsText" />
                </Flex>
                <Flex gap="gap.smaller" hAlign="end">
                    <Button content="Cancel" secondary onClick={() => microsoftTeams.tasks.submitTask(undefined)} />
                    <Button content="Share" primary onClick={saveNote} />
                </Flex>
            </Flex>
        </>
    )
}

export default ShareAssets;