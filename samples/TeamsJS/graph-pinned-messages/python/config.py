#!/usr/bin/env python3
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os

""" Bot Configuration """

class DefaultConfig:
    PORT = 3978
    APP_ID = os.environ.get("MicrosoftAppId", "<<MICROSOFT-APP-ID>>")
    APP_PASSWORD = os.environ.get("MicrosoftAppPassword", "<<MICROSOFT-APP-PASSWORD>>")
    AAD_APP_TENANT_ID = os.environ.get("AAD_APP_TENANT_ID")
