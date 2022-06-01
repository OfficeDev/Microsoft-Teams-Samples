import { ThemeInput } from '@fluentui/react-northstar';
import { Context } from '@microsoft/teams-js';
import { useContext, useMemo } from 'react';
import { TeamsContext, getTheme } from './TeamsProvider';

export function useTeamsContext(): Context {
  const providerContext = useContext(TeamsContext);
  // trigger context refresh?
  // providerContext.microsoftTeams.getContext(providerContext.setContext);
  return providerContext.context;
}

export function useAADId(): string {
  const ctx = useTeamsContext();
  return ctx.userObjectId ? ctx.userObjectId : '';
}

export function useTheme(): ThemeInput {
  const ctx = useTeamsContext();
  return useMemo(() => getTheme(ctx.theme), [ctx.theme]);
}
