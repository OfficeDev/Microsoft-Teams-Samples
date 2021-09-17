export interface ICandidateDetails {
  attachments: string;
  candidateName: string;
  role: string;
  email: string;
  experience: string;
  mobile: string;
  skills: string;
  source: string;
}

export interface IQuestionSet {
  meetingId: string;
  questionId?: string,
  question: string;
  setBy: string;
  isDelete: number;
}