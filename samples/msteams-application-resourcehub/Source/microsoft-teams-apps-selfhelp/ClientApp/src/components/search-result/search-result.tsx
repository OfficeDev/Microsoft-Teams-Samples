import * as React from 'react';
import { Flex, Divider, Loader, Video, Image, List, Text, Input, SearchIcon, Header, ShareGenericIcon } from '@fluentui/react-northstar'
import './search-result.css'
import { TFunction, withTranslation, WithTranslation } from 'react-i18next';
import withContext, { IWithContext } from '../../providers/context-provider';
import IArticle from '../../models/article'
import { geBingSearchResultsAsync } from '../../api/bing-search-api';
import { ItemType } from '../../models/item-type';
import { Breadcrumb } from '@fluentui/react-northstar';
import { ChevronStartIcon } from '@fluentui/react-icons-northstar';
import { logCustomEvent } from '../../api/log-event-api';
interface ISearchResultesProps extends WithTranslation, IWithContext {
    searchText: string;
    botId: string;
}

const SearchResultes = (props): React.ReactElement => {
    const localize: TFunction = props.t;
    const [searchText, setSearchText] = React.useState("");
    const [isLoading, setLoading] = React.useState(true);
    const [isMobileView, setMobileView] = React.useState(window.outerWidth <= 750);
    const [videoSearchResultes, setvideoSearchResultes] = React.useState([]);
    const [articleSearchResultes, setarticleSearchResultes] = React.useState([]);
    const [internalSearchResultes, setInternalSearchResultes] = React.useState([]);
    const onSearchClick = async () => {
        initializeSearchData(searchText);
    }

    /**
     * Set columns based on screen size
     */
    const screenResize = () => {
        setMobileView(window.outerWidth <= 750);
    }

    const initializeSearchData = async (query) => {
        setLoading(true);
        let querySearch = {
            query: query
        };
        var response = await geBingSearchResultsAsync(querySearch);
        if (response.data) {
            if (response.data.webPages != null && response.data.webPages.value != null && response.data.webPages.value != undefined && response.data.webPages.value.length > 0) {
                setarticleSearchResultes(response.data.webPages.value);
                setLoading(false);
            }

            if (response.data.videos != null && response.data.videos.value != null && response.data.videos.value != undefined && response.data.videos.value.length > 0) {
                setvideoSearchResultes(response.data.videos.value);
                setLoading(false);
            }

            if (response.data.articles != null && response.data.articles != undefined && response.data.articles.length > 0) {
                setInternalSearchResultes(response.data.articles);
                setLoading(false);
            }
        }
        else {
            setLoading(false);
        }

        logCustomEvent({
            eTag: "",
            timestamp: new Date(),
            partitionKey: "",
            rowKey: "",
            eventId: "",
            learningContentId: "",
            eventType: "Search",
            createdOn: new Date(),
            userAadId: props.userObjectId!,
            searchkey: query,
            tenantId: props.tid!,
            sharedToUserIds: "",
            sharedToChannelIds: "",
        });
        setLoading(false);
    }

    const onSearchTextChange = (query: string) => {
        setSearchText(query);
    }

    const onEnterKeyPress = (event) => {
        if (event.key === 'Enter') {
            if (searchText.trim() !== "" || searchText.trim() !== '') {
                initializeSearchData(searchText.trim());
            }
            else {
                setLoading(false);
            }
        }
    }

    React.useEffect(() => {
        setLoading(true);
        screenResize();
        window.addEventListener("resize", screenResize);

        if (props.searchText.trim() !== "" || props.searchText.trim() !== '') {
            setSearchText(props.searchText);
            initializeSearchData(props.searchText);
        }
        else {
            setLoading(false);
        }
    }, []);

    const onShareClick = (learningId: string) => {
        props.microsoftTeams.tasks.startTask({
            title: localize("shareContent"),
            height: 600,
            width: 600,
            url: `${window.location.origin}/view-content-share?id=${learningId}`
        }, (error: any, result: any) => {

        });
    }

    const onExternalVideoDescriptionClick = (webSearchUrl) => {
        window.open(webSearchUrl);
    }

    const onExternalArticleDescriptionClick = (url) => {
        window.open(url);
    }

    const onCardClick = (itemtype, learningId) => {
        if (itemtype === ItemType.Video) {
            props.microsoftTeams.tasks.startTask({
                title: localize("viewArticle"),
                height: 600,
                width: 600,
                url: `${window.location.origin}/view-video-content?id=${learningId}&status=${true}`
            }, (error: any, result: any) => {
                if (result.message === "isFeedbackOpen") {
                    props.microsoftTeams.tasks.startTask({
                        title: localize("feedbackText"),
                        height: 320,
                        width: 700,
                        url: `${window.location.origin}/user-feedback?status=${result.status}`
                    });
                }
            });
        }
        else {
            var appId = props.botId;
            var baseUrl = `${window.location.origin}/view-image-content?id=${learningId}`
            let url = `https://teams.microsoft.com/l/stage/${appId}/0?context={"contentUrl":"${baseUrl}","websiteUrl":"${baseUrl}","name":"View article"}`;
            props.microsoftTeams.executeDeepLink(encodeURI(url));
        }
    }

    const getExt = (filename: any) => {
        var ext = filename.split('.').pop();
        if (ext == filename) return "";
        return ext;
    }

    /** 
    * Render table of questions. 
    */
    const renderSearchViewDesktop = () => {
        let rows: any[] = [];
        let articleRows: any[] = [];
        let videoRows: any[] = [];
        let internalSearchRows: any[] = [];
        if (articleSearchResultes?.length > 0) {
            articleRows = articleSearchResultes.map((item: any, index: number) => {
                return (<>
                    <Flex gap="gap.smaller" className="container"   >
                        <Flex gap="gap.smaller" padding="padding.medium" column styles={{ width: "100%" }} >
                            <Flex gap="gap.small">
                                <Flex.Item size="size.medium">
                                    <div>
                                        <Image className="image-size" src={item.thumbnailUrl !== null ? item.thumbnailUrl : window.location.origin + "/images/Card6.png"} />
                                    </div>
                                </Flex.Item>
                                <Flex.Item grow >
                                    <Flex column vAlign="stretch" >
                                        <Flex className="header-text">
                                            <Header className="title-text" as="h3" content={
                                                <a target="_blank" href={item.url}><Text content={item.name} title={item.name} className="text-ellipsis" /></a>} />

                                        </Flex>
                                        <Flex className="search-item-description-format" onClick={() => onExternalArticleDescriptionClick(item.url)} >
                                            <Text content={item.snippet} title={item.description} className="text-ellipsis" />
                                        </Flex>
                                    </Flex>
                                </Flex.Item>
                            </Flex>
                        </Flex>
                    </Flex >
                    <Divider />
                </>);
            });
        }

        if (videoSearchResultes?.length > 0) {
            videoRows = videoSearchResultes.map((item: any, index: number) => {
                return (<>
                    <Flex gap="gap.smaller" className="container"   >
                        <Flex gap="gap.smaller" padding="padding.medium" column styles={{ width: "100%" }} >
                            <Flex gap="gap.small">
                                <Flex.Item size="size.medium" push>
                                    <div>
                                        <Video className="image-size"
                                            poster={item.thumbnailUrl}
                                            src={item.contentUrl}
                                        />
                                    </div>
                                </Flex.Item>
                                <Flex.Item grow >
                                    <Flex column gap="gap.smaller" vAlign="stretch" >
                                        <Flex className="header-text">
                                            <Header className="title-text" as="h3" content={
                                                <a target="_blank" href={item.webSearchUrl}><Text content={item.name} title={item.name} className="text-ellipsis" /></a>} />
                                        </Flex>
                                        <Flex className="search-item-description-format" onClick={() => onExternalVideoDescriptionClick(item.webSearchUrl)}>
                                            <Text content={item.description} title={item.description} className="text-ellipsis" />
                                        </Flex>
                                    </Flex>
                                </Flex.Item>
                            </Flex>
                        </Flex>
                    </Flex >
                    <Divider />
                </>);
            });
        }

        if (internalSearchResultes?.length > 0) {
            internalSearchRows = internalSearchResultes.map((item: IArticle, index: number) => {
                let ismp4File = false;
                let ext = getExt(item.itemlink);
                if (ext == "mp4" || ext == "MP4") {
                    ismp4File = true;
                }
                return (<>
                    <Flex gap="gap.smaller" className="container"  >
                        <Flex gap="gap.smaller" padding="padding.medium" column styles={{ width: "100%" }} >
                            <Flex gap="gap.small">
                                <Flex.Item size="size.medium" push>
                                    <div>
                                        {item.itemType == ItemType.Video ?
                                            ismp4File ?
                                                <Video className="image-size"
                                                    poster={item.tileImageLink}

                                                    src={item.itemlink}
                                                /> : <iframe
                                                    className="image-size"
                                                    src={item.itemlink}
                                                    frameBorder="0"
                                                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                                                    allowFullScreen>
                                                </iframe>
                                            :
                                            <Image className="image-size" styles={{ height: "118px", width: "192px", borderRadius: "8px" }} src={item.tileImageLink !== null ? item.tileImageLink : window.location.origin + "/images/Card6.png"} />
                                        }
                                    </div>
                                </Flex.Item>
                                <Flex.Item grow >
                                    <Flex column gap="gap.smaller" vAlign="stretch" onClick={() => { onCardClick(item.itemType, item.learningId) }}>
                                        <Flex className="header-text">
                                            <Header className="title-text" as="h3"><Text content={item.title} title={item.title} className="text-ellipsis" /></Header>
                                        </Flex>
                                        <Flex className="search-item-description-format"   >
                                            <Text content={item.description} title={item.description} className="text-ellipsis" />
                                        </Flex>
                                    </Flex>
                                </Flex.Item>
                                <Flex.Item push>
                                    <Flex hAlign="center" vAlign="center" styles={{ marginRight: "0px" }}>
                                        <ShareGenericIcon className="sharePointer" outline onClick={() => { onShareClick(item.learningId) }} />
                                    </Flex>
                                </Flex.Item>
                            </Flex>
                        </Flex>
                    </Flex >
                    <Divider />
                </>);
            });
        }

        if (internalSearchRows.length > 0) {
            rows = rows.concat(internalSearchRows);
        }

        if (articleRows.length > 0) {
            rows = rows.concat(articleRows);
        }
        if (videoRows.length > 0) {
            rows = rows.concat(videoRows);
        }

        return (
            rows.length == 0 ? <Flex hAlign="center"><Text content={"No Record Found"} /> </Flex> : <List items={rows} />
        );
    }

    const renderDesktopView = () => {
        return (
            <Flex column gap="gap.medium" className="search body-width" styles={{ marginLeft: "auto", marginRight: "auto" }} >
                <Flex column gap="gap.large" >
                    <Input className="search-input"
                        onChange={(event: any) => onSearchTextChange(event.target.value)} value={searchText}
                        fluid icon={<SearchIcon onClick={onSearchClick} />} placeholder={localize("search")}
                        onKeyPress={(event: any) => onEnterKeyPress(event)} style={{ background: "#FFFFFF" }}
                    />
                    <Flex>
                        <Breadcrumb aria-label="breadcrumb" styles={{ paddingLeft: "25px", cursor: "pointer" }} onClick={props.onHomeBreadcrumClick} >
                            <Breadcrumb.Divider>
                                <ChevronStartIcon />
                            </Breadcrumb.Divider>
                            <Breadcrumb.Item>
                                <Text weight="semibold" content={localize("backButton")} />
                            </Breadcrumb.Item>
                        </Breadcrumb>
                    </Flex>
                </Flex>

                {isLoading ?
                    <Flex hAlign="center" vAlign="center" styles={{ padding: "0.5rem", height: "5rem" }}>
                        <Loader />
                    </Flex> :
                    renderSearchViewDesktop()
                }
            </Flex>
        );
    }

    const renderMobileView = () => {
        return (
            <Flex column gap="gap.medium">
                <Flex column gap="gap.large" >
                    <Flex hAlign="stretch" className="search-box-mobile-view">
                        <Input
                            onChange={(event: any) => onSearchTextChange(event.target.value)} value={searchText}
                            fluid icon={<SearchIcon onClick={onSearchClick} />} placeholder={localize("search")}
                            onKeyPress={(event: any) => onEnterKeyPress(event)} style={{ background: "#FFFFFF" }}
                        />
                    </Flex>
                    <Flex>
                        <Breadcrumb aria-label="breadcrumb" styles={{ paddingLeft: "25px", cursor: "pointer" }} onClick={props.onHomeBreadcrumClick} >
                            <Breadcrumb.Divider>
                                <ChevronStartIcon />
                            </Breadcrumb.Divider>
                            <Breadcrumb.Item>
                                <Text weight="semibold" content={localize("backButton")} />
                            </Breadcrumb.Item>
                        </Breadcrumb>
                    </Flex>
                </Flex>

                {isLoading ?
                    <Flex hAlign="center" vAlign="center" styles={{ padding: "0.5rem", height: "5rem" }}>
                        <Loader />
                    </Flex> :
                    renderSearchViewMobile()
                }
            </Flex>
        );
    }

    /** 
    * Render table of questions. 
    */
    const renderSearchViewMobile = () => {
        let rows: any[] = [];
        let articleRows: any[] = [];
        let videoRows: any[] = [];
        let internalSearchRows: any[] = [];
        if (articleSearchResultes?.length > 0) {
            articleRows = articleSearchResultes.map((item: any, index: number) => {
                return (<>
                    <Flex gap="gap.smaller">
                        <Flex gap="gap.smaller" padding="padding.medium" column styles={{ width: "100%" }} >
                            <Flex gap="gap.small">
                                <Flex.Item size="size.medium">
                                    <div>
                                        <Image className="image-size-mobile" src={item.thumbnailUrl !== null ? item.thumbnailUrl : window.location.origin + "/images/Card6.png"} />
                                    </div>
                                </Flex.Item>
                                <Flex.Item grow >
                                    <Flex column vAlign="stretch" >
                                        <Flex className="header-text-mobile">
                                            <Header className="title-text" as="h3" content={
                                                <a target="_blank" href={item.url}><Text content={item.name} title={item.name} className="text-ellipsis" /></a>} />

                                        </Flex>
                                        <Flex className="search-item-description-format-mobile" onClick={() => onExternalArticleDescriptionClick(item.url)} >
                                            <Text content={item.snippet} title={item.description} className="text-ellipsis" />
                                        </Flex>
                                    </Flex>
                                </Flex.Item>
                            </Flex>
                        </Flex>
                    </Flex >
                    <Divider />
                </>);
            });
        }

        if (videoSearchResultes?.length > 0) {
            videoRows = videoSearchResultes.map((item: any, index: number) => {
                return (<>
                    <Flex gap="gap.smaller">
                        <Flex gap="gap.smaller" padding="padding.medium" column styles={{ width: "100%" }} >
                            <Flex gap="gap.small">
                                <Flex.Item size="size.medium" push>
                                    <div>
                                        <Video className="image-size-mobile"
                                            poster={item.thumbnailUrl}
                                            src={item.contentUrl}
                                        />
                                    </div>
                                </Flex.Item>
                                <Flex.Item grow >
                                    <Flex column gap="gap.smaller" vAlign="stretch" >
                                        <Flex className="header-text-mobile">
                                            <Header className="title-text" as="h3" content={
                                                <a target="_blank" href={item.webSearchUrl}><Text content={item.name} title={item.name} className="text-ellipsis" /></a>} />
                                        </Flex>
                                        <Flex className="search-item-description-format-mobile" onClick={() => onExternalVideoDescriptionClick(item.webSearchUrl)}>
                                            <Text content={item.description} title={item.description} className="text-ellipsis" />
                                        </Flex>
                                    </Flex>
                                </Flex.Item>
                            </Flex>
                        </Flex>
                    </Flex >
                    <Divider />
                </>);
            });
        }

        if (internalSearchResultes?.length > 0) {
            internalSearchRows = internalSearchResultes.map((item: IArticle, index: number) => {
                let ismp4File = false;
                let ext = getExt(item.itemlink);
                if (ext == "mp4" || ext == "MP4") {
                    ismp4File = true;
                }
                return (<>
                    <Flex gap="gap.smaller">
                        <Flex gap="gap.smaller" padding="padding.medium" column styles={{ width: "100%" }} >
                            <Flex gap="gap.small">
                                <Flex.Item size="size.medium" push>
                                    <div>
                                        {item.itemType == ItemType.Video ?
                                            ismp4File ?
                                                <Video className="image-size-mobile"
                                                    poster={item.tileImageLink}

                                                    src={item.itemlink}
                                                /> : <iframe
                                                    className="image-size-mobile"
                                                    src={item.itemlink}
                                                    frameBorder="0"
                                                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                                                    allowFullScreen>
                                                </iframe>
                                            :
                                            <Image className="image-size-mobile" src={item.tileImageLink !== null ? item.tileImageLink : window.location.origin + "/images/Card6.png"} />
                                        }
                                    </div>
                                </Flex.Item>
                                <Flex.Item grow >
                                    <Flex column gap="gap.smaller" vAlign="stretch" onClick={() => { onCardClick(item.itemType, item.learningId) }}>
                                        <Flex className="header-text-mobile">
                                            <Header className="title-text" as="h3"><Text content={item.title} title={item.title} className="text-ellipsis" /></Header>
                                        </Flex>
                                        <Flex className="search-item-description-format-mobile">
                                            <Text content={item.description} title={item.description} className="text-ellipsis" />
                                        </Flex>
                                    </Flex>
                                </Flex.Item>
                                <Flex.Item push>
                                    <Flex hAlign="center" vAlign="center" styles={{ marginRight: "0px" }}>
                                        <ShareGenericIcon className="sharePointer" outline onClick={() => { onShareClick(item.learningId) }} />
                                    </Flex>
                                </Flex.Item>
                            </Flex>
                        </Flex>
                    </Flex >
                    <Divider />
                </>);
            });
        }

        if (internalSearchRows.length > 0) {
            rows = rows.concat(internalSearchRows);
        }

        if (articleRows.length > 0) {
            rows = rows.concat(articleRows);
        }

        if (videoRows.length > 0) {
            rows = rows.concat(videoRows);
        }

        return (
            rows.length == 0 ? <Flex hAlign="center"><Text content={"No Record Found"} /> </Flex> : <Flex className="search-item-view-mobile">
                <List items={rows} /></Flex>
        );
    }

    return (
        isMobileView ? renderMobileView() : renderDesktopView()
    )
}

export default withTranslation()(withContext(SearchResultes));