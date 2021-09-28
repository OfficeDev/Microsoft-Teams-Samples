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
  MeetingId: string;
  QuestionId?: string,
  Question: string;
  SetBy: string;
  IsDelete: number;
}

export interface IAssetDetails {
 message: string;
}