Configure below values in the `env` file.

https=true

AzureOpenAIEndpoint = "https://{Your Azure Open AI Endpoint Service Name}.openai.azure.com/"
AzureOpenAIApiKey= "{Your Azure Open AI API Key}"
AzureOpenAIDeploymentName = "text-embedding-ada-002"

CosmosDBEndpoint = "https://{Your Cosmos db endpoint name}.documents.azure.com:443/"
CosmosDBKey = "{Your Cosmos db key}"
CosmosDBDatabaseId = "{Your cosmos db id}"
CosmosDBContainerId = "{Your cosmos db container id}"
SimilarityScore = 0.70

Run the sample via pressing F5 in Visual Studio Code.

Use the endpoing URL to test the API in Postman or any other API testing tool.
like: http://localhost:3978/search?query= What is Self-Consistency?
