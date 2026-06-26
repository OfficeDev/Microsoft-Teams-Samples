import os
import json

def get_translated_res(language_code: str):
    """
    Returns a response in the selected Teams language.
    If the language file is not found, it defaults to English (en-us).
    """
    try:
        # Construct the path to the translation file based on the language code
        file_path = os.path.join(os.path.dirname(__file__), f"../translations/{language_code}/common.json")
        with open(file_path, "r", encoding="utf-8") as file:
            return json.load(file)
    except (FileNotFoundError, json.JSONDecodeError):
        # Default to English if the specified language file doesn't exist
        default_path = os.path.join(os.path.dirname(__file__), "../translations/en-us/common.json")
        with open(default_path, "r", encoding="utf-8") as file:
            return json.load(file)