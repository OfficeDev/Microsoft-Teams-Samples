# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from urllib.parse import urlparse, urljoin, quote

from requests_oauthlib import OAuth2Session

RESOURCE = "https://graph.microsoft.com"
API_VERSION = "v1.0"


class SimpleGraphClient:
    def __init__(self, token: str):
        self.token = token
        self.client = OAuth2Session(
            token={"access_token": token, "token_type": "Bearer"}
        )

    async def search_mail_inbox(self, search_query: str):
        query = quote(search_query)

        # Searches the user's mail Inbox using the Microsoft Graph API
        search_filter = {
            "requests": [
                {
                    "entityTypes": ["message"],
                    "query": {"queryString": query},
                    "from": 0,
                    "size": 20,
                }
            ]
        }
        response = self.client.post(
            self.api_endpoint("search/query"),
            headers={"Content-Type": "application/json"},
            json=search_filter,
        )
        response_json = response.json()
        total_results = response_json["value"][0]["hitsContainers"][0]["total"]
        if int(total_results) > 0:
            return response_json["value"][0]["hitsContainers"][0]["hits"]

        return {}

    def api_endpoint(self, url):
        """Convert a relative path such as /me/photo/$value to a full URI based
        on the current RESOURCE and API_VERSION settings.
        """
        if urlparse(url).scheme in ["http", "https"]:
            return url
        return urljoin(f"{RESOURCE}/{API_VERSION}/", url.lstrip("/"))
