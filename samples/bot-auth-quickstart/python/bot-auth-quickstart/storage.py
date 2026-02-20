"""
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.

"""

from typing import Dict, Any, Optional


class LocalStorage:
    """Simple in-memory storage for user and conversation state."""

    def __init__(self):
        self._data: Dict[str, Any] = {}

    def get(self, key: str) -> Optional[Any]:
        return self._data.get(key)

    def set(self, key: str, value: Any) -> None:
        self._data[key] = value

    def delete(self, key: str) -> None:
        if key in self._data:
            del self._data[key]


class UserState:
    """User state for tracking token confirmation flow."""

    def __init__(self):
        self.token: str = ""


# Create storage instance
storage = LocalStorage()


def get_user_state(user_id: str) -> UserState:
    """Get or create user state for a given user ID."""
    key = f"user_{user_id}"
    state = storage.get(key)
    if state is None:
        state = UserState()
        storage.set(key, state)
    return state


def set_user_state(user_id: str, state: UserState) -> None:
    """Save user state for a given user ID."""
    key = f"user_{user_id}"
    storage.set(key, state)
