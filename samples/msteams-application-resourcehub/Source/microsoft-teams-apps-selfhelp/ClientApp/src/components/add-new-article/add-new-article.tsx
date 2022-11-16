import React from 'react'
import { FormDropdown, Input, Flex, FlexItem, Button, DropdownItemProps, Text } from '@fluentui/react-northstar'
import { TFunction, WithTranslation, withTranslation } from 'react-i18next';
import withContext, { IWithContext } from '../../providers/context-provider';
import * as microsoftTeams from "@microsoft/teams-js";
import './add-new-article.css'
import IArticle from '../../models/article'
import { ItemType } from '../../models/item-type';
import { SelectionType } from '../../models/selection-type';
import { SourceType } from '../../models/source-type';
import { createLearningContent } from '../../api/article-api';

const labelId = 'choose-friend-label'

interface IFeedbackState {
    loading: boolean;
};

interface IDropdownProps {
    header: string;
    id: number;
}

interface IAddNewTaskmoduleProps extends WithTranslation, IWithContext {

}

const AddNewTaskmodule: React.FunctionComponent<IAddNewTaskmoduleProps> = props => {
    const localize: TFunction = props.t;

    const [isArticleView, setIsArticleView] = React.useState(false);

    const [title, setTitle] = React.useState("");
    const [isTitleValid, setTitleValid] = React.useState(false);

    const [isDisableButton, setDisableButton] = React.useState(true);

    const [primaryTag, setPrimaryTag] = React.useState("");
    const [isPrimaryTagValid, setPrimaryTagValid] = React.useState(false);

    const [secondaryTag, setSecondaryTag] = React.useState("");
    const [isSecondaryTagValid, setSecondaryTagValid] = React.useState(false);

    const [itemLink, setItemLink] = React.useState("");
    const [isItemLinkValid, setItemLinkValid] = React.useState(false);

    const [description, setDescription] = React.useState("");

    const [knowMoreLink, setKnowMoreLink] = React.useState("");

    const [length, setLength] = React.useState("");

    const [tileImageLink, setTileImageLink] = React.useState("");

    const [isItemTypeValid, setItemTypeValid] = React.useState(false);
    const [itemTypeItems, setItemTypeItems] = React.useState<IDropdownProps[]>([]);
    const [selectedItemType, setSelectedItemType] = React.useState<IDropdownProps>({ header: "", id: 0 });

    const [isSectionValid, setSectionValid] = React.useState(false);
    const [selectionTypeItems, setSelectionTypeItems] = React.useState<IDropdownProps[]>([]);
    const [selectedSelectionType, setSelectedSelectionType] = React.useState<IDropdownProps>({ header: "", id: 0 });

    const [isSourceValid, setSourceValid] = React.useState(false);
    const [sourceType, setSourceType] = React.useState<IDropdownProps[]>([]);
    const [selectedSource, setSelectedSource] = React.useState<IDropdownProps>({ header: "", id: 0 });

    /**
     * Checks if all inputs are valid
     * @returns Returns true if all inputs are valid, else false
     */
    const validateInputs = () => {
        var isValidate = true;
        setDisableButton(false);
        if (title.trim() === "") {
            setTitleValid(false);
            setDisableButton(true);
            isValidate = false;
        }

        if (itemLink.trim() === "") {
            setItemLinkValid(false);
            setDisableButton(true);
            isValidate = false;
        }

        if (selectedItemType.header.trim() === "") {
            setItemTypeValid(false);
            setDisableButton(true);
            isValidate = false;
        }

        if (selectedSelectionType.header.trim() === "") {
            setSectionValid(false);
            setDisableButton(true);
            isValidate = false;
        }

        if (selectedSource.header.trim() === "") {
            setSourceValid(false);
            setDisableButton(true);
            isValidate = false;
        }

        if (primaryTag.trim() === "") {
            setPrimaryTagValid(false);
            setDisableButton(true);
            isValidate = false;
        }

        if (secondaryTag.trim() === "") {
            setSecondaryTagValid(false);
            setDisableButton(true);
            isValidate = false;
        }

        return isValidate;
    };

    const onTitleChange = (event: any) => {

        if (event.target.value.trim() == "") {
            setTitleValid(false);
            setDisableButton(true);
        }
        else {
            setTitleValid(true);
            setDisableButton(false);
        }
        setTitle(event.target.value);
        validateInputs();
    };

    const onPrimaryTagChange = (event: any) => {

        if (event.target.value.trim() == "") {
            setPrimaryTagValid(false);
            setDisableButton(true);
        }
        else {
            setPrimaryTagValid(true);
            setDisableButton(false);
        }
        setPrimaryTag(event.target.value);
        validateInputs();
    };

    const onSecondaryTagChange = (event: any) => {

        if (event.target.value.trim() == "") {
            setSecondaryTagValid(false);
            setDisableButton(true);
        }
        else {
            setSecondaryTagValid(true);
            setDisableButton(false);
        }
        setSecondaryTag(event.target.value);
        validateInputs();
    };

    const onItemLinkChange = (event: any) => {

        if (event.target.value.trim() == "") {
            setItemLinkValid(false);
            setDisableButton(true);
        }
        else {
            setItemLinkValid(true);
            setDisableButton(false);
        }
        setItemLink(event.target.value);
        validateInputs();
    };

    const onDescriptionChange = (event: any) => {
        setDescription(event.target.value);
        validateInputs();
    };

    const onKnowMoreLinkChange = (event: any) => {
        setKnowMoreLink(event.target.value);
        validateInputs();
    };

    const onLengthChange = (event: any) => {
        setLength(event.target.value);
        validateInputs();
    };

    const onTileImageLinkChange = (event: any) => {
        setTileImageLink(event.target.value);
        validateInputs();
    };

    const onSectionChange = (event, data) => {
        let selecteditem = selectionTypeItems.find((item, index) => index === data.value.id);

        if (selecteditem) {
            setSelectedSelectionType(selecteditem);
        }

        if (selecteditem?.header.trim() === "") {
            setSectionValid(false);
            setDisableButton(true);
        }
        else {
            setSectionValid(true);
            setDisableButton(false);
        }
        validateInputs();
    };

    const onItemTypeChange = (event, data) => {
        let selecteditem = itemTypeItems.find((item, index) => index === data.value.id);

        if (selecteditem) {
            setSelectedItemType(selecteditem);

            if (selecteditem.id == 0) {
                setIsArticleView(false);
            }
            else {
                setIsArticleView(true);
            }
        }

        if (selecteditem?.header.trim() === "") {
            setItemTypeValid(false);
            setDisableButton(true);
        }
        else {
            setItemTypeValid(true);
            setDisableButton(false);
        }

        if (selecteditem?.id == 0) {
            setIsArticleView(false);
        }
        else {
            setIsArticleView(true);
        }

        validateInputs();
    };

    const onSourceChange = (event, data) => {
        let selecteditem = sourceType.find((item, index) => index === data.value.id);

        if (selecteditem) {
            setSelectedSource(selecteditem);
        }

        if (selecteditem?.header.trim() === "") {
            setSourceValid(false);
            setDisableButton(true);
        }
        else {
            setSourceValid(true);
            setDisableButton(false);
        }
        validateInputs();
    };

    React.useEffect(() => {
        microsoftTeams.initialize();
        let sourceListArray: IDropdownProps[] = [];
        sourceListArray.push({
            id: SourceType.Internal,
            header: "Internal",
        })
        sourceListArray.push({
            id: SourceType.External,
            header: "External",
        })
        setSourceType(sourceListArray);

        let itemTypeListArray: IDropdownProps[] = [];
        itemTypeListArray.push({
            id: ItemType.Video,
            header: "Video",
        });
        itemTypeListArray.push({
            id: ItemType.Articles,
            header: "Article",
        });
        setItemTypeItems(itemTypeListArray);

        let selectionTypeListArray: IDropdownProps[] = [];
        selectionTypeListArray.push({
            id: SelectionType.GettingStarted,
            header: localize("gettingStartedText"),
        });
        selectionTypeListArray.push({
            id: SelectionType.Scenarios,
            header: localize("scenarioText"),
        });
        selectionTypeListArray.push({
            id: SelectionType.TrendingNow,
            header: localize("trendingTopicText"),
        });
        selectionTypeListArray.push({
            id: SelectionType.LearningPath,
            header: localize("learningPathText"),
        });
        setSelectionTypeItems(selectionTypeListArray);
    }, []);

    const OnSaveClick = async () => {
        setDisableButton(true);

        if (validateInputs()) {
            var articleToAdd: IArticle = {
                createdBy: props.teamsContext?.userObjectId!,
                createdOn: new Date(),
                eTag: "",
                itemlink: itemLink,
                itemType: selectedItemType.id,
                knowmoreLink: knowMoreLink,
                primaryTag: primaryTag,
                secondaryTag: secondaryTag,
                tileImageLink: tileImageLink,
                title: title,
                sectionType: selectedSelectionType.id,
                source: selectedSource.id,
                learningId: '',
                partitionKey: '',
                rowKey: '',
                timestamp: new Date(),
                description: description,
                length: length,
            };

            var response = await createLearningContent(articleToAdd);
            const confirmMessage = {
                confirm: true,
            }
            microsoftTeams.tasks.submitTask(JSON.stringify(confirmMessage));
            return true;
        }
    }

    const cancelPage = () => {
        const confirmMessage = {
            confirm: false,
        }
        microsoftTeams.tasks.submitTask(JSON.stringify(confirmMessage));
        return true;
    }

    return (
        <Flex column gap="gap.small" className="container">
            <Flex gap="gap.small" space="between">
                <FormDropdown
                    fluid
                    value={selectedSelectionType.header === "" ? undefined : selectedSelectionType}
                    className={isSectionValid ? "" : "error-border-dropdown"}
                    noResultsMessage={localize("noResultMessage")}
                    onChange={onSectionChange}
                    label={{
                        content: localize("section"),
                        id: labelId,
                    }}
                    items={selectionTypeItems}
                    aria-labelledby={labelId}

                    placeholder={localize("chooseASection")}
                />
                <FormDropdown
                    fluid
                    value={selectedItemType.header === "" ? undefined : selectedItemType}
                    className={isItemTypeValid ? "" : "error-border-dropdown"}
                    noResultsMessage={localize("noResultMessage")}
                    onChange={onItemTypeChange}

                    label={{
                        content: localize("itemType"),
                        id: labelId,
                    }}
                    items={itemTypeItems}
                    aria-labelledby={labelId}
                    placeholder={localize("chooseAType")}
                />
                <FormDropdown
                    fluid
                    value={selectedSource.header === "" ? undefined : selectedSource}
                    className={isSourceValid ? "" : "error-border-dropdown"}
                    noResultsMessage={localize("noResultMessage")}
                    onChange={onSourceChange}
                    label={{
                        content: localize("source"),
                        id: labelId,
                    }}
                    items={sourceType}
                    aria-labelledby={labelId}
                    placeholder={localize("chooseASource")}
                />
            </Flex>
            <Input fluid onChange={onTitleChange}
                value={title}
                label={localize("title")} required showSuccessIndicator={false} />

            <Text className="primary_tag" color="grey" content={localize("maxTen")} size="smaller" temporary timestamp />
            <Input fluid onChange={onPrimaryTagChange}
                value={primaryTag}
                label={localize("primaryTag")} maxLength={10} required showSuccessIndicator={false} />

            <Text className="secondary_tag" color="grey" content={localize("maxTen")} size="smaller" temporary timestamp />
            <Input fluid onChange={onSecondaryTagChange}
                value={secondaryTag}
                label={localize("secondaryTag")} maxLength={10} required showSuccessIndicator={false} />

            <Input fluid onChange={onItemLinkChange}
                value={itemLink}
                label={localize("itemLink")} required showSuccessIndicator={false} />

            <Input fluid onChange={onDescriptionChange}
                value={description}
                label={localize("description")} showSuccessIndicator={false} />

            <Input fluid onChange={onKnowMoreLinkChange}
                value={knowMoreLink}
                label={localize("knowMoreLink")} showSuccessIndicator={false} />
            <Input fluid onChange={onLengthChange}
                value={length}
                label={localize("Length")} showSuccessIndicator={false} />

            <Input fluid onChange={onTileImageLinkChange}
                value={tileImageLink}
                label={localize("thumbnailLink")} showSuccessIndicator={false} />
            <Flex gap="gap.small">
                <FlexItem push>
                    <Button content={localize("cancel")} onClick={cancelPage} />
                </FlexItem>
                <Button onClick={OnSaveClick} content={localize("add")} primary loader={!isDisableButton} disabled={isDisableButton} />
            </Flex>
        </Flex>
    )
}
export default withTranslation()(withContext(AddNewTaskmodule));