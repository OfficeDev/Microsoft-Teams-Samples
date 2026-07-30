#!/usr/bin/env python3
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
from dotenv import load_dotenv

load_dotenv()

""" Bot + Auth Configuration """


class DefaultConfig:
    """Configuration for Bot Framework and Auth0"""

    PORT = 3978
    APP_ID = os.environ.get("MicrosoftAppId", "")
    APP_PASSWORD = os.environ.get("MicrosoftAppPassword", "")
    APP_TYPE = os.environ.get("MicrosoftAppType", "SingleTenant")
    APP_TENANTID = os.environ.get("MicrosoftAppTenantId", "")
    AUTH0_DOMAIN = os.environ.get("AUTH0_DOMAIN", "<<your-auth0-domain>>")
    AUTH0_CLIENT_ID = os.environ.get("AUTH0_CLIENT_ID", "<<your-auth0-client-id>>")
    AUTH0_CLIENT_SECRET = os.environ.get("AUTH0_CLIENT_SECRET", "<<your-auth0-client-secret>>")
    BOT_ENDPOINT = os.environ.get("BOT_ENDPOINT", "http://localhost:3978")
