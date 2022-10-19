import * as React from "react";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { Flex, Image, Text, Video, Button } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import IArticle from "../../models/article";
import { ItemType } from "../../models/item-type";
import "./user-dashboard.scss";
import Carousel from "./Carousel";
import { FeedbackType } from "../../models/feedback-type";

interface ICarouselCardProps extends WithTranslation {
    carouselItem: IArticle[];
    botId: string;
}

interface ICarouselState {
    carouselColumms: number;
    isMobileView: boolean;
}

class CarouselCard extends React.Component<ICarouselCardProps, ICarouselState> {
    localize: TFunction;
    constructor(props: any) {
        super(props);
        this.localize = this.props.t;
        this.state = {
            carouselColumms: 3,
            isMobileView: window.outerWidth <= 750,
        }

    }

    componentDidMount() {
        this.screenResize();
        window.addEventListener("resize", this.screenResize);
    }

    componentWillUnmount() {
        window.removeEventListener("resize", this.screenResize);
    }

    private screenResize = () => {
        let isMobileView: boolean = window.outerWidth <= 750;
        this.setState({ isMobileView: isMobileView });
        let carouselColumms = 1;
        if (window.innerWidth > 1900 && window.innerWidth < 2000) {
            carouselColumms = 4;
        }
        else if (window.innerWidth > 2000 && window.innerWidth < 3000) {
            carouselColumms = 6;
        }
        else if (window.innerWidth > 3000 && window.innerWidth < 4030) {
            carouselColumms = 8;
        }
        else if (window.innerWidth > 1000 && window.innerWidth < 1200) {
            carouselColumms = 2;
        }
        else if (window.innerWidth > 600 && window.innerWidth < 1000) {
            carouselColumms = 1;
        }
        else {
            carouselColumms = 3;
        }
        this.setState({ carouselColumms });
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

    getExt(filename) {
        var ext = filename.split('.').pop();
        if (ext == filename) return "";
        return ext;
    }

    /** Renders carousel card */
    render() {
        const carosuelItem = this.props.carouselItem.map((item: IArticle) => {
            let ismp4File = false;
            let ext = this.getExt(item.itemlink);
            if (ext == "mp4" || ext == "MP4") {
                ismp4File = true;
            }

            return (
                <Flex styles={{ marginLeft: "0.1rem" }} className="card-grid-tileCarousel">
                    <Flex column className="card-Grid-SubtitleCarousel">
                        {item.itemType == ItemType.Video ?
                            ismp4File ?
                                <Video className="card-image-details-Carousel"
                                    poster={item.tileImageLink}
                                    src={item.itemlink}
                                    styles={{ width: "160px", marginTop: "1rem" }}

                                />
                                :
                                <iframe width="160"
                                    className="card-image-details-Carousel"
                                    style={{ marginTop: "1rem" }}
                                    src={item.itemlink}
                                    frameBorder="0"
                                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                                    allowFullScreen>
                                </iframe>

                            :
                            <Image className="card-image-details-Carousel" src={item.tileImageLink} styles={{ width: "160px", marginTop: "1rem", cursor: "pointer" }} onClick={() => { this.onCardClick(item.itemType, item.learningId) }} />
                        }
                        <Flex>
                            <Flex styles={{ overflow: "hidden", whiteSpace: "nowrap", textOverflow: "ellipsis", width: "140px" }} className="card-Span">
                                <Button text
                                    content={<Text content={item.title}
                                        title={item.title} styles={{ width: "135px" }}
                                        className="Content-trending-card-title-Carousel" />} className="trending-card-title-carousel" title={item.title} onClick={() => { this.onCardClick(item.itemType, item.learningId) }}>
                                </Button>
                            </Flex>
                        </Flex>
                        {
                            item.itemType == ItemType.Video ? <Text className="trending-card-desc-Carousel" content={item.length + " min"} /> : <Text className="trending-card-desc-Carousel" content={item.length + " min read"} />
                        }
                    </Flex>
                </Flex>
            )
        });
        return (
            <div>
                <Carousel show={this.state.carouselColumms} isScenario={false}>
                    {carosuelItem}
                </Carousel>
            </div>
        );
    }
}

export default withTranslation()(CarouselCard);