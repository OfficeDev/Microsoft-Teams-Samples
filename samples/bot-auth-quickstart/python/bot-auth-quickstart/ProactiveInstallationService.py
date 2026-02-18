"""
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.

"""

import asyncio
import json
import logging
from typing import Dict, Any, List, Optional

import httpx

from microsoft_teams.api import ConversationReference, ConversationAccount
from microsoft_teams.api.models.account import Account
from microsoft_teams.api.clients.conversation.params import CreateConversationParams
from microsoft_teams.apps import ActivityContext


# Proactive App Installation Helper

class ProactiveAppInstallationHelper:
    """Helper class for proactive app installation via Microsoft Graph API."""

    _cached_catalog_app_id: Optional[str] = None

    def __init__(
        self,
        client_id: str,
        client_secret: str,
        app_catalog_team_app_id: str = "",
        teams_app_id: str = "",
    ):
        self.client_id = client_id
        self.client_secret = client_secret
        self.app_catalog_team_app_id = app_catalog_team_app_id
        self.teams_app_id = teams_app_id

    async def get_app_access_token(self, tenant_id: str) -> str:
        """
        Retrieves an access token using client credentials flow (app-only).

        :param tenant_id: The Microsoft tenant ID.
        :return: Access token string.
        """
        url = f"https://login.microsoftonline.com/{tenant_id}/oauth2/v2.0/token"
        headers = {"Content-Type": "application/x-www-form-urlencoded"}
        data = {
            "grant_type": "client_credentials",
            "client_id": self.client_id,
            "scope": "https://graph.microsoft.com/.default",
            "client_secret": self.client_secret,
        }

        async with httpx.AsyncClient() as client:
            response = await client.post(url, headers=headers, data=data)
            result = response.json()
            return result.get("access_token", "")

    async def get_catalog_app_id(
        self, tenant_id: str, known_user_id: Optional[str] = None
    ) -> str:
        """
        Discovers the catalog app ID for this bot. Tries multiple strategies:
        1. Return cached value if available
        2. Try appCatalog lookup by externalId (needs AppCatalog.Read.All)
        3. If that fails, look up a known user's installed apps to find the bot's
           catalog ID via teamsApp/externalId == CLIENT_ID
        4. Fall back to APP_CATALOG_TEAM_APP_ID env var
        """
        # Return cached value
        if ProactiveAppInstallationHelper._cached_catalog_app_id:
            return ProactiveAppInstallationHelper._cached_catalog_app_id

        access_token = await self.get_app_access_token(tenant_id)
        headers = {"Authorization": f"Bearer {access_token}"}

        # Strategy 1: Try catalog lookup (needs AppCatalog.Read.All)
        if self.client_id:
            try:
                url = (
                    f"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps"
                    f"?$filter=externalId eq '{self.client_id}'"
                )
                async with httpx.AsyncClient() as client:
                    response = await client.get(url, headers=headers)
                    if response.status_code == 200:
                        apps = response.json().get("value", [])
                        if apps:
                            catalog_id = apps[0].get("id", "")
                            logging.info(
                                f"Found bot in catalog (direct lookup): {catalog_id}"
                            )
                            ProactiveAppInstallationHelper._cached_catalog_app_id = (
                                catalog_id
                            )
                            return catalog_id
            except Exception as e:
                logging.debug(f"Catalog lookup failed: {e}")

        # Strategy 2: Look up from a known user's installed apps
        if known_user_id:
            try:
                url = (
                    f"https://graph.microsoft.com/v1.0/users/{known_user_id}"
                    f"/teamwork/installedApps?$expand=teamsApp"
                )
                async with httpx.AsyncClient() as client:
                    response = await client.get(url, headers=headers)
                    if response.status_code == 200:
                        installed_apps = response.json().get("value", [])
                        logging.info(
                            f"Strategy 2: Found {len(installed_apps)} installed apps "
                            f"for user {known_user_id}"
                        )

                        for app_entry in installed_apps:
                            teams_app = app_entry.get("teamsApp", {})
                            ext_id = teams_app.get("externalId", "")

                            if ext_id and (
                                ext_id == self.client_id
                                or ext_id == self.teams_app_id
                            ):
                                catalog_id = teams_app.get("id", "")
                                if catalog_id:
                                    logging.info(
                                        f"Found bot in catalog (via user lookup): "
                                        f"{catalog_id} (externalId={ext_id})"
                                    )
                                    ProactiveAppInstallationHelper._cached_catalog_app_id = (
                                        catalog_id
                                    )
                                    return catalog_id

                        logging.warning(
                            f"Strategy 2: Bot not found among {len(installed_apps)} "
                            f"installed apps (looking for externalId="
                            f"{self.client_id} or {self.teams_app_id})"
                        )
            except Exception as e:
                logging.debug(f"User-based catalog lookup failed: {e}")

        # Strategy 3: Fall back to env var
        if self.app_catalog_team_app_id:
            logging.warning(
                f"Using APP_CATALOG_TEAM_APP_ID fallback: "
                f"{self.app_catalog_team_app_id}"
            )
        else:
            logging.error("Could not determine catalog app ID by any method")
        return self.app_catalog_team_app_id

    async def install_app_in_personal_scope(
        self,
        tenant_id: str,
        user_id: str,
        known_user_id: Optional[str] = None,
    ) -> int:
        """
        Installs the Teams app in a user's personal scope.

        :return: The HTTP status code of the installation request.
        """
        catalog_app_id = await self.get_catalog_app_id(
            tenant_id, known_user_id=known_user_id
        )
        if not catalog_app_id:
            logging.error("No catalog app ID available for installation")
            return 500

        access_token = await self.get_app_access_token(tenant_id)
        url = (
            f"https://graph.microsoft.com/v1.0/users/{user_id}"
            f"/teamwork/installedApps"
        )
        headers = {
            "Content-Type": "application/json",
            "Authorization": f"Bearer {access_token}",
        }
        data = json.dumps(
            {
                "teamsApp@odata.bind": (
                    f"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/"
                    f"{catalog_app_id}"
                )
            }
        )

        async with httpx.AsyncClient() as client:
            response = await client.post(url, headers=headers, content=data)

            # If installation fails, attempt to re-trigger conversation update
            if response.status_code >= 400:
                await self.trigger_conversation_update(tenant_id, user_id)

            return response.status_code

    async def trigger_conversation_update(
        self, tenant_id: str, user_id: str
    ) -> List[Any]:
        """
        Checks if the app is already installed and attempts to install
        in personal chat scope.
        """
        access_token = await self.get_app_access_token(tenant_id)
        url = (
            f"https://graph.microsoft.com/v1.0/users/{user_id}"
            f"/teamwork/installedApps"
            f"?$expand=teamsApp,teamsAppDefinition"
            f"&$filter=teamsApp/externalId eq '{self.client_id}'"
        )
        headers = {"Authorization": f"Bearer {access_token}"}

        async with httpx.AsyncClient() as client:
            response = await client.get(url, headers=headers)
            installed_apps = response.json().get("value", [])

            tasks = [
                self.install_app_in_personal_chat_scope(
                    access_token, user_id, app["id"]
                )
                for app in installed_apps
            ]

            return await asyncio.gather(*tasks) if tasks else []

    async def install_app_in_personal_chat_scope(
        self, access_token: str, user_id: str, app_id: str
    ) -> Any:
        """
        Installs the app in the user's personal chat scope.
        """
        url = (
            f"https://graph.microsoft.com/v1.0/users/{user_id}"
            f"/teamwork/installedApps/{app_id}/chat"
        )
        headers = {"Authorization": f"Bearer {access_token}"}

        async with httpx.AsyncClient() as client:
            response = await client.get(url, headers=headers)
            return (
                response.json() if response.status_code == 200 else response.text
            )


