import { CompleteState } from "./complete-state";
import { ItemType } from "./item-type";
import { SelectionType } from "./selection-type";
import { SourceType } from "./source-type";

export default interface ILearningPath {
    learningPathId: string;
    learningContentId: string;
    completeState: CompleteState;
    userAadId: string | undefined;
    lastModifiedOn: Date;
    eTag:string;
    timestamp:Date;
    partitionKey:string;
    rowKey:String;
}