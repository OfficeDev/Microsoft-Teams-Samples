#!/usr/bin/env python3
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os

from dotenv import load_dotenv

load_dotenv()


class Config:
    """Bot Configuration"""

    PORT = 3978
    APP_TYPE = os.environ.get("BOT_TYPE", "")
    APP_ID = os.environ.get("CLIENT_ID", "")
    APP_PASSWORD = os.environ.get("CLIENT_SECRET", "")
    APP_TENANTID = os.environ.get("TENANT_ID", "")
    BOT_ENDPOINT = os.environ.get("BOT_ENDPOINT", "")
    TEAMS_APP_ID = os.environ.get("TEAMS_APP_ID", "")
