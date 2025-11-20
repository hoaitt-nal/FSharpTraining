namespace Shop.Business

module Pipelines =
    open Shop.Models
    open Shop.DataAccess
    
    // Day 10-11: Pipeline operators and composition
    let (>=>) f g = fun x -> f x |> Result.bind g
    
    let (<*>) fResult xResult =
        match fResult, xResult with
        | Ok f, Ok x -> Ok (f x)
        | Error e, _ -> Error e
        | _, Error e -> Error e
    
    // Product processing pipeline
    let validateProductName (name: string) =
        if String.length name > 3 then
            Ok name
        else
            Error [EmptyField "Product name must be longer than 3 characters"]
    
    let validateProductPrice (price: decimal) =
        if price > 0m then
            Ok price
        else
            Error [InvalidPrice price]
    
    let validateProductStock (stock: int) =
        if stock >= 0 then
            Ok stock
        else
            Error [InsufficientStock (stock, 0)]
    
    // Customer validation pipeline
    let validateEmail (email: string) =
        if email.Contains("@") && email.Contains(".") then
            Ok email
        else
            Error [InvalidEmail email]
    
    let validatePhone (phone: string option) =
        match phone with
        | None -> Ok None
        | Some p when p.Length >= 10 -> Ok (Some p)
        | Some p -> Error [InvalidPhone p]
    
    // Order processing pipeline
    let calculateOrderItemTotal (item: OrderItem) =
        item.UnitPrice * decimal item.Quantity
    
    let calculateOrderTotal (order: Order) =
        order.Items
        |> List.sumBy calculateOrderItemTotal
    
    let applyTax (taxRate: decimal) (amount: decimal) =
        amount * (1m + taxRate)
    
    let processOrderPipeline (taxRate: decimal) (order: Order) =
        order
        |> calculateOrderTotal
        |> applyTax taxRate
    
    // Search pipeline
    let searchPipeline (query: string) (products: Product list) =
        products
        |> ProductRepository.searchProducts query
        |> List.take (min 10 products.Length) // Limit results
    
    // Data transformation pipelines
    let productToInventoryRecord (product: Product) : InventoryRecord =
        {
            ProductId = let (ProductId id) = product.Id in id
            ProductName = product.Name
            Category = product.Category
            CurrentStock = product.Stock
            ReorderLevel = max (product.Stock / 4) 5 // 25% of current or min 5
            LastRestocked = product.CreatedAt
        }
    
    let orderToSalesRecord (order: Order) : SalesRecord list =
        order.Items
        |> List.map (fun item ->
            {
                Date = order.OrderDate
                ProductId = let (ProductId id) = item.Product.Id in id
                ProductName = item.Product.Name
                Quantity = item.Quantity
                UnitPrice = item.UnitPrice
                Total = calculateOrderItemTotal item
                CustomerName = order.Customer.Name
            })
    
    // Composition helpers
    let compose f g x = f (g x)
    let (>>) f g = compose g f
    
    // Result combining
    let combineResults (results: Result<'a, ValidationError list> list) =
        let folder (acc: Result<'a list, ValidationError list>) (current: Result<'a, ValidationError list>) =
            match acc, current with
            | Ok items, Ok item -> Ok (item :: items)
            | Error errors, Ok _ -> Error errors
            | Ok _, Error errors -> Error errors
            | Error accErrors, Error currErrors -> Error (accErrors @ currErrors)
        
        results
        |> List.fold folder (Ok [])
        |> Result.map List.rev