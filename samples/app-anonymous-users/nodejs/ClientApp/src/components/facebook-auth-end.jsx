// <copyright file="facebook-auth-end.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";

const FacebookAuthEnd = props => {

    useEffect(() => {
        getAuthToken();
    });

    // Get face book client side token.
    const getAuthToken = () => {
        microsoftTeams.app.initialize().then(() => {

            localStorage.removeItem("auth.error");
            let url = getHashParameters();
            var code = url.searchParams.get("code");
            var state = url.searchParams.get("state");
            if (state === "ValidateState") {
                // Success -- return token information to the parent page.
                // Use localStorage to avoid passing the token via notifySuccess; instead we send the item key.
                
                microsoftTeams.authentication.notifySuccess(code);
            } else {
                // State does not match, report error
                microsoftTeams.authentication.notifyFailure("StateDoesNotMatch");
            }
        });
    }

    // Parse hash parameters into key-value pairs
    const getHashParameters = () => {
        var urlString = window.location.href;
        var url = new URL(urlString);
        return url;
    }
};

export default FacebookAuthEnd;