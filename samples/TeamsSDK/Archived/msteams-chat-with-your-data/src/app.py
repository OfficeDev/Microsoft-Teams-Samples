"""
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
"""

from aiohttp import web

from api import api
from config import Config

if __name__ == "__main__":
    web.run_app(api, host="localhost", port=Config.PORT)
