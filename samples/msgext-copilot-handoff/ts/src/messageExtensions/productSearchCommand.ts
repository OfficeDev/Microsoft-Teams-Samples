import {
    CardFactory,
    TurnContext,
    MessagingExtensionQuery,
    MessagingExtensionResponse,
} from "botbuilder";
import { searchProducts } from "../northwindDB/products";
import cardHandler from "../adaptiveCards/cardHandler";

const COMMAND_ID = "inventorySearch";

let queryCount = 0;
async function handleTeamsMessagingExtensionQuery(
    context: TurnContext,
    query: MessagingExtensionQuery
): Promise<MessagingExtensionResponse> {

    let productName, categoryName, inventoryStatus, supplierCity, stockLevel;

    // For now we have the ability to pass parameters comma separated for testing until the UI supports it.
    // So try to unpack the parameters but when issued from Copilot or the multi-param UI they will come
    // in the parameters array.
    if (query.parameters.length === 1 && query.parameters[0]?.name === "productName") {
        [productName, categoryName, inventoryStatus, supplierCity, stockLevel] = (query.parameters[0]?.value.split(','));
    } else { 
        productName = cleanupParam(query.parameters.find((element) => element.name === "productName")?.value);
        categoryName = cleanupParam(query.parameters.find((element) => element.name === "categoryName")?.value);
        inventoryStatus = cleanupParam(query.parameters.find((element) => element.name === "inventoryStatus")?.value);
        supplierCity = cleanupParam(query.parameters.find((element) => element.name === "supplierCity")?.value);
        stockLevel = cleanupParam(query.parameters.find((element) => element.name === "stockQuery")?.value);
    }
    console.log(`🔎 Query #${++queryCount}:\nproductName=${productName}, categoryName=${categoryName}, inventoryStatus=${inventoryStatus}, supplierCity=${supplierCity}, stockLevel=${stockLevel}`);    

    const products = await searchProducts(productName, categoryName, inventoryStatus, supplierCity, stockLevel);

    console.log(`Found ${products.length} products in the Northwind database`)
    const attachments = [];
    products.forEach((product) => {
        const preview = CardFactory.heroCard(product.ProductName,
            `Supplied by ${product.SupplierName} of ${product.SupplierCity}<br />${product.UnitsInStock} in stock`,
            [product.ImageUrl]);
        
        const resultCard = cardHandler.getEditCard(product);
        const attachment = { ...resultCard, preview };
        attachments.push(attachment);
    });
    return {
        composeExtension: {
            type: "result",
            attachmentLayout: "list",
            attachments: attachments,
        },
    };
}

function cleanupParam(value: string): string {

    if (!value) {
        return "";
    } else {
        let result = value.trim();
        result = result.split(',')[0];          // Remove extra data
        result = result.replace("*", "");       // Remove wildcard characters from Copilot
        return result;
    }
}

export default { COMMAND_ID, handleTeamsMessagingExtensionQuery }
