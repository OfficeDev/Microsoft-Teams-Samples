import {
    TABLE_NAME, Product, ProductEx, Supplier, Category, OrderDetail
} from './model';

import { TableClient, TableEntityResult } from "@azure/data-tables";
import config from "../config";
import { getInventoryStatus } from '../adaptiveCards/utils';

// NOTE: We're force fitting a relational database into a non-relational database so please
// forgive the inefficiencies. This is just for demonstration purposes.

// #region searchProducts() and supporting functions

export async function searchProducts(productName: string, categoryName: string, inventoryStatus: string,
    supplierCity: string, stockLevel: string): Promise<ProductEx[]> {

    let result = await getAllProductsEx();

    // Filter products
    if (productName) {
        result = result.filter((p) => p.ProductName.toLowerCase().startsWith(productName.toLowerCase()));
    }
    if (categoryName) {
        result = result.filter((p) => p.CategoryName.toLowerCase().startsWith(categoryName.toLowerCase()));
    }
    if (inventoryStatus) {
        result = result.filter((p) => isMatchingStatus(inventoryStatus, p));
    }
    if (supplierCity) {
        result = result.filter((p) => p.SupplierCity.toLowerCase().startsWith(supplierCity.toLowerCase()));
    }
    if (stockLevel) {
        result = result.filter((p) => isInRange(stockLevel, p.UnitsInStock));
    }

    return result;
}

export async function getDiscountedProductsByCategory(categoryName: string): Promise<ProductEx[]> {

    let result = await getAllProductsEx();

    // Anything with >5% average discount counts as discounted
    result = result.filter((p) => p.AverageDiscount > 5);
    if (categoryName) {
        result = result.filter((p) => p.CategoryName.toLowerCase().startsWith(categoryName.toLowerCase()));
    }

    return result;
}

export async function getProductsByRevenueRange(revenueRange: string): Promise<ProductEx[]> {

    let result = await getAllProductsEx();

    if (revenueRange) {
        let range = revenueRange;
        // Handle "low" and "high" keywords
        if (revenueRange.toLowerCase().startsWith('l')) range = "0-5000";
        if (revenueRange.toLowerCase().startsWith('h')) range = "50000-"
        // Filter by numeric range
        result = result.filter((p) => isInRange(range, p.Revenue));
    }

    return result;
}

// Returns true if the inventory status in a product matches the query using
// the inventory data rather than just a text match for better accuracy
function isMatchingStatus(inventoryStatusQuery: string, product: ProductEx): boolean {

    const query = inventoryStatusQuery.toLowerCase();
    if (query.startsWith("out")) {
        // Out of stock
        return product.UnitsInStock === 0;
    } else if (query.startsWith("low")) {
        // Low stock
        return product.UnitsInStock <= product.ReorderLevel;
    } else if (query.startsWith("on")) {
        // On order
        return product.UnitsOnOrder > 0;
    } else {
        // In stock
        return product.UnitsInStock > 0;
    }
}

// Used to filter based on a range entered in the stockLevel parameter
// Returns true iff a value is within the range specified in the range expression
function isInRange(rangeExpression: string, value: number) {

    let result = false;     // Return false if the expression is malformed

    if (rangeExpression.indexOf('-')< 0) {
        // If here, we have a single value or a malformed expression
        const val = Number(rangeExpression);
        if (!isNaN(val)) {
            result = value === val;
        }
    } else if (rangeExpression.indexOf('-') === rangeExpression.length-1) {
        // If here we have a single lower bound or a malformed expression
        const lowerBound = Number(rangeExpression.slice(0,-1));
        if (!isNaN(lowerBound)) {
            result = value >= lowerBound;
        }
    } else {
        // If here we have a range or a malformed expression
        const bounds = rangeExpression.split('-');
        const lowerBound = Number(bounds[0]);
        const upperBound = Number(bounds[1]);
        if (!isNaN(lowerBound) && !isNaN(upperBound)) {
            result = lowerBound <= value && upperBound >= value;
        }
    }
    return result;
}

// #endregion

// #region Reference data handling

interface ReferenceData<DataType> {
    [index: string]: DataType;
}

async function loadReferenceData<DataType>(tableName): Promise<ReferenceData<DataType>> {

    const tableClient = TableClient.fromConnectionString(config.storageAccountConnectionString, tableName);

    const entities = tableClient.listEntities();

    let result = {};
    for await (const entity of entities) {
        result[entity.rowKey] = entity;
    }
    return result;

}

interface OrderTotals {
    [productId: string]: {
        totalQuantity: number,
        totalRevenue: number,
        totalDiscount: number
    }
}

