// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';

/**
 * This component is loaded to grant consent for graph permissions.
 */
class ConsentPopup extends React.Component {

    componentDidMount() {
        const params = new URLSearchParams(window.location.search);
        const authId = params.get('authId');
        const method = params.get('oauthRedirectMethod');
        const redirectUrl = params.get('hostRedirectUrl');
        const state = `{"authId":"${authId}","method":"${method}"}`;
        
            const queryObj = {
                state,
                client_id: params.get('googleId'),
                response_type: 'code',
                access_type: 'offline',
                scope: 'email%20profile'
            };
        
            const query = Object.entries(queryObj).map(([k, v]) => `&${k}=${v}`).join('');
        
            window.location.href = `https://accounts.google.com/o/oauth2/v2/auth?redirect_uri=${redirectUrl}${query}`;
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