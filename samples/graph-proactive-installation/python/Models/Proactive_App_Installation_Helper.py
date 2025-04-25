# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
import json
import requests
import asyncio

class ProactiveAppInstallationHelper():
    async def get_access_token(self, microsoft_tenant_id):
        """
        Retrieves an access token from Microsoft Identity Platform using client credentials.
        
        :param microsoft_tenant_id: The Microsoft tenant ID.
        :return: Access token string if successful, else the response text.
        """
        url = f'https://login.microsoftonline.com/{microsoft_tenant_id}/oauth2/v2.0/token'
        headers = {'Content-Type': 'application/x-www-form-urlencoded'}
        data = {
            'grant_type': 'client_credentials',
            'client_id': os.getenv('MicrosoftAppId'),
            'scope': 'https://graph.microsoft.com/.default',
            'client_secret': os.getenv('MicrosoftAppPassword')
        }
        
        response = requests.post(url, headers=headers, data=data)
        return response.json().get('access_token', response.text)

    async def install_app_in_personal_scope(self, microsoft_tenant_id, user_id):
        """
        Installs the Teams app in a user's personal scope.
        
        :param microsoft_tenant_id: The Microsoft tenant ID.
        :param user_id: The user's Azure AD object ID.
        :return: The HTTP status code of the installation request.
        """
        access_token = await self.get_access_token(microsoft_tenant_id)
        url = f'https://graph.microsoft.com/v1.0/users/{user_id}/teamwork/installedApps'
        headers = {
            'Content-Type': 'application/json',
            'Authorization': f'Bearer {access_token}'
        }
        data = json.dumps({
            'teamsApp@odata.bind': f'https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{os.getenv("AppCatalogTeamAppId")}'
        })
        
        response = requests.post(url, headers=headers, data=data)
        
        # If installation fails, attempt to re-trigger conversation update
        if response.status_code >= 400:
            await self.trigger_conversation_update(microsoft_tenant_id, user_id)
        
        return response.status_code

    async def trigger_conversation_update(self, microsoft_tenant_id, user_id):
        """
        Checks if the app is already installed for a user and attempts to install it in a personal chat scope.
        
        :param microsoft_tenant_id: The Microsoft tenant ID.
        :param user_id: The user's Azure AD object ID.
        :return: Results of all installation attempts.
        """
        access_token = await self.get_access_token(microsoft_tenant_id)
        url = (f'https://graph.microsoft.com/v1.0/users/{user_id}/teamwork/installedApps'
               f'?$expand=teamsApp,teamsAppDefinition&$filter=teamsApp/externalId eq "{os.getenv("MicrosoftAppId")}"')
        
        headers = {'Authorization': f'Bearer {access_token}'}
        
        response = requests.get(url, headers=headers)
        installed_apps = response.json().get('value', [])
        
        tasks = []
        for app in installed_apps:
            tasks.append(self.install_app_in_personal_chat_scope(access_token, user_id, app['id']))
        
        return await asyncio.gather(*tasks)

    async def install_app_in_personal_chat_scope(self, access_token, user_id, app_id):
        """
        Installs the app in the user's personal chat scope if available.
        
        :param access_token: The Microsoft Graph API access token.
        :param user_id: The user's Azure AD object ID.
        :param app_id: The installed app's ID.
        :return: Response data if successful, else response text.
        """
        url = f'https://graph.microsoft.com/v1.0/users/{user_id}/teamwork/installedApps/{app_id}/chat'
        headers = {'Authorization': f'Bearer {access_token}'}
        
        response = requests.get(url, headers=headers)
        return response.json() if response.status_code == 200 else response.text
