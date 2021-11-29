import React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Flex, Menu, Button, Text } from '@fluentui/react-northstar'
import "../recruiting-details/recruiting-details.css"
import BasicDetails from "./basic-details/basic-details"
import Timeline from "./basic-details/timeline"
import Notes from "./basic-details/notes"
import QuestionsMobile from './questions/questions-mobile';
import BasicDetailsMobile from './basic-details/basic-details-mobile';
import Questions from './basic-details/questions';
import { getQuestions, saveFeedback, download } from "./services/recruiting-detail.service";
import { IFeedbackDetails, IQuestionDetails } from '../../types/recruitment.types';

const RecruitingDetails = () => {
    const mobileMenuItems = [
        {
            key: 'overview',
            content: 'Overview',
        },
        {
            key: 'questions',
            content: 'Questions',
        }
    ];

    const [activeMobileMenu, setActiveMobileMenu] = React.useState(0);
    const [questionDetails, setQuestionDetails] = React.useState<IQuestionDetails[]>([]);
    const [selectedIndex, setSelectedIndex] = React.useState(0);
    const [currentCandidateEmail, setCurrentCandidateEmail] = React.useState<string>('');
    const [feedbackSubmitted, setFeedbackSubmitted] = React.useState(false);
    const [frameContext, setframeContext] = React.useState<any>('');
    const [hostClientType, sethostClientType] = React.useState<any>('');

    const setSelectedCandidateIndex = (index: number, email: string) => {
        setSelectedIndex(index);
        setCurrentCandidateEmail(email);
    }

    // Method to set the rating for a question.
    const setRating = (event: any) => {
        const currentQuestions = [...questionDetails];
        const questToUpdate = currentQuestions.find(quest => quest.rowKey == event.currentTarget.id)!;
        questToUpdate.rating = event.target.innerText;
        setQuestionDetails(currentQuestions);
    }

    // Method to set comment on a question.
    const saveComment = (rowKey: string) => {
        const currentQuestions = [...questionDetails];
        const questToUpdate = currentQuestions.find(quest => quest.rowKey == rowKey)!;
        const doc: any = document.getElementById(rowKey + "_textarea");
        questToUpdate.comment = doc.value;
        setQuestionDetails(currentQuestions);
    }

    const setShowAddComment = (rowKey: string, isShow: boolean) => {
        const currentQuestions = [...questionDetails];
        const questToUpdate = currentQuestions.find(quest => quest.rowKey == rowKey)!;
        questToUpdate.showAddComment = isShow ? true : false;
        setQuestionDetails(currentQuestions);
    }

    // Method to load the questions in the question container.
    const loadQuestions = () => {
        microsoftTeams.getContext((context) => {
            getQuestions(context.meetingId!)
                .then((res) => {
                    console.log(res)
                    const questions = res.data as IQuestionDetails[];
                    const inMeetingQuestions = questions.map((questionDetail) => {
                        return {
                            ...questionDetail,
                            rating: 0,
                            comment: '',
                            commentedBy: ''
                        }
                    })
                    setQuestionDetails(inMeetingQuestions)
                })
                .catch((ex) => {
                    console.log("Error while getting the question details" + ex)
                });
        });
    }

    // Method to submit feedback for all questions.
    const submitFeedback = () => {
        const feedback = questionDetails.map((detail: IQuestionDetails) => {
            return {
                question: detail.question,
                rating: detail.rating,
                comment: detail.comment
            }
        })
        const feedbackJson = JSON.stringify(feedback);
        microsoftTeams.getContext((context) => {
            const feedbackDetails: IFeedbackDetails = {
                meetingId: questionDetails[0].meetingId,
                candidateEmail: currentCandidateEmail,
                feedbackJson: feedbackJson,
                interviewer: context?.userPrincipalName!
            }
            saveFeedback(feedbackDetails)
                .then((res) => {
                    setFeedbackSubmitted(true);
                })
                .catch((ex) => {
                    console.log("Error while submitting the feedback.")
                    console.log(ex)
                });
        })
    }

    const downloadFile = () => {
        // API call to download.
        download()
            .then((res) => {
                var link = document.createElement("a")
                var blob = new Blob([res.data as Blob], { type: res.headers["content-type"] });
                const blobUrl = window.URL.createObjectURL(blob)
                setTimeout(function () {
                    link.href = blobUrl; // data url
                    link.download = "test.pdf";
                    document.body.appendChild(link);
                    link.click();
                }, 0);
            })
            .catch((ex) => {
                console.log("Error while downloading the file" + ex)
            });
    }

    React.useEffect(() => {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context) => {
            setframeContext(context.frameContext);
            sethostClientType(context.hostClientType);
        });
        loadQuestions();
    }, [])


    return (
        <>
            {/* Content for stage view */}
            <Flex hidden={frameContext != "content"} gap="gap.small" padding="padding.medium" className="container">
                <Flex column gap="gap.small" padding="padding.medium" className={hostClientType == "web" || hostClientType == "desktop"? "detailsContainer" :"detailsContainerMobile"}>
                    <BasicDetails setSelectedCandidateIndex={setSelectedCandidateIndex} downloadFile={downloadFile} />
                    <Timeline />
                    <Notes currentCandidateEmail={currentCandidateEmail} />
                    <Flex hidden={hostClientType == "web" || hostClientType == "desktop"} className="questionsContainerMobile">
                        <Questions />
                    </Flex>
                </Flex>
                <Flex hidden={hostClientType != "web" && hostClientType != "desktop"} column gap="gap.small" padding="padding.medium" className="questionsContainer">
                    <Questions />
                </Flex>
            </Flex>

            {/* Content for sidepanel/mobile view */}
            <Flex hidden={frameContext != "sidePanel"} gap="gap.small" className={hostClientType == "web" || hostClientType == "desktop" ? "container-sidePanel":"container-mobile"} column>
                <Menu
                    defaultActiveIndex={0}
                    items={mobileMenuItems}
                    underlined
                    onItemClick={(event: any, options: any) => setActiveMobileMenu(options.index)}
                    className="menu-item"
                    primary />
                <Flex column gap="gap.small">
                    <>
                        {!activeMobileMenu && <BasicDetailsMobile selectedIndex={selectedIndex} downloadFile={downloadFile} />}
                        {feedbackSubmitted && activeMobileMenu == 1 && <Text>Feedback submitted!</Text>}
                        {!feedbackSubmitted && questionDetails.length > 0 && activeMobileMenu == 1 &&
                            <Flex column gap="gap.smaller"> 
                                <Flex column gap="gap.smaller" className="questionCardsMobile">
                                    <QuestionsMobile
                                        questionsSet={questionDetails}
                                        setRating={setRating}
                                        saveComment={saveComment}
                                        setShowAddComment={setShowAddComment} />
                                </Flex>
                                <Flex gap="gap.smaller">
                                    <Button
                                        size="small"
                                        content="Discard"
                                        secondary
                                        onClick={loadQuestions} />
                                    <Button size="small" content="Submit" primary onClick={submitFeedback} />
                                </Flex>
                            </Flex>
                        }
                    </>
                </Flex>
            </Flex>
        </>
    )
}

export default (RecruitingDetails);