"""
Memory Store - Python equivalent of server/services/store.js
Provides pure RAM-based storage (no disk persistence)
"""

import threading

class MemoryStore:
    """
    Memory store that provides pure RAM-based storage
    Data is stored only in memory and will be lost when the application restarts
    Thread-safe operations for concurrent requests
    """
    
    def __init__(self, storage_name='details-tab-app'):
        self._storage_name = storage_name
        self._data = {}
        self._lock = threading.Lock()  # Thread safety for concurrent requests
        self._initialize_defaults()
    
    def _initialize_defaults(self):
        """
        Initialize default values if they don't exist
        Ensures agenda list is always available
        """
        if "agendaList" not in self._data:
            self._data["agendaList"] = []
            print("STORE INIT: Created empty agendaList (RAM only)")

    def get_item(self, key):
        """
        Get item from store - matches store.getItem(key)
        """
        with self._lock:
            value = self._data.get(key)
            print(f"STORE GET: {key} = {value}")
            return value

    def set_item(self, key, value):
        """
        Set item in store - matches store.setItem(key, value)
        """
        with self._lock:
            print(f"STORE SET: {key} = {value}")
            self._data[key] = value
            print(f"STORE STATE: {list(self._data.keys())}")

    def remove_item(self, key):
        """
        Remove item from store - matches store.removeItem(key)
        """
        with self._lock:
            if key in self._data:
                del self._data[key]

    def clear(self):
        """
        Clear all items from store - matches store.clear()
        """
        with self._lock:
            self._data.clear()
    
    def keys(self):
        """
        Get all keys from store
        """
        with self._lock:
            return list(self._data.keys())
    
    def debug_info(self):
        """
        Debug information about stored data
        """
        with self._lock:
            agenda_list = self._data.get('agendaList', [])
            return {
                'storage_name': self._storage_name,
                'storage_type': 'RAM-only (no persistence)',
                'stored_keys': list(self._data.keys()),
                'conversation_id': self._data.get('conversationId'),
                'service_url': self._data.get('serviceUrl'),
                'agenda_list_count': len(agenda_list),
                'agenda_polls': [
                    {
                        'id': poll.get('Id'),
                        'title': poll.get('title'),
                        'is_sent': poll.get('IsSend', False)
                    } for poll in agenda_list
                ],
                'all_data': self._data
            }
    
    def get_agenda_summary(self):
        """
        Get a summary of the agenda list for debugging
        """
        with self._lock:
            agenda_list = self._data.get('agendaList', [])
            return {
                'total_polls': len(agenda_list),
                'polls': [
                    {
                        'id': poll.get('Id'),
                        'title': poll.get('title'),
                        'option1': poll.get('option1'),
                        'option2': poll.get('option2'),
                        'is_sent': poll.get('IsSend', False),
                        'votes': len(poll.get('personAnswered', {}))
                    } for poll in agenda_list
                ]
            }

# Create global store instance (matches Node.js module.exports = new MemoryStorage('details-tab-app'))
store = MemoryStore('details-tab-app')
