# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os

""" Bot Configuration """
from dotenv import load_dotenv

load_dotenv()

class DefaultConfig:
    """ Bot Configuration """

    PORT = int(os.environ.get("PORT", 3978))
    APP_ID = os.environ.get("MicrosoftAppId", os.environ.get("CLIENT_ID", ""))
    APP_PASSWORD = os.environ.get("MicrosoftAppPassword", os.environ.get("CLIENT_SECRET", ""))
    APP_TYPE = os.environ.get("MicrosoftAppType", os.environ.get("BOT_TYPE", ""))
    APP_TENANTID = os.environ.get("TENANT_ID", "")
    
    # Azure OpenAI Configuration
    AZURE_OPENAI_API_KEY = os.environ.get("AZURE_OPENAI_API_KEY", "")
    AZURE_OPENAI_ENDPOINT = os.environ.get("AZURE_OPENAI_ENDPOINT", "")
    AZURE_OPENAI_DEPLOYMENT_NAME = os.environ.get("AZURE_OPENAI_DEPLOYMENT_NAME", "gpt-4o-mini")
    AZURE_OPENAI_API_VERSION = os.environ.get("AZURE_OPENAI_API_VERSION", "2024-10-21")
    