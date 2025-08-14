#!/usr/bin/env python3
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os


class DefaultConfig:
    """ Bot Configuration """

    PORT = 3978
    APP_ID = os.environ.get("MicrosoftAppId", "<<YOUR_MICROSOFT_APP_ID>>")
    APP_PASSWORD = os.environ.get("MicrosoftAppPassword", "<<YOUR-MICROSOFT_APP_PASSWORD>>")
