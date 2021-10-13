import { TeamsMsal2Provider } from '@microsoft/mgt-teams-msal2-provider';
import React from "react";

class GraphSignIn extends React.Component {
    
    componentDidMount(){
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