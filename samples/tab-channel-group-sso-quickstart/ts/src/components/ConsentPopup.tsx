// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import * as msal from "@azure/msal-browser";

/**
 * This component is loaded to grant consent for graph permissions.
 */
class ConsentPopup extends React.Component {

    componentDidMount() {

        microsoftTeams.app.initialize().then(() => {
            // Get the tab context, and use the information to navigate to Azure AD login page
            microsoftTeams.app.getContext().then(async (context) => {
                var scope = "User.Read email openid profile offline_access Chat.Read Chat.ReadWrite ChatMessage.Send";
                var loginHint = context.user!.loginHint;
				
                const msalConfig: msal.Configuration = {
                    auth: {
                        clientId: process.env.REACT_APP_AZURE_APP_REGISTRATION_ID!,
                        authority: `https://login.microsoftonline.com/${context.user!.tenant!.id}`,
                        navigateToLoginRequestUrl: false
                    },
                    cache: {
                        cacheLocation: "sessionStorage",
                    },
                };
				
                const msalInstance = new msal.PublicClientApplication(msalConfig);
				
                const scopesArray = scope.split(" ");
                const scopesRequest = {
                    scopes: scopesArray,
                    redirectUri: window.location.origin + `/auth-end`,
                    loginHint: loginHint
                };
				
                await msalInstance.loginRedirect(scopesRequest);
            });
        });
    }

    render() {
      return (
        <div>
          <h1>Redirecting to consent page.</h1>
        </div>
      );
    }
}

export default ConsentPopup;