open System
open System.Threading.Tasks
open Microsoft.Azure.Cosmos
open Models
open CosmosRepository

// ============ Azure Cosmos DB Demo Program ============

let cosmosConfig = {
    EndpointUrl = "https://localhost:8081/"  // Cosmos DB Emulator endpoint
    PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="  // Emulator key
    DatabaseId = "FSharpCosmosDemo"
    CustomerContainerId = "Customers"
    OrderContainerId = "Orders"  
    ProductContainerId = "Products"
    AnalyticsContainerId = "Analytics"
}

// Helper function to handle task results
let handleResult taskResult =
    task {
        let! result = taskResult
        match result with
        | Ok value -> 
            return value
        | Error errorMsg ->
            printfn "❌ Error: %s" errorMsg
            return Unchecked.defaultof<_>
    }

// Demo functions
module CosmosDemo =
    
    let demoCreateOperations (repo: CosmosRepository) =
        task {
            printfn "\n============ CREATE Operations Demo ============"
            
            // Create sample customer
            let customer = SampleData.generateSampleCustomer()
            printfn "\n📋 Creating customer: %s" customer.customerId
            let! customerResult = repo.CreateCustomerAsync customer
            match customerResult with
            | Ok createdCustomer -> 
                printfn "✅ Customer created successfully"
                printfn "   ID: %s" createdCustomer.id
                printfn "   Email: %s" createdCustomer.email
                printfn "   Country: %s (Partition Key)" createdCustomer.country
            | Error error -> 
                printfn "❌ Failed to create customer: %s" error
            
            // Create sample product
            let product = SampleData.generateSampleProduct()
            printfn "\n📦 Creating product: %s" product.productId
            let! productResult = repo.CreateProductAsync product
            match productResult with
            | Ok createdProduct -> 
                printfn "✅ Product created successfully"
                printfn "   ID: %s" createdProduct.id
                printfn "   Name: %s" createdProduct.name
                printfn "   Category: %s (Partition Key)" createdProduct.category
                printfn "   Price: $%.2f" createdProduct.price
            | Error error -> 
                printfn "❌ Failed to create product: %s" error
            
            // Create sample order
            let order = SampleData.generateSampleOrder customer.customerId
            printfn "\n🛒 Creating order: %s" order.orderId
            let! orderResult = repo.CreateOrderAsync order
            match orderResult with
            | Ok createdOrder -> 
                printfn "✅ Order created successfully"
                printfn "   ID: %s" createdOrder.id
                printfn "   Customer ID: %s" createdOrder.customerId
                printfn "   Order Date: %s (Partition Key: %s)" 
                    (createdOrder.orderDate.ToString("yyyy-MM-dd"))
                    (PartitionKeys.orderPartition createdOrder.orderDate)
                printfn "   Total: $%.2f" createdOrder.totalAmount
                printfn "   Status: %s" createdOrder.status
            | Error error -> 
                printfn "❌ Failed to create order: %s" error
            
            return (customer, product, order)
        }
    
    let demoReadOperations (repo: CosmosRepository) (customer: Customer, product: Product, order: Order) =
        task {
            printfn "\n============ READ Operations Demo ============"
            
            // Read customer by ID and partition key
            printfn "\n👤 Reading customer by ID and partition key..."
            let! customerResult = repo.ReadCustomerAsync customer.id customer.country
            match customerResult with
            | Ok readCustomer ->
                printfn "✅ Customer found:"
                printfn "   ID: %s" readCustomer.id
                printfn "   Name: %s %s" readCustomer.firstName readCustomer.lastName
                printfn "   Email: %s" readCustomer.email
                printfn "   Loyalty Tier: %s" readCustomer.loyaltyTier
                printfn "   Total Spent: $%.2f" readCustomer.totalSpent
            | Error error ->
                printfn "❌ Failed to read customer: %s" error
            
            // Read product by ID and partition key
            printfn "\n📦 Reading product by ID and partition key..."
            let! productResult = repo.ReadProductAsync product.id product.category
            match productResult with
            | Ok readProduct ->
                printfn "✅ Product found:"
                printfn "   ID: %s" readProduct.id
                printfn "   Name: %s" readProduct.name
                printfn "   Brand: %s" readProduct.brand
                printfn "   Category: %s" readProduct.category
                printfn "   Price: $%.2f" readProduct.price
                printfn "   In Stock: %b (%d units)" readProduct.inStock readProduct.stockQuantity
            | Error error ->
                printfn "❌ Failed to read product: %s" error
            
            // Read order by ID and partition key  
            printfn "\n🛒 Reading order by ID and partition key..."
            let! orderResult = repo.ReadOrderAsync order.id order.orderDate
            match orderResult with
            | Ok readOrder ->
                printfn "✅ Order found:"
                printfn "   ID: %s" readOrder.id
                printfn "   Customer ID: %s" readOrder.customerId
                printfn "   Order Date: %s" (readOrder.orderDate.ToString("yyyy-MM-dd HH:mm:ss"))
                printfn "   Status: %s" readOrder.status
                printfn "   Total Amount: $%.2f %s" readOrder.totalAmount readOrder.currency
                printfn "   Items Count: %d" (List.length readOrder.items)
            | Error error ->
                printfn "❌ Failed to read order: %s" error
        }
    
    let demoUpsertOperations (repo: CosmosRepository) (customer: Customer, product: Product, order: Order) =
        task {
            printfn "\n============ UPSERT Operations Demo ============"
            
            // Upsert customer (update existing)
            let updatedCustomer = { customer with 
                                     totalOrders = customer.totalOrders + 1
                                     totalSpent = customer.totalSpent + 99.99m
                                     loyaltyTier = "Gold"
                                     lastLoginDate = Some DateTime.Now }
            
            printfn "\n👤 Upserting customer (updating existing)..."
            let! upsertResult = repo.UpsertCustomerAsync updatedCustomer
            match upsertResult with
            | Ok upsertedCustomer ->
                printfn "✅ Customer upserted successfully:"
                printfn "   Total Orders: %d (was %d)" upsertedCustomer.totalOrders customer.totalOrders
                printfn "   Total Spent: $%.2f (was $%.2f)" upsertedCustomer.totalSpent customer.totalSpent
                printfn "   Loyalty Tier: %s (was %s)" upsertedCustomer.loyaltyTier customer.loyaltyTier
                printfn "   Last Login: %A" upsertedCustomer.lastLoginDate
            | Error error ->
                printfn "❌ Failed to upsert customer: %s" error
            
            // Upsert new product (create new)
            let newProduct = { SampleData.generateSampleProduct() with 
                                name = "New Upserted Product"
                                category = product.category  // Same partition
                                price = 299.99m }
            
            printfn "\n📦 Upserting new product (creating new)..."
            let! newProductResult = repo.UpsertProductAsync newProduct
            match newProductResult with
            | Ok upsertedProduct ->
                printfn "✅ New product upserted successfully:"
                printfn "   ID: %s" upsertedProduct.id
                printfn "   Name: %s" upsertedProduct.name
                printfn "   Category: %s" upsertedProduct.category
                printfn "   Price: $%.2f" upsertedProduct.price
            | Error error ->
                printfn "❌ Failed to upsert new product: %s" error
        }
    
    let demoPatchOperations (repo: CosmosRepository) (customer: Customer, product: Product, order: Order) =
        task {
            printfn "\n============ PATCH Operations Demo ============"
            
            // Patch customer with partial updates
            let customerPatchOps = [
                PatchOperation.Set("/loyaltyTier", "Platinum")
                PatchOperation.Increment("/totalOrders", 5)
                PatchOperation.Set("/lastLoginDate", DateTime.Now)
            ]
            
            printfn "\n👤 Patching customer with partial updates..."
            let! patchResult = repo.PatchCustomerAsync customer.id customer.country customerPatchOps
            match patchResult with
            | Ok patchedCustomer ->
                printfn "✅ Customer patched successfully:"
                printfn "   Loyalty Tier: %s" patchedCustomer.loyaltyTier
                printfn "   Total Orders: %d" patchedCustomer.totalOrders
                printfn "   Last Login: %A" patchedCustomer.lastLoginDate
            | Error error ->
                printfn "❌ Failed to patch customer: %s" error
            
            // Patch order status
            let orderPatchOps = [
                PatchOperation.Set("/status", "Shipped")
                PatchOperation.Set("/trackingNumber", "TRK123456789")
            ]
            
            printfn "\n🛒 Patching order status..."
            let! orderPatchResult = repo.PatchOrderAsync order.id order.orderDate orderPatchOps
            match orderPatchResult with
            | Ok patchedOrder ->
                printfn "✅ Order patched successfully:"
                printfn "   Status: %s" patchedOrder.status
                printfn "   Tracking Number: %A" patchedOrder.trackingNumber
            | Error error ->
                printfn "❌ Failed to patch order: %s" error
            
            // Patch product inventory
            let productPatchOps = [
                PatchOperation.Set("/inStock", true)
                PatchOperation.Set("/stockQuantity", 50)
                PatchOperation.Set("/lastUpdated", DateTime.Now)
            ]
            
            printfn "\n📦 Patching product inventory..."
            let! productPatchResult = repo.PatchProductAsync product.id product.category productPatchOps
            match productPatchResult with
            | Ok patchedProduct ->
                printfn "✅ Product patched successfully:"
                printfn "   In Stock: %b" patchedProduct.inStock
                printfn "   Stock Quantity: %d" patchedProduct.stockQuantity
                printfn "   Last Updated: %s" (patchedProduct.lastUpdated.ToString("yyyy-MM-dd HH:mm:ss"))
            | Error error ->
                printfn "❌ Failed to patch product: %s" error
        }
    
    let demoQueryOperations (repo: CosmosRepository) (customer: Customer) =
        task {
            printfn "\n============ QUERY Operations Demo ============"
            
            // Query customers by country (efficient partition key query)
            printfn "\n🌍 Querying customers by country (partition key query)..."
            let! countryCustomers = repo.GetCustomersByCountryAsync customer.country
            match countryCustomers with
            | Ok customers ->
                printfn "✅ Found %d customers in %s:" (List.length customers) customer.country
                customers |> List.take (min 3 (List.length customers)) |> List.iter (fun c ->
                    printfn "   - %s %s (%s) - %s tier" c.firstName c.lastName c.email c.loyaltyTier)
            | Error error ->
                printfn "❌ Failed to query customers: %s" error
            
            // Query active high-value customers (cross-partition query)
            printfn "\n💎 Querying active high-value customers (cross-partition)..."
            let! highValueCustomers = repo.GetActiveHighValueCustomersAsync()
            match highValueCustomers with
            | Ok customers ->
                printfn "✅ Found %d high-value customers:" (List.length customers)
                customers |> List.take (min 3 (List.length customers)) |> List.iter (fun c ->
                    printfn "   - %s %s: $%.2f spent, %s tier" c.firstName c.lastName c.totalSpent c.loyaltyTier)
            | Error error ->
                printfn "❌ Failed to query high-value customers: %s" error
            
            // Query customer order history
            printfn "\n📋 Querying customer order history..."
            let! orderHistory = repo.GetCustomerOrderHistoryAsync customer.customerId
            match orderHistory with
            | Ok orders ->
                printfn "✅ Found %d orders for customer %s:" (List.length orders) customer.customerId
                orders |> List.take (min 3 (List.length orders)) |> List.iter (fun o ->
                    printfn "   - Order %s: $%.2f (%s) on %s" 
                        o.orderId o.totalAmount o.status (o.orderDate.ToString("yyyy-MM-dd")))
            | Error error ->
                printfn "❌ Failed to query order history: %s" error
            
            // Custom SQL query demo
            printfn "\n📊 Custom SQL query - customers with recent activity..."
            let recentDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-ddTHH:mm:ss")
            let sqlQuery = $"SELECT * FROM c WHERE c.isActive = true AND c.lastLoginDate >= '{recentDate}' ORDER BY c.totalSpent DESC"
            let! recentActiveCustomers = repo.QueryCustomersAsync sqlQuery None
            match recentActiveCustomers with
            | Ok customers ->
                printfn "✅ Found %d recently active customers:" (List.length customers)
                customers |> List.take (min 3 (List.length customers)) |> List.iter (fun c ->
                    printfn "   - %s %s: Last login %A, $%.2f spent" 
                        c.firstName c.lastName c.lastLoginDate c.totalSpent)
            | Error error ->
                printfn "❌ Failed to query recent customers: %s" error
        }
    
    let demoAsyncSequenceOperations (repo: CosmosRepository) =
        task {
            printfn "\n============ ASYNC SEQUENCE Operations Demo ============"
            
            // Process customers in batches using async sequence
            printfn "\n🔄 Processing customers with async sequence (batch processing)..."
            let! customerBatch = repo.QueryCustomersAsyncSeq "SELECT * FROM c WHERE c.isActive = true" None
            
            printfn "✅ Processed batch of %d active customers:" (List.length customerBatch)
            customerBatch |> List.take (min 5 (List.length customerBatch)) |> List.iter (fun c ->
                printfn "   - %s %s (%s): %s tier, $%.2f spent" 
                    c.firstName c.lastName c.country c.loyaltyTier c.totalSpent)
            
            // Process orders in current month using async sequence
            let currentMonth = DateTime.Now.ToString("yyyy-MM")
            printfn "\n📅 Processing orders for %s using async sequence..." currentMonth
            let! orderBatch = repo.QueryOrdersAsyncSeq "SELECT * FROM o" (Some currentMonth)
            
            printfn "✅ Processed batch of %d orders for %s:" (List.length orderBatch) currentMonth
            orderBatch |> List.take (min 3 (List.length orderBatch)) |> List.iter (fun o ->
                printfn "   - Order %s: $%.2f (%s) on %s" 
                    o.orderId o.totalAmount o.status (o.orderDate.ToString("yyyy-MM-dd")))
        }
    
    let demoBulkOperations (repo: CosmosRepository) =
        task {
            printfn "\n============ BULK Operations Demo ============"
            
            // Create multiple sample customers
            let sampleCustomers = [ for i in 1..5 -> SampleData.generateSampleCustomer() ]
            
            printfn "\n👥 Bulk creating %d customers..." (List.length sampleCustomers)
            let! bulkResults = repo.BulkCreateCustomersAsync sampleCustomers
            
            let successCount = bulkResults |> List.sumBy (function | Ok _ -> 1 | Error _ -> 0)
            let failureCount = bulkResults |> List.sumBy (function | Ok _ -> 0 | Error _ -> 1)
            
            printfn "✅ Bulk operation completed:"
            printfn "   Successful: %d customers" successCount
            printfn "   Failed: %d customers" failureCount
            
            // Show sample of created customers
            bulkResults 
            |> List.choose (function | Ok customer -> Some customer | Error _ -> None)
            |> List.take 3
            |> List.iter (fun c ->
                printfn "   - Created: %s %s (%s)" c.firstName c.lastName c.country)
            
            // Create multiple sample products
            let sampleProducts = [ for i in 1..3 -> SampleData.generateSampleProduct() ]
            
            printfn "\n📦 Bulk creating %d products..." (List.length sampleProducts)
            let! productBulkResults = repo.BulkCreateProductsAsync sampleProducts
            
            let productSuccessCount = productBulkResults |> List.sumBy (function | Ok _ -> 1 | Error _ -> 0)
            let productFailureCount = productBulkResults |> List.sumBy (function | Ok _ -> 0 | Error _ -> 1)
            
            printfn "✅ Product bulk operation completed:"
            printfn "   Successful: %d products" productSuccessCount
            printfn "   Failed: %d products" productFailureCount
        }
    
    let demoAggregationOperations (repo: CosmosRepository) =
        task {
            printfn "\n============ AGGREGATION Operations Demo ============"
            
            // Get order statistics for current month
            let currentMonth = DateTime.Now.ToString("yyyy-MM")
            printfn "\n📊 Getting order statistics for %s..." currentMonth
            let! statsResult = repo.GetOrderStatisticsByMonthAsync currentMonth
            match statsResult with
            | Ok stats ->
                printfn "✅ Order statistics for %s:" currentMonth
                printfn "   Statistics: %A" stats
            | Error error ->
                printfn "❌ Failed to get order statistics: %s" error
            
            // Get customer analytics summary
            printfn "\n📈 Getting customer analytics summary..."
            let sampleCustomerId = "customer_12345"  // Use a known customer ID
            let! analyticsResult = repo.GetCustomerAnalyticsSummaryAsync sampleCustomerId
            match analyticsResult with
            | Ok (Some analytics) ->
                printfn "✅ Customer analytics found:"
                printfn "   Customer ID: %s" analytics.customerId
                printfn "   Total Orders: %d" analytics.totalOrders
                printfn "   Total Spent: $%.2f" analytics.totalSpent
                printfn "   Average Order Value: $%.2f" analytics.averageOrderValue
                printfn "   Segment: %s" analytics.segment
            | Ok None ->
                printfn "ℹ️  No analytics found for customer %s" sampleCustomerId
            | Error error ->
                printfn "❌ Failed to get customer analytics: %s" error
        }

