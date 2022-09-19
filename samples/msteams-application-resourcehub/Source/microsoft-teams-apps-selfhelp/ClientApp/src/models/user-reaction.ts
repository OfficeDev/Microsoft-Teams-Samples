import { CompleteState } from "./complete-state";
import { ItemType } from "./item-type";
import { ReactionState } from "./reaction-state";
import { SelectionType } from "./selection-type";
import { SourceType } from "./source-type";

export default interface IUserReaction {
    reactionId: string;
    learningContentId: string;
    reactionState: ReactionState;
    lastModifiedOn: Date;
    userAadId: string;
    partitionKey: string;
    rowKey: string;
    timestamp: Date;
    eTag: string;
}