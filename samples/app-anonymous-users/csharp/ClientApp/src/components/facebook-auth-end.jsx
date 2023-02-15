// <copyright file="facebook-auth-end.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import { useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";

const FacebookAuthEnd = props => {

    useEffect(() => {
        microsoftTeams.initialize();
        getAuthToken();
    });

    // Get face book client side token.
    const getAuthToken = () => {
        microsoftTeams.initialize();
        microsoftTeams.app.initialize().then(() => {
            localStorage.removeItem("auth.error");
            let code = getHashParameters();
            let key = "auth.result";
            localStorage.setItem(key, JSON.stringify({
                idToken: code,
            }));
            microsoftTeams.authentication.notifySuccess(key);

        });
    }

    // Parse hash parameters into key-value pairs
    const getHashParameters = () => {
        var urlString = window.location.href;
        var url = new URL(urlString);
        var code = url.searchParams.get("code");
        return code;
    }
};

export default FacebookAuthEnd;