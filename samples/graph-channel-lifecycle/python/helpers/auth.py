import requests
from config import DefaultConfig

def get_access_token(tenant_id):
    client_id = DefaultConfig.APP_ID
    client_secret = DefaultConfig.APP_PASSWORD

    if not client_id or not client_secret:
        return None
    
    url = f"https://login.microsoftonline.com/{tenant_id}/oauth2/v2.0/token"
    
    payload = {
        'grant_type': 'client_credentials',
        'client_id': client_id,
        'client_secret': client_secret,
        'scope': 'https://graph.microsoft.com/.default'
    }

    headers = {
        'Content-Type': 'application/x-www-form-urlencoded'
    }

    try:
        response = requests.post(url, data=payload, headers=headers)
        response.raise_for_status()
        token = response.json().get('access_token')

        if not token:
            raise Exception("No access token returned")

        return token

    except requests.RequestException as e:
        return None
