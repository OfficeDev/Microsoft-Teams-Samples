# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os


class DefaultConfig:
    PORT = 3978
    MicrosoftAppId = os.environ.get("CLIENT_ID", "")
    MicrosoftAppType = os.environ.get("BOT_TYPE", "SingleTenant")
    MicrosoftAppTenantId = os.environ.get("TENANT_ID", "")
    MicrosoftAppPassword = os.environ.get("CLIENT_SECRET", os.environ.get("CLIENT_PASSWORD", ""))
    CONNECTION_NAME = os.environ.get("ConnectionName", "")
