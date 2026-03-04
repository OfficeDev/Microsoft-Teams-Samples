// Type for candidate details
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

// Type for question details
export interface IQuestionSet {
  MeetingId: string;
  QuestionId?: string,
  Question: string;
  SetBy: string;
  IsDelete: number;
}

// Type for asset details
export interface IAssetDetails {
 message: string;
 files: Array<string>;
}