import * as React from "react";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { Breadcrumb, Divider, Flex, Text, Segment, SearchIcon, MoreIcon, Input, Box, Image, Grid, ParticipantAddIcon, Checkbox, Menu, ChevronDownMediumIcon, Slider, Button, Loader } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import withContext, { IWithContext } from '../../providers/context-provider';
import CarouselCard from "./carousel-card";
import TrendingTopicCard from "./trending-topic-card";
import ScenarioCard from "./scenario-card";
import { ChevronEndMediumIcon } from '@fluentui/react-icons-northstar';
import "./user-dashboard.scss";
import SearchResultes from "../search-result/search-result";
import IArticle from "../../models/article";
import { getLearningContentByType } from "../../api/article-api";
import { getCurrentUserLearningPath } from "../../api/learning-path-api";
import { getApplicationSettingsMetadata } from '../../api/authentication-metadata';
import { SelectionType } from "../../models/selection-type";
import { initializeIcons } from "@uifabric/icons";
import { Icon } from '@fluentui/react';
import AdminDashboard from "../../components/admin-dashboard/admin-dashboard";
import { getUserByIdDataAsync, getUserRoleAsync } from "../../api/user-api";
import { ItemType } from "../../models/item-type";
import ILearningPath from "../../models/learning-path";
import CarouselCardMobile from "./carousel-card-mobile";
import { FeedbackType } from "../../models/feedback-type";
import ISubscribe from "../../models/subscribe";
import { createOrUpdateSubscribeAsync, getSubscribeByUserIdAsync } from "../../api/subscribe-api";

interface IUserDashboardProps extends WithTranslation, IWithContext {
}

interface IDashboardState {
    gridColumm: number;
    isSearchView: boolean;
    searchText: string;
    scenariosCards: Array<IArticle>;
    trendingTopicsCards: Array<IArticle>;
    gettingStartedCards: Array<IArticle>;
    learningPathCards: Array<IArticle>;
    isAdminView: boolean;
    userDetails: any;
    currentUserLearningCount: number;
    currentUserLearningData: Array<ILearningPath>;
    isMobileView: boolean;
    learningPathIcon: any;
    showLearningPath: boolean;
    isAdminUser: boolean;
    calculatePercentage: number
    isIntroTeamsLoading: boolean;
    isQuickStartLoading: boolean;
    isGetTeamsLoading: boolean;
    isFrequentlyLoading: boolean;
    botId: string;
    isCompleted: boolean;
    tenantId: string;
    userAadId: string;
    upn: string;
    subscribeData:ISubscribe;
    subscribeStatus:boolean;
};

class UserDashboard extends React.Component<IUserDashboardProps, IDashboardState> {
    localize: TFunction;
    constructor(props: any) {
        super(props);
        this.localize = this.props.t;
        initializeIcons();
        this.onViewUserPageClick = this.onViewUserPageClick.bind(this);
        this.state = {
            isAdminView: false,
            isSearchView: false,
            showLearningPath: false,
            searchText: "",
            gridColumm: 4,
            scenariosCards: [],
            trendingTopicsCards: [],
            gettingStartedCards: [],
            learningPathCards: [],
            userDetails: {},
            currentUserLearningCount: 0,
            learningPathIcon: <ChevronDownMediumIcon className="chevron-icon-path" rotate={180} />,
            isMobileView: window.outerWidth <= 750,
            isAdminUser: false,
            calculatePercentage: 0,
            currentUserLearningData: [],
            isIntroTeamsLoading: false,
            isQuickStartLoading: false,
            isGetTeamsLoading: false,
            isFrequentlyLoading: false,
            botId: "",
            isCompleted: false,
            tenantId: "",
            userAadId:"",
            upn:"",
            subscribeData:{userId:"",tenantId:"",createdBy:"",createdOn:new Date(),email:"",eTag:"",partitionKey:"",RowKey:"",status:false,timestamp:new Date()},
            subscribeStatus:false
        }
    }

