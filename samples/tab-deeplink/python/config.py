#!/usr/bin/env python3
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os

""" Bot Configuration """


class DefaultConfig:
    """ Bot Configuration """

    PORT = 3978
    APP_ID = os.environ.get("MicrosoftAppId", "<<MICROSOFT-APP-ID>>")
    APP_PASSWORD = os.environ.get("MicrosoftAppPassword", "<<MICROSOFT-APP-PASSWORD>>")
    BOT_ENDPOINT = os.environ.get("BaseUrl", "<<BOT-ENDPOINT>>")
    TEAMS_APP_ID = os.environ.get("TeamsAppId", "<<TEAMS-APP-ID>>")
    Tab_Entity_Id = os.environ.get("Tab_Entity_Id", "<<TAB-ENTITY-ID>>")
    Channel_Entity_Id = os.environ.get("Channel_Entity_Id", "<<CHANNEL-ENTITY-ID>>")
    APP_TYPE = os.environ.get("MicrosoftAppType", "")
    APP_TENANTID = os.environ.get("MicrosoftAppTenantId", "")