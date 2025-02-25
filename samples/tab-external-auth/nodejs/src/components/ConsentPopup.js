// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';

/**
 * This component is loaded to grant consent for graph permissions.
 */
class ConsentPopup extends React.Component {

    componentDidMount() {
         // Redirect url path
        var redirectUrl = window.location.origin + "/auth-end";
        const params = new URLSearchParams(window.location.search);
        const authId = params.get('authId');
        const method = params.get('oauthRedirectMethod');
        const hostRedirectUrl = params.get('hostRedirectUrl');
        const state = JSON.stringify({
            authId: authId,
            method: method,
            hostRedirectUrl: hostRedirectUrl
        });
        
        const queryObj = {
            state: encodeURIComponent(state), // Ensure proper URL encoding
            client_id: params.get('googleId'),
            response_type: 'code',
            access_type: 'offline',
            scope: 'email%20profile'
        };
        
        const query = Object.entries(queryObj)
            .map(([k, v]) => `${k}=${v}`) // Remove the extra `&` at the start
            .join('&'); 
        
        window.location.href = `https://accounts.google.com/o/oauth2/v2/auth?redirect_uri=${redirectUrl}&${query}`;
        
       }

    render() {
      return (
        <div>
          <h1>Redirecting to consent page.</h1>
        </div>
      );
    }
}

export default ConsentPopup;