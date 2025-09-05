#!/usr/bin/env python3
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os

""" Bot Configuration """


class DefaultConfig:
    """ Bot Configuration """

    PORT = 3978
    APP_ID = os.environ.get("MicrosoftAppId", "<<MICROSOFTAPP-ID>>")
    APP_PASSWORD = os.environ.get("MicrosoftAppPassword", "MICROSOFT-APP-PASSWORD")
    APP_TYPE = os.environ.get("MicrosoftAppType", "")
    APP_TENANTID = os.environ.get("MicrosoftAppTenantId", "")
