export const TABLE_NAME = {
    CATEGORY: "Categories",
    CUSTOMER: "Customers",
    EMPLOYEE: "Employees",
    ORDER: "Orders",
    ORDER_DETAIL: "OrderDetails",
    PRODUCT: "Products",
    SUPPLIER: "Suppliers"
}

interface Row {
    etag: string;
    partitionKey: string;
    rowKey: string;
    timestamp: Date;
}

export interface Category extends Row {
    CategoryID: string;
    CategoryName: string;
    Description: string;
    Picture: string;
}

export interface Customer extends Row {
    CustomerID: string;
    CompanyName: string;
    ContactName: string;
    ContactTitle: string;
    Address: string;
    City: string;
    Region: string;
    PostalCode: string;
    Country: string;
    Phone: string;
    Fax: string;
    ImageUrl: string;
    FlagUrl: string;
}

export interface Employee extends Row {
    EmployeeID: number;
    LastName: string;
    FirstName: string;
    Title: string;
    TitleOfCourtesy: string;
    BirthDate: Date;
    HireDate: Date;
    Address: string;
    City: string;
    Region: string;
    PostalCode: string;
    Country: string;
    HomePhone: string;
    Extension: string;
    Photo: string;
    Notes: string;
    ReportsTo: number;
    PhotoPath: string;
    ImageUrl: string;
    FlagUrl: string;
}

export interface OrderDetail extends Row {
    OrderID: number;
    ProductID: string;
    UnitPrice: number;
    Quantity: number;
    Discount: number;
}

export interface Order extends Row {
    OrderID: number,
    CustomerID: string,
    EmployeeID: number,
    OrderDate: string,
    RequiredDate?: string,
    ShippedDate?: string,
    OrderDetails: OrderDetail[],
    ShipVia: string,
    Freight: 11.61,
    ShipName: "Toms Spezialitäten",
    ShipAddress: "Luisenstr. 48",
    ShipCity: "Münster",
    ShipRegion: null,
    ShipPostalCode: "44087",
    ShipCountry: "Germany"
}

export interface Product extends Row {
    ProductID: string;
    ProductName: string;
    SupplierID: string;
    CategoryID: string;
    QuantityPerUnit: string;
    UnitPrice: number;
    UnitsInStock: number;
    UnitsOnOrder: number;
    ReorderLevel: number;
    Discontinued: boolean;
    ImageUrl: string;
}

// Denormalized version of product
export interface ProductEx extends Product {
    CategoryName: string,
    SupplierName: string,
    SupplierCity: string,
    InventoryStatus: string,
    InventoryValue: number,
    UnitSales: number,
    Revenue: number,
    AverageDiscount: number
}
export interface Supplier extends Row {
    SupplierID: string;
    CompanyName: string;
    ContactName: string;
    ContactTitle: string;
    Address: string;
    City: string;
    Region: string;
    PostalCode: string;
    Country: string;
    Phone: string;
    Fax: string;
    HomePage: string;
}

