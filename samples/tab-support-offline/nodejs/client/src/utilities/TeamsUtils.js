import * as microsoftTeams from '@microsoft/teams-js';


let teamsInitPromise;
export function ensureTeamsSdkInitialized() {
  if (!teamsInitPromise) {
      teamsInitPromise = microsoftTeams.app.initialize();
  }
  return teamsInitPromise;
}

// async function returns true if we're running in Teams
export async function inTeams() {
    try {
        await ensureTeamsSdkInitialized();
        const context = await microsoftTeams.app.getContext();
        return (context.app.host.name === microsoftTeams.HostName.teams);
    }
    catch (e) {
        console.log(`${e} from Teams SDK, may be running outside of Teams`);    
        return false;
    }
}

// async function returns true if we're running in Teams
export async function getTeamsContext() {
    try {
        await ensureTeamsSdkInitialized();
        const context = await microsoftTeams.app.getContext();
        return context;
    }
    catch (e) {
        console.log(`${e} from Teams SDK, may be running outside of Teams`);    
        return false;
    }
}