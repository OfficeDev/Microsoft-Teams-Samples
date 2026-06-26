# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os

""" Bot Configuration """

class DefaultConfig:
    """ Bot Configuration matching Node.js Teams AI SDK pattern """

    PORT = 3978
    
    # Bot credentials (matching Node.js config.js pattern)
    MicrosoftAppId = os.environ.get("CLIENT_ID", "")
    MicrosoftAppType = os.environ.get("BOT_TYPE", "SingleTenant")
    MicrosoftAppTenantId = os.environ.get("TENANT_ID", "")
    MicrosoftAppPassword = os.environ.get("CLIENT_SECRET", os.environ.get("CLIENT_PASSWORD", ""))
    
    # Connection name for OAuth (matches Node.js pattern)
    CONNECTION_NAME = os.environ.get("ConnectionName", "")
