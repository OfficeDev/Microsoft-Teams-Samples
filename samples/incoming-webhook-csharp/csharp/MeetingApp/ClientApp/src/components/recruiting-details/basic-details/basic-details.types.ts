export interface ICandidateDetails {
  attachments: string;
  candidateName: string;
  role: string;
  email: string;
  experience: string;
  mobile: string;
  skills: string;
  source: string;
  education: string;
  resumeUrl: string;
  linkedInUrl: string;
  twitterUrl: string;
}

export interface IQuestionSet {
  meetingId: string;
  questionId?: string,
  question: string;
  setBy: string;
  isDelete: number;
}

export interface IAssetDetails {
  message: string;
  sharedBy: string;
  meetingId?: string;
  files: Array<string>;
}