namespace Shop.DataAccess

open System
open System.IO
open Shop.Models

module OrderRepository =
    
    // Save order to CSV file (async)
    let saveOrderToCsvAsync (filePath: string) (order: Order) : Async<Result<unit, ShopError>> =
        async {
            try
                // Ensure directory exists
                let directory = Path.GetDirectoryName(filePath)
                if not (String.IsNullOrEmpty(directory)) && not (Directory.Exists(directory)) then
                    Directory.CreateDirectory(directory) |> ignore
                
                let (OrderId orderId) = order.Id
                let (CustomerId customerId) = order.Customer.Id
                
                // Create CSV header if file doesn't exist
                if not (File.Exists filePath) then
                    let header = "OrderId,CustomerId,CustomerName,CustomerEmail,OrderDate,Status,TotalAmount,ProductId,ProductName,Quantity,UnitPrice,LineTotal"
                    do! File.WriteAllTextAsync(filePath, header + Environment.NewLine) |> Async.AwaitTask
                
                // Write order items to CSV
                let csvLines = 
                    order.Items
                    |> List.map (fun item ->
                        let (ProductId productId) = item.Product.Id
                        let lineTotal = item.UnitPrice * decimal item.Quantity
                        let status = 
                            match order.Status with 
                            | Processing -> "Processing" 
                            | Pending -> "Pending" 
                            | Shipped -> "Shipped" 
                            | Delivered -> "Delivered" 
                            | Cancelled -> "Cancelled"
                        let dateStr = order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss")
                        $"{orderId},{customerId},\"{order.Customer.Name}\",{order.Customer.Email},{dateStr},{status},{order.TotalAmount},{productId},\"{item.Product.Name}\",{item.Quantity},{item.UnitPrice},{lineTotal}")
                
                do! File.AppendAllLinesAsync(filePath, csvLines) |> Async.AwaitTask
                return Ok ()
            with
            | :? IOException as ex ->
                return Error (FileError $"File I/O error: {ex.Message}")
            | ex ->
                return Error (FileError $"Unexpected error: {ex.Message}")
        }
    
    // Save order with default CSV path (async)
    let saveOrderAsync (order: Order) : Async<Result<unit, ShopError>> =
        let defaultPath = Path.Combine("Data", "orders.csv")
        saveOrderToCsvAsync defaultPath order

    
    // Load orders from CSV (optional - for future use)
    let loadOrdersFromCsv (filePath: string) : Result<string list, ShopError> =
        try
            if not (File.Exists filePath) then
                Ok []
            else
                let lines = File.ReadAllLines(filePath) |> Array.toList
                Ok lines
        with
        | :? IOException as ex ->
            Error (FileError $"File I/O error: {ex.Message}")
        | ex ->
            Error (FileError $"Unexpected error: {ex.Message}")