async function loadOrderTotals(): Promise<OrderTotals> {

    const tableClient = TableClient.fromConnectionString(config.storageAccountConnectionString, TABLE_NAME.ORDER_DETAIL);

    const entities = tableClient.listEntities();

    let totals: OrderTotals = {};
    for await (const entity of entities) {
        const p = entity.ProductID as string;
        if (!totals[p]) {
            totals[p] = {
                totalQuantity: Number(entity.Quantity),
                totalRevenue: Number(entity.Quantity) * Number(entity.UnitPrice) * (1-Number(entity.Discount)),
                totalDiscount: Number(entity.Quantity) * Number(entity.UnitPrice) * Number(entity.Discount) 
            }
        } else {
            totals[p].totalQuantity += Number(entity.Quantity);
            totals[p].totalRevenue += Number(entity.Quantity) * Number(entity.UnitPrice) * (1-Number(entity.Discount));
            totals[p].totalDiscount += Number(entity.Quantity) * Number(entity.UnitPrice) * Number(entity.Discount);
        }
    }
    return totals;

}

// Reference tables never change in this demo app - so they're cached here
let categories: ReferenceData<Category> = null;
let suppliers: ReferenceData<Supplier> = null;
let orderTotals: OrderTotals = null;

// #endregion

// #region Data Access functions

async function getAllProductsEx(): Promise<ProductEx[]> {

    // Ensure reference data are loaded
    categories = categories ?? await loadReferenceData<Category>(TABLE_NAME.CATEGORY);
    suppliers = suppliers ?? await loadReferenceData<Supplier>(TABLE_NAME.SUPPLIER);
    orderTotals = orderTotals ?? await loadOrderTotals();

    // We always read the products fresh in case somebody made a change
    const result: ProductEx[] = [];
    const tableClient = TableClient.fromConnectionString(config.storageAccountConnectionString, TABLE_NAME.PRODUCT);

    const entities = tableClient.listEntities();

    for await (const entity of entities) {
        const p = getProductExForEntity(entity);
        result.push(p);
    }
    return result;
}

function getProductExForEntity(entity: TableEntityResult<Record<string, unknown>>): ProductEx {

    let result: ProductEx = {
        etag: entity.etag as string,
        partitionKey: entity.partitionKey as string,
        rowKey: entity.rowKey as string,
        timestamp: new Date(entity.timestamp),
        ProductID: entity.ProductID as string,
        ProductName: entity.ProductName as string,
        SupplierID: entity.SupplierID as string,
        CategoryID: entity.CategoryID as string,
        QuantityPerUnit: entity.QuantityPerUnit as string,
        UnitPrice: Number(entity.UnitPrice),
        UnitsInStock: Number(entity.UnitsInStock),
        UnitsOnOrder: Number(entity.UnitsOnOrder),
        ReorderLevel: Number(entity.ReorderLevel),
        Discontinued: entity.Discontinued as boolean,
        ImageUrl: entity.ImageUrl as string,
        CategoryName: "",
        SupplierName: "",
        SupplierCity: "",
        InventoryStatus: "",
        InventoryValue: 0,
        UnitSales: 0,
        Revenue: 0,
        AverageDiscount: 0
    };

    // Fill in extended properties
    result.CategoryName = categories[result.CategoryID].CategoryName;
    result.SupplierName = suppliers[result.SupplierID].CompanyName;
    result.SupplierCity = suppliers[result.SupplierID].City;
    result.UnitSales =  orderTotals[result.ProductID].totalQuantity;
    result.InventoryValue = Math.round(result.UnitsInStock * result.UnitPrice);
    result.Revenue = Math.round(orderTotals[result.ProductID].totalRevenue);
    result.AverageDiscount = +(result.Revenue / orderTotals[result.ProductID].totalDiscount).toFixed(1);

    // 'in stock', 'low stock', 'on order', or 'out of stock'
    result.InventoryStatus = getInventoryStatus(result);          

    return result;
}

export async function getProductEx(productId: number): Promise<ProductEx> {
    const tableClient = TableClient.fromConnectionString(config.storageAccountConnectionString, TABLE_NAME.PRODUCT);
    const entity = await tableClient.getEntity(TABLE_NAME.PRODUCT, productId.toString());
    const p = getProductExForEntity(entity);

    return p;
}

export async function updateProduct(updatedProduct: Product): Promise<void> {
    const tableClient = TableClient.fromConnectionString(config.storageAccountConnectionString, TABLE_NAME.PRODUCT);
    const product = await tableClient.getEntity(TABLE_NAME.PRODUCT, updatedProduct.ProductID.toString()) as Product;
    if (!product) {
        throw new Error("Product not found");
    }
    await tableClient.updateEntity({ ...product, ...updatedProduct }, "Merge");
}

// #endregion

// #region -- NOT USED, NOT TESTED ---------------------------------------------------------

// export async function createProduct (product: Product): Promise<void> {
//     const newProduct: Product = {
//         partitionKey: TABLE_NAME.PRODUCT,
//         rowKey: product.ProductID,
//         ...product,
//     }
//     const tableClient = TableClient.fromConnectionString(config.storageAccountConnectionString, TABLE_NAME.PRODUCT);
//     await tableClient.createEntity(newProduct);
// };

// export async function deleteProduct (productId: number): Promise<void> {
//     const tableClient = TableClient.fromConnectionString(config.storageAccountConnectionString, TABLE_NAME.PRODUCT);
//     await tableClient.deleteEntity(TABLE_NAME.PRODUCT, productId.toString());
// };

//#endregion


