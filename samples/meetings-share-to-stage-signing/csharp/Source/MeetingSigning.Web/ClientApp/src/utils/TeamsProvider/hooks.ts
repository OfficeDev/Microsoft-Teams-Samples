import { ThemeInput } from '@fluentui/react-northstar';
import * as microsoftTeams from '@microsoft/teams-js';
import { useContext, useMemo } from 'react';
import { TeamsContext, getTheme } from './TeamsProvider';

export function useTeamsContext(): microsoftTeams.app.Context {
  const providerContext = useContext(TeamsContext);
  // trigger context refresh?
  // providerContext.microsoftTeams.getContext(providerContext.setContext);
  return providerContext.context;
}

export function useAADId(): string {
  const ctx = useTeamsContext();
  return ctx?.user?.id ?? '';
}

export function useTheme(): ThemeInput {
  const ctx = useTeamsContext();
  return useMemo(() => getTheme(ctx.app.theme), [ctx.app.theme]);
}

export function useUserIsAnonymous(): boolean {
  const ctx = useTeamsContext();
  return ctx?.user?.licenseType === 'Anonymous' ?? false;
}
