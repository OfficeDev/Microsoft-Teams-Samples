import React from "react";
import { useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import withContext, { IWithContext } from '../../providers/context-provider';
import { Flex } from '@fluentui/react-northstar';
import { Text, TextArea, Button, Form, FormRadioGroup, Label, Image } from '@fluentui/react-northstar';
import "./user-feedback.scss";
import IFeedback from "../../models/feedback";
import { FeedbackType } from "../../models/feedback-type";
import { FeedbackHelpfulState } from "../../models/feedback-helpful-status";
import { addNewFeedback } from "../../api/feedback-api";
import Constants from "../../constants/constants";
import { Icon, initializeIcons } from "@fluentui/react";

interface IUserFeedbackProps extends WithTranslation, IWithContext {
}

const UserFeedback = (props): React.ReactElement => {
    const localize: TFunction = props.t;
    initializeIcons();
    //inputs
    let params = new URLSearchParams(window.location.search);
    const learningId = params.get("id")!;
    const learningStatus = params.get("status");

    const [isSuper, setIsSuper] = React.useState(false);
    const [isMedium, setIsMedium] = React.useState(false);
    const [isNotHelpful, setIsNotHelpful] = React.useState(false);
    const [isHelpful, setIsHelpful] = React.useState(true);
    const [rating, setRating] = React.useState(0);
    const [feedbackText, setFeedbackText] = React.useState("");
    const [rateOne, setRateOne] = React.useState(false);
    const [rateTwo, setRateTwo] = React.useState(false)
    const [rateThree, setRateThree] = React.useState(false)
    const [rateFour, setRateFour] = React.useState(false)
    const [rateFive, setRateFive] = React.useState(false)

    const [isTaskSubmitedSuccessfully, setIsTaskSubmitedSuccessfully] = React.useState(false);

    const [isMobileView, setMobileView] = React.useState(window.outerWidth <= Constants.maxWidthForMobileView);

    /* Callback when  isSuper medium or not helpfull selected */
    const onHelpfulSelect = (event: string) => {

        if (event == 'Super') {
            setIsSuper(true);
            setIsMedium(false);
            setIsNotHelpful(false);

        } else if (event == 'Medium') {
            setIsSuper(false);
            setIsMedium(true);
            setIsNotHelpful(false);
        } else if (event == 'NotHelpful') {
            setIsSuper(false);
            setIsMedium(false);
            setIsNotHelpful(true);
        }
    }

    /* Callback when  isHelpfull is change */
    const onIsHelpfulChange = (event) => {

        if (event == 'Yes') {

            setIsHelpful(true);
        } else {
            setIsHelpful(false);

        }
    }

    /* Callback when  rating added */
    const onRatingAdded = (event: any) => {
        setRating(event.target.value);
    }

    /* Callback when  feedback text added */
    const onFeedbackTextAdded = (event: any) => {
        setFeedbackText(event.target.value);
    }

    useEffect(() => {
        microsoftTeams.initialize();
        window.addEventListener("resize", onScreenResize);
        return () => {
            window.removeEventListener("resize", onScreenResize);
        }
    }, []);

    //for mobile screen
    const onScreenResize = () => {
        setMobileView(window.outerWidth <= Constants.maxWidthForMobileView);
    }

    /*Callback when send button is clicked */
    const onSendFeedbackClick = async () => {
        var feedbackType = FeedbackType.GeneralFeedback;
        var state = FeedbackHelpfulState.Super;

        if (isSuper == true) {
            state = FeedbackHelpfulState.Super;
        }
        else if (isMedium == true) {
            state = FeedbackHelpfulState.Medium;
        }
        else if (isNotHelpful == true) {
            state = FeedbackHelpfulState.NotHelpful;
        }

        if (learningStatus === "0") {
            feedbackType = FeedbackType.GeneralFeedback;
        }
        else if (learningStatus === "1") {
            feedbackType = FeedbackType.LearningContentFeedback;
        }
        else if (learningStatus === "2") {
            feedbackType = FeedbackType.FeedbackFromLearningPath;
        }
        else if (learningStatus === "3") {
            feedbackType = FeedbackType.FeedbackFromLearningPathCompleted;
        }

        var sendFeedback: IFeedback = {
            feedbackId: "",
            feedbackType: props.isStageView ? FeedbackType.LearningContentFeedback: feedbackType,
            learningContentId: learningId === null ? "" : learningId,
            helpfulStatus: state,
            isHelpful: isHelpful,
            rating: rating,
            feedback: feedbackText,
            createdOn: new Date(),
            createdBy: props.teamsContext?.userObjectId!,
            eTag: "",
            partitionKey: "",
            rowKey: "",
            timestamp: new Date()
        }

        var response = await addNewFeedback(sendFeedback);
        if (response.status === 201) {
            setIsTaskSubmitedSuccessfully(true)
        }
    }

    //for rating
    const onStarClick = (rating) => {
        setRating(rating);

        if (rating == 1) {
            setRateOne(true);
            setRateTwo(false);
            setRateThree(false);
            setRateFour(false);
            setRateFive(false);

        }
        else if (rating == 2) {
            setRateOne(true);
            setRateTwo(true);
            setRateThree(false);
            setRateFour(false);
            setRateFive(false);
        }
        else if (rating == 3) {
            setRateOne(true);
            setRateTwo(true);
            setRateThree(true);
            setRateFour(false);
            setRateFive(false);
        }
        else if (rating == 4) {
            setRateOne(true);
            setRateTwo(true);
            setRateThree(true);
            setRateFour(true);
            setRateFive(false);
        }
        else if (rating == 5) {
            setRateOne(true);
            setRateTwo(true);
            setRateThree(true);
            setRateFour(true);
            setRateFive(true);
        }
    }

    /* callback for cancle button */
    const onCancelFeedbackClick = async () => {
        microsoftTeams.tasks.submitTask();
        return true;
    }

    const renderMobileView = () => {
        if (isTaskSubmitedSuccessfully === false) {
            return (
                <Flex column className="feedback-containerMobile" styles={{ marginLeft: "1rem", marginRight: "2rem" }}>
                    <Flex className="feedback-headingMobile" >
                        <Text content={localize("feedbackGreeting")} />
                    </Flex>
                    <Flex className="containerMobile" styles={{ marginLeft: "0.5rem"}} >
                        <div>
                            <Flex className={isSuper === true ? "feedback-leftColor" : "feedback-left"}>
                                <Button icon={<Icon iconName="Emoji2" className="icon-Color"/>} text content={<Text content={localize("superHelpful")} className={isSuper === true ? "feedback-button-textColor" : "feedback-button-text"} />} onClick={() => onHelpfulSelect("Super")} />
                            </Flex>
                            <Flex className={isMedium === true ? "feedback-leftColor" : "feedback-left"} styles={{ marginTop: "1rem" }}>
                                <Button icon={<Icon iconName="EmojiNeutral" className="icon-Color"/>} text content={<Text content={localize("mediumHelpful")} className={isMedium === true ? "feedback-button-textColor" : "feedback-button-text"} />} onClick={() => onHelpfulSelect("Medium")} />
                            </Flex>
                            <Flex className={isNotHelpful === true ? "feedback-leftColor" : "feedback-left"} styles={{ marginTop: "1rem" }}>
                                <Button icon={<Icon iconName="EmojiDisappointed" className="icon-Color"/>} text content={<Text content={localize("notHelpful")} className={isNotHelpful === true ? "feedback-button-textColor" : "feedback-button-text"} />} onClick={() => onHelpfulSelect("NotHelpful")} />
                            </Flex>
                        </div>
                    </Flex>
                    <Flex column gap="gap.small" className="feedback-rightMobile" >
                        <Flex gap='gap.medium'>
                            <FormRadioGroup onCheckedValueChange={(e, data) => onIsHelpfulChange(data?.value)} defaultCheckedValue="Yes" label={<Text className="feedback-radio-text" content={localize("doYouFindHelpUseful")} />}
                                    items={[
                                        {
                                            key: '1',
                                            label: 'Yes',
                                            value: 'Yes',
                                        },
                                        {
                                            key: '2',
                                            label: 'No',
                                            value: 'No',
                                        },
                                    ]}
                                />
                            </Flex>
                            <Flex>
                                <Text content={localize("whatCanImproved")} className="whatCanimprove" />
                            </Flex>
                            <TextArea fluid placeholder={localize("typeSomething")} onChange={onFeedbackTextAdded} className="textArea" />
                            <Flex>
                                <Text content={localize("overallRating")} className="overallRating" />
                                {rateOne == true ? <Button iconOnly icon={<Icon iconName="FavoriteStarFill" className="start-icon-Gold"/>} style={{ paddingLeft: "0.8rem !important",minWidth:"0px !important" }} onClick={() => onStarClick(1)}  text className="star-ButtonIcon" ></Button>:<Button iconOnly icon={<Icon iconName="FavoriteStar" className="star-icon"/>} style={{ paddingLeft: "0.8rem !important",minWidth:"0px !important" }} text onClick={() => onStarClick(1)} className="star-ButtonIcon"></Button>}
                                {rateTwo == true ?<Button iconOnly icon={<Icon iconName="FavoriteStarFill" className="start-icon-Gold" />} style={{ marginLeft: "0rem !important",minWidth:"0px !important" }} onClick={() => onStarClick(2)}  text className="star-ButtonIcon"></Button>:<Button iconOnly icon={<Icon iconName="FavoriteStar"className="star-icon"/>} style={{ marginLeft: "0rem !important",minWidth:"0px !important" }} text onClick={() => onStarClick(2)} className="star-ButtonIcon"></Button>}
                                {rateThree == true ?<Button iconOnly icon={<Icon iconName="FavoriteStarFill"  className="start-icon-Gold" />} style={{ marginLeft: "0rem !important",minWidth:"0px !important" }} onClick={() => onStarClick(3)}  text className="star-ButtonIcon"></Button>:<Button iconOnly icon={<Icon iconName="FavoriteStar" className="star-icon"/>} style={{ marginLeft: "0rem !important",minWidth:"0px !important" }} text onClick={() => onStarClick(3)} className="star-ButtonIcon"></Button>}
                                {rateFour == true ?<Button iconOnly icon={<Icon iconName="FavoriteStarFill"  className="start-icon-Gold" />} style={{ marginLeft: "0rem !important",minWidth:"0px !important" }} onClick={() => onStarClick(4)}  text className="star-ButtonIcon"></Button>:<Button iconOnly icon={<Icon iconName="FavoriteStar" className="star-icon"/>} style={{ marginLeft: "0rem !important",minWidth:"0px !important" }} text onClick={() => onStarClick(4)} className="star-ButtonIcon"></Button>}
                                {rateFive == true ?<Button iconOnly icon={<Icon iconName="FavoriteStarFill" className="start-icon-Gold"/>} style={{ marginLeft: "0rem !important",minWidth:"0px !important" }} onClick={() => onStarClick(5)}  text className="star-ButtonIcon"></Button>:<Button iconOnly icon={<Icon iconName="FavoriteStar" className="star-icon"/>} style={{ marginLeft: "0rem !important",minWidth:"0px !important" }} text onClick={() => onStarClick(5)} className="star-ButtonIcon"></Button>}
                            </Flex>
                        </Flex>
                    <Flex gap="gap.medium" styles={{ marginTop: "2rem"}}>
                        <Flex.Item push>
                            <Button content={localize("cancelButton")} secondary onClick={props.isStageView ? props.onAddFeedbackClick : onCancelFeedbackClick} />
                        </Flex.Item>
                        <Button content={localize("submitButton")} primary onClick={onSendFeedbackClick} />
                    </Flex>
                </Flex>
            );
        }
        else if (isTaskSubmitedSuccessfully === true && !props.isStageView) {
            return (
                <>
                    <Flex column styles={{ marginLeft: "2rem", marginRight: "2rem",marginTop:"10rem" }} >
                        <Flex column gap="gap.medium" hAlign="center" vAlign="center" styles={{ marginLeft: "2rem", marginRight: "2rem" }}>
                            <Image src="/images/thank-you.png" className="thank-you-image" />
                            <Text weight="bold" content={localize("thankYouForFeedback")} />
                            <Text content={localize("weWork")} />
                        </Flex>
                        <Flex className="thank-you-closeMobile" >
                            <Flex.Item push>
                                <Button content={localize("closeButton")}
                                    secondary onClick={props.isStageView ? props.onAddFeedbackClick : onCancelFeedbackClick} />
                            </Flex.Item>

                        </Flex>
                    </Flex>
                </>
            );
        }
        else if (isTaskSubmitedSuccessfully === true && props.isStageView) {
            return (
                <>
                    <Flex column styles={{ marginLeft: "2rem", marginRight: "2rem"}} >
                        <Flex column gap="gap.medium" hAlign="center" vAlign="center" styles={{ marginLeft: "2rem", marginRight: "2rem" }}>
                            <Image src="/images/thank-you.png" className="thank-you-image" />
                            <Text weight="bold" content={localize("thankYouForFeedback")} />
                            <Text content={localize("weWork")} />
                        </Flex>
                        <Flex hAlign="center">
                            <Flex.Item push>
                                <Button styles={{ margin: "2rem" }} content={localize("closeButton")}
                                    secondary onClick={props.isStageView ? props.onAddFeedbackClick : onCancelFeedbackClick} />
                            </Flex.Item>

                        </Flex>
                    </Flex>
                </>
            );
        }
    }

    const renderDesktopView = () => {
        if (isTaskSubmitedSuccessfully === false) {
            return (
                <Flex column className="feedback-container" styles={{ marginLeft: "2rem", marginRight: "2rem" }}>
                    <Flex className="feedback-heading" >
                        <Text weight="bold" content={localize("feedbackGreeting")} styles={{ marginLeft: "2rem", marginRight: "2rem" }} />
                    </Flex>
                    <Flex gap="gap.small" className="container" >
                        <div>
                            <Flex className={isSuper === true ? "feedback-leftColor" : "feedback-left"}>
                                <Button icon={<Icon iconName="Emoji2" className="icon-Color"/>} text content={<Text content={localize("superHelpful")} className={isSuper === true ? "feedback-button-textColor" : "feedback-button-text"} />} onClick={() => onHelpfulSelect("Super")} />
                            </Flex>
                            <Flex className={isMedium === true ? "feedback-leftColor" : "feedback-left"} styles={{ marginTop: "1rem" }}>
                                <Button icon={<Icon iconName="EmojiNeutral" className="icon-Color"/>} text content={<Text content={localize("mediumHelpful")} className={isMedium === true ? "feedback-button-textColor" : "feedback-button-text"} />} onClick={() => onHelpfulSelect("Medium")} />
                            </Flex>
                            <Flex className={isNotHelpful === true ? "feedback-leftColor" : "feedback-left"} styles={{ marginTop: "1rem" }}>
                                <Button icon={<Icon iconName="EmojiDisappointed" className="icon-Color"/>} text content={<Text content={localize("notHelpful")} className={isNotHelpful === true ? "feedback-button-textColor" : "feedback-button-text"} />} onClick={() => onHelpfulSelect("NotHelpful")} />
                            </Flex>
                        </div>
                        <Flex column gap="gap.small" className="feedback-right" >
                            <Flex gap='gap.medium' className="radioButton_Click">
                                <FormRadioGroup onCheckedValueChange={(e, data) => onIsHelpfulChange(data?.value)} defaultCheckedValue="Yes" label={<Text className="feedback-radio-text" content={localize("doYouFindHelpUseful")} />}
                                    items={[
                                        {
                                            key: '1',
                                            label: 'Yes',
                                            value: 'Yes',
                                        },
                                        {
                                            key: '2',
                                            label: 'No',
                                            value: 'No',
                                        },
                                    ]}
                                />
                            </Flex>
                            <Flex>
                                <Text content={localize("whatCanImproved")} className="whatCanimprove" />
                            </Flex>
                            <TextArea fluid placeholder={localize("typeSomething")} onChange={onFeedbackTextAdded} className="textArea" />
                            <Flex>
                                <Text content={localize("overallRating")} className="overallRating" />
                                {rateOne == true ? <Button iconOnly icon={<Icon iconName="FavoriteStarFill" className="start-icon-Gold"/>} style={{ paddingLeft: "0.8rem !important",minWidth:"0px !important" }} onClick={() => onStarClick(1)}  text className="star-ButtonIcon" ></Button>:<Button iconOnly icon={<Icon iconName="FavoriteStar" className="star-icon"/>} style={{ paddingLeft: "0.8rem !important",minWidth:"0px !important" }} text onClick={() => onStarClick(1)} className="star-ButtonIcon"></Button>}
                                {rateTwo == true ?<Button iconOnly icon={<Icon iconName="FavoriteStarFill" className="start-icon-Gold" />} style={{ marginLeft: "0rem !important",minWidth:"0px !important" }} onClick={() => onStarClick(2)}  text className="star-ButtonIcon"></Button>:<Button iconOnly icon={<Icon iconName="FavoriteStar"className="star-icon"/>} style={{ marginLeft: "0rem !important",minWidth:"0px !important" }} text onClick={() => onStarClick(2)} className="star-ButtonIcon"></Button>}
                                {rateThree == true ?<Button iconOnly icon={<Icon iconName="FavoriteStarFill"  className="start-icon-Gold" />} style={{ marginLeft: "0rem !important",minWidth:"0px !important" }} onClick={() => onStarClick(3)}  text className="star-ButtonIcon"></Button>:<Button iconOnly icon={<Icon iconName="FavoriteStar" className="star-icon"/>} style={{ marginLeft: "0rem !important",minWidth:"0px !important" }} text onClick={() => onStarClick(3)} className="star-ButtonIcon"></Button>}
                                {rateFour == true ?<Button iconOnly icon={<Icon iconName="FavoriteStarFill"  className="start-icon-Gold" />} style={{ marginLeft: "0rem !important",minWidth:"0px !important" }} onClick={() => onStarClick(4)}  text className="star-ButtonIcon"></Button>:<Button iconOnly icon={<Icon iconName="FavoriteStar" className="star-icon"/>} style={{ marginLeft: "0rem !important",minWidth:"0px !important" }} text onClick={() => onStarClick(4)} className="star-ButtonIcon"></Button>}
                                {rateFive == true ?<Button iconOnly icon={<Icon iconName="FavoriteStarFill" className="start-icon-Gold"/>} style={{ marginLeft: "0rem !important",minWidth:"0px !important" }} onClick={() => onStarClick(5)}  text className="star-ButtonIcon"></Button>:<Button iconOnly icon={<Icon iconName="FavoriteStar" className="star-icon"/>} style={{ marginLeft: "0rem !important",minWidth:"0px !important" }} text onClick={() => onStarClick(5)} className="star-ButtonIcon"></Button>}
                            </Flex>
                        </Flex>
                    </Flex>
                    <Flex gap="gap.small" styles={{ marginLeft: "2rem", marginRight: "2rem", marginTop: "0.7rem" }} >
                        <Flex.Item push>
                            <Button content={localize("cancelButton")} secondary onClick={props.isStageView ? props.onAddFeedbackClick : onCancelFeedbackClick} />
                        </Flex.Item>
                        <Button content={localize("submitButton")} primary onClick={onSendFeedbackClick} />
                    </Flex>
                </Flex>
            );
        }
        else if (isTaskSubmitedSuccessfully === true) {
            return (
                <>
                    <Flex column className="containerFeedback-Content">
                        <Flex hAlign="center" gap="gap.large"><Image src="/images/thank-you.png" className="thank-you-image" /></Flex>
                        <Flex hAlign="center" gap="gap.large" className="feedback-success-message"><Text weight="bold" content={localize("thankYouForFeedback")} /></Flex>
                        <Flex hAlign="center" gap="gap.large" className="feedback-success-message"><Text content={localize("weWork")} styles={{ margin: "1rem" }} /></Flex>
                        <Flex hAlign="end" vAlign="end" gap="gap.large" className="feedback-success-button"><Button content={localize("closeButton")}
                            secondary onClick={props.isStageView ? props.onAddFeedbackClick : onCancelFeedbackClick} /></Flex>
                    </Flex>                 
                </>
            );
        }
    }

    return (
        <div className="containerFeedback-Content">
            {isMobileView ? renderMobileView() : renderDesktopView()}
        </div>
    );
}

export default withTranslation()(withContext(UserFeedback));