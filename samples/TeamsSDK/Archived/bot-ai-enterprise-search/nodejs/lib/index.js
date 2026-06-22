"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.generateEmbeddingForUserPromptAsync = generateEmbeddingForUserPromptAsync;
exports.uploadTextFileToBlobAsync = uploadTextFileToBlobAsync;
exports.uploadPdfFileToBlobAsync = uploadPdfFileToBlobAsync;
// Import required packages
const restify = __importStar(require("restify"));
// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const botbuilder_1 = require("botbuilder");
// This bot's main dialog.
const teamsBot_1 = require("./teamsBot");
const config_1 = __importDefault(require("./config"));
const textsplitters_1 = require("@langchain/textsplitters");
const openai_1 = __importDefault(require("openai"));
const fs = require('fs');
const redis_1 = require("redis");
const { v1: uuidv1 } = require("uuid");
// Redis DB client creation.
const redisClient = (0, redis_1.createClient)({
    url: process.env.REDIS_CONNECTION
});
//Blob storage client creation.
const { BlobServiceClient } = require("@azure/storage-blob");
const azureOpenApiKey = config_1.default.azureOpenApiKey;
const PREFIX = "enterprisedoc"; // Prefix for the document keys
const INDEX_NAME = "enterprise-docs"; // Name of the search index
const containerName = 'enterprise-search';
const serviceUnavailableCode = "404";
// Azure open ai client creation.
const openai = new openai_1.default({
    apiKey: azureOpenApiKey,
    baseURL: config_1.default.completionModelUrl,
    defaultHeaders: { 'api-key': azureOpenApiKey },
    defaultQuery: { 'api-version': '2023-03-15-preview' }
});
// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const credentialsFactory = new botbuilder_1.ConfigurationServiceClientCredentialFactory({
    MicrosoftAppId: config_1.default.botId,
    MicrosoftAppPassword: config_1.default.botPassword,
    MicrosoftAppType: "SingleTenant",
    MicrosoftAppTenantId: config_1.default.botTenantId,
});
const botFrameworkAuthentication = new botbuilder_1.ConfigurationBotFrameworkAuthentication({}, credentialsFactory);
const adapter = new botbuilder_1.CloudAdapter(botFrameworkAuthentication);
// Catch-all for errors.
const onTurnErrorHandler = async (context, error) => {
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       application insights.
    console.error(`\n [onTurnError] unhandled error: ${error}`);
    // Send a trace activity, which will be displayed in Bot Framework Emulator
    await context.sendTraceActivity("OnTurnError Trace", `${error}`, "https://www.botframework.com/schemas/error", "TurnError");
    // Send a message to the user
    await context.sendActivity(`The bot encountered unhandled error:\n ${error.message}`);
    await context.sendActivity("To continue to run this bot, please fix the bot source code.");
};
// Set the onTurnError for the singleton CloudAdapter.
adapter.onTurnError = onTurnErrorHandler;
// Create the bot that will handle incoming messages.
const bot = new teamsBot_1.TeamsBot();
// Create HTTP server.
const server = restify.createServer();
server.use(restify.plugins.bodyParser());
server.listen(process.env.port || process.env.PORT || 3978, () => {
    console.log(`\nBot Started, ${server.name} listening to ${server.url}`);
});
// Listen for incoming requests.
server.post("/api/messages", async (req, res) => {
    await adapter.process(req, res, async (context) => {
        await bot.run(context);
    });
});
// Create float buffer for embedding vectors.
function float32Buffer(arr) {
    return Buffer.from(new Float32Array(arr).buffer);
}
async function ensureRedisConnectionAsync() {
    if (!redisClient.isOpen) {
        await redisClient.connect();
    }
}
// Generate embeddings for uploaded file contents.
async function generateEmbeddingAsync(pageContent, docUrl) {
    var _a, _b;
    const response = await fetch(config_1.default.embeddingModelUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            'api-key': config_1.default.azureOpenApiKey
        },
        body: JSON.stringify({
            input: pageContent,
            user: "ABC"
        }),
    });
    if (!response.ok) {
        return;
    }
    const responseJson = await response.json();
    if (responseJson === null || responseJson === void 0 ? void 0 : responseJson.error) {
        return;
    }
    const embeddings = (_b = (_a = responseJson === null || responseJson === void 0 ? void 0 : responseJson.data) === null || _a === void 0 ? void 0 : _a[0]) === null || _b === void 0 ? void 0 : _b.embedding;
    if (embeddings) {
        await insertBufferDataInRedisDBAsync(embeddings, pageContent, docUrl);
    }
}
// Insert key and value in redis DB for page contents.
async function insertBufferDataInRedisDBAsync(embeddings, pageContent, docUrl) {
    try {
        const key = PREFIX + ":" + "hr" + ":" + uuidv1();
        const mapping = { context_vector: float32Buffer(embeddings), context: pageContent, docUrl: docUrl };
        await redisClient.hSet(key, mapping);
    }
    catch (err) {
        console.error(err);
    }
}
// Generate embeddings for user prompt.
async function generateEmbeddingForUserPromptAsync(context, userPrompt) {
    var _a, _b;
    try {
        await ensureRedisConnectionAsync();
    }
    catch (ex) {
        console.log(ex);
        return true;
    }
    try {
        const response = await fetch(config_1.default.embeddingModelUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'api-key': config_1.default.azureOpenApiKey
            },
            body: JSON.stringify({
                input: userPrompt,
                user: "ABC"
            }),
        });
        if (!response.ok) {
            return;
        }
        const responseJson = await response.json();
        if (responseJson === null || responseJson === void 0 ? void 0 : responseJson.error) {
            return;
        }
        const embeddings = (_b = (_a = responseJson === null || responseJson === void 0 ? void 0 : responseJson.data) === null || _a === void 0 ? void 0 : _a[0]) === null || _b === void 0 ? void 0 : _b.embedding;
        if (embeddings) {
            return await searchInDBAsync(context, embeddings, userPrompt);
        }
    }
    catch (err) {
        console.error(err);
        return serviceUnavailableCode;
    }
}
// Search user prompt in Redis DB.
async function searchInDBAsync(context, userPromptEmbedding, userPrompt) {
    try {
        // var hybrid_fields = "*";
        // var k = 15;
        var base_query = "*=>[KNN 5 @context_vector $context_vector AS vector_score]";
        var return_fields = ["context_vector", "context", "docUrl", "vector_score"];
        try {
            // Perform a K-Nearest Neighbors vector similarity search
            // Documentation: https://redis.io/docs/stack/search/reference/vectors/#pure-knn-queries
            const results = await redisClient.ft.search(INDEX_NAME, base_query, {
                PARAMS: {
                    context_vector: float32Buffer(userPromptEmbedding)
                },
                SORTBY: 'vector_score',
                DIALECT: 2,
                RETURN: return_fields,
            });
            await parseResultAndCallCompletionAsync(context, results, userPrompt);
        }
        catch (ex) {
            console.log(ex);
        }
    }
    catch (err) {
        console.error(err);
    }
}
// Parse embedding results and call final completion model.
async function parseResultAndCallCompletionAsync(context, results, userPrompt) {
    try {
        const query = userPrompt;
        const contexttokenlimit = 2000;
        const answertokenlimit = 1000;
        const completionModel = 'gpt-35-turbo';
        // Build prompt with retrieved contexts
        const prompt_start = "Prompt: \n\n You are a helpful assistant who can help users with answers to their questions. Using only the context below, you have to answer user's question to the best of your ability.When you create the answer, where applicable, you must indicate which file(s) you picked your answer from. You can do this by adding the URLs with hyperlink at the end of the respective sentence within []. If a sentence is derived from multiple sources, include all relevant URLs. If a sentence is based on general knowledge not attributable to a specific source, you do not need to include a URL. Follow the output format shown below -\n\n Example output response: Sentence 1 [source_url1, source_url2]... where source URL1 and URL2 are the URLs mentioned in the respective context.If the text does not relate to the query, simply state 'Text Not Found'\n\n";
        let promptText = prompt_start;
        const links = [];
        let sum_of_tokencount = 0;
        let tokencount_strings = '';
        // Iterating over the array and printing
        results.documents.forEach(element => {
            let docUrl = element.value['docUrl'];
            const hypelinkTitle = decodeURI(docUrl).split("/").pop();
            docUrl = "<a href=" + docUrl + ">" + hypelinkTitle.substring(0, hypelinkTitle.lastIndexOf('.')) || hypelinkTitle + "</a>";
            links.push(docUrl + "\n\n");
            sum_of_tokencount += (element.value['context'].length);
            if (sum_of_tokencount <= contexttokenlimit) {
                const contextstring = "[" + element.value['docUrl'] + "]" + element.value['context'].toString();
                tokencount_strings += contextstring;
            }
        });
        // Join the contexts with separator
        const joined_tokencount_string = "\n\n---\n\n" + tokencount_strings;
        promptText = (prompt_start + "Context: " +
            "\n\n---\n\n" + joined_tokencount_string + "\n\n---\n\n" +
            "Query:" + userPrompt + "\nAnswer:");
        // Error handling for API response
        try {
            const completion = await openai.chat.completions.create({
                max_tokens: answertokenlimit,
                top_p: 0,
                frequency_penalty: 0,
                presence_penalty: 0,
                temperature: 0,
                model: completionModel,
                messages: [
                    {
                        "role": "system",
                        "content": promptText
                    },
                    {
                        "role": "user",
                        "content": query
                    }
                ]
            });
            // Return the result
            const finalResult = {
                "prompt": promptText,
                "answer": completion.choices[0].message.content, "source": links
            };
            // console.log("Final Answer: " + finalResult.answer + finalResult.source[0]);
            await sendFinalAnswerAsync(context, finalResult);
            return finalResult;
        }
        catch (err) {
            console.log("Error while running the query: " + err.message);
            return serviceUnavailableCode;
        }
    }
    catch (err) {
        console.error(err);
        return serviceUnavailableCode;
    }
}
// Send final answer to user.
async function sendFinalAnswerAsync(context, finalResult) {
    // Convert final result to string.
    let finalResultStr = JSON.stringify(finalResult.answer);
    // Regular expression to match comma-separated values inside square brackets
    const regex = /\[(.*?)\]/g;
    // Array to store all the matches
    const matches = [];
    let match;
    while ((match = regex.exec(finalResultStr))) {
        // Extract the value inside the square brackets (excluding the brackets)
        const valueInsideBrackets = match[1];
        // Split the value by commas to get individual values
        valueInsideBrackets.split(',').map((value) => {
            (matches.push(value.trim()));
        });
    }
    const uniqueFileLinks = [...new Set(matches)]; // Get unique links.
    uniqueFileLinks.forEach(docLink => {
        const hypelinkTitle = decodeURI(docLink).split("/").pop();
        let fileUrl = "<a href=" + docLink + ">" + hypelinkTitle.substring(0, hypelinkTitle.lastIndexOf('.')) || hypelinkTitle + "</a>";
        fileUrl += "</a>";
        const linkToReplace = new RegExp(docLink, 'g');
        finalResultStr = finalResultStr.replace(linkToReplace, fileUrl);
    });
    await context.sendActivity({ type: botbuilder_1.ActivityTypes.Message, text: finalResultStr });
    let reply;
    if (finalResultStr.includes("Text Not Found")) {
        reply = botbuilder_1.MessageFactory.text(`<i>Sorry, I could not find any answer to your query. Please try again with different query or contact admin.</i>`);
    }
    else if (serviceUnavailableCode.includes("404")) {
        reply = botbuilder_1.MessageFactory.text(`<i>Sorry, Service might be busy or error occurred. Please try after some time.</i>`);
    }
    else {
        reply = botbuilder_1.MessageFactory.text(`<i>I hope I have answered your query. Let me know if you have any further questions.</i>`);
    }
    reply.textFormat = 'xml';
    await context.sendActivity(reply);
}
// Upload text file to blob container.
async function uploadTextFileToBlobAsync(fileContentsAsString, fileName) {
    try {
        // Connection string
        const connString = config_1.default.azureStorageConnStr;
        if (!connString)
            throw Error('Azure Storage Connection string not found');
        // Create the BlobServiceClient object with connection string.
        const blobServiceClient = BlobServiceClient.fromConnectionString(connString);
        // Create a unique name for the container.
        // Get a reference to a container
        const containerClient = blobServiceClient.getContainerClient(containerName);
        // Check if container exists or not, if not, create a new container.
        // public access at container level.
        const options = {
            access: 'container'
        };
        // Create container if it does not exist.
        containerClient.createIfNotExists(options);
        const blobName = fileName;
        // Create blob client from container client.
        const blockBlobClient = containerClient.getBlockBlobClient(blobName);
        {
            // public access at container level.
            const options = {
                access: 'blob'
            };
            // Upload text file contents as a blob in Azure storage container.
            await blockBlobClient.upload(fileContentsAsString, fileContentsAsString.length, options);
            // console.log(`Blob was uploaded successfully. requestId: ${uploadBlobResponse.requestId}`);
        }
        // Once blob is created, get it's URL and save it.
        // We need to store blob URL in Redis DB while creating the embedding.
        const fileBlobUrl = blockBlobClient.url;
        // Generate embeddings for file contents.
        await createEmbeddingForFileAsync(fileContentsAsString, fileBlobUrl);
    }
    catch (ex) {
        console.error(ex);
        throw ex;
    }
}
// Upload pdf file to blob container.
async function uploadPdfFileToBlobAsync(filePath, fileName, contents) {
    try {
        // Connection string
        const connString = config_1.default.azureStorageConnStr;
        if (!connString)
            throw Error('Azure Storage Connection string not found');
        // Create the BlobServiceClient object with connection string
        const blobServiceClient = BlobServiceClient.fromConnectionString(connString);
        // Create a unique name for the container.
        // Get a reference to a container
        const containerClient = blobServiceClient.getContainerClient(containerName);
        // Check if container exists or not, if not, create a new container.
        // public access at container level.
        const options = {
            access: 'container'
        };
        // Create container if it does not exist.
        containerClient.createIfNotExists(options);
        const blobName = fileName;
        // Create blob client from container client
        const blockBlobClient = containerClient.getBlockBlobClient(blobName);
        // public access at container level.
        {
            const options = {
                access: 'blob'
            };
            const readableStream = fs.createReadStream(filePath, options);
            // Upload data to block blob using a readable stream
            const uploadBlobResponse = await blockBlobClient.uploadStream(readableStream);
            console.log(`Blob was uploaded successfully. requestId: ${uploadBlobResponse.requestId}`);
        }
        // Once blob is created, get it's URL and save it.
        // We need to store blob URL in Redis DB while creating the embedding.
        // Generate embeddings for file contents.
        await createEmbeddingForFileAsync(contents, blockBlobClient.url);
    }
    catch (ex) {
        console.log(ex);
    }
}
// Create embedding for the file contents.
async function createEmbeddingForFileAsync(fileContentsAsString, docUrl) {
    // Create the chunks for file contents and generate embeddings.
    const splitter = new textsplitters_1.RecursiveCharacterTextSplitter({
        chunkSize: 400,
        chunkOverlap: 20,
        separators: ['\n\n', '\n', ' ', '']
    });
    // Create chunks/paragraphs for documents.
    const fileChunks = await splitter.createDocuments([fileContentsAsString]);
    try {
        await ensureRedisConnectionAsync();
    }
    catch (ex) {
        console.log(ex);
    }
    await Promise.all(fileChunks.map(async (element) => {
        try {
            await generateEmbeddingAsync(element.pageContent, docUrl);
        }
        catch (ex) {
            console.log(ex);
        }
    }));
    redisClient.on('error', err => console.log('Redis Server Error', err));
    // Create index if does not exists.
    const VECTOR_DIM = 1536; // length of the vectors
    try {
        // Documentation: https://redis.io/docs/stack/search/reference/vectors/
        await redisClient.ft.create(INDEX_NAME, {
            context_vector: {
                type: 'VECTOR',
                ALGORITHM: 'HNSW',
                TYPE: 'FLOAT32',
                DIM: VECTOR_DIM,
                DISTANCE_METRIC: 'COSINE'
            },
            context: {
                type: 'TEXT'
            },
            docUrl: {
                type: 'TEXT'
            },
        }, {
            ON: 'HASH',
            PREFIX: PREFIX
        });
    }
    catch (e) {
        if (e.message === 'Index already exists') {
            console.log('Index exists already, skipped creation.');
        }
        else {
            // Something went wrong, perhaps RediSearch isn't installed...
            console.error(e);
        }
    }
}
//# sourceMappingURL=index.js.map