# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
import json

class MemoryStore:
    def __init__(self, app_id):
        self.app_id = app_id
        self.store = {}

    def set_item(self, key, value):
        self.store[key] = value

    def get_item(self, key):
        return self.store.get(key)

    def remove_item(self, key):
        self.store.pop(key, None)

    @property
    def length(self):
        return len(self.store)


# Initialize store with Microsoft App ID
store = MemoryStore(os.getenv("MicrosoftAppId"))

# Helper functions
def parse_while_saving(data):
    try:
        return json.loads(data)
    except (TypeError, json.JSONDecodeError):
        return data

def parse_while_retrieving(data):
    try:
        return json.loads(data)
    except (TypeError, json.JSONDecodeError):
        return data


# Main storage service functions
def store_save(key, value):
    store.set_item(key, parse_while_saving(value))
    return parse_while_retrieving(store.get_item(key))

def store_fetch(key):
    return parse_while_retrieving(store.get_item(key))

def store_remove(key):
    store.remove_item(key)

def store_update(key, value):
    store.remove_item(key)
    store.set_item(key, parse_while_saving(value))
    return parse_while_retrieving(store.get_item(key))

def store_length():
    return store.length

def store_check(key):
    return bool(parse_while_retrieving(store.get_item(key)))
