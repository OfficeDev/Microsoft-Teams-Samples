#!/usr/bin/env python3
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os

""" Bot Configuration """

env_file_path = os.path.join(os.path.dirname(__file__), 'env', '.env.local')
class DefaultConfig:
    """ Bot Configuration """
    PORT = 3978
    APP_ID = os.environ.get("AAD_APP_CLIENT_ID") or os.environ.get("MicrosoftAppId")
    APP_PASSWORD = os.environ.get("MicrosoftAppPassword")