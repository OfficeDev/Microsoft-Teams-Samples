import { useContext } from "react";
import { TeamsFxContext } from "./Context";
import React from "react";
import { Agenda, Person, applyTheme,People,PeoplePicker,Tasks,Todo, PersonCard, ViewType } from "@microsoft/mgt-react";
import { Button } from "@fluentui/react-components";

import { Providers, ProviderState } from "@microsoft/mgt-react";
import { TeamsFxProvider } from "@microsoft/mgt-teamsfx-provider";
import {
  TeamsUserCredential,
  TeamsUserCredentialAuthConfig,
} from "@microsoft/teamsfx";

const authConfig: TeamsUserCredentialAuthConfig = {
  clientId: process.env.REACT_APP_CLIENT_ID!,
  initiateLoginEndpoint: process.env.REACT_APP_START_LOGIN_PAGE_URL!,
};

const scopes = [
  'Bookmark.Read.All',
    'Calendars.Read',
    'Channel.ReadBasic.All',
    'Group.ReadWrite.All',
    'People.Read',
    'Presence.Read.All',
    'User.Read',
    'Tasks.ReadWrite',
    'Team.ReadBasic.All',
    'User.Read.All'
];
const credential = new TeamsUserCredential(authConfig);
const provider = new TeamsFxProvider(credential, scopes);
Providers.globalProvider = provider;

export default function Tab() {
  const { themeString } = useContext(TeamsFxContext);
  const [loading, setLoading] = React.useState<boolean>(false);
  const [consentNeeded, setConsentNeeded] = React.useState<boolean>(false);

  const [activeDiv, setActiveDiv] = React.useState(null);

  React.useEffect(() => {
    const init = async () => {
      try {
        await credential.getToken(scopes);
        Providers.globalProvider.setState(ProviderState.SignedIn);
      } catch (error) {
        setConsentNeeded(true);
      }
    };

    init();
  }, []);

  const consent = async () => {
    setLoading(true);
    await credential.login(scopes);
    Providers.globalProvider.setState(ProviderState.SignedIn);
    setLoading(false);
    setConsentNeeded(false);
  };

  const toggleVisibility = (divName:any) => {
    setActiveDiv(divName === activeDiv ? null : divName);
  };

  React.useEffect(() => {
    applyTheme(themeString === "default" ? "light" : "dark");
  }, [themeString]);

  return (
    <div>
      {consentNeeded && (
        <>
        <div className="loginbtn">
          <Button appearance="primary" disabled={loading} onClick={consent}>
            Login
          </Button>
        </div>
        </>
      )}
      {!consentNeeded && (
        <>
        <div className="mgtDetails">
          <Person personQuery="me" view={ViewType.oneline} />
          <div>
            <h3 className="cursorPointer" onClick={() => toggleVisibility('Agenda')}><i className="arrow right"></i>Agenda</h3> 
            <div style={{ display: activeDiv === 'Agenda' ? 'block' : 'none' }}> 
            <Agenda></Agenda> 
            </div>
          </div>

          <div>
            <h3 className="cursorPointer" onClick={() => toggleVisibility('PeoplePicker')}><i className="arrow right"></i>PeoplePicker</h3> 
            <div style={{ display: activeDiv === 'PeoplePicker' ? 'block' : 'none' }}> 
            <PeoplePicker></PeoplePicker>
          </div>
          </div>

          <div>
            <h3 className="cursorPointer" onClick={() => toggleVisibility('Todo')}><i className="arrow right"></i>ToDo</h3> 
            <div style={{ display: activeDiv === 'Todo' ? 'block' : 'none' }}>
              <Todo></Todo> 
            </div>
          </div>
            
          <div>
            <h3 className="cursorPointer" onClick={() => toggleVisibility('Person')}><i className="arrow right"></i>Person Card</h3> 
            <div style={{ display: activeDiv === 'Person' ? 'block' : 'none' }}>
            <Person personQuery="me" view={ViewType.fourlines} /> 
            </div>
          </div>

          <div>
            <h3 className="cursorPointer" onClick={() => toggleVisibility('People')}><i className="arrow right"></i>Person</h3> 
            <div style={{ display: activeDiv === 'People' ? 'block' : 'none' }}>
            <People></People>
            </div>
          </div>

          <div>
            <h3 className="cursorPointer" onClick={() => toggleVisibility('Tasks')}><i className="arrow right"></i>Tasks</h3> 
            <div style={{ display: activeDiv === 'Tasks' ? 'block' : 'none' }}>
            <Tasks></Tasks>
            </div>
          </div>
        </div>
        </>
      )}
    </div>
  );
}