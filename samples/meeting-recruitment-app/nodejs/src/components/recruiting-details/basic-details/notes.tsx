import * as React from 'react';
import { Flex, Card, Button, Text, AddIcon } from '@fluentui/react-northstar'
import { INoteDetails } from '../../../types/recruitment.types';
import "../../recruiting-details/recruiting-details.css"
import { getNotes, saveNote } from '../services/recruiting-detail.service';
import * as microsoftTeams from "@microsoft/teams-js";

export interface INotesProps {
    currentCandidateEmail: string
}

// Component for Notes details
const Notes = (props: INotesProps) => {
    const [notes, setNotes] = React.useState<any[]>([]);
    const [hostClientType, sethostClientType] = React.useState<any>('');

    // Method to start task module to add a note.
    const addNotesTaskModule = () => {
        let taskInfo = {
            title: "Notes",
            height: 300,
            width: 400,
            url: `${window.location.origin}/addNote`,
        };

        microsoftTeams.tasks.startTask(taskInfo, (err, note) => {
            if (err) {
                console.log("Some error occurred in the task module")
                return
            }

            microsoftTeams.getContext((context) => {
                // The note details to save.
                const noteDetails: INoteDetails = {
                    CandidateEmail: props.currentCandidateEmail,
                    Note: note,
                    AddedBy: context.userPrincipalName!
                };

                // API call to save the question to storage.
                saveNote(noteDetails)
                    .then((res) => {
                        loadNotes()
                    })
                    .catch((ex) => {
                        console.log("Error while saving note details" + ex)
                    });
            })
        });
    };

    // Method to load the notes in the question container.
    const loadNotes = () => {
        getNotes(props.currentCandidateEmail)
            .then((res) => {
                const notes = res.data as INoteDetails[];
                setNotes(notes)
            })
            .catch((ex) => {
                console.log("Error while getting the notes" + ex)
            });
    }

    React.useEffect((): any => {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context) => {
            sethostClientType(context.hostClientType);
        });
        loadNotes();
    }, [props.currentCandidateEmail])

    return (
        <Card fluid aria-roledescription="card with basic details" className="notes-card">
            <Card.Header>
                <Flex gap="gap.small" space="between">
                    <Text content="Notes" weight="bold" />
                    <Flex >
                        <Button
                            size="small"
                            icon={<AddIcon size="small" />}
                            content="Add a note"
                            iconPosition="before"
                            onClick={addNotesTaskModule} />
                    </Flex>
                </Flex>
                <hr className="details-separator" />
            </Card.Header>
            <Card.Body>
                <Flex className={hostClientType == "web" || hostClientType == "desktop" ? "notesContainer" :"notesContainerMobile"} column>
                    {notes.length == 0 && <Text content="No notes yet" />}
                    {
                        notes.length > 0 && notes.map((noteDetail: INoteDetails, index) => {
                            let timestamp = new Date(noteDetail.Timestamp as string);
                            let formatted_date = timestamp.getFullYear() + "-" +
                                (timestamp.getMonth() + 1) + "-" +
                                timestamp.getDate() + " " + timestamp.getHours() + ":" + timestamp.getMinutes()
                            return (
                                <Flex column key={index} padding="padding.medium">
                                    <Flex gap="gap.small">
                                        <Text content={noteDetail.AddedByName} weight="bold" title={noteDetail.AddedBy} />
                                        <Text content={formatted_date} weight="light" />
                                    </Flex>
                                    <Text content={noteDetail.Note} />
                                </Flex>
                            )
                        })
                    }
                </Flex>
            </Card.Body>
        </Card>
    )
}

export default (Notes);