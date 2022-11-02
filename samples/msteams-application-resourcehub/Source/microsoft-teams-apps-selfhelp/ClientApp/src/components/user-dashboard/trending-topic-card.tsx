import * as React from "react";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { Flex, Card, Image, Text, Video, MoreIcon, ShareGenericIcon, Menu, Button } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import "./user-dashboard.scss";
import { ItemType } from "../../models/item-type";
import IArticle from "../../models/article";
import { FeedbackType } from "../../models/feedback-type";

interface ITrendingCardProps extends WithTranslation {
    tiles: IArticle;
    botId: string;
}

class TrendingTopicCard extends React.Component<ITrendingCardProps> {
    localize: TFunction;
    constructor(props: any) {
        super(props);
        this.localize = this.props.t;
    }

    private onCardClick = (itemtype, learningId) => {
        if (itemtype === ItemType.Video) {
            microsoftTeams.tasks.startTask({
                title: this.localize("viewArticle"),
                height: 600,
                width: 600,
                url: `${window.location.origin}/view-video-content?id=${learningId}&status=${true}`
            }, (error: any, result: any) => {
                if (result) {
                    if (result.message === "isFeedbackOpen") {
                        microsoftTeams.tasks.startTask({
                            title: this.localize("feedbackText"),
                            height: 350,
                            width: 700,
                            url: `${window.location.origin}/user-feedback?id=${result.learningId}&status=${FeedbackType.LearningContentFeedback}`
                        }, (error: any, result: any) => {
                        });
                    }
                    else if (result.message === "isShareArticleOpen") {
                        microsoftTeams.tasks.startTask({
                            title: this.localize("shareContent"),
                            height: 600,
                            width: 750,
                            url: `${window.location.origin}/view-content-share?id=${result.learningId}`
                        }, (error: any, result: any) => {
                        });
                    }
                }
            });
        }
        else {
            var appId = this.props.botId;
            var baseUrl = `${window.location.origin}/view-image-content?id=${learningId}`
            let url = `https://teams.microsoft.com/l/stage/${appId}/0?context={"contentUrl":"${baseUrl}","websiteUrl":"${baseUrl}","name":"View article"}`;
            microsoftTeams.executeDeepLink(encodeURI(url));
        }
    }

    private onShareClick = (learningId: string) => {
        microsoftTeams.tasks.startTask({
            title: this.localize("shareContent"),
            height: 600,
            width: 600,
            url: `${window.location.origin}/view-content-share?id=${learningId}`
        }, (error: any, result: any) => {

        });
    }

    render() {
        var ext = this.props.tiles.itemlink.split('.').pop();
        const card = [
            <Flex gap="gap.medium" padding="padding.medium" className="card-grid-tile">
                <Card className="trending-card-details" inverted size="smaller" >
                    <Card.Body fitted>
                        <Flex column padding="padding.medium">
                            {this.props.tiles.itemType == ItemType.Video ?
                                ext == "mp4" || ext == "MP4" ?
                                    <Video className="card-image-details"
                                        poster={this.props.tiles.tileImageLink}
                                        src={this.props.tiles.itemlink}
                                        styles={{ width: "258px" }}
                                    />
                                    : <iframe width="258"
                                        className="card-image-details"
                                        src={this.props.tiles.itemlink}
                                        frameBorder="0"
                                        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                                        allowFullScreen>
                                    </iframe>
                                : <Image className="card-image-details" src={this.props.tiles.tileImageLink} styles={{ width: "258px", cursor: "pointer" }} onClick={() => { this.onCardClick(this.props.tiles.itemType, this.props.tiles.learningId) }} />}
                            <Text className="popular-text" content={this.props.tiles.primaryTag} />
                            <Flex>
                                <Flex styles={{ overflow: "hidden", whiteSpace: "nowrap", textOverflow: "ellipsis", width: "230px" }} className="card-Span-scenario"><Button text content={<Text content={this.props.tiles.title} title={this.props.tiles.title} styles={{ width: "170px" }} className="Content-trending-card-title" />} className="trending-card-title-scenario" onClick={() => { this.onCardClick(this.props.tiles.itemType, this.props.tiles.learningId) }}></Button></Flex>
                                <Flex className="spanthreeDot"><Text content={<Menu
                                    items={[
                                        {
                                            icon: (
                                                <MoreIcon
                                                    {...{
                                                        outline: true,
                                                    }}
                                                />
                                            ),
                                            key: 'menuButton2',
                                            'aria-label': 'More options',
                                            indicator: false,
                                            menu: {
                                                items: [
                                                    {
                                                        key: '5',
                                                        content: <Text content="Share" onClick={() => { this.onShareClick(this.props.tiles.learningId) }} />,
                                                        icon: <ShareGenericIcon onClick={() => { this.onShareClick(this.props.tiles.learningId) }} outline />,
                                                    },
                                                ],
                                            },
                                        },
                                    ]}
                                    iconOnly
                                />} />
                                </Flex>
                            </Flex>
                            {
                                this.props.tiles.itemType == ItemType.Video ? <Text className="trending-card-desc" content={this.props.tiles.length + " min"} /> : <Text className="trending-card-desc" content={this.props.tiles.length + " min read"} />
                            }
                        </Flex>
                    </Card.Body>
                </Card>
            </Flex>
        ]
        return card;
    }
}

export default withTranslation()(TrendingTopicCard);