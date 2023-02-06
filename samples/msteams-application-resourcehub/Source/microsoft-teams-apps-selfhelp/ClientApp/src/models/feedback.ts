import { CompleteState } from "./complete-state";
import { FeedbackHelpfulState } from "./feedback-helpful-status";
import { FeedbackType } from "./feedback-type";

export default interface IFeedback {
    feedbackId: string;
    feedbackType: FeedbackType;
    learningContentId: string;
    helpfulStatus: FeedbackHelpfulState;
    isHelpful: boolean;
    rating: number;
    feedback: string;
    createdOn: Date;
    createdBy: string;
    partitionKey: string;
    rowKey: string;
    eTag: string;
    timestamp: Date;
}