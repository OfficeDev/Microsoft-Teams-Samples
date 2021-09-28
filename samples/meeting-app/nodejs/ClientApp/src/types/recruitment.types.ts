export interface IQuestionDetails {
    ETag: string,
    IsDelete: number,
    MeetingId: string,
    PartitionKey: string,
    Question: string,
    RowKey: string,
    SetBy: string,
    Timestamp: string,
    Rating?: number,
    Comment?: string,
    CommentedBy?: string,
    ShowAddComment?: boolean
}

export interface INoteDetails {
  CandidateEmail: string,
  AddedBy: string,
  AddedByName?: string,
  Note: string,
  Timestamp?: string
}

// Interface for feedback submiited for questions.
export interface IFeedbackDetails {
    MeetingId: string,
    CandidateEmail: string,
    FeedbackJson: string,
    Interviewer: string
  }