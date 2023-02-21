// <copyright file="auth-end.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import * as msal from "@azure/msal-browser";

const AuthEnd = props => {

    useEffect(() => {

        async function AuthenticationEnd() {

            const context = await microsoftTeams.app.getContext();

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

            // Initializing the PublicClientApplication object
            // In order to use MSAL.js, you need to instantiate a PublicClientApplication object. You must provide the client id (appId) of your application.
            const msalInstance = new msal.PublicClientApplication(msalConfig);

            msalInstance.handleRedirectPromise()
                .then((tokenResponse) => {
                    if (tokenResponse !== null) {
                        microsoftTeams.authentication.notifySuccess("Authentication succeeded.");
                    } else {
                        microsoftTeams.authentication.notifyFailure("Get empty response.");
                    }
                })
                .catch((error) => {
                    microsoftTeams.authentication.notifyFailure(JSON.stringify(error));
                });
        }

        AuthenticationEnd();
              
    }, []);
};

export default AuthEnd;