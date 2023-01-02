// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import * as msal from "@azure/msal-browser";

/**
 * This component is used to redirect the user to the Azure authorization endpoint from a popup
 */
class End extends React.Component {

    componentDidMount() {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.getContext().then(async (context) => {
                const msalConfig = {
                    auth: {
                        clientId: process.env.REACT_APP_MICROSOFT_APP_ID,
                        authority: `https://login.microsoftonline.com/${context.tid}`,
                        navigateToLoginRequestUrl: false
                    },
                    cache: {
                        cacheLocation: "sessionStorage",
                    },
                };
				
                const msalInstance = new msal.PublicClientApplication(msalConfig);
				
                msalInstance.handleRedirectPromise()
                    .then((tokenResponse) => {
                        alert(JSON.stringify(tokenResponse));
                        if (tokenResponse !== null) {
                            microsoftTeams.authentication.notifySuccess("Authentication succedded");
                        } else {
                            microsoftTeams.authentication.notifyFailure("Get empty response.");
                        }
                    })
                    .catch((error) => {
                        microsoftTeams.authentication.notifyFailure(JSON.stringify(error));
                    });
            });
        });
    }     

    render() {
      return (
        <div>
            <h1>Consent flow complete.</h1>
        </div>
      );
    }
}

export default End;