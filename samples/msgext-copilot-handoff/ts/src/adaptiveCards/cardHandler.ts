import {
    TurnContext,
    CardFactory
} from "botbuilder";
import { updateProduct, getProductEx } from "../northwindDB/products";
import { ProductEx } from '../northwindDB/model';
import editCard from './editCard.json';
import successCard from './successCard.json';
import successCardHandoff from "./successCardHandoff.json";
import * as ACData from "adaptivecards-templating";

import { CreateActionErrorResponse, CreateAdaptiveCardInvokeResponse, getInventoryStatus } from './utils';

function getEditCard(product: ProductEx, context: TurnContext): any {

    var template = new ACData.Template(editCard);
    var card = template.expand({
        $root: {
            productName: product.ProductName,
            unitsInStock: product.UnitsInStock,
            productId: product.ProductID,
            categoryId: product.CategoryID,
            imageUrl: product.ImageUrl,
            supplierName: product.SupplierName,
            supplierCity: product.SupplierCity,
            categoryName: product.CategoryName,
            inventoryStatus: product.InventoryStatus,
            unitPrice: product.UnitPrice,
            quantityPerUnit: product.QuantityPerUnit,
            unitsOnOrder: product.UnitsOnOrder,
            reorderLevel: product.ReorderLevel,
            unitSales: product.UnitSales,
            inventoryValue: product.InventoryValue,
            revenue: product.Revenue,
            averageDiscount: product.AverageDiscount,
            botId: getBotMri(context),
            continuationToken: product.ProductName + "-continuation",
        }
    });
    return CardFactory.adaptiveCard(card);
}

async function handleTeamsCardActionRefreshCard(context: TurnContext) {
    const request = context.activity.value;
    const data = request.action.data;
    console.log(`🎬 Handling refresh action, productId=${data.productId}`);
  
    if (data.productId) {
      const product = await getProductEx(data.productId);
  
      var template = new ACData.Template(successCard);
      var card = template.expand({
        $root: {
          productName: product.ProductName,
          unitsInStock: product.UnitsInStock,
          productId: product.ProductID,
          categoryId: product.CategoryID,
          imageUrl: product.ImageUrl,
          supplierName: product.SupplierName,
          supplierCity: product.SupplierCity,
          categoryName: product.CategoryName,
          inventoryStatus: getInventoryStatus(product),
          unitPrice: product.UnitPrice,
          quantityPerUnit: product.QuantityPerUnit,
          unitsOnOrder: product.UnitsOnOrder,
          reorderLevel: product.ReorderLevel,
          unitSales: product.UnitSales,
          inventoryValue: product.UnitsInStock * product.UnitPrice,
          revenue: product.Revenue,
          averageDiscount: product.AverageDiscount,
          botId: getBotMri(context),
          continuationToken: product.ProductName + "-continuation",
          // Card message
          message: `Card refreshed successfully!`,
        },
      });
  
      return CreateAdaptiveCardInvokeResponse(200, card);
    } else {
      return CreateActionErrorResponse(400, 0, "Invalid request");
    }
  }

async function handleTeamsCardActionUpdateStock(context: TurnContext) {

    const request = context.activity.value;
    const data = request.action.data;
    console.log(`🎬 Handling update stock action, quantity=${data.txtStock}`);

    if (data.txtStock && data.productId) {
        
        const product = await getProductEx(data.productId);
        product.UnitsInStock = Number(data.txtStock);
        await updateProduct(product);
        
        var template = new ACData.Template(successCard);
        var card = template.expand({
            $root: {
                productName: product.ProductName,
                unitsInStock: product.UnitsInStock,
                productId: product.ProductID,
                categoryId: product.CategoryID,
                imageUrl: product.ImageUrl,
                supplierName: product.SupplierName,
                supplierCity: product.SupplierCity,
                categoryName: product.CategoryName,
                inventoryStatus: getInventoryStatus(product),
                unitPrice: product.UnitPrice,
                quantityPerUnit: product.QuantityPerUnit,
                unitsOnOrder: product.UnitsOnOrder,
                reorderLevel: product.ReorderLevel,
                unitSales: product.UnitSales,
                inventoryValue: product.UnitsInStock * product.UnitPrice,
                revenue: product.Revenue,
                averageDiscount: product.AverageDiscount,
                botId: getBotMri(context),
                continuationToken: product.ProductName + "-continuation",
                // Card message
                message: `Stock updated for ${product.ProductName} to ${product.UnitsInStock}!`
            }
        });
       
        return CreateAdaptiveCardInvokeResponse(200, card );

    } else {
       
        return CreateActionErrorResponse(400,0, "Invalid request");
    }
}

