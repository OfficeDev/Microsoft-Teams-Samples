import { ItemType } from "./item-type";

export default interface ITelemetry {
    articleTitle: string;
    totalLikeCount: number;
    totalDislikeCount: number;
    itemType: ItemType;
}