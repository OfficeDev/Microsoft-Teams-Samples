// openaiClient.js
const { OpenAIClient, AzureKeyCredential } = require("@azure/openai");
const { RecursiveCharacterTextSplitter } = require("langchain/text_splitter");
const config = require("./config");

// Function to check compliance of checklist items against a predefined document
async function checkCompliance(checkListItems, predefinedDocument) 
{
     // System prompt to instruct the AI model on how to assess compliance
    const systemPrompt = `You are an expert in assessing and comparing contracts and contractual terms. You must compare proposals to a predefined policy guideline and make decisions on whether a proposal either meets, exceeds or fails compliance using your expertise and referring the examples outlined below and return the output based on the format given below that. The proposal is compliant only if each item favours the guidelines. Compare the numeric values shared in the proposal with those in the policy guideline, and decide whether the proposal complies with the policy or not. Remember that compliance depends on the context of the item, and therefore decide on a case-by-case basis.
                        ###
                        Examples:
                        - Payment Terms: if the number of days in the proposal is 45 and the number of days in the policy guideline is 30, then respond Yes as the proposal terms favour the guideline.
                        - Quality Assurance: if the number of days in the proposal is 20 and the number of days in the policy guideline is 30, then the proposal complies as the terms favour the guideline.
                        - Audit and Inspection Rights: if the number of days in the proposal is 20 and the number of days in the policy guideline is 30, then the proposal complies as the terms favour the guideline. Respond Yes only if the number of days is equal to or below the policy guideline.
                        ###
                        Output - for each item being compared: 
                        - If the proposal complies with the policy guideline, return a justification for your decision followed by "Yes".
                        - If the proposal doesn't comply with the policy guideline, return a justification for your decision followed by "No".`;

    let fileChunks = [];

    let counter = 0;
    
    // Split the predefined document into chunks for processing
    const splitter = new RecursiveCharacterTextSplitter({
        chunkSize: 32000, // Size of each chunk
        chunkOverlap: 2, // Overlap between chunks
    });

    fileChunks = await splitter.createDocuments([predefinedDocument]);

    let responsesResults = [];
  
    // Overlap between chunks
    for (const chunk of fileChunks) {
        const chunkContent = chunk.pageContent;

        counter++;
        
         // Create the user prompt for the AI model
        let userPrompt = `Document:\n${chunkContent}\n\nChecklist item:\n`;
        
         // Add each checklist item to the user prompt
        checkListItems.forEach(item => {
            userPrompt += `- ${item.content}\n`;
        });
        
        // Create an instance of OpenAIClient with the given endpoint and API key
        const client = new OpenAIClient(config.endpoint, new AzureKeyCredential(config.apiKey));
        try {

            // Prepare the messages to be sent to the AI model
            const messages = [
                { role: "system", content: systemPrompt },
                { role: "user", content: userPrompt }
            ];

             // Call the AI model to get chat completions based on the messages
            const response = await client.getChatCompletions(config.deploymentId, messages, { maxTokens: 2000 });

            // Extract the response content from the AI model's response
            const responseContent = response.choices[0].message.content;

             // Handle the response based on the number of file chunks
            if (fileChunks.length == 1) {
                responsesResults.push(responseContent);  // If there is only one chunk, add the response content directly
            }
            if (fileChunks.length >= 2) {
                 // If there are multiple chunks, process each line of the response
                const responselines = responseContent.split('\n').filter(line => line.trim() !== '');
                responselines.forEach(content => {
                    const yesRegex = /Yes/i;
                    if (yesRegex.test(content)) {
                        // If the response is "Yes", remove the corresponding checklist item
                        let cleanedText = content.replace(/- /g, '');
                        let cleanedTextResult = cleanedText.split(':')[0];
                        const foundItemIndex = checkListItems.findIndex(item => item.content === cleanedTextResult);
                        if (foundItemIndex !== -1) {
                            checkListItems.splice(foundItemIndex, 1);
                            responsesResults.push(content);
                        }
                    } else {
                        // If the response is "No" or the last chunk, add the response content
                        if (counter === fileChunks.length) {
                            responsesResults.push(content);
                        }
                    }
                });
            }
        }
        catch (error) {
            console.error('Error during getCompletions:', error);
        }
    }
    // Return the combined responses
    return responsesResults.join('\n');

}

// Function to prepare checklist items from guidelines text
async function prepareChecklistItems(guidelinesText) 
{
    let responseCheclist= "";  // Variable to store the response checklist

     // System prompt to instruct the AI model on how to extract checklist items
    const systemPrompt = `Pick out the 7 primary checklist items based on the below provided data. Each item should be concise and you should extract the corresponding numeric limit for each checklist item:
For example:
        1. Payment Terms: Ensure invoices are paid within 60 days unless otherwise agreed
        2. Quality Standards: Defective goods must be reported and replaced or refunded within 30 days of delivery.`;
    const client = new OpenAIClient(config.endpoint, new AzureKeyCredential(config.apiKey));
    try {

        // Prepare the messages to be sent to the AI model
        const messages = [
            { role: "system", content: systemPrompt },
            { role: "user", content: guidelinesText }
        ];

        // Call the AI model to get chat completions based on the messages
        const response = await client.getChatCompletions(config.deploymentId, messages, { maxTokens: 2000 });

         // Extract the response content from the AI model's response
        responseCheclist = response.choices[0].message.content;
    }
    catch (error) {
        console.error('Error during getCompletions:', error);
    }

    // Return the response checklist
    return responseCheclist;
}

module.exports = { checkCompliance, prepareChecklistItems };