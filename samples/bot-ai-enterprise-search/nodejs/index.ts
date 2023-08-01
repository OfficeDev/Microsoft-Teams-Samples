// Import required packages
import * as restify from "restify";

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
import {
  CloudAdapter,
  ConfigurationServiceClientCredentialFactory,
  ConfigurationBotFrameworkAuthentication,
  TurnContext,
  ActivityTypes,
  MessageFactory,
} from "botbuilder";

// This bot's main dialog.
import { TeamsBot } from "./teamsBot";
import config from "./config";
import { RecursiveCharacterTextSplitter } from "langchain/text_splitter";
const fs = require('fs');
import { createClient, SchemaFieldTypes, VectorAlgorithms } from 'redis';

const { v1: uuidv1 } = require("uuid");

// Redis DB client creation.
const redisClient = createClient({
  url: process.env.REDIS_CONNECTION
});

//Blob storage client creation.
const { BlobServiceClient } = require("@azure/storage-blob");

// Azure open AI.
const { Configuration, OpenAIApi } = require("openai");

const azureOpenApiKey = config.azureOpenApiKey;
var PREFIX = "enterprisedoc";  // Prefix for the document keys
const INDEX_NAME = "enterprise-docs"; // Name of the search index
const containerName = 'enterprise-search';
var errorCode = "200";

// Azure Open AI configuration.
const configuration = new Configuration({
  apiKey: azureOpenApiKey,
  basePath: config.completionModelUrl,
  baseOptions: {
    headers: { 'api-key': azureOpenApiKey },
    params: {
      'api-version': '2023-03-15-preview' // this might change. I got the current value from the sample code at https://oai.azure.com/portal/chat
    }
  }
});

// Azure open ai client creation.
const openai = new OpenAIApi(configuration);

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.
const credentialsFactory = new ConfigurationServiceClientCredentialFactory({
  MicrosoftAppId: config.botId,
  MicrosoftAppPassword: config.botPassword,
  MicrosoftAppType: "MultiTenant",
});

const botFrameworkAuthentication = new ConfigurationBotFrameworkAuthentication(
  {},
  credentialsFactory
);

const adapter = new CloudAdapter(botFrameworkAuthentication);

// Catch-all for errors.
const onTurnErrorHandler = async (context: TurnContext, error: Error) => {
  // This check writes out errors to console log .vs. app insights.
  // NOTE: In production environment, you should consider logging this to Azure
  //       application insights.
  console.error(`\n [onTurnError] unhandled error: ${error}`);

  // Send a trace activity, which will be displayed in Bot Framework Emulator
  await context.sendTraceActivity(
    "OnTurnError Trace",
    `${error}`,
    "https://www.botframework.com/schemas/error",
    "TurnError"
  );

  // Send a message to the user
  await context.sendActivity(`The bot encountered unhandled error:\n ${error.message}`);
  await context.sendActivity("To continue to run this bot, please fix the bot source code.");
};

// Set the onTurnError for the singleton CloudAdapter.
adapter.onTurnError = onTurnErrorHandler;

// Create the bot that will handle incoming messages.
const bot = new TeamsBot();

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
  var buffer = Buffer.from(new Float32Array(arr).buffer);
  return buffer;
}

// Generate embeddings for uploaded file contents.
async function generateEmbeddingAsync(pageContent, docUrl) {
  var embeddings = "";

  await fetch(config.embeddingModelUrl, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'api-key': config.azureOpenApiKey
    },
    body: JSON.stringify({
      input: pageContent,
      user: "ABC"
    }),
  })
    .then((response) => {
      if (response.ok) {
        return response.json();
      } else {
      }
    })
    .then((responseJson) => {
      if (responseJson.error) {
      }
      else {
        embeddings = responseJson['data'][0]["embedding"];
        InsertBufferDataInRedisDBAsync(embeddings, pageContent, docUrl);
      }
    });
};

// Insert key and value in redis DB for page contents.
async function InsertBufferDataInRedisDBAsync(Embeddings, pageContent, docUrl) {
  try {
    var key = PREFIX + ":" + "hr" + ":" + uuidv1();
    var mapping = { context_vector: float32Buffer(Embeddings), context: pageContent, docUrl: docUrl };

    Promise.all([redisClient.hSet(key, mapping)]);
  } catch (err) {
    console.error(err);
  }
};

