import { User } from '.';

// This is a Typescript representation of a C# class https://msteams-captain.visualstudio.com/xGrowth%20App%20Templates/_git/msteams-poc-meetingsigning?path=Source/MeetingSigning.Domain/Models/Signature.cs
export type Signature = {
  id: string;
  signer: User;
  signedDateTime: Date;
  text?: string;
  isSigned: boolean;
};