    componentDidMount() {
        this.getSubscribeData();
        microsoftTeams.initialize();
        microsoftTeams.getContext((context) => {
            this.setState({ tenantId: context.tid!, userAadId: context.userObjectId!,upn:context.userPrincipalName!});
            this.getUserRole(context.upn!);
            this.initializeGettingStartedCardData();
            this.initializeScenarioCardData();
            this.initializeTrendingTopicsCardData();
            this.initializeLearningPathCardData("");
            this.getAppsettings();
        });

        this.screenResize();
        window.addEventListener("resize", this.screenResize);
    }

    componentWillUnmount() {
        window.removeEventListener("resize", this.screenResize);
    }

    private getAppsettings = async () => {
        var response = await getApplicationSettingsMetadata();
        if (response.data) {
            this.setState({ botId: response.data });
        }
    }

    private getUserRole = async (upn) => {
        var response = await getUserRoleAsync(upn);
        if (response.data) {
            this.setState({ isAdminUser: response.data });
        }
    }
    
    private getSubscribeData = async () => {
        var response = await getSubscribeByUserIdAsync();
        if (response.data) {
            this.setState({subscribeStatus:response.data[0].status})
            this.setState({subscribeData: response.data});
        }
    }

    private initializeScenarioCardData = async () => {
        this.setState({ isGetTeamsLoading: true });
        var response = await getLearningContentByType(SelectionType.Scenarios.toString());
        if (response.data && response.data.length > 0) {
            this.setState({ scenariosCards: response.data });
            this.setState({ isGetTeamsLoading: false });
        }
        else {
            this.setState({ isGetTeamsLoading: false });
        }
    }

    private initializeTrendingTopicsCardData = async () => {
        this.setState({ isFrequentlyLoading: true });
        var response = await getLearningContentByType(SelectionType.TrendingNow.toString());
        if (response.data && response.data.length > 0) {
            this.setState({ trendingTopicsCards: response.data });
            this.setState({ isFrequentlyLoading: false });
        }
        else {
            this.setState({ isFrequentlyLoading: false });
        }
    }

    private initializeLearningPathCardData = async (doneText: string) => {
        this.setState({ isQuickStartLoading: true });
        this.setState({ learningPathCards: [], currentUserLearningData: [], currentUserLearningCount: 0, calculatePercentage: 0 })
        var response = await getLearningContentByType(SelectionType.LearningPath.toString());
        if (response.data && response.data.length > 0) {
            this.setState({ learningPathCards: response.data });
            var responseCurrentUserLearningPath = await getCurrentUserLearningPath(this.state.userAadId);
            var resUserLearningPath = responseCurrentUserLearningPath.data.filter(item => {
                return response.data.find(card => card.learningId === item.learningContentId);
            });
            if (resUserLearningPath && resUserLearningPath.length > 0) {
                this.setState({ currentUserLearningData: resUserLearningPath });
                this.setState({ currentUserLearningCount: resUserLearningPath.length });
                let calculateSliderValue = (resUserLearningPath.length / response.data.length) * 100;
                let numberSlider = parseInt(calculateSliderValue.toString());
                if (doneText === "Done") {
                    if (calculateSliderValue.toString() === "100") {
                        this.setState({ isCompleted: true })
                        this.onAddFeedbackClick(FeedbackType.FeedbackFromLearningPathCompleted);
                    }
                }
                if (calculateSliderValue.toString() === "100") {
                    this.setState({ isCompleted: true })
                }
                this.setState({ calculatePercentage: numberSlider });
                this.setState({ isQuickStartLoading: false });
            }
            else {
                this.setState({ isQuickStartLoading: false });
            }
        }
        else {
            this.setState({ isQuickStartLoading: false });
        }
    }

    private initializeGettingStartedCardData = async () => {
        this.setState({ isIntroTeamsLoading: true });
        var response = await getLearningContentByType(SelectionType.GettingStarted.toString());
        if (response.data && response.data.length > 0) {
            this.setState({ gettingStartedCards: response.data });
            this.setState({ isIntroTeamsLoading: false });

        } else {
            this.setState({ isIntroTeamsLoading: false });
        }
    }

