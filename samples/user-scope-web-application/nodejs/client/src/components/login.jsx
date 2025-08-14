// <copyright file="login.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from '@fluentui/react-northstar';
import * as msal from '@azure/msal-browser';

const Login = () => {
    const navigate = useNavigate();
    const [token, setToken] = useState("");
    const [processingRequest, setProcessingRequest] = useState("");

    let pca = undefined;

    const msalConfig = {
        auth: {
            clientId: "<<Microsoft-App-Id>>",
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

    const onButtonClick = async () => {
        setProcessingRequest("Processig request...")
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
                sendTokenForUserData(accessToken);
            })
            .catch(function (error) {
                pca.acquireTokenPopup(accessTokenRequest)
                    .then(function (accessTokenResponse) {
                        // Acquire token interactive success
                        let accessToken = accessTokenResponse.accessToken;
                        // Call your API with token
                        setToken(accessToken);
                        sendTokenForUserData(accessToken);
                    })
                    .catch(function (error) {
                        // Acquire token interactive failure
                        console.log("Error---->", error);
                    });
                console.log("Error---->", error);
            });
    }

    async function sendTokenForUserData(accessToken) {
        navigate(`/userScopeTestApp/?token=${encodeURIComponent(accessToken)}`);
    }

    return <div className={"mainContainer"}>
        <div className={"titleContainer"}>
            <div>Login</div>
            <br></br>
        </div>

        <div className={"inputContainer"}>
            <Button content="Login" primary onClick={onButtonClick} />
        </div>
        <br></br>
        <div>
            <h4>{processingRequest}</h4>
        </div>
    </div>
}

export default Login