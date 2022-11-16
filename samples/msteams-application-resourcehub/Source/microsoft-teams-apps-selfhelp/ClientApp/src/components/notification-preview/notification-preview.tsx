import React, { useEffect } from 'react'
import { TFunction, WithTranslation, withTranslation } from 'react-i18next';
import withContext, { IWithContext } from '../../providers/context-provider';
import * as microsoftTeams from "@microsoft/teams-js";
import './notification-preview.css';
import { Input, Flex, FlexItem, Button, Text, Image, Box, List } from '@fluentui/react-northstar'
import IArticleCheckBox from '../../models/articleCheckBox';
import { getAllLearningContent } from '../../api/article-api';
import { ItemType } from '../../models/item-type';
import { sendNotificationAsync } from '../../api/user-api';

interface INotificationPreviewProps extends WithTranslation, IWithContext {
}
const NotificationPreview: React.FunctionComponent<INotificationPreviewProps> = (props) => {

    const localize: TFunction = props.t;

    const [articles, setarticles] = React.useState(Array<IArticleCheckBox>());
    const [isSendSuccessfully, setIsSendSuccessfully] = React.useState(false);
    const [showConfirmation, setShowConfirmation] = React.useState(false);
    const [titleText, setTitleText] = React.useState("")
    const [loading, setLoading] = React.useState(false);
    const [buttonLoading, setbuttonLoading] = React.useState(false);
    let params = new URLSearchParams(window.location.search);

    useEffect(() => {
        initializeDataAsync();
    }, []);

    const initializeDataAsync = async () => {
        setLoading(true);
        let paramArticles = params.get("id");
        let articlesArrayloop: IArticleCheckBox[] = [];
        var response = await getAllLearningContent();
        if (response.data) {
            let articlesArray: IArticleCheckBox[] = response.data;
            paramArticles?.split(",").forEach((strparam: String) => {
                let articlesArrayList = articlesArray.find(s => s.rowKey == strparam)!;
                if (articlesArrayList === undefined) { }
                else
                    articlesArrayloop.push(articlesArrayList);
            });
            setarticles(articlesArrayloop);
            setLoading(false);
        }
    }

    const onCancelClick = async () => {
        microsoftTeams.tasks.submitTask({ message: "Cancel", status: true });
        return true;
    }

    const onCloseClick = async () => {
        microsoftTeams.tasks.submitTask({ message: "Close", status: true });
        return true;
    }

    const onSendNotificationClick = () => {
        setShowConfirmation(true);
    }

    const onYesClick = async () => {
        setbuttonLoading(true);
        var response = await sendNotificationAsync(articles, titleText);
        if (response.data) {
            setIsSendSuccessfully(true);
        }
    }

    const onNoClick = () => {
        setIsSendSuccessfully(false);
        setShowConfirmation(false);
    }

    const onTitleTextAdded = (event: any) => {
        setTitleText(event.target.value);
    }

    const items = articles.map(element => {
        return ({
            key: 'index',
            media: (
                <Image
                    src={element.tileImageLink !== "" ? element.tileImageLink : window.location.origin + '/images/Card2.png'  }
                    styles={{ height: "30px", width: "30px" }}
                />
            ),
            header: element.title,
            content: <Text content={element.itemType == ItemType.Articles ? localize("article")
                : element.itemType == ItemType.Video ? localize("video")
                    : element.itemType == ItemType.Image ? localize("image") :
                        localize("searchResult")} />
        }
        );
    })

    const renderMainView = () => {
        return (
            <>
                < Flex gap="gap.small" styles={{ marginLeft: "3rem", marginRight: "3rem" }} column>
                    <Flex column gap="gap.small" >
                        <Flex className="title-box">
                            <Text content={localize("title")} />
                        </Flex>
                        <Flex>
                            <Input onChange={onTitleTextAdded} fluid placeholder={localize("enterATitleForTheNotificationCard")} />
                        </Flex>
                        <Flex>
                            <Text content={localize("notificationPreview")} />
                        </Flex>
                        <Flex className="preview-box">
                            <Flex gap="gap.small" className="preview-box-internal" column >
                                <Text styles={{ marginLeft: "20px", marginTop: "10px" }} weight="bold" content={titleText} />
                                <Box>
                                    <List selectable items={items} />
                                </Box>
                            </Flex>
                        </Flex>
                    </Flex>
                    <Flex gap="gap.small" styles={{ marginTop: "10px" }}  >
                        <FlexItem push>
                            <Button onClick={onCancelClick} content={localize("cancelButton")} secondary />
                        </FlexItem>
                        <Button disabled={titleText == ""} onClick={onSendNotificationClick} content={localize("sendButton")} primary />
                    </Flex>
                </Flex>
            </>
        );
    }

    const renderConfirmationView = () => {
        return (
            <>
                < Flex gap="gap.small" styles={{ marginLeft: "3rem", marginRight: "3rem" }} column>
                    <Flex column gap="gap.small" >
                        <Flex>
                            <Text className="title-box" content={localize("finalPreview")} />
                        </Flex>
                        <Flex className="preview-box">
                            <Flex gap="gap.small" className="preview-box-internal" column >
                                <Text styles={{ marginLeft: "20px", marginTop: "10px" }} weight="bold" content={titleText} />
                                <Box>
                                    <List selectable items={items} />
                                </Box>
                            </Flex>
                        </Flex>
                        <Flex >
                            <Text content={localize("sendNotificationsToAllUsers")} styles={{ marginBottom: "5rem" }} />
                        </Flex>
                    </Flex>
                    <Flex gap="gap.small" >
                        <FlexItem push>
                            <Button onClick={onNoClick} content={localize("no")} disabled={buttonLoading} secondary />
                        </FlexItem>
                        <Button loading={buttonLoading} disabled={buttonLoading} onClick={onYesClick} content={localize("confirmButton")} primary />
                    </Flex>
                </Flex>
            </>
        );
    }

    const renderSuccessView = () => {
        return (
            <>
                < Flex gap="gap.small" styles={{ marginLeft: "3rem", marginRight: "3rem" }} column>
                    <Flex column  >
                        <Flex column gap="gap.medium" hAlign="center" vAlign="center" >
                            <Image className="send-notification-image" src="/images/thank-you.png" />
                            <Text content={localize("notificationSentSuccessfully")} styles={{ marginBottom: "12rem", marginTop: "1rem" }} />
                        </Flex>
                        <Flex gap="gap.small" className="close-button-footer">
                            <FlexItem push>
                                <Button onClick={onCloseClick} content={localize("closeButton")} secondary />
                            </FlexItem>
                        </Flex>
                    </Flex>
                </Flex>
            </>
        );
    }

    return (
        (isSendSuccessfully === false && showConfirmation === false) ? renderMainView()
            : (showConfirmation === true && isSendSuccessfully === false) ? renderConfirmationView()
                : (isSendSuccessfully === true) ? renderSuccessView() : <></>
    );
}
export default withTranslation()(withContext(NotificationPreview));