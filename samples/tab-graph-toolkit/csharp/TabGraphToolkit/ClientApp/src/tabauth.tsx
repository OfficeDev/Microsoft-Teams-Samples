import * as microsoftTeams from "@microsoft/teams-js";
import { TeamsProvider } from '@microsoft/mgt';
import { TeamsMsal2Provider } from '@microsoft/mgt-teams-msal2-provider';

import React from "react";

class GraphSignIn extends React.Component {
    
    componentDidMount(){
        //TeamsProvider.microsoftTeamsLib = microsoftTeams;
        //TeamsProvider.handleAuth();// This is where error occured
        TeamsMsal2Provider.handleAuth();
    }

    render(){
        return(
            <div id="auth">
            </div>
        )
    }
}


export default GraphSignIn;