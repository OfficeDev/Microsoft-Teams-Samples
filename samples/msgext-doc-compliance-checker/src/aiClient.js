// openaiClient.js
const { OpenAIClient, AzureKeyCredential } = require("@azure/openai");
const { RecursiveCharacterTextSplitter } = require("langchain/text_splitter");
const config = require("./config");

async function checkCompliance(checkListItems, predefinedDocument) 
{
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


async function prepareChecklistItems(guidelinesText) 
{
    let responseCheclist= "";
    const systemPrompt = `Pick out the 7 primary checklist items based on the below provided data. Each item should be concise and you should extract the corresponding numeric limit for each checklist item:
For example:
        1. Payment Terms: Ensure invoices are paid within 60 days unless otherwise agreed
        2. Quality Standards: Defective goods must be reported and replaced or refunded within 30 days of delivery.`;
    const client = new OpenAIClient(config.endpoint, new AzureKeyCredential(config.apiKey));
    try {
        const messages = [
            { role: "system", content: systemPrompt },
            { role: "user", content: guidelinesText }
        ];
        const response = await client.getChatCompletions(config.deploymentId, messages, { maxTokens: 2000 });
        responseCheclist = response.choices[0].message.content;
    }
    catch (error) {
        console.error('Error during getCompletions:', error);
    }

    return responseCheclist;
}

module.exports = { checkCompliance, prepareChecklistItems };