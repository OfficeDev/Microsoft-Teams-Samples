// <copyright file="MeetingTranscriptRecording.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useState, useEffect } from 'react';
import { Button} from '@fluentui/react-components';
import * as msal from "@azure/msal-browser";

const Msal = () => {

    const [meData, setMeData] = useState(null);
    const [token, setToken] = useState();
    const [isLoggedIn, setIsLoggedIn] = useState(false);

    let pca = undefined;

    const msalConfig = {
        auth: {
            clientId: "<clientId>",
            authority: "https://login.microsoftonline.com/common",
            supportsNestedAppAuth: true // Enable native bridging.
        }
    };

    useEffect(() => {
        async function fetchMyAPI() {
            pca = await msal.PublicClientNext.createPublicClientApplication(msalConfig);
        }
        fetchMyAPI()
      }, [])


    // Tab sso authentication.
    const msallogin = () => {
       const account = pca.getActiveAccount();

        const accessTokenRequest = {
            scopes: ["user.read"],
            account: account,
        };

        pca.acquireTokenPopup(accessTokenRequest)
        .then(function (accessTokenResponse) {
            // Acquire token silent success
            let accessToken = accessTokenResponse.accessToken;
            setToken("Token - " + accessToken);
            // Call your API with token
            callApi(accessToken);
        })
        .catch(function (error) {
            //Acquire token silent failure, and send an interactive request
            if (error instanceof InteractionRequiredAuthError) {
            publicClientApplication
                .acquireTokenPopup(accessTokenRequest)
                .then(function (accessTokenResponse) {
                // Acquire token interactive success
                let accessToken = accessTokenResponse.accessToken;
                // Call your API with token
                callApi(accessToken);
                setToken(accessToken);
                })
                .catch(function (error) {
                // Acquire token interactive failure
                console.log(error);
                });
            }
            console.log(error);
            });
    }

    async function callApi(accessToken)
    {
        // Call the Microsoft Graph API with the access token.
         const response = await fetch(
            `https://graph.microsoft.com/v1.0/me`,
            {
            headers: { Authorization: accessToken },
            }
         );
        
        if (response.ok) {
            // Write file names to the console.
            const jsonValue = await response.json();
            const alignedJSON = JSON.stringify(jsonValue, null, 2);
            setMeData(alignedJSON);
            setIsLoggedIn(true);
        } else {
            const errorText = await response.text();
            console.error("Microsoft Graph call failed - error text: " + errorText);
        }
    }

    return (
        <div>
            <div className="">
            {!isLoggedIn && <Button appearance="primary" onClick={msallogin}>Login</Button>}
            </div>
            {isLoggedIn && <h1 style={{ color: 'black' }}>Welcome! You are login information.</h1>}
            <div>
              <pre style={{ whiteSpace: 'pre-wrap', color: 'black'}}>{meData}</pre>
            </div>
        </div>
    );
};
export default Msal;
