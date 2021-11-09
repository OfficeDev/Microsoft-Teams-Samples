import React from "react";
import { Flex, Button, Header, TextArea, Checkbox } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"
import * as microsoftTeams from "@microsoft/teams-js";

const ShareAssets = (): React.ReactElement => {
    const [checkboxValues, setCheckboxValues] = React.useState(
        {
            note: "",
            checkedValues: [
                {
                    id: 1,
                    name: "FAQ.pdf",
                    isChecked: false
                },
                {
                    id: 2,
                    name: "Docs.docx",
                    isChecked: false
                },
                {
                    id: 3,
                    name: "T&C.docx",
                    isChecked: false
                }
            ]
        }
    );

    React.useEffect(() => {
        microsoftTeams.initialize();
    }, [])

    const saveNote = () => {
        let temp = new Array();
        checkboxValues.checkedValues.map(item => {
            if (item.isChecked == true) {
                temp.push(item.name);
            }
        })

        setCheckboxValues({ ...checkboxValues, checkedValues: temp });
        microsoftTeams.tasks.submitTask(JSON.stringify(checkboxValues));
        return true;
    }

    const onCheckboxChange = (event: any, data: any, index: number) => {
        let newArray = [...checkboxValues.checkedValues];
        let label = "";
        if (index == 1) {
            label = "FAQ.pdf";
        }
        else if (index == 2) {
            label = "Docs.docx";
        }
        else if (index == 3) {
            label = "T&C.docx";
        }
        newArray[index - 1] = { id: index, name: label, isChecked: data.checked };
        setCheckboxValues({ ...checkboxValues, checkedValues: newArray });
    }

    return (
        <>
            <Flex column gap="gap.smaller" padding="padding.medium" className="shareAssetsTaskModule">
                <Flex column>
                    <Header as="h5" content={'Select the files to include'} />
                    <Checkbox label="FAQs.pdf" onChange={(event: any, data: any) => onCheckboxChange(event, data, 1)} />
                    <Checkbox label="Document checklist.docx" onChange={(event: any, data: any) => onCheckboxChange(event, data, 2)} />
                    <Checkbox label="Standard terms and conditions" onChange={(event: any, data: any) => onCheckboxChange(event, data, 3)} />
                </Flex>
                <Flex column>
                    <Header as="h5" content={'Notes'} />
                    <TextArea
                        fluid
                        placeholder="Add note"
                        onChange={(event: any) => setCheckboxValues({ ...checkboxValues, note: event.target.value })}
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