    /**
     * Set columns based on screen size
     */
    private screenResize = () => {
        let isMobileView: boolean = window.outerWidth <= 750;
        this.setState({ isMobileView: isMobileView });
        let gridColumm = 1;
        if (window.innerWidth > 180) {
            gridColumm = Math.floor((window.innerWidth / 250));
        }
        this.setState({ gridColumm });
    }

    private onAddFeedbackClick = (status: FeedbackType) => {
        this.props.microsoftTeams.tasks.startTask({
            title: this.localize("feedbackText"),
            height: 315,
            width: 700,
            url: `${window.location.origin}/user-feedback?status=${status}`
        }, (error: any, result: any) => {
            if (result) {

            }
        });
    }

    private onAddSubscribeClick = async () => {
        let status:boolean;
        if(this.state.subscribeStatus === true)
        {
            status = false;
        }
        else
        {
            status = true;
        }
        let subscribeDetails : ISubscribe ={
            RowKey:"",
            partitionKey:"",
            email:this.state.upn,
            userId:this.state.userAadId,
            tenantId:this.state.tenantId,
            createdBy:"",
            createdOn: new Date(),
            eTag: "",
            timestamp: new Date(),
            status:status
        }
        var resultSubscribe = await createOrUpdateSubscribeAsync(subscribeDetails)
        if(resultSubscribe.data)
        {
            this.getSubscribeData();
        }
    }

    private onSearchClick = async () => {
        if (this.state.searchText != "") {
            this.setState({ isSearchView: true });
        }
    }

    private onSearchTextChange = (query: string) => {
        this.setState({
            searchText: query,
        });
    }

    private onEnterKeyPress = (event) => {
        if (event.key === 'Enter') {
            if (this.state.searchText.trim() !== "" || this.state.searchText.trim() !== '') {
                this.setState({ isSearchView: true });
            }
        }
    }

    private onHomeBreadcrumClick = () => {
        this.setState({ isSearchView: false, searchText: '' });
    }

    private onCompletedButtonClick = async (learningId, itemType) => {
        if (itemType === ItemType.Video) {
            microsoftTeams.tasks.startTask({
                title: this.localize("viewArticle"),
                height: 600,
                width: 600,
                url: `${window.location.origin}/view-video-content?id=${learningId}&status=${true}`
            }, (error: any, result: any) => {
                if (result) {
                    if (result.message === "completedLearning") {
                        this.initializeLearningPathCardData("Done");
                    }
                    else if (result.message === "isFeedbackOpen") {
                        this.props.microsoftTeams.tasks.startTask({
                            title: this.localize("feedbackText"),
                            height: 315,
                            width: 700,
                            url: `${window.location.origin}/user-feedback?status=${result.status}`
                        });
                    }
                }
                else {
                    this.initializeLearningPathCardData("");
                }
            });
        }
        else {
            var appId = this.state.botId;
            var baseUrl = `${window.location.origin}/view-image-content?id=${learningId}`
            let url = `https://teams.microsoft.com/l/stage/${appId}/0?context={"contentUrl":"${baseUrl}","websiteUrl":"${baseUrl}","name":"View article"}`;
            microsoftTeams.executeDeepLink(encodeURI(url));
        }
    }