[<EntryPoint>]
let main argv =
    async {
        try
            printfn "🚀 Azure Cosmos DB F# Demo Starting..."
            printfn "============================================"
            
            // Initialize repository
            use repo = new CosmosRepository(cosmosConfig)
            
            printfn "\n📋 Cosmos DB Configuration:"
            printfn "   Endpoint: %s" cosmosConfig.EndpointUrl
            printfn "   Database: %s" cosmosConfig.DatabaseId
            printfn "   Containers: %s, %s, %s, %s" 
                cosmosConfig.CustomerContainerId 
                cosmosConfig.OrderContainerId 
                cosmosConfig.ProductContainerId 
                cosmosConfig.AnalyticsContainerId
            
            // Run all demos
            let! (customer, product, order) = CosmosDemo.demoCreateOperations repo
            do! CosmosDemo.demoReadOperations repo (customer, product, order)
            do! CosmosDemo.demoUpsertOperations repo (customer, product, order) 
            do! CosmosDemo.demoPatchOperations repo (customer, product, order)
            do! CosmosDemo.demoQueryOperations repo customer
            do! CosmosDemo.demoAsyncSequenceOperations repo
            do! CosmosDemo.demoBulkOperations repo
            do! CosmosDemo.demoAggregationOperations repo
            
            printfn "\n🎉 All Cosmos DB operations completed successfully!"
            printfn "\n============================================"
            printfn "📚 Key Concepts Demonstrated:"
            printfn "   ✅ Databases, Containers, Items"
            printfn "   ✅ Partition Keys & IDs"
            printfn "   ✅ CreateItemAsync - Create new items"
            printfn "   ✅ ReadItemAsync - Point reads with partition key"
            printfn "   ✅ UpsertItemAsync - Create or update items"
            printfn "   ✅ PatchItemAsync - Partial updates"
            printfn "   ✅ queryCosmos - SQL queries"
            printfn "   ✅ queryCosmosAsyncSeq - Async batch processing"
            printfn "   ✅ Cross-partition queries"
            printfn "   ✅ Partition-specific queries"
            printfn "   ✅ Bulk operations"
            printfn "   ✅ Aggregation queries"
            printfn "============================================"
            
            return 0
        with
        | ex ->
            printfn "\n💥 Error: %s" ex.Message
            printfn "Stack trace: %s" ex.StackTrace
            return 1
    } |> Async.RunSynchronously