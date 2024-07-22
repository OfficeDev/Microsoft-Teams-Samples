"""
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
"""

import os

from dotenv import load_dotenv

load_dotenv()


class Config:
    """Bot Configuration"""

    PORT = 3978
    APP_ID = os.environ["BOT_ID"]
    APP_PASSWORD = os.environ["BOT_PASSWORD"]
    AZURE_OPENAI_KEY = os.environ.get("AZURE_OPENAI_KEY", "")
    AZURE_OPENAI_ENDPOINT = os.environ.get("AZURE_OPENAI_ENDPOINT", "")
    AZURE_SEARCH_ENDPOINT = os.environ.get("AZURE_SEARCH_ENDPOINT", "")
    AZURE_SEARCH_KEY = os.environ.get("AZURE_SEARCH_KEY", "")
    AZURE_SEARCH_INDEX = os.environ.get("AZURE_SEARCH_INDEX", "")
