import os
import asyncio
from azure.core.credentials import AzureKeyCredential
from azure.search.documents.indexes.aio import SearchIndexClient
from utils import delete_index

async def main():
    index_name = 'faq-index'
    search_api_key = os.getenv('AZURE_SEARCH_KEY')
    search_api_endpoint = os.getenv('AZURE_SEARCH_ENDPOINT')

    if not search_api_key or not search_api_endpoint:
        raise ValueError("Azure Search key or endpoint is not set in environment variables.")

    credentials = AzureKeyCredential(search_api_key)

    # Use `async with` to ensure the client is properly closed
    async with SearchIndexClient(endpoint=search_api_endpoint, credential=credentials) as search_index_client:
        await delete_index(search_index_client, index_name)

if __name__ == "__main__":
    asyncio.run(main())
