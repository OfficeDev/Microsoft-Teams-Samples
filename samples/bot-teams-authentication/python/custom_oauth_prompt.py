# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

"""
Custom OAuthPrompt with Bug Fix

This is a minimal wrapper around the standard OAuthPrompt that fixes a known bug
where the SDK tries to access .token on an ErrorResponse object.

Bug: AttributeError: 'ErrorResponse' object has no attribute 'token'
Location: botbuilder.dialogs.prompts.oauth_prompt line ~507

This wrapper only overrides the _recognize_token method to add the hasattr check.
All other functionality uses the standard SDK implementation.
"""

from typing import Dict
from botbuilder.dialogs.prompts import OAuthPrompt as SDKOAuthPrompt, OAuthPromptSettings
from botbuilder.dialogs import DialogContext
from botbuilder.dialogs.prompts.prompt_recognizer_result import PromptRecognizerResult
from botbuilder.core import TurnContext
from botbuilder.schema import TokenResponse, Activity
from botframework.connector.token_api.models import TokenExchangeRequest
from http import HTTPStatus
import logging


class OAuthPrompt(SDKOAuthPrompt):
    """
    Fixed version of OAuthPrompt that safely handles ErrorResponse objects.
    
    Usage:
        Replace: from botbuilder.dialogs.prompts import OAuthPrompt
        With:    from custom_oauth_prompt import OAuthPrompt
    """

    async def _recognize_token(self, dialog_context: DialogContext) -> Dict:
        """
        Override to fix the ErrorResponse.token bug.
        
        Original SDK code doesn't check if token_exchange_response has a 'token' 
        attribute before accessing it, causing AttributeError when an ErrorResponse 
        is returned.
        """
        context = dialog_context.context

        # Handle token exchange for Teams SSO
        if (
            context.activity.name == "signin/tokenExchange"
            and context.activity.value
        ):
            try:
                # Attempt token exchange
                if hasattr(context.adapter, "exchange_token"):
                    # For adapters that support token exchange directly
                    token_exchange_response = await context.adapter.exchange_token(
                        context,
                        self._settings.connection_name,
                        context.activity.from_property.id,
                        TokenExchangeRequest(token=context.activity.value.get("token")),
                    )
                else:
                    # For CloudAdapter, use UserTokenClient
                    from botbuilder.core.cloud_adapter_base import CloudAdapterBase
                    token_client = context.turn_state.get(CloudAdapterBase.USER_TOKEN_CLIENT_KEY)
                    
                    if token_client:
                        token_exchange_response = await token_client.exchange_token(
                            context.activity.from_property.id,
                            self._settings.connection_name,
                            context.activity.channel_id,
                            TokenExchangeRequest(token=context.activity.value.get("token")),
                        )
                    else:
                        token_exchange_response = None
            except Exception as ex:
                logging.warning(f"Token exchange failed: {ex}")
                token_exchange_response = None

            # **BUG FIX**: Check if token attribute exists before accessing
            if not token_exchange_response or not hasattr(token_exchange_response, 'token') or not token_exchange_response.token:
                # Token exchange failed - send failure response
                await context.send_activity(
                    self._get_token_exchange_invoke_response(
                        int(HTTPStatus.PRECONDITION_FAILED),
                        "The bot is unable to exchange token. Proceed with regular login.",
                    )
                )
                # Return failed result instead of None
                result = PromptRecognizerResult()
                result.succeeded = False
                return result
            else:
                # Token exchange succeeded
                await context.send_activity(
                    self._get_token_exchange_invoke_response(
                        int(HTTPStatus.OK), None, context.activity.value.get("id")
                    )
                )
                
                # Return successful result with token
                token = TokenResponse(
                    connection_name=self._settings.connection_name,
                    token=token_exchange_response.token,
                    expiration=token_exchange_response.expiration,
                )
                result = PromptRecognizerResult()
                result.succeeded = True
                result.value = token
                return result

        # For all other cases, use the parent implementation
        return await super()._recognize_token(dialog_context)

    def _get_token_exchange_invoke_response(
        self, status: int, failure_detail: str = None, activity_id: str = None
    ) -> Activity:
        """Helper to create token exchange invoke response."""
        from botbuilder.schema import (
            Activity,
            ActivityTypes,
            TokenExchangeInvokeResponse,
        )
        from botbuilder.core import InvokeResponse

        token_exchange_invoke_response = TokenExchangeInvokeResponse(
            id=activity_id,
            connection_name=self._settings.connection_name,
            failure_detail=failure_detail,
        )

        return Activity(
            type=ActivityTypes.invoke_response,
            value=InvokeResponse(
                status=status, body=token_exchange_invoke_response
            ),
        )
