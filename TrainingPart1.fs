namespace OrderProcessor

open System
open System.Collections.Generic
// Types are now defined in Models.fs

module OrderFunctions =
    let calculateOrderItemTotal (item: OrderItem) : decimal =
        item.Product.Price * decimal item.Quantity
    let applyDiscount (total: decimal) (discountRate: decimal) : decimal =
        total * (1.0M - discountRate)
    let calculateOrderTotal (order: Order) : decimal =
        let total =
            order.Items
            |> List.sumBy calculateOrderItemTotal
        if order.IsDiscounted then
            applyDiscount total 0.1M // Apply a 10% discount
        else
            total 
    let calculateOrderTotalWithTax (order: Order) (taxRate: decimal) : decimal =
        let total = calculateOrderTotal order
        total * (1.0M + taxRate)

module Program =
    
    [<EntryPoint>]
    let main argv =
        // Products are now loaded from DataLayer
        let allProducts = DataLayer.getAllProducts()
        
        // Helper function to get product by ID safely
        let getProduct id = 
            match DataLayer.getProductById id with
            | Some product -> product
            | None -> failwith $"Product with ID {id} not found"
        
        // Display all available products from DataLayer
        printfn "=== AVAILABLE PRODUCTS FROM DATALAYER (%d items) ===" allProducts.Length
        printfn "ID\tName\t\t\tPrice"
        printfn "=================================================="
        
        allProducts
        |> List.iter (fun p -> printfn "%d\t%-20s\t%M" p.Id p.Name p.Price)
        
        
        // Display product statistics
        printfn "\n--- PRODUCT STATISTICS ---"
        let stats = DataLayer.getProductStats()
        printfn "Total Products: %d" stats.TotalProducts
        printfn "Average Price: %M" stats.AveragePrice  
        printfn "Cheapest Product: %s (%M)" stats.MinPriceProduct.Name stats.MinPriceProduct.Price
        printfn "Most Expensive: %s (%M)" stats.MaxPriceProduct.Name stats.MaxPriceProduct.Price
        printfn "Total Inventory Value: %M" stats.TotalInventoryValue
        printfn "Category Distribution: Electronics(%d), Office(%d), Lifestyle(%d)" 
            stats.ElectronicsCount stats.OfficeCount stats.LifestyleCount
        
        printfn "\n=== CREATING ORDER ===" 
        let orderSpec = [
            (1, 1)   // Laptop x1
            (4, 5)   // Monitor x1
            (7, 1)   // Headphones x1
            (17, 3)  // Water Bottle x3
            (19, 2)  // Wireless Charger x2
        ]
        let orderItems = 
            orderSpec
            |> List.map (fun (productId, quantity) -> 
                { Product = getProduct productId; Quantity = quantity })
        let order = 
            { Id = 1
                // how to items by product id and sum quantity
              Items = orderItems
                |> List.groupBy (fun item -> item.Product.Id)
                |> List.map (fun (productId, items) ->
                    let product = items.Head.Product
                    let totalQuantity = items |> List.sumBy (fun item -> item.Quantity)
                    { Product = product; Quantity = totalQuantity })
              OrderDate = DateTime.Now
              IsDiscounted = true }
        // Display order in a nicely formatted table
        printfn "\n=== ORDER SUMMARY ==="
        printfn "Order ID: %d" order.Id
        printfn "Order Date: %s" (order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"))
        printfn "Discount Applied: %b" order.IsDiscounted
        printfn ""
        let totalItems = order.Items |> List.sumBy (fun item -> item.Quantity)
        printfn "Total Items: %d | Item Types: %d" totalItems order.Items.Length
        printfn ""
        printfn "┌─────┬─────────────────────┬──────────┬───────────┬─────────────┐"
        printfn "│ ID  │ Product             │ Quantity │   Price   │    Total    │"
        printfn "├─────┼─────────────────────┼──────────┼───────────┼─────────────┤"
        
        for item in order.Items do
            let itemTotal = OrderFunctions.calculateOrderItemTotal item
            printfn "│ %-3d │ %-19s │    %-5d │ %8.2M │ %10.2M │" 
                order.Id 
                (if item.Product.Name.Length > 19 then item.Product.Name.Substring(0, 16) + "..." else item.Product.Name)
                item.Quantity 
                item.Product.Price 
                itemTotal
        
        printfn "└─────┴─────────────────────┴──────────┴───────────┴─────────────┘"
        
        let originalSubtotal = order.Items |> List.sumBy OrderFunctions.calculateOrderItemTotal
        let subtotal = OrderFunctions.calculateOrderTotal order
        let totalWithTax = OrderFunctions.calculateOrderTotalWithTax order 0.07M
        let taxAmount = totalWithTax - subtotal
        let discountAmount = originalSubtotal - subtotal
        
        printfn ""
        printfn "ORDER TOTALS:"
        printfn "Subtotal (before discount): %10.2M" originalSubtotal
        if order.IsDiscounted then
            printfn "Discount (10%%):           -%10.2M" discountAmount
            printfn "You saved:                  %10.2M" discountAmount
        printfn "Subtotal (after discount):  %10.2M" subtotal
        printfn "Tax (7%%):                  +%10.2M" taxAmount
        printfn "                           ─────────────"
        printfn "TOTAL:                      %10.2M" totalWithTax
        
        if order.IsDiscounted then
            printfn ""
            printfn "💰 Congratulations! You saved %M with the 10%% discount!" discountAmount
        0 // return an integer exit code 
    

