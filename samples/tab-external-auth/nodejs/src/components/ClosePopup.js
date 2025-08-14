// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
/**
 * This component is used to redirect the user to the Azure authorization endpoint from a popup.
 */
class ClosePopup extends React.Component {

    componentDidMount() {
        const params = new URLSearchParams(window.location.search);
        const result = params.get('code');
        const { authId, method, hostRedirectUrl } = JSON.parse(params.get('state'));
        const baseUrl = hostRedirectUrl.split('authId')[0];
        if (method === 'deeplink') {
            window.location.href = `${baseUrl}authId=${authId}&result=${result}`
        } else {
            alert("failed");
        }
    }     

    render() {
      return (
        <div>
            <h1>Consent flow complete.</h1>
        </div>
      );
    }
}

export default ClosePopup;