// <copyright file="auth-start.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import * as msal from "@azure/msal-browser";

const AuthStart = props => {

    useEffect(() => {

        async function AuthenticationStart() {

            const context = await microsoftTeams.app.getContext();
            var scope = "User.Read email openid profile offline_access";
            var loginHint = context.user.loginHint;

            const msalConfig = {
                auth: {
                    clientId: process.env.REACT_APP_MICROSOFT_APP_ID,
                    authority: `https://login.microsoftonline.com/${context.user.tenant.id}`,
                    navigateToLoginRequestUrl: false
                },
                cache: {
                    cacheLocation: "sessionStorage",
                },
            };

            // Initializing the PublicClientApplication object
            // In order to use MSAL.js, you need to instantiate a PublicClientApplication object. You must provide the client id (appId) of your application.
            const msalInstance = new msal.PublicClientApplication(msalConfig);

            const scopesArray = scope.split(" ");
            const scopesRequest = {
                scopes: scopesArray,
                redirectUri: window.location.origin + `/auth-end`,
                loginHint: loginHint
            };

            await msalInstance.loginRedirect(scopesRequest);
        }

        AuthenticationStart();

    }, []);
};

export default AuthStart;