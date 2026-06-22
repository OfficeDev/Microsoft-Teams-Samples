# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import aiohttp

from config import DefaultConfig


class ProactiveAppInstallationHelper:
    def __init__(self, config: DefaultConfig):
        self.config = config

    async def _get_access_token(self, tenant_id: str) -> str:
        url = f"https://login.microsoftonline.com/{tenant_id}/oauth2/v2.0/token"
        data = {
            "grant_type": "client_credentials",
            "client_id": self.config.APP_ID,
            "scope": "https://graph.microsoft.com/.default",
            "client_secret": self.config.APP_PASSWORD,
        }

        async with aiohttp.ClientSession() as session:
            async with session.post(url, data=data) as response:
                result = await response.json()
                return result.get("access_token", "")

    async def install_app_in_personal_scope(self, tenant_id: str, user_id: str) -> int:
        access_token = await self._get_access_token(tenant_id)
        url = f"https://graph.microsoft.com/v1.0/users/{user_id}/teamwork/installedApps"
        headers = {
            "Content-Type": "application/json",
            "Authorization": f"Bearer {access_token}",
        }
        payload = {
            "teamsApp@odata.bind": (
                f"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{self.config.TEAMS_APP_ID}"
            )
        }

        async with aiohttp.ClientSession() as session:
            async with session.post(url, headers=headers, json=payload) as response:
                status = response.status

        if status >= 400:
            await self._trigger_conversation_update(tenant_id, user_id)

        return status

    async def _trigger_conversation_update(self, tenant_id: str, user_id: str):
        access_token = await self._get_access_token(tenant_id)
        url = (
            f"https://graph.microsoft.com/v1.0/users/{user_id}/teamwork/installedApps"
            f"?$expand=teamsApp,teamsAppDefinition&$filter=teamsApp/externalId eq '{self.config.APP_ID}'"
        )
        headers = {"Authorization": f"Bearer {access_token}"}

        async with aiohttp.ClientSession() as session:
            async with session.get(url, headers=headers) as response:
                result = await response.json()
                installed_apps = result.get("value", [])

            for app in installed_apps:
                app_url = f"https://graph.microsoft.com/v1.0/users/{user_id}/teamwork/installedApps/{app['id']}/chat"
                async with session.get(app_url, headers=headers) as chat_response:
                    await chat_response.read()