async function handleTeamsCardActionCancelRestock(context: TurnContext) {

    const request = context.activity.value;
    const data = request.action.data;
    console.log(`🎬 Handling cancel restock action`)

    if (data.productId) {

        const product = await getProductEx(data.productId);
        product.UnitsOnOrder = 0;
        await updateProduct(product);

        var template = new ACData.Template(successCard);
        var card = template.expand({
            $root: {
                productName: product.ProductName,
                unitsInStock: product.UnitsInStock,
                productId: product.ProductID,
                categoryId: product.CategoryID,
                imageUrl: product.ImageUrl,
                supplierName: product.SupplierName,
                supplierCity: product.SupplierCity,
                categoryName: product.CategoryName,
                inventoryStatus: getInventoryStatus(product),
                unitPrice: product.UnitPrice,
                quantityPerUnit: product.QuantityPerUnit,
                unitsOnOrder: product.UnitsOnOrder,
                reorderLevel: product.ReorderLevel,
                unitSales: product.UnitSales,
                inventoryValue: product.UnitsInStock * product.UnitPrice,
                revenue: product.Revenue,
                averageDiscount: product.AverageDiscount,
                botId: getBotMri(context),
                continuationToken: product.ProductName + "-continuation",
                // Card message                
                message: `Restock cancelled for ${product.ProductName}.`
            }
        });       
        return CreateAdaptiveCardInvokeResponse(200,card);

    } else {
        return CreateActionErrorResponse(400,0, "Invalid request");
    }
}
async function handleTeamsCardActionRestock(context: TurnContext) {
    const request = context.activity.value;
    const data = request.action.data;
    console.log(`🎬 Handling restock action, quantity=${data.txtStock}`)
    if (data.productId) {

        const product = await getProductEx(data.productId);
        product.UnitsOnOrder = Number(product.UnitsOnOrder) + Number(data.txtStock);
        await updateProduct(product);

        var template = new ACData.Template(successCard);
        var card = template.expand({
            $root: {
                productName: product.ProductName,
                unitsInStock: product.UnitsInStock,
                productId: product.ProductID,
                categoryId: product.CategoryID,
                imageUrl: product.ImageUrl,
                supplierName: product.SupplierName,
                supplierCity: product.SupplierCity,
                categoryName: product.CategoryName,
                inventoryStatus: getInventoryStatus(product),
                unitPrice: product.UnitPrice,
                quantityPerUnit: product.QuantityPerUnit,
                unitsOnOrder: product.UnitsOnOrder,
                reorderLevel: product.ReorderLevel,
                unitSales: product.UnitSales,
                inventoryValue: product.UnitsInStock * product.UnitPrice,
                revenue: product.Revenue,
                averageDiscount: product.AverageDiscount,
                botId: getBotMri(context),
                continuationToken: product.ProductName + "-continuation",
                // Card message
                message: `Restocking ${product.ProductName} placed order for ${data.txtStock ?? 0} units.`
            }
        });
        return CreateAdaptiveCardInvokeResponse(200, card);

    } else {
        return CreateActionErrorResponse(400,0, "Invalid request");
    }
}

// Function to return bot MRI
function getBotMri(context: TurnContext): string {
    return context.activity.channelData?.source?.name?.toLowerCase() ===
      "copilot" && !context.activity.recipient.id.startsWith("28:")
      ? `28:${context.activity.recipient.id}`
      : context.activity.recipient.id;
  }

  async function handleTeamsCardActionHandOff(context: TurnContext) {
    const request = context.activity.value;
    const data = request.action.data;
    console.log(
      `🎬 Handling copilot handoff case, continuationToken=${data.continuationToken}`
    );
  
    if (data.continuationToken) {
      var template = new ACData.Template(successCardHandoff);
      var card = template.expand({
        $root: {
          continuationToken: data.continuationToken,
        },
      });
  
      return CreateAdaptiveCardInvokeResponse(200, card);
    } else {
      return CreateActionErrorResponse(400, 0, "Invalid request");
    }
  }

  function handleTeamsCardActionHandOffWithContinuation(
    continuationToken: string
  ) {
    console.log(
      `🎬 Handling copilot handoff case, continuationToken=${continuationToken}`
    );
  
    var template = new ACData.Template(successCardHandoff);
    var card = template.expand({
      $root: {
        continuationToken,
      },
    });
  
    return CardFactory.adaptiveCard(card);
  }

export default { getEditCard, handleTeamsCardActionRefreshCard, handleTeamsCardActionUpdateStock, handleTeamsCardActionRestock, handleTeamsCardActionCancelRestock, handleTeamsCardActionHandOff, handleTeamsCardActionHandOffWithContinuation }
