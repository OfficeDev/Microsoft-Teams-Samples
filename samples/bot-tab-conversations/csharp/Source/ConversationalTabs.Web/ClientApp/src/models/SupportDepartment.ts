import { CustomerInquiry } from "./CustomerInquiry";
import { MsTeamsBotData } from "./MsTeamsBotData";

export type SupportDepartment = {
  supportDepartmentId: string;
  title: string;
  description: string;
  teamChannelId: string;
  tenantId: string;
  proactiveBotData: MsTeamsBotData;
  subEntities: CustomerInquiry[];
};

export type SupportDepartmentInput = {
  title: string;
  description: string;
  teamChannelId: string;
  teamId: string;
  groupId: string;
  tenantId: string;
};