    private renderMainView() {
        const tiles = this.state.trendingTopicsCards.map((value: IArticle, index) => {
            return <TrendingTopicCard tiles={value} key={index} botId={this.state.botId} />
        });

        let checkBox;
        if (this.state.learningPathCards.length > 0) {
            checkBox = this.state.learningPathCards.map((item: IArticle, index: number) => {
                if (index < 5) {
                    const filterCheckBox = this.state.currentUserLearningData.find(s => s.learningContentId == item.learningId);
                    if (filterCheckBox === undefined || filterCheckBox === null) {
                        return (
                            <Box styles={{ width: "10px", height: "20px" }}>
                                <Checkbox label={<Button content={item.title} text onClick={() => { this.onCompletedButtonClick(item.learningId, item.itemType) }} />} checked={false} />
                            </Box>
                        );
                    }
                    else {
                        return (
                            <Box styles={{ width: "10px", height: "20px" }}>
                                <Checkbox label={<Button content={item.title} text onClick={() => { this.onCompletedButtonClick(item.learningId, item.itemType) }} />} checked={true} />
                            </Box>
                        );
                    }
                }
            });
        }

        return (
            <>
                <div className="sub-page-container">
                    <Flex className="main-page" column>
                        <Box>
                            <Flex className="getting-started-bar-container" gap="gap.small" padding="padding.medium">
                                <Flex.Item className="getting-started-item-container" size="size.half">
                                    <Segment className="getting-started-item"
                                        content={<Flex column gap="gap.small" className="getting-started-text" vAlign="center">
                                            <Flex className="getting-started-headerText">
                                                <Text content={"Here to help you"} />
                                            </Flex>
                                            <Flex>
                                                <Input value={this.state.searchText} onChange={(event: any) => this.onSearchTextChange(event.target.value)}
                                                    onKeyPress={(event: any) => this.onEnterKeyPress(event)}
                                                    fluid icon={<SearchIcon onClick={this.onSearchClick} />} placeholder="What are you looking for?" className="ui-Input-Text" />
                                            </Flex>
                                        </Flex>} />
                                </Flex.Item>
                                <Flex.Item className="getting-started-item-container" size="size.half">
                                    <Segment className="getting-started-carousel" content={<Flex column>
                                        <Text content={this.localize("gettingStartedText")} styles={{ marginLeft: "1rem", marginTop: "1rem" }} className="intro-To-Teams" />
                                        {this.state.isIntroTeamsLoading ? <Loader /> :

                                            <Flex styles={{ marginBottom: "0.5rem" }}>
                                                {this.state.gettingStartedCards.length === 0 ? <Text content={this.localize("noRecordFound")} /> :
                                                    <CarouselCard carouselItem={this.state.gettingStartedCards} botId={this.state.botId} />
                                                }
                                            </Flex>
                                        }
                                    </Flex>} />
                                </Flex.Item>
                                <Flex.Item className="quickstart-item-container" size="size.small">
                                    <Segment className="getting-started-QuickstartChecklist" content={<Flex column>
                                        <Flex>
                                            <Text content={this.localize("quickStartChecklist")} styles={{ marginLeft: "1rem", whiteSpace: "nowrap" }} className="intro-To-Teams" />
                                            <Text content={<Menu
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
                                                                    content: <Text content="Feedback" onClick={() => this.onAddFeedbackClick(FeedbackType.FeedbackFromLearningPath)} />,
                                                                    icon: <Icon iconName="Feedback" onClick={() => this.onAddFeedbackClick(FeedbackType.FeedbackFromLearningPath)} />,
                                                                },
                                                            ],
                                                        },
                                                    },
                                                ]}
                                                iconOnly
                                            />} styles={{ marginLeft: "5rem" }} />
                                        </Flex>
                                        {this.state.isQuickStartLoading ? <Loader /> :
                                            <>
                                                {this.state.isCompleted === true ?
                                                    <Box>
                                                        <Flex className="avatarImage">
                                                            <Image src={window.location.origin + "/images/MicrosoftTeams-image.png"} />

                                                        </Flex>
                                                        <Flex styles={{ marginLeft: "6.8rem", marginTop: "0.5rem" }}>
                                                            <Text content="Congrats!" className="completed" /></Flex>
                                                        <Flex styles={{ marginTop: "0.5rem" }}>
                                                            <Slider value={this.state.calculatePercentage} className={this.state.calculatePercentage == 0 ? "sliderDeskTopClass" : "sliderDeskTopClassColor"} />
                                                        </Flex>
                                                        <Flex>
                                                            <Text content={`${this.state.calculatePercentage}% Completed`} className="textCompleted" /></Flex>
                                                        {this.state.gettingStartedCards.length === 0 ? <Text content={this.localize("noRecordFound")} /> : <div className="checkBoxClass">
                                                            {checkBox}
                                                        </div>}
                                                    </Box>
                                                    :
                                                    <>
                                                        <><><Flex>
                                                            <Slider value={this.state.calculatePercentage} className={this.state.calculatePercentage == 0 ? "sliderDeskTopClass" : "sliderDeskTopClassColor"} />
                                                        </Flex><Text content={`${this.state.calculatePercentage}% Completed`} className="trending-card-desc" styles={{ marginLeft: "1rem" }} /></>{this.state.gettingStartedCards.length === 0 ? <Text content={this.localize("noRecordFound")} /> : <div className="checkBoxClass">
                                                            {checkBox}
                                                        </div>}</>
                                                    </>
                                                }
                                            </>
                                        }
                                    </Flex>

                                    } />
                                </Flex.Item>
                            </Flex>
                        </Box>
                    </Flex><Flex column className="scenario-container" gap="gap.small" styles={{ marginLeft: "1.5rem" }}>
                        <Text content={this.localize("scenarioText")} aria-label={this.localize("scenarioText")} className="ui-Card_Text" />

                        {this.state.isGetTeamsLoading ? <Loader /> :
                            <Flex>
                                {this.state.scenariosCards.length === 0 ? <Text content={this.localize("noRecordFound")} styles={{ marginLeft: "50rem" }} /> :
                                    <ScenarioCard scenarioItem={this.state.scenariosCards} botId={this.state.botId} cardCount={this.state.gridColumm} />
                                }
                            </Flex>
                        }
                    </Flex><Flex column className="trending-topic-container" gap="gap.medium" styles={{ marginLeft: "1.5rem" }}>
                        <Text content={this.localize("trendingTopicText")} aria-label={this.localize("trendingTopicText")} className="ui-Card_Text" styles={{ marginTop: "1rem" }} />
                        {this.state.isFrequentlyLoading ? <Loader /> :
                            <Flex>
                                {this.state.trendingTopicsCards.length === 0 ? <Text content={this.localize("noRecordFound")} styles={{ marginLeft: "50rem" }} /> :
                                    <Grid content={tiles} columns={this.state.gridColumm - 1} className="grid-Class-trending" />

                                }
                            </Flex>
                        }
                    </Flex>
                </div>
            </>
        );
    }

    private renderSearchView() {
        return (
            <SearchResultes searchText={this.state.searchText} tid={this.state.tenantId} userObjectId={this.state.userAadId} botId={this.state.botId} onHomeBreadcrumClick={this.onHomeBreadcrumClick} />
        );
    }

    private onViewAdminPageClick = () => {
        this.setState({
            isAdminView: true
        });
    }

    public onViewUserPageClick = () => {
        this.setState({
            isAdminView: false
        });
    }

    private renderDesktopView = () => {
        return (
            !this.state.isAdminView ?
                <Flex column className="maine-page-container">
                    <Flex className="home-nav-bar-container">
                        <Flex vAlign='center'>
                            <Breadcrumb>
                                <Breadcrumb.Item className="cursor-pointer">
                                    <Icon iconName="Home" onClick={this.onHomeBreadcrumClick} />
                                </Breadcrumb.Item>
                            </Breadcrumb>
                            <Divider vertical className="vertical-divider" />
                        </Flex>
                        <Flex vAlign="center" hidden={!this.state.isSearchView} className="search-view-breadcrum-text">
                            <Breadcrumb>
                                <Breadcrumb.Divider >
                                    <ChevronEndMediumIcon styles={{ paddingBottom: "5px" }} />
                                </Breadcrumb.Divider>
                            </Breadcrumb>
                            <Divider vertical className="vertical-divider" />
                            <Text content={this.localize("searchViewText")} styles={{ paddingBottom: "2px" }} />
                        </Flex>
                        <Flex.Item push >
                            <Flex className="nav-bar-feedback" vAlign='center' gap="gap.small">
                                <Flex vAlign="center" gap="gap.smaller" className="items-container cursor-pointer" onClick={() => this.onAddFeedbackClick(FeedbackType.GeneralFeedback)} >
                                    <Icon iconName="Feedback" />
                                    <Text content={this.localize("Feedback")} weight="semibold" aria-label={this.localize("feedbackText")} className="feedbackText" />
                                </Flex>
                                {this.state.subscribeStatus === true ?
                                    <Flex vAlign="center" gap="gap.smaller" className="items-container cursor-pointer" onClick={() => this.onAddSubscribeClick()} >
                                    <Icon iconName="Unsubscribe" onClick={() => this.onAddSubscribeClick()}/>
                                        <Text content={this.localize("UnsubscribeText")} title={this.localize("SubscribeTextTooltip")} weight="semibold" aria-label={this.localize("UnsubscribeText")} className="feedbackText" />
                                </Flex>:
                                <Flex vAlign="center" gap="gap.smaller" className="items-container cursor-pointer" onClick={() => this.onAddSubscribeClick()} >
                                <Icon iconName="Subscribe" onClick={() => this.onAddSubscribeClick()}/>
                                        <Text content={this.localize("SubscribeText")} title={this.localize("SubscribeTextTooltip")} weight="semibold" aria-label={this.localize("SubscribeText")} className="feedbackText" />
                            </Flex>
                            }
                                <Flex hidden={!this.state.isAdminUser} vAlign="center" gap="gap.smaller" className="items-container cursor-pointer" onClick={this.onViewAdminPageClick}>
                                    <ParticipantAddIcon outline size="small" />
                                    <Text content={this.localize("switchUserText")} weight="semibold" aria-label={this.localize("switchUserText")} className="switchUser" />
                                </Flex>

                            </Flex>
                        </Flex.Item>
                    </Flex>
                    {
                        !this.state.isSearchView ? this.renderMainView() : this.renderSearchView()
                    }
                </Flex>
                : <>
                    <AdminDashboard onViewUserPageClick={this.onViewUserPageClick} />
                </>
        );
    }

    private handleLearningPathClick = () => {
        let showLearningPath = !this.state.showLearningPath;
        let icon = this.state.showLearningPath ? <ChevronDownMediumIcon className="chevron-icon-path" /> : <ChevronDownMediumIcon className="chevron-icon-path" rotate={180} />
        this.setState({ showLearningPath: showLearningPath, learningPathIcon: icon });
    }

    private renderMobileView = () => {
        return (
            !this.state.isAdminView ?
                <Flex column className="maine-page-container">
                    <Flex className="home-nav-bar-mobile-container">
                        <Flex vAlign='center'>
                            <Breadcrumb>
                                <Breadcrumb.Item className="cursor-pointer">
                                    <Icon iconName="Home" onClick={this.onHomeBreadcrumClick} />
                                </Breadcrumb.Item>
                            </Breadcrumb>
                            <Divider vertical className="vertical-divider" />
                        </Flex>
                        <Flex hidden={!this.state.isSearchView} className="search-view-breadcrum-text">
                            <Divider vertical className="vertical-divider" />
                            <Text content={this.localize("searchViewText")} />
                        </Flex>
                        <Flex.Item push >
                            <Flex className="nav-bar-feedback" vAlign='center' gap="gap.small">
                                <Flex vAlign="center" gap="gap.smaller" className="items-container cursor-pointer" onClick={() => this.onAddFeedbackClick(FeedbackType.GeneralFeedback)} >
                                    <Icon iconName="Feedback" onClick={() => this.onAddFeedbackClick(FeedbackType.GeneralFeedback)} />
                                </Flex>
                                {this.state.subscribeStatus === true ?
                                <Flex vAlign="center" gap="gap.smaller" className="items-container cursor-pointer" onClick={() => this.onAddSubscribeClick()} >
                                    <Icon iconName="Unsubscribe" onClick={() => this.onAddSubscribeClick()}/>
                                        <Text content={this.localize("UnsubscribeText")} weight="semibold" aria-label={this.localize("UnsubscribeText")} className="feedbackText" />
                                </Flex>:
                                <Flex vAlign="center" gap="gap.smaller" className="items-container cursor-pointer" onClick={() => this.onAddSubscribeClick()} >
                                <Icon iconName="Subscribe" onClick={() => this.onAddSubscribeClick()}/>
                                        <Text content={this.localize("SubscribeText")} weight="semibold" aria-label={this.localize("SubscribeText")} className="feedbackText" />
                            </Flex>
                            }
                                <Flex hidden={!this.state.isAdminUser} vAlign="center" gap="gap.smaller" className="items-container cursor-pointer" onClick={this.onViewAdminPageClick}>
                                    <ParticipantAddIcon outline size="small" onClick={this.onViewAdminPageClick} />
                                </Flex>

                            </Flex>
                        </Flex.Item>
                    </Flex>
                    {
                        !this.state.isSearchView ? this.renderMainMobileView() : this.renderSearchView()
                    }
                </Flex>
                : <>
                    <AdminDashboard onViewUserPageClick={this.onViewUserPageClick} />
                </>
        );
    }

    private renderMainMobileView = () => {
        const tiles = this.state.trendingTopicsCards.map((value: IArticle, index) => {
            return <TrendingTopicCard tiles={value} key={index} botId={this.state.botId} />
        });

        let checkBox;
        if (this.state.learningPathCards.length > 0) {
            checkBox = this.state.learningPathCards.map((item: IArticle, index: number) => {
                const filterCheckBox = this.state.currentUserLearningData.find(s => s.learningContentId == item.learningId);
                if (filterCheckBox === undefined || filterCheckBox === null) {
                    return (
                        <Box styles={{ width: "10px", height: "20px" }}>
                            <Checkbox label={<Button content={item.title} text onClick={() => { this.onCompletedButtonClick(item.learningId, item.itemType) }} />} checked={false} className="chkButtonClass" />
                        </Box>
                    );
                }
                else {
                    return (
                        <Box styles={{ width: "10px", height: "20px" }}>
                            <Checkbox label={<Button content={item.title} text onClick={() => { this.onCompletedButtonClick(item.learningId, item.itemType) }} />} checked={true} className="chkButtonClass" />
                        </Box>
                    );
                }
            });
        }

        return (
            <Flex className="maine-page-containerMobile">
                <div className="main-page main-div-mobile">
                    <div className="getting-started-bar-container-mobile">

                        {/* getting started*/}
                        <div className="getting-started-item-container">
                            <Segment className="getting-started-item"
                                content={
                                    <Flex column gap="gap.small" className="getting-started-text" vAlign="center">
                                        <Flex className="getting-started-headerText">
                                            <Text content={this.localize("hereToHelpYou")} />
                                        </Flex>
                                        <Flex styles={{ marginBottom: "1.8rem" }}>
                                            <Input value={this.state.searchText} onChange={(event: any) => this.onSearchTextChange(event.target.value)}
                                                onKeyPress={(event: any) => this.onEnterKeyPress(event)}
                                                fluid icon={<SearchIcon onClick={this.onSearchClick} />} placeholder={this.localize("whatAreYouLookingFor")} className="ui-Input-Text" />

                                        </Flex>
                                    </Flex>
                                }
                            />
                        </div>

                        {/*learning path*/}
                        <div className="learning-path-container pading-top-div-mobile">
                            <Segment className="learning-path-label" content={
                                <div onClick={this.handleLearningPathClick}>
                                    <Flex styles={{ width: "100%" }}>
                                        <Text className="learningPath-label" content={this.localize("quickStartChecklist")} weight="semibold"
                                            styles={{ whiteSpace: "nowrap", width: "100%" }} />
                                        {this.state.learningPathIcon}</Flex><br />
                                    {!this.state.showLearningPath &&
                                        <><Flex styles={{ width: "100%", marginLeft: "1rem", marginTop: "0.6rem" }} className="sliderClass">
                                            <Slider value={this.state.calculatePercentage} styles={{ width: "-webkit-fill-available" }} />
                                        </Flex><Text content={`${this.state.calculatePercentage}% Completed`} className="trending-card-descMobile" /></>
                                    }
                                </div>
                            } />
                            {
                                this.state.showLearningPath ?
                                    <div>
                                        <Segment className="learning-path-segment" content={<Flex column>
                                            {this.state.isQuickStartLoading ? <Loader /> :
                                                <>
                                                    {this.state.isCompleted === true ?
                                                        <Box>
                                                            <Flex hAlign="center" className="avatarImageMobile">
                                                                <Flex.Item>
                                                                    <Image src={window.location.origin + "/images/MicrosoftTeams-image.png"} />
                                                                </Flex.Item>
                                                            </Flex>
                                                            <Flex styles={{ marginTop: "0.8rem" }}>
                                                                <Text content={this.localize("congrats")} className="completed" /></Flex>
                                                            <Flex styles={{ marginTop: "0.8rem" }}>
                                                                <Slider value={this.state.calculatePercentage}
                                                                    styles={{ width: "-webkit-fill-available" }}
                                                                    className={this.state.calculatePercentage == 0 ? "sliderDeskTopClassMobile" : "sliderDeskTopClassColorMobile"} />
                                                            </Flex>
                                                            <Flex>
                                                                <Text content={`${this.state.calculatePercentage}% Completed`} className="textCompletedMobile" /></Flex>
                                                            {this.state.gettingStartedCards.length === 0 ? <Text content={this.localize("noRecordFound")} /> : <div className="checkBoxClass">
                                                                {checkBox}
                                                            </div>}
                                                        </Box>
                                                        :
                                                        <>
                                                            <><><Flex>
                                                                <Slider value={this.state.calculatePercentage} styles={{ width: "-webkit-fill-available" }}
                                                                    className={this.state.calculatePercentage == 0 ? "sliderDeskTopClassMobile" : "sliderDeskTopClassColorMobile"} />
                                                            </Flex><Text content={`${this.state.calculatePercentage}% Completed`} className="trending-card-desc" styles={{ marginLeft: "1rem" }} /></>{this.state.gettingStartedCards.length === 0 ? <Text content={this.localize("noRecordFound")} /> : <div className="checkBoxClass">
                                                                {checkBox}
                                                            </div>}</>
                                                        </>
                                                    }
                                                </>
                                            }
                                        </Flex>} />
                                    </div>

                                    : <></>
                            }
                        </div>

                        {/*getting started carousel*/}

                        <div className="getting-started-item-container pading-top-div-mobile">
                            <Segment className="getting-started-carousel" content={
                                <Flex column gap="gap.small">
                                    <Text content={this.localize("gettingStartedText")} weight="semibold" />
                                    {this.state.isIntroTeamsLoading ? <Loader /> :
                                        <CarouselCardMobile carouselItem={this.state.gettingStartedCards} botId={this.state.botId} isShareHide={true} />
                                    }
                                </Flex>
                            } />
                        </div>

                        <div className="getting-started-item-container pading-top-div-mobile">
                            <Segment className="getting-started-carousel" content={
                                <Flex column gap="gap.small">
                                    <Text content={this.localize("scenarioText")} aria-label={this.localize("scenarioText")} className="ui-Card_Text-Mobile" />
                                    {this.state.isGetTeamsLoading ? <Loader /> :
                                        <Flex hAlign="center">
                                            {this.state.scenariosCards.length === 0 ? <Text content={this.localize("noRecordFound")} /> :
                                                <CarouselCardMobile carouselItem={this.state.scenariosCards} botId={this.state.botId} isShareHide={false} />
                                            }
                                        </Flex>
                                    }
                                </Flex>
                            } />
                        </div>
                        <div className="getting-started-item-container pading-top-div-mobile">
                            <Segment styles={{ borderRadius: "6px !important" }} content={
                                <div className="trending-topic-container-mobile">
                                    <div>
                                        <Text content={this.localize("trendingTopicText")} aria-label={this.localize("trendingTopicText")}
                                            className="ui-Card_Text-Mobile" />
                                    </div>
                                    {this.state.isFrequentlyLoading ? <Loader /> :
                                        <Flex hAlign="center">
                                            {this.state.trendingTopicsCards.length === 0 ? <Text content={this.localize("noRecordFound")} /> :
                                                <Grid columns={1} content={tiles} />
                                            }
                                        </Flex>
                                    }
                                </div>
                            } />
                        </div>
                    </div>
                </div>
            </Flex>
        );
    }

    /** Renders user home page */
    render() {
        return (
            <div>
                {this.state.isMobileView ? this.renderMobileView() : this.renderDesktopView()}
            </div>
        );
    }
}

export default withTranslation()(withContext(UserDashboard));