import * as React from "react";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { Flex, Image, Text, Video, MoreIcon, ShareGenericIcon, Menu, Button } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import "./user-dashboard.scss";
import IArticle from "../../models/article";
import { ItemType } from "../../models/item-type";
import Carousel from "./Carousel";
import { FeedbackType } from "../../models/feedback-type";

interface ITrendingCardProps extends WithTranslation {
    scenarioItem: IArticle[];
    botId: string;
}

interface IScenarioState {
    scenarioColumms: number;
    isMobileView: boolean;
}

class ScenarioCard extends React.Component<ITrendingCardProps, IScenarioState> {
    localize: TFunction;
    setFlag: boolean;
    constructor(props: any) {
        super(props);
        this.localize = this.props.t;
        this.state = {
            scenarioColumms: 5,
            isMobileView: window.outerWidth <= 750,
        }
        this.setFlag = false;
    }
    componentDidMount() {
        this.screenResize();
        window.addEventListener("resize", this.screenResize);
    }

    componentWillUnmount() {
        window.removeEventListener("resize", this.screenResize);
    }
/**
 * Set columns based on screen size
 */
    private screenResize = () => {
        let isMobileView: boolean = window.outerWidth <= 750;
        this.setState({ isMobileView: isMobileView });
        let scenarioColumms = 1;
        if (window.innerWidth > 653) {
            scenarioColumms = Math.floor((window.innerWidth / 250));
        }
        if (window.innerWidth <= 652) {
            scenarioColumms = 1
        }

        this.setState({ scenarioColumms });
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
            var baseUrl = `${window.location.origin}/view-image-content?id=${learningId}`;
            let url = `https://teams.microsoft.com/l/stage/${appId}/0?context={"contentUrl":"${baseUrl}","websiteUrl":"${baseUrl}","name":"View article"}`;
            microsoftTeams.executeDeepLink(encodeURI(url));
        }
    }

    newFlag = () => {
        let currentDate = new Date();
        let prevDate = this.props.scenarioItem + "";
        let pd = Date.parse(prevDate);
        let preDate = new Date(pd);
        let diff = this.getDifferenceInHours(preDate, currentDate);
        if (diff < 24) {
            this.setFlag = true;
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

    getDifferenceInHours(date1, date2) {
        const diffInMs = Math.abs(date2 - date1);
        return diffInMs / (1000 * 60 * 60);
    }

    render() {
        const scenarioItem = this.props.scenarioItem.map((item: IArticle) => {
            let ismp4File = false;
            let ext = this.getExt(item.itemlink);
            if (ext == "mp4" || ext == "MP4") {
                ismp4File = true;
            }
            let carouselTag = false
            let currentDate = new Date();
            let prevDate = item.createdOn + "";
            let pd = Date.parse(prevDate);
            let preDate = new Date(pd);
            let diff = this.getDifferenceInHours(preDate, currentDate);
            if (diff < 24) {
                carouselTag = true;
            }
            return (
                <Flex gap="gap.medium" padding="padding.medium" className="card-grid-tileScenario">
                    <>{carouselTag && <Text className="new-text" content="New" />}</>
                    <Flex column className="card-Grid-SubtitleScenario">
                        {item.itemType == ItemType.Video ?
                            ext == "mp4" || ext == "MP4" ?
                                <Video className="card-image-details-scenario"
                                    poster={item.tileImageLink}
                                    src={item.itemlink}
                                />
                                : <iframe
                                    className="card-image-details-scenario"
                                    src={item.itemlink}
                                    frameBorder="0"
                                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                                    allowFullScreen>
                                </iframe>
                            : <Image styles={{ cursor: "pointer" }} className="card-image-details-scenario" src={item.tileImageLink} onClick={() => { this.onCardClick(item.itemType, item.learningId) }} />}
                        <Flex>
                            <Flex styles={{ overflow: "hidden", whiteSpace: "nowrap", textOverflow: "ellipsis", width: "230px" }} className="card-Span-scenario"><Button text content={<Text content={item.title} title={item.title} styles={{ width: "170px" }} className="Content-trending-card-title" />} className="trending-card-title-scenario" onClick={() => { this.onCardClick(item.itemType, item.learningId) }}></Button></Flex>
                            <Flex hidden={false} className="spanthreeDot"><Text content={<Menu
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
                            />} />
                            </Flex>
                        </Flex>
                        {
                            item.itemType == ItemType.Video ? <Text className="trending-card-desc" content={item.length + " min"} /> : <Text className="trending-card-desc" content={item.length + " min read"} />
                        }
                    </Flex>
                </Flex>
            )
        });
        return (
            <div>
                <Carousel show={this.state.scenarioColumms} isScenario={true}>
                    {scenarioItem}
                </Carousel>
            </div>
        );
    }
}

export default withTranslation()(ScenarioCard);