# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import urllib.parse
from botbuilder.core import CardFactory, MessageFactory, TurnContext, UserState
from botbuilder.schema import (
    ThumbnailCard,
    CardImage,
    HeroCard,
    CardAction,
    ActionTypes,
    InvokeResponse
)
from bs4 import BeautifulSoup
from botbuilder.schema.teams import (
    MessagingExtensionAction,
    MessagingExtensionAttachment,
    MessagingExtensionQuery,
    MessagingExtensionSuggestedAction,
    MessagingExtensionActionResponse,
    MessagingExtensionResult,
    MessagingExtensionResponse,
    TaskModuleContinueResponse,
    TaskModuleTaskInfo,
)
import requests
from http import HTTPStatus
from botbuilder.core.teams import TeamsActivityHandler
from simple_graph_client import SimpleGraphClient


class TeamsMessagingExtensionsSearchAuthConfigBot(TeamsActivityHandler):
    def __init__(
        self, user_state: UserState, connection_name: str, site_url: str,
    ):
        if user_state is None:
            raise Exception(
                "[TeamsMessagingExtensionsSearchAuthConfigBot]: Missing parameter. user_state is required"
            )
        if connection_name is None:
            raise Exception(
                "[TeamsMessagingExtensionsSearchAuthConfigBot]: Missing parameter. connection_name is required"
            )
        if site_url is None:
            raise Exception(
                "[TeamsMessagingExtensionsSearchAuthConfigBot]: Missing parameter. site_url is required"
            )
        self.user_state = user_state
        self.connection_name = connection_name
        self.site_url = site_url
        self.user_config_property = user_state.create_property("UserConfiguration")

    async def on_turn(self, turn_context: TurnContext):
        await super().on_turn(turn_context)

        # Save any state changes that might have occurred during the turn.
        await self.user_state.save_changes(turn_context, False)

    async def on_teams_messaging_extension_configuration_query_settings_url(
        self, turn_context: TurnContext, query: MessagingExtensionQuery
    ) -> MessagingExtensionResponse:
        # The user has requested the Messaging Extension Configuration page.
        user_configuration = await self.user_config_property.get(
            turn_context, "UserConfiguration"
        )
        encoded_configuration = ""
        if user_configuration is not None:
            encoded_configuration = urllib.parse.quote_plus(user_configuration)

        return MessagingExtensionResponse(
            compose_extension=MessagingExtensionResult(
                type="config",
                suggested_actions=MessagingExtensionSuggestedAction(
                    actions=[
                        CardAction(
                            type="openUrl",
                            value=f"{self.site_url}/search_settings.html?settings={encoded_configuration}",
                        )
                    ]
                ),
            )
        )

    async def on_invoke_activity(self, turn_context: TurnContext):
        if turn_context.activity.name == "composeExtension/anonymousQueryLink":
            # Handle the anonymous query link
            response = await self.handle_teams_app_based_anonymous_link_query(
                turn_context, turn_context.activity.value
            )
            return InvokeResponse(status=HTTPStatus.OK, body=self.convert_to_dict(response))
        else:
            return await super().on_invoke_activity(turn_context)
        
    async def handle_teams_app_based_anonymous_link_query(self, turn_context: TurnContext, value):
        # Create the Adaptive Card JSON directly
        adaptive_card = {
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Zero Installation Link Unfurling Card",
                    "size": "ExtraLarge",
                },
                {
                    "type": "TextBlock",
                    "text": "Install the app or sign in to view full content of the card.",
                    "size": "Medium",
                },
            ],
        }
        # Create a MessagingExtensionAttachment for the card
        attachment = MessagingExtensionAttachment(
            content_type="application/vnd.microsoft.card.adaptive",
            content=adaptive_card,
        )
        # Create a MessagingExtensionResult
        result = MessagingExtensionResult(
            attachment_layout="list",
            type="auth",
            attachments=[attachment],
        )
        # Return the MessagingExtensionResponse
        return MessagingExtensionResponse(compose_extension=result)
    
    def convert_to_dict(self, response: MessagingExtensionResponse) -> dict:
        """Manually convert MessagingExtensionResponse and MessagingExtensionResult to a JSON-serializable dictionary."""
        compose_extension = response.compose_extension
        if compose_extension is None: return {}
        # Manually serialize the MessagingExtensionResult
        result_dict = {
            "attachmentLayout": compose_extension.attachment_layout,
            "type": compose_extension.type,
            "attachments": [
                {
                    "contentType": attachment.content_type,
                    "content": attachment.content,
                }
                for attachment in compose_extension.attachments
            ],
        }
        # Return the dictionary in the expected format
        return {"composeExtension": result_dict}

    async def on_teams_messaging_extension_configuration_setting(
        self, turn_context: TurnContext, settings
    ):
        # Save the user's settings
        if "state" in settings:
            state = settings["state"]
            if state is not None:
                await self.user_config_property.set(turn_context, state)

    async def on_teams_messaging_extension_query(
        self, turn_context: TurnContext, query: MessagingExtensionQuery,
    ) -> MessagingExtensionResponse:
        search_query = str(query.parameters[0].value).strip()

        user_configuration = await self.user_config_property.get(
            turn_context, "UserConfiguration"
        )
        if user_configuration is not None and "email" in user_configuration:
            return await self._get_auth_or_search_result(
                turn_context, query, search_query
            )

        # The user configuration is NOT set to search Email.
        if search_query is None:
            await turn_context.send_activity(
                MessageFactory.text("You cannot enter a blank string for the search")
            )
            return

        search_results = self._get_search_results(search_query)

        attachments = []
        for obj in search_results:
            hero_card = HeroCard(
                title=obj["name"], tap=CardAction(type="invoke", value=obj)
            )

            attachment = MessagingExtensionAttachment(
                content_type=CardFactory.content_types.hero_card,
                content=HeroCard(title=obj["name"]),
                preview=CardFactory.hero_card(hero_card),
                )
            attachments.append(attachment)
        return MessagingExtensionResponse(
            compose_extension=MessagingExtensionResult(
                type="result", attachment_layout="list", attachments=attachments
            )
        )
    
    async def _get_auth_or_search_result(
        self,
        turn_context: TurnContext,
        query: MessagingExtensionQuery,
        search_query: str,
    ) -> MessagingExtensionResponse:
        # When the Bot Service Auth flow completes, the action.State will contain
        # a magic code used for verification.
        magic_code = ""
        if query.state is not None:
            magic_code = query.state

        token_response = await turn_context.adapter.get_user_token(
            turn_context, self.connection_name, magic_code
        )
        if token_response is None or token_response.token is None:
            # There is no token, so the user has not signed in yet.

            # Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
            sign_in_link = await turn_context.adapter.get_oauth_sign_in_link(
                turn_context, self.connection_name
            )
            return MessagingExtensionResponse(
                compose_extension=MessagingExtensionResult(
                    type="auth",
                    suggested_actions=MessagingExtensionSuggestedAction(
                        actions=[
                            CardAction(
                                type="openUrl",
                                value=sign_in_link,
                                title="Bot Service OAuth",
                            )
                        ]
                    ),
                )
            )
        # User is signed in, so use their token to search email via the Graph Client
        client = SimpleGraphClient(token_response.token)
        search_results = await client.search_mail_inbox(search_query)

        # Here we construct a ThumbnailCard for every attachment, and provide a HeroCard which will be
        # displayed if the user selects that item.
        attachments = []
        for message_meta in search_results:
            message = message_meta["_source"]
            message_from = message["from"] if "from" in message else None
            if message_from:
                subtitle = (
                    f"{message_from['emailAddress']['name']},"
                    f"<{message_from['emailAddress']['address']}>"
                )
            else:
                subtitle = ""

            hero_card = HeroCard(
                title=message["subject"] if "subject" in message else "",
                subtitle=subtitle,
                text=message["bodyPreview"] if "bodyPreview" in message else "",
            )

            thumbnail_card = CardFactory.thumbnail_card(
                ThumbnailCard(
                    title=subtitle,
                    subtitle=message["subject"],
                    images=[
                        CardImage(
                            url="https://botframeworksamples.blob.core.windows.net/samples"
                            "/OutlookLogo.jpg",
                            alt="Outlook Logo",
                        )
                    ],
                )
            )
            attachment = MessagingExtensionAttachment(
                content_type=CardFactory.content_types.hero_card,
                content=hero_card,
                preview=thumbnail_card,
            )
            attachments.append(attachment)

        return MessagingExtensionResponse(
            compose_extension=MessagingExtensionResult(
                type="result", attachment_layout="list", attachments=attachments
            )
        )

    async def on_teams_messaging_extension_submit_action(
        self, turn_context: TurnContext, action: MessagingExtensionAction,
    ) -> MessagingExtensionActionResponse:
        # This method is to handle the 'Close' button on the confirmation Task Module after the user signs out.
        return MessagingExtensionActionResponse(task=None)

    async def on_teams_messaging_extension_fetch_task(
        self, turn_context: TurnContext, action: MessagingExtensionAction,
    ) -> MessagingExtensionActionResponse:
        if action.command_id == "SignOutCommand":
            await turn_context.adapter.sign_out_user(
                turn_context,
                self.connection_name,
                turn_context.activity.from_property.id,
            )
            card = CardFactory.adaptive_card(
                {
                    "actions": [{"type": "Action.Submit", "title": "Close",}],
                    "body": [
                        {
                            "text": "You have been signed out.",
                            "type": "TextBlock",
                            "weight": "bolder",
                        },
                    ],
                    "type": "AdaptiveCard",
                    "version": "1.0",
                }
            )

            task_info = TaskModuleTaskInfo(
                card=card, height=200, title="Adaptive Card Example", width=400
            )
            continue_response = TaskModuleContinueResponse(
                value=task_info
            )
            return MessagingExtensionActionResponse(task=continue_response)
        
        return None

    async def on_teams_messaging_extension_select_item(
        self, turn_context: TurnContext, query
    ) -> MessagingExtensionResponse:
        hero_card = HeroCard(
            title=query["name"],
            subtitle=query["summary"],
            buttons=[
                CardAction(
                    type="openUrl", value=f"https://pypi.org/project/{query['name']}"
                )
            ],
        )
        attachment = MessagingExtensionAttachment(
            content_type=CardFactory.content_types.hero_card, content=hero_card
        )

        return MessagingExtensionResponse(
            compose_extension=MessagingExtensionResult(
                type="result", attachment_layout="list", attachments=[attachment]
            )
        )

    def _get_search_results(self, query: str):
        """
        Fetch the top 10 search results from PyPI using the JSON API.

        :param query: The package name or search query
        :return: List of search results
        """
        # Using PyPI's JSON API endpoint
        search_url = f"https://pypi.org/pypi/{urllib.parse.quote(query)}/json"
        search_results = []

        try:
            # First try exact match
            response = requests.get(
                search_url,
                headers={
                    "Accept": "application/json",
                    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
                },
                timeout=10
            )
            
            if response.status_code == 200:
                # Found exact match
                data = response.json()
                info = data.get("info", {})
                search_results.append({
                    "name": info.get("name", "Unknown"),
                    "version": info.get("version", "N/A"),
                    "summary": info.get("summary", "No description provided.")
                })
            else:
                # If no exact match, use the search endpoint
                search_url = "https://pypi.org/search/"
                params = {
                    "q": query,
                    "o": "",  # ordering
                }
                response = requests.get(
                    search_url,
                    params=params,
                    headers={
                        "Accept": "text/html",
                        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
                    },
                    timeout=10
                )
                response.raise_for_status()

                # Parse the HTML response to extract package information
                soup = BeautifulSoup(response.text, "html.parser")
                results = soup.select("a.package-snippet")

                for package in results[:10]:  # Limit to 10 results
                    name = package.select_one(".package-snippet__name")
                    version = package.select_one(".package-snippet__version")
                    description = package.select_one(".package-snippet__description")

                    search_results.append({
                        "name": name.text.strip() if name else "Unknown",
                        "version": version.text.strip() if version else "N/A",
                        "summary": description.text.strip() if description else "No description provided."
                    })

            print(f"Found {len(search_results)} results")
            for result in search_results:
                print(f"Package: {result['name']} ({result['version']})")

        except requests.exceptions.Timeout:
            print("Request timed out")
        except requests.exceptions.RequestException as e:
            print(f"Error making request: {e}")
        except Exception as e:
            print(f"Unexpected error: {e}")

        return search_results