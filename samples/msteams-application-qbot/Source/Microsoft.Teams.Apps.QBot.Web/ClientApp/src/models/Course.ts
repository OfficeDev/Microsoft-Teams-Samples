export interface Course {
  id: string;
  name?: string;
  teamId: string;
  teamAadObjectId: string;
  hasMultipleTutorialGroups: boolean;
  defaultTutorialGroupId: string;
  iconUrl: string;
  displayName: string;
  knowledgeBaseId?: string;
}
