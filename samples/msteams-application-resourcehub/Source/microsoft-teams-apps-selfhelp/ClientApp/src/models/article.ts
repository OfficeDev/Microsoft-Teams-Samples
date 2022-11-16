import { ItemType } from "./item-type";
import { SelectionType } from "./selection-type";
import { SourceType } from "./source-type";

export default interface IArticle {
    learningId: string;
    sectionType: SelectionType;
    title: string;
    itemType: ItemType;
    source: SourceType;
    primaryTag: string;
    secondaryTag: string;
    itemlink: string;
    knowmoreLink: string;
    tileImageLink: string;
    createdOn: Date;
    createdBy: string;
    partitionKey: string;
    rowKey: string;
    eTag: string;
    timestamp: Date;
    description: string;
    length: string;
}