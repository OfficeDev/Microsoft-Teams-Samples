import os
import requests
from urllib.parse import urlencode

def get_access_token(tenant_id):
    """Get application level access token."""
    
    request_params = {
        'grant_type': 'client_credentials',
        'client_id': os.getenv('ClientId'),
        'client_secret': os.getenv('ClientSecret'),
        'scope': 'https://graph.microsoft.com/.default'
    }
    
    url = f"https://login.microsoftonline.com/{tenant_id}/oauth2/v2.0/token"
    
    try:
        response = requests.post(url, data=urlencode(request_params), headers={
            'Content-Type': 'application/x-www-form-urlencoded'
        })
        
        parsed_body = response.json()
        
        if response.status_code != 200:
            raise Exception(parsed_body.get('error_description', 'Unknown error'))
        
        return parsed_body.get('access_token')
        
    except Exception as e:
        raise Exception(f"Error getting access token: {str(e)}")
