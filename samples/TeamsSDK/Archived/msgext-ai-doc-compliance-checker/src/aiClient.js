// aiClient.js
const { OpenAIClient, AzureKeyCredential } = require("@azure/openai");
const { RecursiveCharacterTextSplitter } = require("langchain/text_splitter");
const config = require("./config");

// Initialize Azure OpenAI client
const client = new OpenAIClient(config.endpoint, new AzureKeyCredential(config.apiKey));

// Function to check compliance of checklist items against a predefined document
async function checkCompliance(checkListItems, predefinedDocument) {
  // System prompt to instruct the AI model on how to assess compliance
  const systemPrompt = `You are an expert in assessing and comparing contracts and contractual terms. You must compare proposals to a predefined policy guideline and make decisions on whether a proposal either meets or fails compliance against the policy guideline. Compare the percentage or numeric values shared in the proposal with those in the policy guideline, and decide based on the context and your expertise whether the proposal complies with the policy or not. Remember that compliance depends on the context of the item, and therefore decide on a case-by-case basis - smaller or larger need not always mean compliant. For example, a 45-day notice for audit when the guideline is 30 days is non-compliant.

Output:
For each item on the checklist: 
- If the proposal document complies with the policy guideline, respond with "Yes", followed by the relevant context based on which the decision of compliance or non-compliance was taken.
- If the proposal document doesn't comply with the policy guideline, respond with "No", followed by the relevant context based on which the decision of compliance or non-compliance was taken.`;

  // Split the predefined document into chunks for processing
  const splitter = new RecursiveCharacterTextSplitter({
    chunkSize: 32000,
    chunkOverlap: 2,
  });

  const fileChunks = await splitter.createDocuments([predefinedDocument]);
  const responsesResults = [];

  // Process each chunk
  for (let i = 0; i < fileChunks.length; i++) {
    const chunkContent = fileChunks[i].pageContent;
    const isLastChunk = i === fileChunks.length - 1;

    // Create the user prompt for the AI model
    let userPrompt = `Document:\n${chunkContent}\n\nChecklist item:\n`;
    checkListItems.forEach(item => {
      userPrompt += `- ${item.content}\n`;
    });

    try {
      // Prepare the messages to be sent to the AI model
      const messages = [
        { role: "system", content: systemPrompt },
        { role: "user", content: userPrompt }
      ];

      // Call the AI model to get chat completions
      const response = await client.getChatCompletions(config.deploymentId, messages, {
        maxTokens: 2000,
        temperature: 0.2,
        top_p: 1.0
      });

      const responseContent = response.choices[0].message.content;

      // Handle response based on number of chunks
      if (fileChunks.length === 1) {
        responsesResults.push(responseContent);
      } else {
        // For multiple chunks, process each line of the response
        const responseLines = responseContent.split('\n').filter(line => line.trim() !== '');
        responseLines.forEach(content => {
          const yesRegex = /Yes/i;
          if (yesRegex.test(content)) {
            // If the response is "Yes", remove the corresponding checklist item
            const cleanedText = content.replace(/- /g, '');
            const cleanedTextResult = cleanedText.split(':')[0];
            const foundItemIndex = checkListItems.findIndex(item => item.content === cleanedTextResult);

            if (foundItemIndex !== -1) {
              checkListItems.splice(foundItemIndex, 1);
              responsesResults.push(content);
            }
          } else if (isLastChunk) {
            // Add response content for "No" or last chunk
            responsesResults.push(content);
          }
        });
      }
    } catch (error) {
      console.error('Error during getCompletions:', error);
      throw error;
    }
  }

  // Return the combined responses
  return responsesResults.join('\n');
}

// Function to prepare checklist items from guidelines text
async function prepareChecklistItems(guidelinesText) {
  const systemPrompt = `Pick out the 7 primary checklist items based on the below provided data. Each item should be concise and you should extract the corresponding numeric limit for each checklist item:
For example:
        1. Payment Terms: Ensure invoices are paid within 60 days unless otherwise agreed
        2. Quality Standards: Defective goods must be reported and replaced or refunded within 30 days of delivery.`;

  try {
    // Prepare the messages to be sent to the AI model
    const messages = [
      { role: "system", content: systemPrompt },
      { role: "user", content: guidelinesText }
    ];

    // Call the AI model to get chat completions
    const response = await client.getChatCompletions(config.deploymentId, messages, { maxTokens: 2000 });

    // Extract the response content from the AI model's response
    return response.choices[0].message.content;
  } catch (error) {
    console.error('Error during getCompletions:', error);
    throw error;
  }
}

module.exports = { checkCompliance, prepareChecklistItems };