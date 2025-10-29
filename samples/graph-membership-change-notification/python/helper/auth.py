# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
import msal

class AuthHelper:
    @staticmethod
    def get_access_token():
        client_id = os.getenv('MicrosoftAppId')
        client_secret = os.getenv('MicrosoftAppPassword')
        tenant_id = os.getenv('MicrosoftAppTenantId')
        authority = f"https://login.microsoftonline.com/{tenant_id}"
        app = msal.ConfidentialClientApplication(
            client_id,
            authority=authority,
            client_credential=client_secret
        )
        result = app.acquire_token_for_client(scopes=['https://graph.microsoft.com/.default'])
        if 'access_token' in result:
            return result['access_token']
        else:
            raise Exception(f"Unable to acquire access token: {result.get('error_description')}")
