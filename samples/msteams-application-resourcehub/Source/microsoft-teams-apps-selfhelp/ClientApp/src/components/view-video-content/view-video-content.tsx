import React from "react";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import * as microsoftTeams from "@microsoft/teams-js";
import withContext, { IWithContext } from '../../providers/context-provider';
import "./view-video-content.scss";
import moment from 'moment';
import {
    Flex,
    Text,
    Image,
    Button,
    Video,
    ShareGenericIcon
} from '@fluentui/react-northstar';
import { getLearningContentById } from "../../api/article-api";
import ILearningPath from "../../models/learning-path";
import { createOrUpdateLearningPathContent } from "../../api/learning-path-api";
import { CompleteState } from "../../models/complete-state";
import { ReactionState } from "../../models/reaction-state";
import IUserReaction from "../../models/user-reaction";
import { createOrUpdateUserReaction, getUserReactionByLearningId } from "../../api/user-reaction-api";
import { Icon, initializeIcons } from "@fluentui/react";
import { SelectionType } from "../../models/selection-type";
import IArticle from "../../models/article";
import { logCustomEvent } from "../../api/log-event-api";
import { FeedbackType } from "../../models/feedback-type";

interface IViewVideoContentProps extends WithTranslation, IWithContext {
}

const ViewVideoContent: React.FunctionComponent<IViewVideoContentProps> = props => {
    const localize: TFunction = props.t;
    initializeIcons();
    const queryParams = new URLSearchParams(window.location.search);
    const [title, setTitle] = React.useState("");
    const [learningId, setLearningId] = React.useState("");
    const [primaryTag, setPrimaryTag] = React.useState("");
    const [secondaryTag, setSecondaryTag] = React.useState("");
    const [itemlink, setItemLink] = React.useState("");
    const [description, setDescription] = React.useState("");
    const [knowmoreLink, setKnowMoreLink] = React.useState("");
    const [createdOn, setCreatedOn] = React.useState("");
    const [createdBy, setCreatedBy] = React.useState("");
    const [tileImageLink, setTileImageLink] = React.useState("");
    const [learningContent, setLearningContent] = React.useState<IArticle>({} as IArticle);
    const [userAadId, setUserAadId] = React.useState<string | undefined>("");
    const [likeOne, setLikeOne] = React.useState(window.location.origin + "/icons/Like1.png");
    const [disLikeOne, setDisLikeOne] = React.useState(window.location.origin + "/icons/Dislike1.png");
    const [isMobileView, setMobileView] = React.useState(window.outerWidth <= 750);

    //reaction
    const [islike, setIsLike] = React.useState(false);
    const [isDislike, setIsDisLike] = React.useState(false);

    const onLikeClick = async () => {
        setLikeOne(window.location.origin + "/icons/Like2.png");
        setIsDisLike(false)
        setDisLikeOne(window.location.origin + "/icons/DisLike1.png");
        var state = ReactionState.Like;
        var sendReaction: IUserReaction = {
            reactionId: "",
            learningContentId: learningId,
            reactionState: state,
            lastModifiedOn: new Date,
            userAadId: userAadId!,
            partitionKey: "",
            rowKey: "",
            timestamp: new Date,
            eTag: ""
        }
        var response = await createOrUpdateUserReaction(sendReaction);
        return true;
    }

    const onDisLikeClick = async () => {
        setIsDisLike(true);
        setDisLikeOne(window.location.origin + "/icons/DisLike2.png");
        setLikeOne(window.location.origin + "/icons/Like1.png");
        setIsLike(false);
        var state = ReactionState.Dislike;
        var sendReaction: IUserReaction = {
            reactionId: "",
            learningContentId: learningId,
            reactionState: state,
            lastModifiedOn: new Date,
            userAadId: userAadId!,
            partitionKey: "",
            rowKey: "",
            timestamp: new Date,
            eTag: ""
        }
        var response = await createOrUpdateUserReaction(sendReaction);
        return true;
    }

    React.useEffect(() => {
        window.addEventListener("resize", onScreenResize);
        return () => {
            window.removeEventListener("resize", onScreenResize);
        }
    }, []);

    //for mobile screen
    const onScreenResize = () => {
        setMobileView(window.outerWidth <= 750);
    }

    React.useEffect(() => {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context) => {
            setUserAadId(context.userObjectId);
            var learningId = queryParams.get("id")!;
            if (learningId !== null) {
                intializeDataAsync(learningId);
                intializeUserReactionAsync(learningId, context.userObjectId!);
                logCustomEvent({
                    eTag: "",
                    timestamp: new Date(),
                    partitionKey: "",
                    rowKey: "",
                    eventId: "",
                    learningContentId: learningId,
                    eventType: "Video",
                    createdOn: new Date(),
                    userAadId: context.userObjectId!,
                    searchkey: "",
                    tenantId: context.tid!,
                    sharedToUserIds: "",
                    sharedToChannelIds: "",
                });
            }
        });
    }, []);

    const intializeUserReactionAsync = async (learningId: string, aadId: string) => {
        let reaction = await getUserReactionByLearningId(learningId, aadId!);
        if (reaction.data == "") {
            setLikeOne(window.location.origin + "/icons/Like1.png");
            setDisLikeOne(window.location.origin + "/icons/DisLike1.png");
        }
        else {
            if (reaction.data.reactionState == ReactionState.Like) {
                setIsLike(true);
                setLikeOne(window.location.origin + "/icons/Like2.png");
                setIsDisLike(false)
                setDisLikeOne(window.location.origin + "/icons/DisLike1.png");
            }
            else {
                setIsDisLike(true);
                setDisLikeOne(window.location.origin + "/icons/DisLike2.png");
                setLikeOne(window.location.origin + "/icons/Like1.png");
                setIsLike(false);
            }
        }
    }

    const intializeDataAsync = async (learningId: string) => {
        let article = await getLearningContentById(learningId);
        if (article.data) {
            setLearningContent(article.data);
            setLearningId(article.data.learningId);
            setTitle(article.data.title);
            setItemLink(article.data.itemlink);
            setDescription(article.data.description);
            setKnowMoreLink(article.data.knowmoreLink);
            setCreatedOn(moment(article.data.createdOn).format("LLL"));
            setCreatedBy(article.data.createdByUserName);
            setPrimaryTag(article.data.primaryTag)
            setSecondaryTag(article.data.secondaryTag)
            setTileImageLink(article.data.tileImageLink);
        }
    }

    const onCloseArticleClick = async () => {
        let learningData: ILearningPath = {
            partitionKey: "",
            rowKey: "",
            learningPathId: "",
            completeState: CompleteState.Completed,
            learningContentId: learningId,
            userAadId: userAadId!,
            lastModifiedOn: new Date(),
            eTag: "",
            timestamp: new Date(),
        }
        let response = await createOrUpdateLearningPathContent(learningData);
        if (response.status === 201 && response.data) {
            microsoftTeams.tasks.submitTask({ learningId: learningId, message: "completedLearning", status: FeedbackType.LearningContentFeedback });
            return true;
        }
        else {
            microsoftTeams.tasks.submitTask();
            return true;
        }
    }

    const onAddFeedbackClick = async () => {
        microsoftTeams.tasks.submitTask({ learningId: learningId, message: "isFeedbackOpen", status: FeedbackType.LearningContentFeedback });
        return true;
    }

    const onShareButtonClick = () => {
        microsoftTeams.tasks.submitTask({ learningId: learningId, message: "isShareArticleOpen" });
        return true;
    }

    const renderDesktopView = () => {
        return (
            <Flex column gap="gap.small" className="container-main" styles={{ marginLeft: "2rem", marginRight: "2rem", background: "#ffffff !important" }} >

                <Flex className="video-content" >
                    {
                        (itemlink.split('.').pop() == "mp4" || itemlink.split('.').pop() == "MP4")
                            ?
                            <Video src={itemlink} poster={tileImageLink} variables={{ width: '100%' }} />
                            : <iframe width="536" height="245"
                                src={itemlink}
                                frameBorder="0"
                                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                                allowFullScreen>
                            </iframe>
                    }
                </Flex>
                <Flex column>
                    <Text weight="bold" content={title} className="heading margin-right" />
                </Flex>
                <Flex space="between" className="margin-right">
                    <Flex>
                        <Text className="pill" content={primaryTag} title={primaryTag} /><Text className="pill" content={secondaryTag} title={secondaryTag} />
                    </Flex>
                    <Flex gap="gap.medium" className="pill-right">
                        <Flex gap="gap.smaller" onClick={onLikeClick}>
                            <Flex className="image-icon" >
                                <Image src={likeOne} />
                            </Flex>
                            <Text size="small" weight="light" content={localize("like")} />
                        </Flex>
                        <Flex gap="gap.small" onClick={onDisLikeClick}>
                            <Flex className="image-icon" >
                                <Image src={disLikeOne} />
                            </Flex>
                            <Text size="small" weight="light" content={localize("disLike")} />
                        </Flex>
                        <Flex gap="gap.small" onClick={() => { onShareButtonClick() }}>
                            <ShareGenericIcon outline />
                            <Text size="small" weight="light" content={localize("share")} />
                        </Flex>
                        <Flex gap="gap.small" onClick={() => { onAddFeedbackClick() }}>
                            <Icon iconName="Feedback" />
                            <Text size="small" weight="light" content={localize("provideFeedbackButton")} />
                        </Flex>
                    </Flex>
                </Flex>
                <Flex className="description margin-right">
                    <Text content={description} />
                </Flex>
                <Flex styles={{ marginTop: "3.5rem" }} className="margin-right button-margin-top">
                    {
                        learningContent.sectionType == SelectionType.LearningPath ? <>
                            <Flex.Item push>
                                <Button content={"Done"} secondary onClick={onCloseArticleClick} />
                            </Flex.Item>
                            {
                                knowmoreLink !== "" && <Flex.Item>
                                    <a target="_blank" href={knowmoreLink}><Button content={localize("knowMore")} primary styles={{ marginLeft: "1rem" }} /></a>
                                </Flex.Item>
                            }
                        </>
                            : knowmoreLink !== "" &&
                            <Flex.Item push>
                                <a target="_blank" href={knowmoreLink}><Button content={localize("knowMore")} primary styles={{ marginLeft: "1rem" }} /></a>
                            </Flex.Item>
                    }
                </Flex>
            </Flex>
        )
    }

    const renderMobileView = () => {
        return (
            <Flex column gap="gap.small" className="main-container-mobile" >
                <Flex className="video-contentMobile" >
                    {
                        (itemlink.split('.').pop() == "mp4" || itemlink.split('.').pop() == "MP4")
                            ?
                            <Video src={itemlink} poster={tileImageLink} />
                            : <iframe height="245"
                                className="video-contentMobile"
                                src={itemlink}
                                frameBorder="0"
                                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                                allowFullScreen>
                            </iframe>
                    }
                </Flex>
                <Flex column>
                    <Text weight="bold" content={title} className="heading" />
                    <Flex className="subHeading" hidden={true}>
                        <Text content={localize("uploaded")} />
                        <Text content={createdOn} styles={{ marginLeft: "0.5rem" }} />
                        <Text content="| by" styles={{ marginLeft: "0.5rem" }} />
                        <Text content={createdBy} styles={{ marginLeft: "0.5rem" }} />
                    </Flex>
                </Flex>
                <Flex>
                    <Text className="pillMobile" content={primaryTag} title={primaryTag} /><Text className="pill" content={secondaryTag} title={secondaryTag} />
                </Flex>
                <Flex>
                    <Flex className="pill-rightMobile">
                        <Flex className="addButtonTextMobile">
                            <Button icon={<Image src={likeOne} />} text content={<Text content={localize("like")} className="iconText" />} onClick={() => onLikeClick()} styles={{ minWidth: "0rem !important" }} />
                            <Button icon={<Image src={disLikeOne} />} text content={<Text content={localize("disLike")} className="iconText" />} onClick={() => onDisLikeClick()} styles={{ minWidth: "0rem !important" }} />
                            <Button icon={<ShareGenericIcon outline />} text content={<Text content={localize("share")} className="iconText" />} styles={{ minWidth: "0rem !important" }} onClick={() => { onShareButtonClick() }} />
                            <Button icon={<Icon iconName="Feedback" />} text content={<Text className="iconText" content={localize("provideFeedbackButton")} />} onClick={onAddFeedbackClick} styles={{ minWidth: "0rem !important", marginRight: "0.5rem !important" }} />
                        </Flex>
                    </Flex>
                </Flex>
                <Flex className="description margin-right">
                    <Text content={description} />
                </Flex>
                <Flex styles={{ marginTop: "5rem" }}>
                    {
                        learningContent.sectionType == SelectionType.LearningPath ? <>
                            <Flex.Item push>
                                <Button content={"Done"} secondary onClick={onCloseArticleClick} />
                            </Flex.Item>
                            {
                                knowmoreLink !== "" && <Flex.Item>
                                    <a target="_blank" href={knowmoreLink}><Button content={localize("knowMore")} primary styles={{ marginLeft: "1rem" }} /></a>
                                </Flex.Item>
                            }
                        </>
                            : knowmoreLink !== "" &&
                            <Flex.Item push>
                                <a target="_blank" href={knowmoreLink}><Button content={localize("knowMore")} primary styles={{ marginLeft: "1rem" }} /></a>
                            </Flex.Item>
                    }
                </Flex>
            </Flex>
        )
    }

    return (
        <Flex className="containerVideo-Content">
            {isMobileView ? renderMobileView() : renderDesktopView()}
        </Flex>
    );
}
export default withTranslation()(withContext(ViewVideoContent));