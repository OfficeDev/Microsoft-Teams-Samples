// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from "react";

import crypto from "crypto";
import * as microsoftTeams from "@microsoft/teams-js";

/**
 * This component is used to redirect the user to the Azure authorization endpoint from a popup
 */
class AuthStart extends React.Component {
  componentDidMount() {
    // Initialize the Microsoft Teams SDK
    microsoftTeams.initialize();

    // Get the user context in order to extract the tenant ID
    microsoftTeams.getContext((context, error) => {
      let tenant = context["tid"]; // Tenant ID of the logged in user
      let client_id = process.env.REACT_APP_AZURE_APP_REGISTRATION_ID; // Client ID of the Azure AD app registration ( may be from different tenant for multitenant apps)
      let graph_scopes = process.env.REACT_APP_GRAPH_SCOPES;

      // Form a query for the Azure implicit grant authorization flow
      // https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-implicit-grant-flow
      let queryParams = {
        tenant: `${tenant}`,
        client_id: `${client_id}`,
        response_type: "token", // token_id in other samples is only needed if using open ID
        scope: `https://graph.microsoft.com/${graph_scopes}`,
        redirect_uri: window.location.origin + "/auth-end",
        nonce: crypto.randomBytes(16).toString("base64"),
        prompt: "consent",
      };
      queryParams = new URLSearchParams(queryParams).toString();

      let url = `https://login.microsoftonline.com/${tenant}/oauth2/v2.0/authorize?`;
      let authorizeEndpoint = url + queryParams;

      // Redirect to the Azure authorization endpoint. When that flow completes, the user will be directed to auth-end
      // Go to AuthEnd.js
      window.location.assign(authorizeEndpoint);
    });
  }

  render() {
    return (
      <div>
        <h1>Redirecting to consent page...</h1>
      </div>
    );
  }
}

export default AuthStart;