# Team Members


async def get_team_members(
    ctx: ActivityContext,
    client_id: str,
    client_secret: str,
    default_tenant_id: str,
) -> List[Dict[str, Any]]:
    """
    Get team/chat members using Graph API with app-level credentials.
    """
    try:
        members: List[Dict[str, Any]] = []

        conversation = ctx.activity.conversation
        if not conversation or not hasattr(conversation, "id"):
            return []

        tenant_id = getattr(conversation, "tenant_id", None) or default_tenant_id
        if not tenant_id:
            return []

        helper = ProactiveAppInstallationHelper(client_id, client_secret)
        access_token = await helper.get_app_access_token(tenant_id)

        conversation_type = getattr(conversation, "conversation_type", None)
        is_group = getattr(conversation, "is_group", None)
        conv_id = conversation.id

        logging.info(
            f"conversation_type={conversation_type}, is_group={is_group}, "
            f"conv_id={conv_id}"
        )

        if conversation_type == "channel":
            team_id = conv_id.split(";")[0] if ";" in conv_id else conv_id
            url = f"https://graph.microsoft.com/v1.0/teams/{team_id}/members"
        elif conversation_type == "groupChat" or is_group:
            url = f"https://graph.microsoft.com/v1.0/chats/{conv_id}/members"
        else:
            logging.info(
                f"Unknown conversation type '{conversation_type}', "
                f"attempting chat members lookup"
            )
            url = f"https://graph.microsoft.com/v1.0/chats/{conv_id}/members"

        headers = {"Authorization": f"Bearer {access_token}"}
        logging.info(f"Fetching members from: {url}")

        async with httpx.AsyncClient() as client:
            response = await client.get(url, headers=headers)
            logging.info(f"Members API response status: {response.status_code}")
            if response.status_code == 200:
                data = response.json()
                members = data.get("value", [])
                logging.info(f"Found {len(members)} members")
            else:
                logging.error(
                    f"Members API error: {response.status_code} - {response.text}"
                )

        return members
    except Exception as e:
        logging.error(f"Error getting team members: {e}")
        return []


