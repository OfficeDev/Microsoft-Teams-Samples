# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
from pathlib import Path
from dotenv import load_dotenv

# Load environment variables from .env.local file
load_dotenv(dotenv_path=Path(__file__).resolve().parent / "env" / ".env.local")

""" Bot Configuration """

class DefaultConfig:
    """ Bot Configuration for Teams SDK """

    PORT = 3978
    CLIENT_ID = os.environ.get("CLIENT_ID", "")
    CLIENT_SECRET = os.environ.get("CLIENT_SECRET", "")
    TENANT_ID = os.environ.get("TENANT_ID", "")
    BASE_URL = os.environ.get("BaseUrl", "http://localhost:3978")