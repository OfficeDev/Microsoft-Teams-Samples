// <copyright file="MeetingTranscriptRecording.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useState, useEffect } from 'react';
import { Button} from '@fluentui/react-components';
import * as msal from "@azure/msal-browser";

const Msal = () => {
    const [meData, setMeData] = useState("");
    const [token, setToken] = useState("");
    let pca = undefined;
    const msalConfig = {
        auth: {
            clientId: "07d429f7-91f9-45ff-ba7e-ac5b26939b45",
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
            const data = await response.json();
            const results = JSON.stringify(data,null,2);
            setMeData(results);
        } else {
            const errorText = await response.text();
            console.error("Microsoft Graph call failed - error text: " + errorText);
        }
    }

    return (
        <div>
            <div className="">
                <Button appearance="primary" onClick={msallogin}>Login</Button>
            </div>
            <br></br>
            <p>{token}</p>
            <br></br>
            <p>
                {meData}
            </p>
        </div>
    );
};
export default Msal;
