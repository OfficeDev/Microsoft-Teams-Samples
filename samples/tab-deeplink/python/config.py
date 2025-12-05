# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os

""" Agent Configuration """


class DefaultConfig:
    """ Configuration for Agent SDK bot """

    PORT = 3978
    # Microsoft App credentials from Azure Bot Service registration
    APP_ID = os.environ.get("MicrosoftAppId", "<<MICROSOFT-APP-ID>>")
    APP_PASSWORD = os.environ.get("MicrosoftAppPassword", "<<MICROSOFT-APP-PASSWORD>>")
    BOT_ENDPOINT = os.environ.get("BaseUrl", "<<BOT-ENDPOINT>>")
    # Teams app configuration
    TEAMS_APP_ID = os.environ.get("TeamsAppId", "<<TEAMS-APP-ID>>")
    Tab_Entity_Id = os.environ.get("Tab_Entity_Id", "<<TAB-ENTITY-ID>>")
    Channel_Entity_Id = os.environ.get("Channel_Entity_Id", "<<CHANNEL-ENTITY-ID>>")
    # App type must be MultiTenant for Agent SDK
    APP_TYPE = os.environ.get("MicrosoftAppType", "MultiTenant")
    APP_TENANTID = os.environ.get("MicrosoftAppTenantId", "")