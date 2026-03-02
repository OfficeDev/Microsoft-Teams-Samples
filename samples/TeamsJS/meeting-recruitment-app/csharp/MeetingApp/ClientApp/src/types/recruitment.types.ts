export interface IQuestionDetails {
    eTag: string,
    isDelete: number,
    meetingId: string,
    partitionKey: string,
    question: string,
    rowKey: string,
    setBy: string,
    timestamp: string,
    rating?: number,
    comment?: string,
    commentedBy?: string,
    showAddComment?: boolean
}

export interface INoteDetails {
  candidateEmail: string,
  addedBy: string,
  addedByName?: string,
  note: string,
  timestamp?: string,
  meetingId?: string
}

// Interface for feedback submiited for questions.
export interface IFeedbackDetails {
    meetingId: string,
    candidateEmail: string,
    feedbackJson: string,
    interviewer: string
  }