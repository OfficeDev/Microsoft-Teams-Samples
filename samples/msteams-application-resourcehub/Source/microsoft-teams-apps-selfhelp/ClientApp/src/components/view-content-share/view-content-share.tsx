import React from "react";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import withContext, { IWithContext } from '../../providers/context-provider';
import * as microsoftTeams from "@microsoft/teams-js";
import {
    Flex,
    Text,
    Dropdown,
    Button,
    TextArea,
    Breadcrumb,
    ChevronEndMediumIcon,
    Video,
    Image,
    Loader,
    RadioGroup
} from '@fluentui/react-northstar';
import "./view-content-share.scss";
import IArticle from "../../models/article";
import { ItemType } from "../../models/item-type";
import { getLearningContentById } from "../../api/article-api";
import { getAllUsersAsync, getAllTeamsAsync } from "../../api/user-api";
import { ImageUtil } from "../../utility/imageutility";
import { shareArticleAsync } from "../../api/share-article-api";
import { logCustomEvent } from "../../api/log-event-api";
import Constants from "../../constants/constants";

interface IViewContentShareProps extends WithTranslation, IWithContext {
    carouselItem: IArticle[];
    isStageView:boolean;
    onShareButtonClick: () => void;
}

type dropdownItemUser = {
    key: string,
    header: string,
    content: string,
    image: string,
    user: {
        email: string,
    },
}

type dropdownItemTeams = {
    key: string,
    header: string,
    content: string,
    teamId: string,
}

