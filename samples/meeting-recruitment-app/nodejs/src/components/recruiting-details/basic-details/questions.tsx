import React from "react";
import {
    Flex,
    Card,
    Button,
    Text,
    Header,
    AddIcon,
    MoreIcon,
    Tooltip,
    CallControlStopPresentingNewIcon,
    EditIcon,
    Loader
} from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"
import * as microsoftTeams from "@microsoft/teams-js";
import { IQuestionSet } from "./basic-details.types";
import { saveQuestions, getQuestions, deleteQuestion as deleteQuestionDetails, editQuestion } from "../services/recruiting-detail.service";

// Component for the Questions section
const Questions = (): React.ReactElement => {
    // The questions array set for a meeting.
    const [questionDetails, setQuestionDetails] = React.useState<any[]>([]);
    const [ratingsArray, setRatingsArray] = React.useState<any[]>([]);
    const [showLoader, setShowLoader] = React.useState<boolean>(false);

    // Method to start task module to add questions.
    const addQuestionsTaskModule = () => {
        let urlDialogInfo = {
            title: "Questions",
            url: `${window.location.origin}/questions`,
            fallbackUrl: `${window.location.origin}/questions`,
            size: {
                height: 300,
                width: 400,
            }
        };

        microsoftTeams.dialog.url.open(urlDialogInfo, (submitHandler) => {
            if (submitHandler.err) {
                console.log("Some error occurred in the task module")
                return
            }
            
            const dataString   = JSON.stringify(submitHandler.result);
            const questionsObject1 = JSON.parse(dataString);
            const questionsObject = JSON.parse(questionsObject1);

            microsoftTeams.app.getContext().then(async (context) => {
                const questDetails: IQuestionSet[] = questionsObject.map((question: any) => {
                    if (question.checked) {
                        // The question details to save.
                        return {
                            meetingId: context.meeting?.id!,
                            question: question.value,
                            setBy: context.user?.userPrincipalName!,
                            isDelete: 0
                        };
                    }
                })

                // API call to save the question to storage.
                saveQuestions(questDetails)
                    .then((res) => {
                        loadQuestions()
                    })
                    .catch((ex) => {
                        console.log("Error while saving question details" + ex)
                    });
            })
        });
    };

    // Method to start task module to edit a question.
    const editQuestionsTaskModule = (editText: string, rowKey: any) => {
       
        let urlDialogInfo = {
            title: "Questions",
            url: `${window.location.origin}/edit?editText=` + editText,
            fallbackUrl: `${window.location.origin}/edit?editText=` + editText,
            size: {
                height: 300,
                width: 400,
            }
        };

        microsoftTeams.dialog.url.open(urlDialogInfo, (submitHandler) => {
            if (submitHandler.err) {
                console.log("Some error occurred in the task module")
                return
            }
            const dataStringEdit   = JSON.stringify(submitHandler.result);
            const questionsObjectEdits = JSON.parse(dataStringEdit);

            microsoftTeams.app.getContext().then(async (context) => {
                const questDetails: IQuestionSet = {
                    MeetingId: context.meeting?.id!,
                    Question: questionsObjectEdits,
                    SetBy: context.user?.userPrincipalName!,
                    IsDelete: 0,
                    QuestionId: rowKey
                };
                setShowLoader(true);
                // API call to save the question to storage.
                editQuestion(questDetails)
                    .then((res) => {
                        loadQuestions();
                        setShowLoader(false);
                    })
                    .catch((ex) => {
                        console.log("Error while saving question details in edit" + ex);
                        setShowLoader(false);
                    });
            })
        });
    };

    // Method to load the questions in the question container.
    const loadQuestions = () => {
        microsoftTeams.app.getContext().then(async (context) => {
            getQuestions(context.meeting?.id!)
                .then((res) => {
                    console.log(res)
                    const questions = res.data as any[];
                    setQuestionDetails(questions)
                })
                .catch((ex) => {
                    console.log("Error while getting the question details" + ex)
                });
        });
    }

    const deleteQuestion = (questionDetail: any) => {
        const questionDetails: IQuestionSet = {
            MeetingId: questionDetail.MeetingId,
            Question: questionDetail.Question,
            QuestionId: questionDetail.RowKey,
            SetBy: questionDetail.SetBy,
            IsDelete: 1
        }
        setShowLoader(true);
        // API call to save the question to storage.
        deleteQuestionDetails(questionDetails)
            .then((res) => {
                loadQuestions();
                setShowLoader(false);
            })
            .catch((ex) => {
                console.log("Error while deleting question details" + ex);
                setShowLoader(false);
            });
    }

    React.useEffect((): any => {
        const prevItems = [];
        for (var i = 1; i <= 5; i++) {
            prevItems.push(
                <Button size="small" circular content={i} />
            )
        }

        // Setting ratings to show in the UI.
        setRatingsArray(prevItems);

        microsoftTeams.app.initialize().then(() => {
            loadQuestions();
        });
    }, [])

    return (
        <>
            <Loader hidden={!showLoader} />
            <Flex column gap="gap.smaller">
                <Flex gap="gap.smaller">
                    <Header as="h4" content="Questions" className="questionsHeader" />
                    <AddIcon onClick={() => addQuestionsTaskModule()} title="Add new questions" />
                </Flex>
                <Text content="Questions added here will appear in meeting with candidate and can help you rate at the point of time" />
                <Flex column gap="gap.smaller" className="questionWrapper">
                    {
                        questionDetails.map((questionDetail, index) => {
                            return (
                                <>
                                    <Card key={index} fluid aria-roledescription="card with question details" className="questionsCard">
                                        <Card.Body>
                                            <Flex gap="gap.smaller" column>
                                                <Flex gap="gap.smaller" space="between">
                                                    <Text content={questionDetail.Question} />
                                                    <Tooltip
                                                        trigger={<MoreIcon />}
                                                        content={
                                                            <Flex column gap="gap.smaller">
                                                                <Flex >
                                                                    <Button icon={<EditIcon />} text content="Edit" className="editIcon"
                                                                        onClick={() => editQuestionsTaskModule(questionDetail.Question, questionDetail.RowKey)} />
                                                                </Flex>
                                                                <Flex>
                                                                    <Button icon={<CallControlStopPresentingNewIcon />} text content="Delete" onClick={() => deleteQuestion(questionDetail)} />
                                                                </Flex>
                                                            </Flex>
                                                        }
                                                        position="below"
                                                    />
                                                </Flex>
                                                <Flex gap="gap.small">
                                                    {ratingsArray}
                                                </Flex>
                                            </Flex>
                                        </Card.Body>
                                    </Card>
                                </>
                            )
                        })
                    }
                </Flex>
            </Flex>
        </>
    )
}

export default (Questions);