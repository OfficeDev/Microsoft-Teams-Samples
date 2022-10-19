import React from "react";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import withContext, { IWithContext } from '../../providers/context-provider';
import "./view-image-content.scss";
import * as microsoftTeams from "@microsoft/teams-js";
import { useEffect } from "react";
import {
    Flex,
    Text,
    Divider,
    Image,
    Button,
    ShareGenericIcon
} from '@fluentui/react-northstar';
import { getLearningContentById, geArticleHtmlAsync } from "../../api/article-api";
import IUserReaction from "../../models/user-reaction";
import { createOrUpdateUserReaction, getUserReactionByLearningId } from "../../api/user-reaction-api";
import { ReactionState } from "../../models/reaction-state";
import ILearningPath from "../../models/learning-path";
import { createOrUpdateLearningPathContent } from "../../api/learning-path-api";
import { StatusCodes } from "http-status-codes";
import { CompleteState } from "../../models/complete-state";
import { Icon, initializeIcons } from "@fluentui/react";
import UserFeedback from "../user-feedback/user-feedback";
import { SelectionType } from "../../models/selection-type";
import IArticle from "../../models/article";
import { logCustomEvent } from "../../api/log-event-api";
import ViewContentShare from "../view-content-share/view-content-share";

interface IViewImageContentProps extends WithTranslation, IWithContext {
}

