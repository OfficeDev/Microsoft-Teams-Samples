#!/usr/bin/env python3
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os

""" Bot Configuration """


class DefaultConfig:
    """ Bot Configuration """

    PORT = 3978
    # Use both possible environment variable names for compatibility
    # Try multiple naming conventions used by Teams Toolkit
    APP_ID = (
        os.environ.get("MicrosoftAppId") or 
        os.environ.get("BotId") or 
        os.environ.get("AAD_APP_CLIENT_ID") or 
        ""
    )
    APP_PASSWORD = (
        os.environ.get("MicrosoftAppPassword") or 
        os.environ.get("BotPassword") or 
        os.environ.get("SECRET_AAD_APP_CLIENT_SECRET") or 
        ""
    )
    
    # Additional configuration
    BASE_URL = os.environ.get("BaseUrl", "https://localhost:3978")