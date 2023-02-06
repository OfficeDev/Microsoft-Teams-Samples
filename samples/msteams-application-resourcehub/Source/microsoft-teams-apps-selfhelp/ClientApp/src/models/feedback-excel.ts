import { FeedbackHelpfulState } from "./feedback-helpful-status";
import { FeedbackType } from "./feedback-type";

export default interface IFeedbackExcel {
    feedbackType: FeedbackType;
    articleTitle: string;
    helpfulStatus: FeedbackHelpfulState;
    isHelpful: boolean;
    rating: number;
    feedback: string;
    submittedOn: Date;
    submittedBy: string;
}