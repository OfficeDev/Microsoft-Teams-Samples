import * as React from 'react';
import { useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { Button, Flex, ArrowUpIcon, LeaveIcon, Text, Table, MoreIcon, Loader, EditIcon, Popup,TrashCanIcon, FlagIcon, AddIcon, Menu, menuAsToolbarBehavior, Dialog, MenuButton, Input, Checkbox } from '@fluentui/react-northstar';
import { withTranslation, WithTranslation } from 'react-i18next';
import withContext, { IWithContext } from '../../providers/context-provider';
import { TFunction } from 'i18next';
import {getAllLearningContent, DeleteLearningContentById} from '../../api/article-api';
import "./admin-dashboard.scss";
import { SelectionType } from '../../models/selection-type';
import { ItemType } from '../../models/item-type';
import { SourceType } from '../../models/source-type';
import "../user-dashboard/user-dashboard.scss"; 
import { CSVLink } from 'react-csv'
import IFeedbackExcel from '../../models/feedback-excel';
import ITelemetryExcel from '../../models/telemetry-excel';
import IFeedback from '../../models/feedback';
import { getAllUserFeedback } from '../../api/feedback-api';
import { SearchIcon, BellIcon } from '@fluentui/react-icons-northstar'
import IArticleCheckBox from '../../models/articleCheckBox';
import { useNotificationContextProvider } from '../../providers/notification-provider';
import { getAllTelemetry } from '../../api/telemetry-api';
export interface ITrendingCardProps { }

const AdminDashboard = (props): React.ReactElement => {
    const localize: TFunction = props.t;
    const {
        state,
        setCheckBoxNotificationList
    } = useNotificationContextProvider();
    const [isLoading, setLoading] = React.useState(true);
    const [articles, setarticles] = React.useState(Array<IArticleCheckBox>());
    const [searchArticles, setSearchArticles] = React.useState(Array<IArticleCheckBox>());
    const [chkSearchArticles, setChkSearchArticles] = React.useState(Array<IArticleCheckBox>());
    const [feedbackExcelEntity, setfeedbackExcelEntity] = React.useState(Array<IFeedbackExcel>());
    const [telemetryExcelEntity, settelemetryExcelEntity] = React.useState(Array<ITelemetryExcel>());
    const [searchText, setSearchText] = React.useState("");
    const [commaString, setCommastring] = React.useState<string>("");
    const tableCreatedColumnDesign = { minWidth: "4vw", maxWidth: "4vw" };
    useEffect(() => {
        initializeDataAsync();
        getFeedback();
        getTelemetry();
    }, []);

    const initializeDataAsync = async () => {
        setLoading(true);
        var response = await getAllLearningContent();
        if (response.data) {
            setarticles(response.data);

        }

        setLoading(false);
    }

    const handleInputChange = (searchText: any) => {
        setLoading(true);
        var data = articles;
        if (searchText !== "" || searchText !== null) {
            data = articles.filter(x => ((x.title?.toUpperCase().indexOf(searchText.toUpperCase()) !== -1)))
        }
        setSearchText(searchText);
        setSearchArticles(data);
        setLoading(false);
    }

    const getFeedback = async () => {
        var response = await getAllUserFeedback();
        let results: IFeedback[] = response.data;
        if (results) {
            let feedbackEntity: IFeedbackExcel[] = [];
            results.map(async (result) => {
                let feedbackType;
                if (result.feedbackType === 0) {
                    feedbackType = localize("generalfeedback")
                }
                else if (result.feedbackType === 1) {
                    feedbackType = localize("learningContentFeedback")
                }
                else if (result.feedbackType === 2) {
                    feedbackType = localize("feedbackFromLearningPath")
                }
                else if (result.feedbackType === 3) {
                    feedbackType = localize("feedbackCompleted")
                }

                let statusHelpful;
                if (result.helpfulStatus === 0) {
                    statusHelpful = localize("Super")
                }
                else if (result.helpfulStatus === 1) {
                    statusHelpful = localize("Medium")
                }
                else if (result.helpfulStatus === 2) {
                    statusHelpful = localize("NotHelpful")
                }

                feedbackEntity.push({
                    feedbackType: feedbackType,
                    articleTitle: result.learningContentId,
                    helpfulStatus: statusHelpful,
                    isHelpful: result.isHelpful,
                    rating: result.rating,
                    feedback: result.feedback,
                    submittedOn: result.createdOn,
                    submittedBy: result.createdBy
                });
            });
            setfeedbackExcelEntity(feedbackEntity);
        }
    }

    const getTelemetry = async () => {
        var response = await getAllTelemetry();
        settelemetryExcelEntity(response.data);
    }

    const onEditArticleClick = (articleId: string) => {
        props.microsoftTeams.tasks.startTask({
            title: localize("editArticleText"),
            height: 600,
            width: 600,
            url: `${window.location.origin}/edit-article/${articleId}`
        }, (error: any, result: any) => {
            var confirmMessage = JSON.parse(result);
            if (confirmMessage !== null && confirmMessage !== undefined && confirmMessage.confirm === true) {
                initializeDataAsync();
            }
        });
    }
    
    const onSendNotificationPreviewClick = () => {
        props.microsoftTeams.tasks.startTask({
            title: localize("notificationPreview"),
            height: 490,
            width: 500,
            url: `${window.location.origin}/notification-preview?id=${commaString}`
        }, (error: any, result: any) => {
            if (result.message === "Close") {
                debugger
                setarticles([]);
                setChkSearchArticles([]);
                initializeDataAsync();
            }
        }
    );
    }

    const onAddNewArticleClick = () => {
        props.microsoftTeams.tasks.startTask({
            title: localize("addNewEntity"),
            height: 800,
            width: 700,
            url: `${window.location.origin}/add-new-article`
        }, (error: any, result: any) => {
            var confirmMessage = JSON.parse(result);
            if (confirmMessage !== null && confirmMessage !== undefined && confirmMessage.confirm === true) {
                initializeDataAsync();
            }

        });
    }

    const openDialog = (id: string) => {
        let taskInfo = {
            title: localize("DeleteTitle"),
            height: 200,
            width: 450,
            url: `${window.location.origin}/delete-article-dialog`,
            fallbackUrl: `${window.location.origin}/delete-article-dialog`,
        };
        const setNewFlag = (id: string) => {

        }
        microsoftTeams.tasks.startTask(taskInfo, (err, jsonResponse) => {

            var confirmMessage = JSON.parse(jsonResponse);
            if (confirmMessage !== null && confirmMessage !== undefined && confirmMessage.confirm === true) {
                DeleteLearningContentById(id);
                let update = articles.filter(d => d.learningId !== id);
                setarticles(update);
                if (searchText !== "") {
                    let update = searchArticles.filter(d => d.learningId !== id);
                    setSearchArticles(update);
                }
            }
        });

        return true;
    }

    const onCheckBoxChange = (article:IArticleCheckBox) =>{
        let arrayArticle: IArticleCheckBox[]=[];
        let existingArticle = chkSearchArticles;
        existingArticle.forEach((article:IArticleCheckBox) => {           
            arrayArticle.push(article)
        });
        if(article.isChecked == undefined || article.isChecked == false)
        {     
        article.isChecked = true;
        arrayArticle.push(article);
        }
        else{
            article.isChecked = false;
            arrayArticle = arrayArticle.filter(s=>s.rowKey != article.rowKey);
        }
        let articlestring: string="";
        arrayArticle.forEach((article:IArticleCheckBox) => {  
            articlestring += article.rowKey+",";
        });
        setCommastring(articlestring);
        setChkSearchArticles(arrayArticle);
    }

    const renderArticleList = () => {       
        let elements: any = [];
        const headers = {
            key: "header",
            items: [
                {
                    content: "",
                    design: tableCreatedColumnDesign
                },
                {
                    content: <Text weight="bold" className="section-details" content={localize("section")} />
                },
                {
                    content: <Text weight="bold" content={localize("title")} />
                },
                {
                    content: <Text weight="bold" content={localize("itemType")} />
                },
                {
                    content: <Text weight="bold" content={localize("Sources")} />
                },
                {
                    content: <Text weight="bold" content={localize("primaryTag")} />
                },
                {
                    content: <Text weight="bold" content={localize("secondaryTag")} />
                },
                {
                    content: <Text weight="bold" content={localize("itemLink")} />
                },
                {
                    content: <Text weight="bold" content={localize("Length")} />
                },
                {
                    content: <Text weight="bold" content={localize("description")} />
                },
                {
                    content: <Text weight="bold" content={localize("knowMoreLink")} />
                },
                {
                    content: <Text weight="bold" content={localize("tileImageLink")} />
                },
                {
                    content: ""
                }
            ]
        };

            elements = (searchText !== ""? searchArticles:articles).map((article: IArticleCheckBox, index: number) => {
                return {
                    "key": index,
                    "items": [
                        {
                            content: <Checkbox checked={article.isChecked} onClick={() => onCheckBoxChange(article)} />,
                            design: tableCreatedColumnDesign
                        },
                        {
                            content: <Text className="section-details" content={article.sectionType == SelectionType.GettingStarted ? localize("gettingStartedText")
                                : article.sectionType == SelectionType.Scenarios ? localize("scenarioText")
                                    : article.sectionType == SelectionType.LearningPath ? localize("learningPathText")
                                        : localize("trendingTopicText")} />
                        },
                        {
                            content: <Text className="title-details" content={article.title} title={article.title} style={{ whiteSpace: "nowrap", position: "absolute" }} />
                        },
                        {
                            content: <Text content={article.itemType == ItemType.Articles ? localize("article")
                                : article.itemType == ItemType.Video ? localize("video")
                                    : article.itemType == ItemType.Image ? localize("image") :
                                        localize("searchResult")} />
                        },
                        {
                            content: <Text content={article.source == SourceType.External ? localize("external") : localize("internal")} />
                        },
                        {
                            content: <Text className="title-details" content={article.primaryTag} title={article.primaryTag} style={{ whiteSpace: "nowrap", position: "absolute" }} />
                        },
                        {
                            content: <Text className="title-details" content={article.secondaryTag} title={article.secondaryTag} style={{ whiteSpace: "nowrap", position: "absolute" }} />
                        },
                        {
                            content: <a target="_blank" href={article.itemlink}>{localize("linkText")}</a>
                        },
                        {
                            content: <Text className="title-details" content={article.length?.toString() + " minutes"} />
                        },
                        {
                            content: <Text className="title-details" content={article.description} title={article.description} style={{ whiteSpace: "nowrap", position: "absolute" }} />
                        },
                        {
                            content: <Text content={<a target="_blank" href={article.knowmoreLink}>{localize("linkText")}</a>} />
                        },
                        {
                            content: <Text content={<a target="_blank" href={article.tileImageLink}>{localize("linkText")}</a>} />
                        },
                        {
                            content: <MenuButton
                                trigger={<Button text iconOnly icon={<MoreIcon outline />} aria-label="Click button" />}
                                menu={[
                                    {
                                        key: '1',
                                        content: localize("edit"),
                                        icon: <EditIcon outline />,
                                        onClick: () => onEditArticleClick(article.learningId),
                                    },
                                    {
                                        key: '2',
                                        content: localize("delete"),
                                        icon: <TrashCanIcon outline />,
                                        onClick: () => openDialog(article.learningId),
                                    },                                   
                                ]}

                                accessibility={menuAsToolbarBehavior}
                                aria-label={localize("composeEditor")}
                            />
                        },
                    ]
                };
            });
            return (
                <div className="sub-page-container">
                    <Flex styles={{ marginTop: "5.3rem" }}>
                        <Table className="table-width" header={headers} rows={elements} />
                    </Flex>
                </div>
            );     
    }

    return (<>
        <Flex className="admin-page-container">
            <Flex className="home-nav-bar-container">
                <Flex vAlign='center'>
                    <Button icon={<AddIcon styles={{ paddingLeft: "10px" }} />} content={localize("addItem")} onClick={onAddNewArticleClick} styles={{ marginLeft: "1rem" }} text />
                    <CSVLink data={feedbackExcelEntity} filename={"report"}><Button secondary icon={<ArrowUpIcon styles={{ paddingLeft: "10px" }} xSpacing="none" />} styles={{ marginLeft: "0.8rem" }} content={localize("exportFeedback")} aria-label={localize("exportFeedback")} text /></CSVLink>
                    <CSVLink data={telemetryExcelEntity} filename={"telemetryreport"}><Button secondary icon={<ArrowUpIcon xSpacing="none" />} styles={{ marginLeft: "0.8rem" }} content={localize("exportTelemetry")} aria-label={localize("exportTelemetry")} text /></CSVLink>
                    {
                        chkSearchArticles.length > 15 ? <Dialog
                            trigger={<Popup content={<Text className="dialog-error-message" error content="You can select max 15 items" />} />}
                        /> : <Button disabled={chkSearchArticles.length > 15 || chkSearchArticles.length == 0} onClick={onSendNotificationPreviewClick} icon={<BellIcon styles={{ paddingLeft: "10px" }} outline rotate={30} />} content={localize("sendNotification")} text />
                    }                    
                </Flex>
                <Flex.Item push >
                    <Flex className="nav-bar-feedback" vAlign='center' gap="gap.small">
                        <Flex vAlign="center" gap="gap.smaller" className="items-container cursor-pointer">

                            <Input onChange={(event: any) => handleInputChange(event.target.value)} icon={<SearchIcon outline />} placeholder={localize("searchTitle")} />

                            <Button icon={<LeaveIcon styles={{ paddingLeft: "10px" }} />} content={localize("switchToUser")} aria-label={localize("focusButton")}
                                text onClick={props.onViewUserPageClick} />
                        </Flex>

                    </Flex>
                </Flex.Item>
            </Flex>
        </Flex>
        {isLoading ?
            <Flex hAlign="center" vAlign="center">
                <Loader />
            </Flex>
            :
            renderArticleList()}
    </>);
}
export default withTranslation()(withContext(AdminDashboard));