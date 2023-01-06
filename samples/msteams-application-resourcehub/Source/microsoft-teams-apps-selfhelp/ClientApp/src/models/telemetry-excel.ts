import { ItemType } from "./item-type";
import { ReactionState } from "./reaction-state";

export default interface ITelemetryExcel {
    articleTitle: string;
    totalViewCount: number;
    totalLikeCount: number;
    totalDislikeCount: number;
    itemType: ItemType;
    shareArticleToUser: number;
    shareArticleToChannel: number;
}