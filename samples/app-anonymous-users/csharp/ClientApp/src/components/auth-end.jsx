// <copyright file="auth-end.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import * as msal from "@azure/msal-browser";

const AuthEnd = props => {
    
    useEffect(() => {
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
    }, []);
};

export default AuthEnd;