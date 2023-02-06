import { useContext } from "react";
import { Context } from '@microsoft/teams-js';
import { TeamsContext } from "utils/TeamsProvider";

export function useTeamsContext(): Context {
  const providerContext = useContext(TeamsContext);

  return providerContext.context;
}
