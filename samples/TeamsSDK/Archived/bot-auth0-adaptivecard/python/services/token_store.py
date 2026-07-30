from typing import Optional
import threading

class TokenStore:
    _instance = None
    _lock = threading.Lock()

    def __new__(cls):
        if cls._instance is None:
            with cls._lock:
                if cls._instance is None:
                    cls._instance = super(TokenStore, cls).__new__(cls)
                    cls._instance._tokens = {}
        return cls._instance

    def set_token(self, user_id: str, token: str):
        self._tokens[user_id] = token

    def get_token(self, user_id: str) -> Optional[str]:
        return self._tokens.get(user_id)

    def remove_token(self, user_id: str):
        self._tokens.pop(user_id, None)

    def clear_tokens(self):
        self._tokens.clear()