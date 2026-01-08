# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

"""
Proactive Messaging Bot using Microsoft Teams SDK for Python.

This bot demonstrates how to:
1. Store conversation references when users interact with the bot
2. Send proactive messages to users using stored conversation references
3. Handle welcome messages when the bot is installed
4. Handle message activities from users
"""

import os
from typing import Dict, Any


class ConversationReferenceStore:
    """Store for conversation references used in proactive messaging."""

    def __init__(self):
        self._references: Dict[str, Any] = {}

    def add_reference(self, conversation_id: str, reference: Any) -> None:
        """Add or update a conversation reference."""
        self._references[conversation_id] = reference

    def get_reference(self, conversation_id: str) -> Any:
        """Get a conversation reference by conversation ID."""
        return self._references.get(conversation_id)

    def get_all_references(self) -> Dict[str, Any]:
        """Get all stored conversation references."""
        return self._references.copy()

    def remove_reference(self, conversation_id: str) -> None:
        """Remove a conversation reference."""
        if conversation_id in self._references:
            del self._references[conversation_id]

    def clear(self) -> None:
        """Clear all conversation references."""
        self._references.clear()


# Global conversation reference store
conversation_references = ConversationReferenceStore()