# Install Command


async def install_app_for_team_members(
    ctx: ActivityContext,
    client_id: str,
    client_secret: str,
    default_tenant_id: str,
    app_catalog_team_app_id: str,
    teams_app_id: str,
    conversation_references: Dict[str, Any],
) -> None:
    """
    Installs the app for all team members in their personal scope.
    """
    if not app_catalog_team_app_id and not teams_app_id:
        await ctx.send(
            "Error: Neither APP_CATALOG_TEAM_APP_ID nor TEAMS_APP_ID is configured."
        )
        return

    await ctx.send("Installing app for team members... Please wait.")

    new_app_install_count = 0
    existing_app_install_count = 0
    error_count = 0

    conversation = ctx.activity.conversation
    tenant_id = getattr(conversation, "tenant_id", None) or default_tenant_id

    if not tenant_id:
        await ctx.send("Error: Could not determine tenant ID.")
        return

    team_members = await get_team_members(
        ctx, client_id, client_secret, default_tenant_id
    )

    if not team_members:
        await ctx.send(
            "Could not retrieve team members. "
            "Make sure this command is run in a team or group chat."
        )
        return

    proactive_helper = ProactiveAppInstallationHelper(
        client_id, client_secret, app_catalog_team_app_id, teams_app_id
    )

    # Get a known user ID for catalog lookup
    known_user_id = None
    for m in team_members:
        m_id = m.get("userId") or m.get("id")
        if m_id and not m_id.startswith("29:"):
            known_user_id = m_id
            break

    for member in team_members:
        user_id = member.get("userId") or member.get("id")

        if user_id and user_id not in conversation_references:
            try:
                status_code = await proactive_helper.install_app_in_personal_scope(
                    tenant_id, user_id, known_user_id=known_user_id
                )

                if status_code == 409:
                    existing_app_install_count += 1
                elif status_code == 201:
                    new_app_install_count += 1
                else:
                    error_count += 1
            except Exception as e:
                logging.error(f"Error installing app for user {user_id}: {e}")
                error_count += 1

    await ctx.send(
        f"**Installation Complete**\n\n"
        f"- Newly Installed: {new_app_install_count}\n"
        f"- Already Installed: {existing_app_install_count}\n"
        f"- Errors: {error_count}"
    )


# Send Command


