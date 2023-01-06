import { ItemType } from "./item-type";
import { SelectionType } from "./selection-type";
import { SourceType } from "./source-type";

export default interface IArticleCheckBox {
    partitionKey: string;
    rowKey: string;
    timestamp: Date;
    eTag: string;
    learningId: string;
    sectionType: SelectionType;
    title: string;
    description: string;
    itemType: ItemType;
    source: SourceType;
    primaryTag: string;
    secondaryTag: string;
    itemlink: string;
    knowmoreLink: string;
    length: string;
    tileImageLink: string;
    createdOn: Date;
    createdBy: string;
    createdByUserName: string;
    isChecked:boolean;
}