const ViewImageContent: React.FunctionComponent<IViewImageContentProps> = props => {
    const localize: TFunction = props.t;
    initializeIcons();
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
    const [likeOne, setLikeOne] = React.useState(window.location.origin + "/icons/Like1.png");
    const [disLikeOne, setDisLikeOne] = React.useState(window.location.origin + "/icons/Dislike1.png");

    //reaction
    const [articleHtml, setArticleHtml] = React.useState("");
    const [islike, setIsLike] = React.useState(false);
    const [isDislike, setIsDisLike] = React.useState(false);
    const [userAadId, setUserAadId] = React.useState<string | undefined>("");
    const [feedbackClick, setFeedbackClick] = React.useState(false);
    const [shareClick, setShareClick] = React.useState(false);
    const [isMobileView, setMobileView] = React.useState(window.outerWidth <= 750);

    //for mobile screen
    const onScreenResize = () => {
        setMobileView(window.outerWidth <= 750);
    }

    React.useEffect(() => {
        window.addEventListener("resize", onScreenResize);
        return () => {
            window.removeEventListener("resize", onScreenResize);
        }
    }, []);

    React.useEffect(() => {
        let params = new URLSearchParams(window.location.search);
        var learningId = params.get("id")!;
        microsoftTeams.initialize();
        microsoftTeams.getContext((context) => {
            setUserAadId(context.userObjectId);
            intializeDataAsync(learningId);
            intializeUserReactionAsync(learningId, context.userObjectId!);
            logCustomEvent({
                eTag: "",
                timestamp: new Date(),
                partitionKey: "",
                rowKey: "",
                eventId: "",
                learningContentId: learningId,
                eventType: "Article",
                createdOn: new Date(),
                userAadId: context.userObjectId!,
                searchkey: "",
                tenantId: context.tid!,
                sharedToUserIds: "",
                sharedToChannelIds: "",
            });
        });
    }, []);

    const intializeUserReactionAsync = async (learningId: string, aadId: string) => {
        let reaction = await getUserReactionByLearningId(learningId, aadId);
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
            setLearningId(article.data.learningId)
            setTitle(article.data.title);
            setItemLink(article.data.itemlink);
            setDescription(article.data.description);
            setKnowMoreLink(article.data.knowmoreLink);

            setPrimaryTag(article.data.primaryTag)
            setSecondaryTag(article.data.secondaryTag)
            setTileImageLink(article.data.tileImageLink);

            let querySearch = {
                learningId: article.data.learningId,
                articleurl: article.data.itemlink
            };
            var response = await geArticleHtmlAsync(querySearch);
            setArticleHtml(response.data.html);
            setCreatedOn(response.data.publishedby);
            setCreatedBy(response.data.publishedon);
        }
    }

    const queryParams = new URLSearchParams(window.location.search);
    const onAddFeedbackClick = () => {
        setFeedbackClick(!feedbackClick);
        setShareClick(false);
    }

    const onShareButtonClick = () => {
        setShareClick(!shareClick);
        setFeedbackClick(false);
    }

    const onLikeClick = async () => {
        setIsLike(true);
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

    const onCloseButtonClick = async () => {
        let learningData: ILearningPath = {
            partitionKey: "",
            rowKey: "",
            learningPathId: "",
            completeState: CompleteState.Completed,
            learningContentId: learningId,
            userAadId: userAadId,
            lastModifiedOn: new Date(),
            eTag: "",
            timestamp: new Date(),
        }
        let response = await createOrUpdateLearningPathContent(learningData);
        if (response.status === StatusCodes.OK && response.data) {
            microsoftTeams.tasks.submitTask();
            return true;
        }
        else {
            microsoftTeams.tasks.submitTask();
            return true;
        }
    }

    const renderFeedback = () => {
        return (<UserFeedback onAddFeedbackClick={onAddFeedbackClick} isStageView={true} />);
    }

    const renderShare = () => {
        return (<ViewContentShare onShareButtonClick={onShareButtonClick} isStageView={true} />);
    }

    return (
        <Flex column gap="gap.small" className="container" styles={{ marginLeft: "2rem", marginRight: "2rem" }}>
            <Flex column gap="gap.small"   >
                <Text weight="bold" content={title} size="large" />


            </Flex>
            {

                !isMobileView ?

                    <Flex space="between" >
                        <Flex>
                            <Text className="pill" content={primaryTag} title={primaryTag} />
                            <Text className="pill" content={secondaryTag} title={secondaryTag} />
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
                            <Flex hidden={true} gap="gap.small" onClick={() => { onShareButtonClick() }}>
                                <ShareGenericIcon outline />
                                <Text size="small" weight="light" content={localize("share")} />
                            </Flex>
                            <Flex gap="gap.small" onClick={() => { onAddFeedbackClick() }}>
                                <Icon iconName="Feedback" />
                                <Text size="small" weight="light" content={localize("provideFeedbackButton")} />
                            </Flex>
                        </Flex>
                    </Flex>
                    :
                    <>
                        <Flex>
                            <Text className="pillMobile" content={primaryTag} title={primaryTag} /><Text className="pill" content={secondaryTag} title={secondaryTag} />
                        </Flex>
                        <Flex>
                            <Flex className="pill-rightMobile">
                                <Flex className="addButtonTextMobile">
                                    <Button icon={<Image src={likeOne} />} text content={<Text content={localize("like")} className="iconText" />} onClick={() => onLikeClick()} styles={{ minWidth: "0rem !important" }} />
                                    <Button icon={<Image src={disLikeOne} />} text content={<Text content={localize("disLike")} className="iconText" />} onClick={() => onDisLikeClick()} styles={{ minWidth: "0rem !important" }} />
                                    <Button icon={<Icon iconName="Feedback" />} text content={<Text className="iconText" content={localize("provideFeedbackButton")} />} onClick={() => { onAddFeedbackClick() }} styles={{ minWidth: "0rem !important", marginRight: "0.5rem !important" }} />
                                </Flex>
                            </Flex>
                        </Flex>
                    </>
            }
            <Divider />
            <Flex gap="gap.small" hAlign="end">
                {
                    feedbackClick && renderFeedback()
                }
                {
                    shareClick && renderShare()
                }
            </Flex>
            <Divider hidden={!feedbackClick && !shareClick} />
            <div>
                {
                    <span dangerouslySetInnerHTML={{ __html: articleHtml }} />
                }
            </div>
            <Flex gap="gap.small" >
                {
                    learningContent.sectionType == SelectionType.LearningPath ? <>
                        <Flex.Item push>
                            <Button content={localize("done")} secondary onClick={onCloseButtonClick} />
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
    );
}
export default withTranslation()(withContext(ViewImageContent));