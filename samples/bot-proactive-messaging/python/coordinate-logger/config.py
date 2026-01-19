# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
from pathlib import Path
from dotenv import load_dotenv

# Load environment variables from .env.local file in parent directory
env_path = Path(__file__).resolve().parent.parent / "env" / ".env.local"
if env_path.exists():
    load_dotenv(dotenv_path=env_path)

# Also try loading from root .env
root_env = Path(__file__).resolve().parent.parent / ".env"
if root_env.exists():
    load_dotenv(dotenv_path=root_env, override=False)

""" Bot Configuration """

class DefaultConfig:
    """ Bot Configuration """
    
    PORT = int(os.environ.get("PORT", "3978"))
    CLIENT_ID = os.environ.get("CLIENT_ID", "")
    CLIENT_SECRET = os.environ.get("CLIENT_SECRET", "")
    TENANT_ID = os.environ.get("TENANT_ID", "")
