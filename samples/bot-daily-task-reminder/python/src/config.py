import os
from pathlib import Path

from dotenv import load_dotenv

# Load environment files from the env folder
env_folder = Path(__file__).parent.parent / "env"
env_file = env_folder / ".env.local"
env_user_file = env_folder / ".env.local.user"

# Load both env files if they exist
if env_file.exists():
    load_dotenv(env_file)
if env_user_file.exists():
    load_dotenv(env_user_file, override=True)

class Config:
    """Bot Configuration"""

    PORT = 3978
    APP_ID = os.environ.get("CLIENT_ID", "")
    APP_PASSWORD = os.environ.get("CLIENT_SECRET", "")
    APP_TYPE = os.environ.get("BOT_TYPE", "")
    APP_TENANTID = os.environ.get("TENANT_ID", "")
