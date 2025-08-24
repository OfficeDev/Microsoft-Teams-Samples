import asyncio
import os
import time
from azure.core.credentials import AzureKeyCredential
from azure.search.documents.indexes.aio import SearchIndexClient
from azure.search.documents.aio import SearchClient
from utils import create_index_if_not_exists, delay, upsert_documents
from data import get_all_data

async def main():
    index_name = 'faq-index'

    search_api_key = os.getenv('AZURE_SEARCH_KEY')
    search_api_endpoint = os.getenv('AZURE_SEARCH_ENDPOINT')

    if not search_api_key or not search_api_endpoint:
        raise ValueError("Azure Search key or endpoint is not set in environment variables.")

    credentials = AzureKeyCredential(search_api_key)

    # Use `async with` to ensure clients are properly closed
    async with SearchIndexClient(endpoint=search_api_endpoint, credential=credentials) as search_index_client:
        await create_index_if_not_exists(search_index_client, index_name)

        # Wait 5 seconds for the index to be created
        await asyncio.sleep(5)

        async with SearchClient(endpoint=search_api_endpoint, index_name=index_name, credential=credentials) as search_client:
            data = get_all_data()
            await upsert_documents(search_client, data)

if __name__ == '__main__':
    asyncio.run(main())
