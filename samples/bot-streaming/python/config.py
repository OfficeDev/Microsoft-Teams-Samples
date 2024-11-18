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
    APP_TENANT_ID = os.getenv("MICROSOFT_APP_TENANT_ID", "")

    # Azure OpenAI settings
    AZURE_OPENAI_ENDPOINT = os.getenv("AzureOpenAIEndpoint", "")
    AZURE_OPENAI_KEY = os.getenv("AzureOpenAIKey", "")
    AZURE_OPENAI_DEPLOYMENT = os.getenv("AzureOpenAIDeployment", "")
