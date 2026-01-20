#!/usr/bin/env python3
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import requests
from config import DefaultConfig

TOKEN_ENDPOINT = "https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token"

# Exchanges SSO token for Graph access token using On-Behalf-Of flow.
def exchange_sso_token_for_graph_token(sso_token: str):
    config = DefaultConfig()
    try:
        if not config.APP_ID:
            raise ValueError("APP_ID not configured")
        client_secret = config.APP_PASSWORD
        if not client_secret or client_secret == "<<MICROSOFT-APP-PASSWORD>>":
            return None
        tenant_id = config.AAD_APP_TENANT_ID
        if not tenant_id:
            return None
        token_url = TOKEN_ENDPOINT.format(tenant=tenant_id)
        payload = {
            "client_id": config.APP_ID,
            "client_secret": client_secret,
            "assertion": sso_token,
            "assertion_type": "urn:ietf:params:oauth:grant-type:jwt-bearer",
            "grant_type": "urn:ietf:params:oauth:grant-type:jwt-bearer",
            "requested_token_use": "on_behalf_of",
            "scope": "https://graph.microsoft.com/.default"
        }
        headers = {
            "Content-Type": "application/x-www-form-urlencoded"
        }
        response = requests.post(token_url, data=payload, headers=headers)
        if response.status_code == 200:
            token_data = response.json()
            return token_data.get("access_token")
        else:
            return None
    except Exception as e:
        return None
