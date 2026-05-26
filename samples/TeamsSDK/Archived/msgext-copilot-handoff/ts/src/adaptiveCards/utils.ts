import { Product } from "../northwindDB/model";
import { AdaptiveCardInvokeResponse, InvokeResponse } from "botbuilder";

export const CreateInvokeResponse = (status: number, body?: unknown): InvokeResponse => {
  return { status, body };
};
export const CreateAdaptiveCardInvokeResponse = (statusCode: number, body?: Record<string, unknown>): AdaptiveCardInvokeResponse => {
  return {
           statusCode: statusCode,
           type: 'application/vnd.microsoft.card.adaptive',
           value: body
       };
};
export const CreateActionErrorResponse = ( statusCode: number, errorCode: number = -1, errorMessage: string = 'Unknown error') => {
  return {
      statusCode: statusCode,
      type: 'application/vnd.microsoft.error',
      value: {
          error: {
              code: errorCode,
              message: errorMessage,
          },
      },
  };
};

export const getInventoryStatus = (product: Product) => {
  if (Number(product.UnitsInStock) >= Number(product.ReorderLevel)) {
    return "In stock";
  } else if (Number(product.UnitsInStock) < Number(product.ReorderLevel) && Number(product.UnitsOnOrder) === 0) {
    return "Low stock";
  } else if (Number(product.UnitsInStock) < Number(product.ReorderLevel) && Number(product.UnitsOnOrder) > 0) {
    return "On order";
  } else if (Number(product.UnitsInStock) === 0) {
    return "Out of stock";
  } else {
    return "Unknown"; //fall back
  }
}
