export interface ICandidateDetails {
  Attachments: string;
  CandidateName: string;
  Role: string;
  Email: string;
  Experience: string;
  Mobile: string;
  Skills: string;
  Source: string;
  Education: string;
  ResumeUrl: string; 
  LinkedInUrl: string;
  TwitterUrl: string;
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
}