import os

class Config:
    """Bot Configuration"""

    PORT = int(os.environ.get("PORT", 3978))
    APP_ID = os.environ.get("CLIENT_ID", "")
    APP_PASSWORD = os.environ.get("CLIENT_SECRET", "")
    APP_TYPE = os.environ.get("BOT_TYPE", "")
    APP_TENANTID = os.environ.get("TENANT_ID", "")
    
    # Auth0 Configuration
    AUTH0_DOMAIN = os.environ.get("AUTH0_DOMAIN", "")
    AUTH0_CLIENT_ID = os.environ.get("AUTH0_CLIENT_ID", "")
    AUTH0_CLIENT_SECRET = os.environ.get("AUTH0_CLIENT_SECRET", "")
    BOT_ENDPOINT = os.environ.get("BOT_ENDPOINT", "")
