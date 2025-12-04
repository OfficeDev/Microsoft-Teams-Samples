import os
import requests


def get_access_token(tenant_id=None):
    """
    Get access token using client credentials flow
    """
    # Use provided tenant_id or fall back to environment variable
    tenant_id = tenant_id or os.getenv('AAD_APP_TENANT_ID')
    
    if not tenant_id:
        raise ValueError("Tenant ID is required")
    
    # Try multiple variable names for client ID
    client_id = (
        os.getenv('AAD_APP_CLIENT_ID') or 
        os.getenv('MicrosoftAppId') or 
        os.getenv('CLIENT_ID')
    )
    
    # Try multiple variable names for client secret
    # For local development, prioritize plain text secrets from .env
    client_secret = (
        os.getenv('MicrosoftAppPassword') or  # Plain text from .env (preferred for local dev)
        os.getenv('CLIENT_SECRET') or
        os.getenv('SECRET_AAD_APP_CLIENT_SECRET')  # TeamsFx encrypted secret
    )
    
    # If we have an encrypted secret, try to decrypt it
    if client_secret and client_secret.startswith('crypto_'):
        print("Found encrypted secret, attempting to decrypt...")
        try:
            decrypted_secret = get_client_secret()
            client_secret = decrypted_secret
            print("Successfully decrypted secret for authentication")
        except Exception as e:
            print(f"Failed to decrypt secret: {e}")
            # For local development, suggest using plain text secret
            raise ValueError(
                f"Failed to decrypt TeamsFx secret: {e}\n"
                "For local development, you can:\n"
                "1. Use a plain text secret by setting MicrosoftAppPassword in .env\n"
                "2. Or use TeamsFx CLI to manage encrypted secrets\n"
                "Current .env should have: MicrosoftAppPassword=<your-plain-text-secret>"
            )
    
    if not client_id:
        raise ValueError("Client ID is required. Check AAD_APP_CLIENT_ID or MicrosoftAppId environment variable")
    
    if not client_secret:
        raise ValueError(
            "Client Secret is required. For local development:\n"
            "1. Check that MicrosoftAppPassword is set in .env file\n"
            "2. Or set CLIENT_SECRET environment variable\n"
            "3. Or ensure SECRET_AAD_APP_CLIENT_SECRET is properly configured for TeamsFx"
        )
    
    url = f"https://login.microsoftonline.com/{tenant_id}/oauth2/v2.0/token"

    request_params = {
        'grant_type': 'client_credentials',
        'client_id': client_id,
        'client_secret': client_secret,
        'scope': 'https://graph.microsoft.com/.default'
    }

    try:
        response = requests.post(url, data=request_params)
        response.raise_for_status()
        data = response.json()

        if 'access_token' in data:
            return data['access_token']
        else:
            error_msg = data.get('error_description', 'Unknown error while fetching token')
            raise Exception(f"Token request failed: {error_msg}")

    except requests.exceptions.RequestException as e:
        print(f"Request error: {e}")
        raise Exception(f"Failed to get access token: {str(e)}")
    except Exception as e:
        print(f"Authentication error: {e}")
        raise