// Generate embeddings for user prompt.
export async function generateEmbeddingForUserPromptAsync(context, userPrompt) {

  try {
    (async () => {
      if (!redisClient.isOpen)
        redisClient.connect();
    })();
  } catch (ex) {
    console.log(ex)
    return true;
  }

  var embeddings = "";
  try {
    await fetch(config.embeddingModelUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'api-key': config.azureOpenApiKey
      },
      body: JSON.stringify({
        input: userPrompt,
        user: "ABC"
      }),
    })
      .then((response) => {
        if (response.ok) {
          return response.json();
        } else {
        }
      })
      .then((responseJson) => {
        if (responseJson.error) {
        }
        else {
          embeddings = responseJson['data'][0]["embedding"];
          var finalResult = SearchInDBAsync(context, embeddings, userPrompt);
          return finalResult;
        }
      });
  }
  catch (err) {
    console.error(err);
    return errorCode;
  }
};

// Search user prompt in Redis DB.
async function SearchInDBAsync(context, userPromptEmbedding, userPrompt) {
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
    } catch (ex) {
      console.log(ex);
    }
  }
  catch (err) {
    console.error(err);
  }
};

// Parse embedding results and call final completion model.
async function parseResultAndCallCompletionAsync(context, results, userPrompt) {
  try {
    var query = userPrompt;
    var contexttokenlimit = 2000;
    var answertokenlimit = 1000;
    var completion_model = 'gpt-35-turbo'

    // Build prompt with retrieved contexts
    var prompt_start = "Prompt: \n\n You are a helpful assistant who can help users with answers to their questions. Using only the context below, you have to answer user's question to the best of your ability.When you create the answer, where applicable, you must indicate which file(s) you picked your answer from. You can do this by adding the URLs with hyperlink at the end of the respective sentence within []. If a sentence is derived from multiple sources, include all relevant URLs. If a sentence is based on general knowledge not attributable to a specific source, you do not need to include a URL. Follow the output format shown below -\n\n Example output response: Sentence 1 [source_url1, source_url2]... where source URL1 and URL2 are the URLs mentioned in the respective context.If the text does not relate to the query, simply state 'Text Not Found'\n\n"
    var promptText = prompt_start;//+ prompt_end
    var links = []
    var sum_of_tokencount = 0

    var tokencount_strings = ''
    // Iterating over the array and printing
    results.documents.forEach(async element => {
      var docUrl = element.value['docUrl'];
      var hypelinkTitle = decodeURI(docUrl).split("/").pop();
      docUrl = "<a href=" + docUrl + ">" + hypelinkTitle.substring(0, hypelinkTitle.lastIndexOf('.')) || hypelinkTitle + "</a>";
      links.push(docUrl + "\n\n");

      sum_of_tokencount += (element.value['context'].length)
      if (sum_of_tokencount <= contexttokenlimit) {
        var contextstring = "[" + element.value['docUrl'] + "]" + element.value['context'].toString()
        tokencount_strings += contextstring
      }
    });

    // Join the contexts with separator
    var joined_tokencount_string = "\n\n---\n\n" + tokencount_strings;

    promptText = (prompt_start + "Context: " +
      "\n\n---\n\n" + joined_tokencount_string + "\n\n---\n\n" +
      "Query:" + userPrompt + "\nAnswer:"
    )

    joined_tokencount_string = joined_tokencount_string.replace('\n', '\n')

    // Error handling for API response
    try {

      const completion = await openai.createChatCompletion({
        max_tokens: answertokenlimit,
        top_p: 0,
        frequency_penalty: 0,
        presence_penalty: 0,
        temperature: 0,
        model: completion_model,
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
      var finalResult = {
        "prompt": promptText,
        "answer": completion.data.choices[0].message.content, "source": links
      }

      // console.log("Final Answer: " + finalResult.answer + finalResult.source[0]);
      await sendFinalAnswerAsync(context, finalResult)

      return finalResult;
    }
    catch (err) {
      console.log("Error while running the query: " + err.message);
      return errorCode;
    }
  }
  catch (err) {
    console.error(err);
    return errorCode;
  }
};

// Send final answer to user.
async function sendFinalAnswerAsync(context, finalResult) {

  // Convert final result to string.
  var finalResultStr = JSON.stringify(finalResult.answer);

  // Regular expression to match comma-separated values inside square brackets
  const regex = /\[(.*?)\]/g;

  // Array to store all the matches
  const matches = [];

  let match;
  while ((match = regex.exec(finalResultStr))) {
    // Extract the value inside the square brackets (excluding the brackets)
    const valueInsideBrackets = match[1];
    // Split the value by commas to get individual values
    const commaSeparatedValues = valueInsideBrackets.split(',').map((value) => {
      (
        matches.push(value.trim())
      )
    });

  }

  var uniqueFileLinks = [...new Set(matches)]; // Get unique links.
  uniqueFileLinks.forEach(docLink => {
    var hypelinkTitle = decodeURI(docLink).split("/").pop();
    var fileUrl = "<a href=" + docLink + ">" + hypelinkTitle.substring(0, hypelinkTitle.lastIndexOf('.')) || hypelinkTitle + "</a>";
    fileUrl += "</a>";
    var linkToReplace = new RegExp(docLink, 'g');
    finalResultStr = finalResultStr.replace(linkToReplace, fileUrl);
  });

  await context.sendActivity({ type: ActivityTypes.Message, text: finalResultStr });
  var reply;

  if (finalResultStr.includes("Text Not Found")) {
    reply = MessageFactory.text(`<i>Sorry, I could not find any answer to your query. Please try again with different query or contact admin.</i>`);
  }
  else if (errorCode.includes("404")) {
    reply = MessageFactory.text(`<i>Sorry, Service might be busy or error occurred. Please try after some time.</i>`);
  }
  else {
    reply = MessageFactory.text(`<i>I hope I have answered your query. Let me know if you have any further questions.</i>`);
  }
  reply.textFormat = 'xml';
  await context.sendActivity(reply);
};

// Upload text file to blob container.
export async function uploadTextFileToBlobAsync(fileContentsAsString, fileName) {
  try {

    // Connection string
    const connString = config.azureStorageConnStr;
    if (!connString) throw Error('Azure Storage Connection string not found');

    // Create the BlobServiceClient object with connection string.
    var blobServiceClient = BlobServiceClient.fromConnectionString(connString);

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
    var blobName = fileName;

    // Create blob client from container client.
    const blockBlobClient = containerClient.getBlockBlobClient(blobName);

    {
      // public access at container level.
      const options = {
        access: 'blob'
      };

      // Upload text file contents as a blob in Azure storage container.
      const uploadBlobResponse = await blockBlobClient.upload(fileContentsAsString, fileContentsAsString.length, options);
      // console.log(`Blob was uploaded successfully. requestId: ${uploadBlobResponse.requestId}`);
    }

    // Once blob is created, get it's URL and save it.
    // We need to store blob URL in Redis DB while creating the embedding.
    var fileBlobUrl = blockBlobClient.url;

    // Generate embeddings for file contents.
    await createEmbeddingForFileAsync(fileContentsAsString, fileBlobUrl);
  }
  catch (ex) {
    console.error(ex);
    throw ex;
  }
}

// Upload pdf file to blob container.
export async function uploadPdfFileToBlobAsync(filePath, fileName, contents) {
  try {

    // Connection string
    const connString = config.azureStorageConnStr;
    if (!connString) throw Error('Azure Storage Connection string not found');

    // Create the BlobServiceClient object with connection string
    const blobServiceClient = BlobServiceClient.fromConnectionString(
      connString
    );

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

    var blobName = fileName;
    // Create blob client from container client
    const blockBlobClient = containerClient.getBlockBlobClient(blobName);

    // public access at container level.
    {
      const options = {
        access: 'blob'
      };

      const readableStream = fs.createReadStream(filePath, options);

      // Upload data to block blob using a readable stream
      var uploadBlobResponse = await blockBlobClient.uploadStream(readableStream);
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
  const splitter = new RecursiveCharacterTextSplitter({
    chunkSize: 400,
    chunkOverlap: 20,
    separators: ['\n\n', '\n', ' ', '']
  });

  // Create chunks/paragraphs for documents.
  const fileChunks = await splitter.createDocuments([fileContentsAsString]);

  try {
    (async () => {
      if (!redisClient.isOpen)
        redisClient.connect();
    })();
  } catch (ex) {
    console.log(ex)
  }

  // Iterating over the array and printing
  fileChunks.forEach(async element => {
    try {
      await generateEmbeddingAsync(element.pageContent, docUrl);
    } catch (ex) {
      console.log(ex);
    }
  });

  redisClient.on('error', err => console.log('Redis Server Error', err));

  // Create index if does not exists.
  const VECTOR_DIM = 1536; // length of the vectors

  try {
    // Documentation: https://redis.io/docs/stack/search/reference/vectors/
    await redisClient.ft.create(INDEX_NAME, {
      context_vector: {
        type: SchemaFieldTypes.VECTOR,
        ALGORITHM: VectorAlgorithms.HNSW,
        TYPE: 'FLOAT32',
        DIM: VECTOR_DIM,
        DISTANCE_METRIC: 'COSINE'
      },
      context: {
        type: SchemaFieldTypes.TEXT
      },
      docUrl: {
        type: SchemaFieldTypes.TEXT
      },
    }, {
      ON: 'HASH',
      PREFIX: PREFIX
    });
  } catch (e) {
    if (e.message === 'Index already exists') {
      console.log('Index exists already, skipped creation.');
    } else {
      // Something went wrong, perhaps RediSearch isn't installed...
      console.error(e);
    }
  }
};
