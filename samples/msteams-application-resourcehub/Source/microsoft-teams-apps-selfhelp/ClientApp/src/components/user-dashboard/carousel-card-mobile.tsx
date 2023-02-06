import * as React from "react";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { Carousel, Flex, Card, Image, Text, Video, MoreIcon, Menu, ShareGenericIcon, Button } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import IArticle from "../../models/article";
import { ItemType } from "../../models/item-type";
import "./user-dashboard.scss";
import { FeedbackType } from "../../models/feedback-type";

interface ICarouselCardProps extends WithTranslation {
    carouselItem: IArticle[];
    botId: string;
    isShareHide: true;
}

class CarouselCardMobile extends React.Component<ICarouselCardProps> {
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
            width: 750,
            url: `${window.location.origin}/view-content-share?id=${learningId}`
        }, (error: any, result: any) => {

        });
    }

    getExt(filename) {
        var ext = filename.split('.').pop();
        if (ext == filename) return "";
        return ext;
    }

    /** Renders carousel card */
    render() {
        const carosuelItem = this.props.carouselItem.map((item: IArticle, index: number) => {
            let ismp4File = false;
            let ext = this.getExt(item.itemlink);
            if (ext == "mp4" || ext == "MP4") {
                ismp4File = true;
            }

            return {
                id: index,
                content:
                    <Flex padding="padding.medium">
                        <Card className="carousel-card-details" size="smaller">
                            <Card.Body fitted>
                                <Flex column padding="padding.medium" >
                                    {item.itemType == ItemType.Video ?
                                        ismp4File ?
                                            <Video className="card-image-details-CarouselMobile"
                                                poster={item.tileImageLink}
                                                src={item.itemlink}
                                                styles={{ width: "230px", marginLeft: "2.5rem", marginTop: "1rem" }}

                                            />
                                            :
                                            <iframe width="230"
                                                className="card-image-details-CarouselMobile"
                                                style={{ marginLeft: "2.5rem", marginTop: "1rem" }}
                                                src={item.itemlink}
                                                frameBorder="0"
                                                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                                                allowFullScreen>
                                            </iframe>

                                        :
                                        <Image className="card-image-details-CarouselMobile" src={item.tileImageLink}
                                            styles={{ width: "230px", marginLeft: "2.5rem", marginTop: "1rem" }} />
                                    }
                                    <Flex >
                                        <span style={{ width: "210px", overflow: "hidden", whiteSpace: "nowrap", textOverflow: "ellipsis" }}>
                                            <Button text content={<Text content={item.title} title={item.title}
                                                styles={{ width: "170px", marginLeft: "1.6rem !important" }}
                                                className="Content-trending-card-title" />} className="trending-card-title"
                                                title={item.title} onClick={() => { this.onCardClick(item.itemType, item.learningId) }}>
                                            </Button>
                                        </span>
                                        <span hidden={this.props.isShareHide} className="spanthreeDot"><Text content={<Menu
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
                                                                content: <Text content="Share" onClick={() => { this.onShareClick(item.learningId) }} />,
                                                                icon: <ShareGenericIcon onClick={() => { this.onShareClick(item.learningId) }} outline />,
                                                            },
                                                        ],
                                                    },
                                                },
                                            ]}
                                            iconOnly
                                        />
                                        }
                                        />
                                        </span>
                                    </Flex>
                                    {
                                        item.itemType == ItemType.Video ? <Text className="article-length-margin-left" content={item.length + this.localize("min")} />
                                            : <Text className="article-length-margin-left" content={item.length + this.localize("minRead") } />
                                    }
                                </Flex>
                            </Card.Body>
                        </Card>
                    </Flex>
            };
        });
        return (
            <Flex className="carousel-item" hAlign="center">
                <Carousel items={carosuelItem} ></Carousel>
            </Flex>
        );
    }
}

export default withTranslation()(CarouselCardMobile);