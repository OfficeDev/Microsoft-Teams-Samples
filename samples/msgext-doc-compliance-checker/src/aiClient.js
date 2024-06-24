// openaiClient.js
const { OpenAIClient, AzureKeyCredential } = require("@azure/openai");
const { RecursiveCharacterTextSplitter } = require("langchain/text_splitter");
const config = require("./config");

async function checkCompliance(checkListItems, predefinedDocument) {
    const systemPrompt = `You are an assistant that helps verify documents against a given checklist. For each item on the checklist:
- If the item is present in the document, provide the relevant content followed by "Yes".
- If the item is missing in the document, respond with "No" and indicate that no additional details are available.`;

    let fileChunks = [];

    let counter = 0;

    const splitter = new RecursiveCharacterTextSplitter({
        chunkSize: 32000,
        chunkOverlap: 2,
    });
    
    fileChunks = await splitter.createDocuments([predefinedDocument]);

    let responsesResults = [];

    for (const chunk of fileChunks) {
        const chunkContent = chunk.pageContent;

        counter++;

        let userPrompt = `Document:\n${chunkContent}\n\nChecklist item:\n`;

        checkListItems.forEach(item => {
            userPrompt += `- ${item.content}\n`;
        });

        const client = new OpenAIClient(config.endpoint, new AzureKeyCredential(config.apiKey));
        try {
            const messages = [
                { role: "system", content: systemPrompt },
                { role: "user", content: userPrompt }
            ];
            const response = await client.getChatCompletions(config.deploymentId, messages, { maxTokens: 2000 });
            const responseContent = response.choices[0].message.content;
            if (fileChunks.length == 1) {
                responsesResults.push(responseContent);
            }
            if (fileChunks.length >= 2) {
                const responselines = responseContent.split('\n').filter(line => line.trim() !== '');
                responselines.forEach(content => {
                    const yesRegex = /Yes/i;
                    if (yesRegex.test(content)) {
                        let cleanedText = content.replace(/- /g, '');
                        let cleanedTextResult = cleanedText.split(':')[0];
                        const foundItemIndex = checkListItems.findIndex(item => item.content === cleanedTextResult);
                        if (foundItemIndex !== -1) {
                            checkListItems.splice(foundItemIndex, 1);
                            responsesResults.push(content);
                        }
                    } else {
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
    return responsesResults.join('\n');

}

module.exports = { checkCompliance };