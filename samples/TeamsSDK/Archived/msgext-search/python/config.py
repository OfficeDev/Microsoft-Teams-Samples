#!/usr/bin/env python3
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os


class DefaultConfig:
    """ Bot Configuration """

    PORT = 3978
    MicrosoftAppType = os.environ.get("MicrosoftAppType", "")
    MicrosoftAppId = os.environ.get("MicrosoftAppId", "")
    MicrosoftAppPassword = os.environ.get("MicrosoftAppPassword", "")
    MicrosoftAppTenantId = os.environ.get("MicrosoftAppTenantId", "")
