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