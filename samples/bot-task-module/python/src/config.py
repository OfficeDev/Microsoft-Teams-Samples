import os

from dotenv import load_dotenv

load_dotenv()

class Config:
    """Bot Configuration"""

    APP_ID = os.environ.get("CLIENT_ID", "")
    APP_PASSWORD = os.environ.get("CLIENT_SECRET", "")
    APP_TYPE = os.environ.get("BOT_TYPE", "")
    APP_TENANTID = os.environ.get("TENANT_ID", "")
    
    # Base URL for serving static pages (task modules)
    # This should be the public endpoint where your bot is hosted
    BASE_URL = os.environ.get("BOT_ENDPOINT", "http://localhost:3978")
    
    # Port for the application
    PORT = int(os.environ.get("PORT", 3978))