const ViewContentShare: React.FunctionComponent<IViewContentShareProps> = props => {
    const localize: TFunction = props.t;
    const [isShareSuccess, setIsShareSuccess] = React.useState(false);
    const [isShareSuccessWithError, setIsShareSuccessWithError] = React.useState(false);
    const [userLoader, setUserLoader] = React.useState(false);
    const [disableButton, setDisableButton] = React.useState(true);
    const [shareButtonLoading, setShareButtonLoading] = React.useState(false);
    const [isShareToUser, setisShareToUser] = React.useState(true);
    const [userAadId, setUserAadId] = React.useState("");
    const [tenantId, setTenantId] = React.useState("");
    const [teamsLoader, setTeamsLoader] = React.useState(true);
    const [learningId, setLearningId] = React.useState("");
    const [itemlink, setItemLink] = React.useState("");
    const [tileImageLink, setTileImageLink] = React.useState("");
    const [message, setMessage] = React.useState("");
    const [itemType, seItemType] = React.useState<ItemType>();
    const [users, SetUsers] = React.useState([]);
    const [selectedUsers, SetSelectedUsers] = React.useState<dropdownItemUser[]>([]);
    const [teams, SetTeams] = React.useState([]);
    const [selectedTeams, SetSelectedTeams] = React.useState<dropdownItemTeams[]>([]);
    const [ismp4File, setismp4File] = React.useState(false);
    const [isMobileView, setMobileView] = React.useState(window.outerWidth <= 750);

    React.useEffect(() => {
        let params = new URLSearchParams(window.location.search);
        microsoftTeams.initialize();
        microsoftTeams.getContext((context) => {
            setUserAadId(context.userObjectId!);
            setTenantId(context.tid!);
            var learningId = params.get("id")!;
            if (learningId !== null) {
                setLearningId(learningId);
                intializeDataAsync(learningId);
                getTeamsList();
            }
        });
    }, []);

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

    const getManagerList = async (searchText:string) => {
        try {
            const response = await getAllUsersAsync(searchText);
            if(response.data && response.data.length >0)
            {
            SetUsers(response.data);
            }
            else
            {
                SetUsers([]);
            }

        } catch (error) {
            return error;
        }
    }

    const getTeamsList = async () => {
        try {
            setTeamsLoader(true);
            const response = await getAllTeamsAsync();
            SetTeams(response.data);
            setTeamsLoader(false);

        } catch (error) {
            setTeamsLoader(false);
            return error;
        }
    }

    const getExt = (filename: any) => {
        var ext = filename.split('.').pop();
        if (ext == filename) return "";
        return ext;
    }

    const intializeDataAsync = async (learningId: string) => {
        let article = await getLearningContentById(learningId);
        if (article.data) {
            setLearningId(article.data.learningId)
            setItemLink(article.data.itemlink);
            setTileImageLink(article.data.tileImageLink);
            seItemType(article.data.itemType);
            let ext = getExt(article.data.itemlink);
            if (ext == "mp4" || ext == "MP4") {
                setismp4File(true);
            }
        }

    }

    const onBackClick = () => {
        microsoftTeams.tasks.submitTask();
        return true;
    }

    const getUserItems = () => {
        const resultedUsers: dropdownItemUser[] = [];
        if (users) {
            users.forEach((element: any) => {
                resultedUsers.push({
                    key: element.userId,
                    header: element.displayName,
                    content: element.email,
                    image: (element.image === null || element.image === "null" || element.image === undefined || element.image === "") ?
                        ImageUtil.makeInitialImage(element.displayName) : element.image,
                    user: {
                        email: element.email
                    }
                });
            });
        }
        return resultedUsers;
    }

    const getTeamsItems = () => {
        const resultedTeams: dropdownItemTeams[] = [];
        if (teams) {
            teams.forEach((element: any) => {
                resultedTeams.push({
                    key: element.channelId,
                    header: (element.teamName + " > " + element.channelName),
                    content: (element.teamName + " > " + element.channelName),
                    teamId: element.teamId,

                });
            });
        }
        return resultedTeams;
    }

    const onSearchQueryChange = async (event: any, itemsData: any) => {
        if (itemsData && itemsData.value) {
        getManagerList(itemsData.searchQuery)
        }
    }

    const onUserSelectionChange = async (event: any, itemsData: any) => {
        if (itemsData && itemsData.value) {
            SetSelectedUsers(itemsData.value);
        }

        if (itemsData && itemsData.value.length > 0) {
            setDisableButton(false);
        }
        else {
            setDisableButton(true);
        }
    }

    const onTeamsSelectionChange = async (event: any, itemsData: any) => {

        if (itemsData && itemsData.value) {
            SetSelectedTeams(itemsData.value);
        }

        if (itemsData && itemsData.value.length > 0) {
            setDisableButton(false);
        }
        else {
            setDisableButton(true);
        }
    }

    const shareArticleClicked = async () => {
        setDisableButton(true);
        setShareButtonLoading(true);
        let share = {
            learningId: learningId,
            users: JSON.stringify(selectedUsers.map(a => a.key)),
            teamId: JSON.stringify(selectedTeams.map(a => a.teamId)),
            channelId: JSON.stringify(selectedTeams.map(a => a.key)),
            isShareToUser: isShareToUser,
            message: message
        };

        var response = await shareArticleAsync(share);
        if (response.status === 200) {
            setIsShareSuccess(true);
            logCustomEvent({
                eTag: "",
                timestamp: new Date(),
                partitionKey: "",
                rowKey: "",
                eventId: "",
                learningContentId: learningId,
                eventType: isShareToUser ? Constants.Shared_To_User : Constants.Shared_To_Channel,
                createdOn: new Date(),
                userAadId: userAadId,
                searchkey: "",
                tenantId: tenantId,
                sharedToUserIds: JSON.stringify(selectedUsers.map(a => a.key)),
                sharedToChannelIds: JSON.stringify(selectedTeams.map(a => a.key)),
            });
        }
        else {
            setIsShareSuccess(true);
            setIsShareSuccessWithError(true);
        }
    }

    const onMessageTextChange = (text: string) => {
        setMessage(text);
    }

    const onChange = (event) => {
        if (event == "1") {
            setisShareToUser(false)
        }
        else {
            setisShareToUser(true)
        }
    }

    const renderMainView = () => {
        return (
            <div className="containershare-Content">
                <Flex>
                    <Text className="text-shareTo" content={localize("shareWith")} styles={{ marginLeft: "2rem", marginTop: "2rem" }} /><RadioGroup
                        onCheckedValueChange={(e, data) => onChange(data?.value)} defaultCheckedValue="2"
                        items={[
                            {
                                key: '1',
                                label: 'Teams',
                                value: '1',
                            },
                            {
                                key: '2',
                                label: 'Users',
                                value: '2',
                            },
                        ]}
                        styles={{ marginTop: "2rem", marginLeft: "2rem" }} />
                </Flex>
                <Flex column gap="gap.small" >
                    <Flex hidden={!isShareToUser} className="view-content-dropdown-details" column>
                        <Text className="text-shareTo" content={localize("shareTo")} styles={{ marginTop: "1rem" }} />
                        <Dropdown className="width-manager-input font-weight-manager"
                            clearable
                            search
                            fluid
                            multiple
                            onSearchQueryChange={onSearchQueryChange}
                            items={getUserItems()}
                            onChange={onUserSelectionChange}
                            value={selectedUsers}
                            placeholder={localize("userPlaceholderText")}
                            noResultsMessage={localize("NoMatchMessage")}
                            loading={!userLoader}
                            loadingMessage="Loading..."
                            styles={{ marginTop: "1rem" }}
                        >
                        </Dropdown>
                        <Loader hidden={!userLoader} size="smallest" labelPosition="start" style={isMobileView ? { marginTop: "4rem", marginLeft: "28.8rem" } : { marginTop: "4rem", marginLeft: "27rem" }} styles={{ position: "absolute", zIndex: "0" }} />
                    </Flex>
                    <Flex hidden={isShareToUser} className="view-content-dropdown-details-share" column gap="gap.small">
                        <Text className="text-shareTo" content={localize("shareTo")} />
                        <Dropdown className="width-manager-input font-weight-manager"
                            clearable
                            search
                            fluid
                            multiple
                            items={getTeamsItems()}
                            onChange={onTeamsSelectionChange}
                            value={selectedTeams}
                            placeholder={localize("teamsPlaceholderText")}
                            noResultsMessage={localize("NoMatchMessage")}
                            loading={!teamsLoader}
                            loadingMessage="Loading..."
                        >
                        </Dropdown>
                        <Loader hidden={!teamsLoader} size="smallest" labelPosition="start" style={isMobileView ? { marginTop: "4rem", marginLeft: "28.8rem" } : { marginTop: "3.3rem", marginLeft: "27rem" }} styles={{ position: "absolute", zIndex: "0" }} />
                    </Flex>
                    <Flex className="view-content-dropdown-details-textArea" column>
                        <Text content={localize("typeAMessage")} style={{ marginTop: "0.8rem" }} className="typeMessage-Text" />
                        <TextArea className="text-area" fluid placeholder={localize("Type here")}
                            onChange={(event: any) => onMessageTextChange(event.target.value)} value={message} />
                        {itemType == ItemType.Video ?
                            ismp4File ?
                                <Video className="view-share-content-details"
                                    poster={tileImageLink}
                                    src={itemlink} styles={{ backgroundColor: "rgb(243, 242, 241)" }}

                                />
                                :
                                <><iframe
                                    className="view-share-content-details"
                                    src={itemlink}
                                    frameBorder="0"
                                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                                    allowFullScreen>
                                </iframe></>
                            : <Image className="view-share-content-details" src={tileImageLink} />}

                    </Flex>
                    <Flex className="view-content-dropdown-details-button" space="between">
                        <Breadcrumb aria-label="breadcrumb">
                            <Button content={localize("backButton")} icon={<ChevronEndMediumIcon rotate={180} />} onClick={onBackClick} text styles={{ minWidth: "0rem !important" }} />
                        </Breadcrumb>
                        <Button disabled={disableButton} loading={shareButtonLoading} content={localize("share")} primary onClick={() => { shareArticleClicked() }} />
                    </Flex>
                </Flex>
            </div >
        );
    }

    const successView = () => {
        return (
            <div className="success-message-container">
                <Flex column styles={{ marginLeft: "2rem", marginRight: "2rem" }} >
                    <Flex column gap="gap.medium" hAlign="center" vAlign="center" styles={{ marginLeft: "2rem", marginRight: "2rem" }}>
                        <Image src="/images/thank-you.png" className="thank-you-image" />
                        <Text weight="bold" content={localize("shareArticleSuccessMessage")} />
                        <Text content={localize("weWork")} />
                    </Flex>
                    <Flex className="thank-you-close-button-share" >
                        <Flex.Item push>
                            <Button content={localize("closeButton")} secondary onClick={props.isStageView ? props.onShareButtonClick : onBackClick}/>
                        </Flex.Item>
                    </Flex>
                </Flex>
            </div >
        );
    }

    const renderErrorPage = () => {
        return (
            <div className="success-message-container">
                <div className="error-message">
                    <Text content={localize("errorMessage")} error size="medium" />
                </div>
            </div>
        );
    }

    return (
        (!isShareSuccess && !isShareSuccessWithError) ? renderMainView()
            : (isShareSuccess && !isShareSuccessWithError) ? successView()
                : (isShareSuccess && isShareSuccessWithError) ? renderErrorPage() : <></>
    );
}
export default withTranslation()(withContext(ViewContentShare));