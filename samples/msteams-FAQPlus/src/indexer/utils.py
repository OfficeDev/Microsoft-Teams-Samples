import asyncio
from typing import List, Dict, Any
from azure.search.documents.indexes.aio import SearchIndexClient
from azure.search.documents.aio import SearchClient
from azure.search.documents.indexes.models import SearchIndex, SearchField, CorsOptions
from azure.search.documents.models import IndexingResult

WAIT_TIME = 4000

def document_key_retriever(document: Dict[str, Any]) -> str:
    return document["id"]

async def delay(time_in_ms: int) -> None:
    await asyncio.sleep(time_in_ms / 1000)

async def delete_index(client: SearchIndexClient, name: str) -> None:
    await client.delete_index(name)

async def upsert_documents(client: SearchClient, documents: List[Dict[str, Any]]) -> List[IndexingResult]:
    return await client.merge_or_upload_documents(documents=documents)

async def create_index_if_not_exists(client: SearchIndexClient, name: str) -> None:
    faq_index = SearchIndex(
        name=name,
        fields=[
            SearchField(name="content", type="Edm.String", searchable=True, filterable=False, retrievable=True, stored=True, sortable=False, facetable=False),
            SearchField(name="title", type="Edm.String", searchable=True, filterable=False, retrievable=True, stored=True, sortable=False, facetable=False),
            SearchField(name="id", type="Edm.String", key=True, searchable=False, filterable=True, retrievable=True, stored=True, sortable=True, facetable=False),
            SearchField(name="lastUpdated", type="Edm.String", searchable=False, filterable=True, retrievable=True, stored=True, sortable=True, facetable=False),
            SearchField(name="url", type="Edm.String", searchable=False, filterable=True, retrievable=True, stored=True, sortable=False, facetable=False)
        ],
        semantic_search={
            "defaultConfiguration": None,
            "configurations": [
                {
                    "name": "default",
                    "prioritizedFields": {
                        "titleField": {
                            "fieldName": "title"
                        },
                        "prioritizedContentFields": [
                            {
                                "fieldName": "content"
                            }
                        ],
                        "prioritizedKeywordsFields": []
                    }
                }
            ]
        },
        similarity={"@odata.type": "#Microsoft.Azure.Search.BM25Similarity"},
        cors_options=CorsOptions(
            allowed_origins=["*"]
        )
    )

    await client.create_or_update_index(faq_index)