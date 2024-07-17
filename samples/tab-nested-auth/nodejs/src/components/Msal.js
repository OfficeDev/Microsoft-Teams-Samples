// <copyright file="MeetingTranscriptRecording.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useState, useEffect } from 'react';
import { Button } from '@fluentui/react-components';
import { createNestablePublicClientApplication } from "@azure/msal-browser";
import { app, nestedAppAuth } from "@microsoft/teams-js";

const Msal = () => {

    const [meData, setMeData] = useState(null); // State to store user data
    const [isLoggedIn, setIsLoggedIn] = useState(false); // State to track login status
    const [errorMessage, setErrorMessage] = useState('');

    let pca = undefined; // Public client application instance

    // MSAL configuration
    const msalConfig = {
        auth: {
            clientId: "{{clientId}}",
            authority: "https://login.microsoftonline.com/common"
        }
    };

    // Function to initialize the public client application
    function initializePublicClient() {
        console.log("Starting initializePublicClient");
        return createNestablePublicClientApplication(msalConfig).then(
            (result) => {
                console.log("Client app created");
                pca = result;
                return pca;
            }
        );
    }

    // Function to get the active account
    const getActiveAccount = async () => {
        console.log("Starting getActiveAccount");
        let activeAccount = null;
        try {
            console.log("Getting active account");
            activeAccount = pca.getActiveAccount();
        } catch (error) {
            console.log(error);
        }
        if (!activeAccount) {
            console.log("No active account, trying login popup");
            try {
                const context = await app.getContext();
                const accountFilter = {
                    tenantId: context.user?.tenant?.id,
                    homeAccountId: context.user?.id,
                    loginHint: context.user?.loginHint
                };
                alert(accountFilter.loginHint);
                const accountWithFilter = pca.getAccount(accountFilter);
                if (accountWithFilter) {
                    activeAccount = accountWithFilter;
                    pca.setActiveAccount(activeAccount);
                }
            } catch (error) {
                console.log(error);
            }
        }
        return activeAccount;
    }

    // Function to get the token for the user
    const getToken = async () => {
        let activeAccount = await getActiveAccount();
        const tokenRequest = {
            scopes: ["User.Read"],
            account: activeAccount || undefined,
        };
        return pca.acquireTokenSilent(tokenRequest)
            .then((result) => {
                console.log(result);
                return result.accessToken;
            })
            .catch((error) => {
                console.log(error);
                // Try to get token via popup if silent acquisition fails
                return pca.acquireTokenPopup(tokenRequest)
                    .then(async (result) => {
                        console.log(result);
                        return result.accessToken;
                    })
                    .catch((error) => {
                        console.error(error); // Log the error for debugging purposes
                        return undefined; // Return undefined to indicate an error occurred
                    });
            });
    }

    // Function to initiate login and token retrieval
    const msallogin = async () => {
        let isNAAResults = await nestedAppAuth.isNAAChannelRecommended();
        if (isNAAResults == true) {
            console.log("Nested app auth capabilities supported");
            return initializePublicClient().then((_client) => {
                return getToken().then((token) => {
                    callApi(token);
                });
            });
        } else {
            console.log("Nested app auth capabilities not supported");
        }
    }

    // Function to call the Microsoft Graph API with the access token
    async function callApi(accessToken) {
        try {
            const response = await fetch(
                `https://graph.microsoft.com/v1.0/me`,
                {
                    headers: { Authorization: accessToken },
                }
            );
            if (response.ok) {
                // Parse and set the user data
                const jsonValue = await response.json();
                const alignedJSON = JSON.stringify(jsonValue, null, 2);
                setMeData(alignedJSON);
                setIsLoggedIn(true);
            } else {
                const errorText = await response.text();
                console.error("Microsoft Graph call failed - error text: " + errorText);
                setErrorMessage("An error occurred while trying to log in. Please try again.");
            }
        } catch (error) {
            console.error("Network error: ", error);
            setErrorMessage("A network error occurred. Please check your connection and try again.");
        }
    }

    return (
        <div>
            <div className="">
                {!isLoggedIn && <Button appearance="primary" onClick={msallogin}>Login</Button>}
                {errorMessage && <div className="error-message">{errorMessage}</div>}
            </div>
            {isLoggedIn && <h1 style={{ color: 'black' }}>Welcome! Your login information:</h1>}
            <div>
                <pre style={{ whiteSpace: 'pre-wrap', color: 'black' }}>{meData}</pre>
            </div>
        </div>
    );
};
export default Msal;
