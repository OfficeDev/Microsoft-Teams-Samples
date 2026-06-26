# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import json
import os

# Simulated memory store using a dictionary
_store = {}

# App-specific namespace (optional, like using appId in JS)
APP_ID = os.environ.get("MicrosoftAppId", "default_app_id")

def _get_store_namespace():
    if APP_ID not in _store:
        _store[APP_ID] = {}
    return _store[APP_ID]

def store_save(key, value):
    store = _get_store_namespace()
    store[key] = _parse_while_saving(value)
    return _parse_while_retrieving(store.get(key))

def store_fetch(key):
    store = _get_store_namespace()
    return _parse_while_retrieving(store.get(key))

def store_remove(key):
    store = _get_store_namespace()
    if key in store:
        del store[key]

def store_update(key, value):
    store_remove(key)
    return store_save(key, value)

def store_length():
    store = _get_store_namespace()
    return len(store)

def store_check(key):
    store = _get_store_namespace()
    return bool(_parse_while_retrieving(store.get(key)))

def _parse_while_saving(data):
    if isinstance(data, str):
        try:
            return json.loads(data)
        except json.JSONDecodeError:
            return data
    return data

def _parse_while_retrieving(data):
    if isinstance(data, str):
        try:
            return json.loads(data)
        except json.JSONDecodeError:
            return data
    return data