async def send_proactive_notification(
    ctx: ActivityContext,
    bot_id: str,
    client_id: str,
    client_secret: str,
    default_tenant_id: str,
    app_catalog_team_app_id: str,
    teams_app_id: str,
    conversation_references: Dict[str, Any],
) -> None:
    """
    Sends a proactive message to all team members using the SDK's built-in
    conversation creation and proactive send APIs.

    Key insights from SDK source:
    - ctx.api uses the service URL from the incoming activity, which is the
      correct tenant-specific URL.
    - ConversationResource model requires activityId and serviceUrl, but the
      Bot Framework API only returns {"id": "..."} for 1:1 conversations
      without an initial activity, causing Pydantic validation errors.
      We work around this by making the HTTP call directly.
    """
    await ctx.send("Sending proactive notifications... Please wait.")

    team_members = await get_team_members(
        ctx, client_id, client_secret, default_tenant_id
    )

    if not team_members:
        await ctx.send(
            "Could not retrieve team members. "
            "Make sure this command is run in a team or group chat."
        )
        return

    sent_count = 0
    error_count = 0

    conversation = ctx.activity.conversation
    tenant_id = getattr(conversation, "tenant_id", None) or default_tenant_id

    if not tenant_id:
        await ctx.send("Error: Could not determine tenant ID.")
        return

    # Use the service URL from the incoming activity (ctx.api) — this is the
    # correct tenant-specific URL.
    service_url = ctx.api.service_url
    logging.info(f"Proactive send: using service_url={service_url}")

    # Ensure the bot app is installed for all members first.
    proactive_helper = ProactiveAppInstallationHelper(
        client_id, client_secret, app_catalog_team_app_id, teams_app_id
    )

    # Find a known user AAD ID for catalog lookup
    current_user_id = None
    for m in team_members:
        m_id = m.get("userId") or m.get("id")
        if m_id and not m_id.startswith("29:"):
            current_user_id = m_id
            break

    logging.info(f"Using known_user_id={current_user_id} for catalog lookup")
    await ctx.send("Ensuring bot is installed for all members...")

    for member in team_members:
        user_id = member.get("userId") or member.get("id")
        if user_id:
            try:
                status = await proactive_helper.install_app_in_personal_scope(
                    tenant_id, user_id, known_user_id=current_user_id
                )
                name = member.get("displayName", "Unknown")
                if status == 201:
                    logging.info(f"Installed bot for {name}")
                elif status == 409:
                    logging.info(f"Bot already installed for {name}")
                else:
                    logging.warning(f"Install returned {status} for {name}")
            except Exception as e:
                logging.error(f"Failed to install bot for {user_id}: {e}")

    # Brief pause to allow installations to propagate
    await asyncio.sleep(2)

    for member in team_members:
        user_aad_id = member.get("userId") or member.get("id")
        display_name = member.get("displayName", "Unknown")

        if not user_aad_id:
            continue

        try:
            # Use ctx.api instead of app.api — ctx.api has the correct
            # tenant-specific service URL from the incoming activity.
            params = CreateConversationParams(
                bot=Account(id=bot_id, role="bot"),
                members=[Account(id=user_aad_id, role="user")],
                tenant_id=tenant_id,
                is_group=False,
            )
            payload = params.model_dump(by_alias=True)
            logging.info(
                f"Creating conversation for {display_name} "
                f"with payload: {json.dumps(payload)}"
            )

            # Make the HTTP call directly and parse only the 'id' field
            # to avoid the ConversationResource Pydantic validation error.
            try:
                response = await ctx.api.conversations.http.post(
                    f"{service_url}/v3/conversations",
                    json=payload,
                )
                response_data = response.json()
            except httpx.HTTPStatusError as http_err:
                error_body = (
                    http_err.response.text
                    if http_err.response
                    else "no response body"
                )
                logging.error(
                    f"Create conversation failed for {display_name}: "
                    f"{http_err.response.status_code} - {error_body}"
                )
                error_count += 1
                continue

            conversation_id = response_data.get("id")

            if not conversation_id:
                logging.error(
                    f"No conversation ID returned for {display_name}: "
                    f"{response_data}"
                )
                error_count += 1
                continue

            logging.info(
                f"Created conversation for {display_name}: {conversation_id}"
            )

            # Build a ConversationReference with the correct service URL
            # so ctx.send() dispatches to the right endpoint.
            conv_ref = ConversationReference(
                channel_id="msteams",
                service_url=service_url,
                bot=Account(id=bot_id, role="bot"),
                conversation=ConversationAccount(
                    id=conversation_id, conversation_type="personal"
                ),
            )

            await ctx.send(
                "Proactive hello! This is a proactive message from the Auth Bot.",
                conversation_ref=conv_ref,
            )
            sent_count += 1
            logging.info(f"  Sent proactive message to {display_name}")

        except Exception as e:
            logging.error(
                f"Error sending proactive message to "
                f"{display_name} ({user_aad_id}): {e}"
            )
            error_count += 1

    await ctx.send(
        f"**Notification Complete**\n\n"
        f"- Messages Sent: {sent_count}\n"
        f"- Errors: {error_count}"
    )
