namespace Shop.Models

open System

// Day 8-9: Domain Models với String processing
type ProductId = ProductId of string
type CustomerId = CustomerId of string  
type OrderId = OrderId of string

type Product = {
    Id: ProductId
    Name: string
    Description: string
    Price: decimal
    Category: string
    Stock: int
    Tags: string list
    CreatedAt: DateTime
}

type Customer = {
    Id: CustomerId
    Name: string
    Email: string
    Address: string
    Phone: string option
    RegisteredAt: DateTime
}

type OrderItem = {
    Product: Product
    Quantity: int
    UnitPrice: decimal
}

type OrderStatus = 
    | Pending 
    | Processing 
    | Shipped 
    | Delivered 
    | Cancelled

type Order = {
    Id: OrderId
    Customer: Customer
    Items: OrderItem list
    Status: OrderStatus
    OrderDate: DateTime
    TotalAmount: decimal
}

// Day 10-11: Result types cho error handling
type ValidationError = 
    | InvalidEmail of string
    | InvalidPhone of string
    | EmptyField of string
    | InvalidPrice of decimal
    | InsufficientStock of int * int

type ShopError = 
    | ValidationErrors of ValidationError list
    | ProductNotFound of ProductId
    | CustomerNotFound of CustomerId
    | OrderNotFound of OrderId
    | FileError of string
    | JsonError of string

// Day 12-13: Configuration model cho JSON
type ShopConfig = {
    DatabasePath: string
    LogLevel: string
    TaxRate: decimal
    Currency: string
    MaxOrderItems: int
    AdminEmail: string
}

// Day 14: CSV models
type SalesRecord = {
    Date: DateTime
    ProductId: string
    ProductName: string
    Quantity: int
    UnitPrice: decimal
    Total: decimal
    CustomerName: string
}

type InventoryRecord = {
    ProductId: string
    ProductName: string
    Category: string
    CurrentStock: int
    ReorderLevel: int
    LastRestocked: DateTime
}