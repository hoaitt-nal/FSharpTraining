open System
open System.Threading.Tasks
open Microsoft.Azure.Cosmos
open Microsoft.Azure.Cosmos.Linq
open Newtonsoft.Json
open Models

// ============ Azure Cosmos DB Repository Implementation ============

type CosmosConfig = {
    EndpointUrl: string
    PrimaryKey: string
    DatabaseId: string
    CustomerContainerId: string
    OrderContainerId: string  
    ProductContainerId: string
    AnalyticsContainerId: string
}

type CosmosRepository(config: CosmosConfig) =
    
    // Initialize Cosmos Client with connection string
    let cosmosClient = new CosmosClient(config.EndpointUrl, config.PrimaryKey)
    
    // Get database reference
    let database = cosmosClient.GetDatabase(config.DatabaseId)
    
    // Get container references
    let customerContainer = database.GetContainer(config.CustomerContainerId)
    let orderContainer = database.GetContainer(config.OrderContainerId)
    let productContainer = database.GetContainer(config.ProductContainerId)
    let analyticsContainer = database.GetContainer(config.AnalyticsContainerId)
    
    // ============ CRUD Operations - CreateItemAsync ============
    
    /// Create a new customer
    member this.CreateCustomerAsync (customer: Customer) = 
        task {
            try
                let partitionKey = PartitionKey(PartitionKeys.customerPartition customer.country)
                let! response = customerContainer.CreateItemAsync(customer, partitionKey)
                return Ok response.Resource
            with
            | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.Conflict ->
                return Error $"Customer with id {customer.id} already exists"
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    /// Create a new order
    member this.CreateOrderAsync (order: Order) =
        task {
            try
                let partitionKey = PartitionKey(PartitionKeys.orderPartition order.orderDate)
                let! response = orderContainer.CreateItemAsync(order, partitionKey)
                return Ok response.Resource
            with
            | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.Conflict ->
                return Error $"Order with id {order.id} already exists"
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    /// Create a new product
    member this.CreateProductAsync (product: Product) =
        task {
            try
                let partitionKey = PartitionKey(PartitionKeys.productPartition product.category)
                let! response = productContainer.CreateItemAsync(product, partitionKey)
                return Ok response.Resource
            with
            | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.Conflict ->
                return Error $"Product with id {product.id} already exists"
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    // ============ CRUD Operations - ReadItemAsync ============
    
    /// Read customer by id and partition key
    member this.ReadCustomerAsync (customerId: string) (country: string) =
        task {
            try
                let partitionKey = PartitionKey(PartitionKeys.customerPartition country)
                let! response = customerContainer.ReadItemAsync<Customer>(customerId, partitionKey)
                return Ok response.Resource
            with
            | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.NotFound ->
                return Error $"Customer with id {customerId} not found"
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    /// Read order by id and partition key  
    member this.ReadOrderAsync (orderId: string) (orderDate: DateTime) =
        task {
            try
                let partitionKey = PartitionKey(PartitionKeys.orderPartition orderDate)
                let! response = orderContainer.ReadItemAsync<Order>(orderId, partitionKey)
                return Ok response.Resource
            with
            | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.NotFound ->
                return Error $"Order with id {orderId} not found"
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    /// Read product by id and partition key
    member this.ReadProductAsync (productId: string) (category: string) =
        task {
            try
                let partitionKey = PartitionKey(PartitionKeys.productPartition category)
                let! response = productContainer.ReadItemAsync<Product>(productId, partitionKey)
                return Ok response.Resource
            with
            | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.NotFound ->
                return Error $"Product with id {productId} not found"
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    // ============ CRUD Operations - UpsertItemAsync ============
    
    /// Upsert customer (create if not exists, update if exists)
    member this.UpsertCustomerAsync (customer: Customer) =
        task {
            try
                let partitionKey = PartitionKey(PartitionKeys.customerPartition customer.country)
                let! response = customerContainer.UpsertItemAsync(customer, partitionKey)
                return Ok response.Resource
            with
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    /// Upsert order
    member this.UpsertOrderAsync (order: Order) =
        task {
            try
                let partitionKey = PartitionKey(PartitionKeys.orderPartition order.orderDate)
                let! response = orderContainer.UpsertItemAsync(order, partitionKey)
                return Ok response.Resource
            with
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    /// Upsert product  
    member this.UpsertProductAsync (product: Product) =
        task {
            try
                let partitionKey = PartitionKey(PartitionKeys.productPartition product.category)
                let! response = productContainer.UpsertItemAsync(product, partitionKey)
                return Ok response.Resource
            with
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    // ============ CRUD Operations - PatchItemAsync ============
    
    /// Patch customer with partial updates
    member this.PatchCustomerAsync (customerId: string) (country: string) (patchOps: PatchOperation list) =
        task {
            try
                let partitionKey = PartitionKey(PartitionKeys.customerPartition country)
                let! response = customerContainer.PatchItemAsync<Customer>(customerId, partitionKey, patchOps)
                return Ok response.Resource
            with
            | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.NotFound ->
                return Error $"Customer with id {customerId} not found"
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    /// Patch order with partial updates
    member this.PatchOrderAsync (orderId: string) (orderDate: DateTime) (patchOps: PatchOperation list) =
        task {
            try
                let partitionKey = PartitionKey(PartitionKeys.orderPartition orderDate)
                let! response = orderContainer.PatchItemAsync<Order>(orderId, partitionKey, patchOps)
                return Ok response.Resource
            with
            | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.NotFound ->
                return Error $"Order with id {orderId} not found"
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    /// Patch product with partial updates
    member this.PatchProductAsync (productId: string) (category: string) (patchOps: PatchOperation list) =
        task {
            try
                let partitionKey = PartitionKey(PartitionKeys.productPartition category)
                let! response = productContainer.PatchItemAsync<Product>(productId, partitionKey, patchOps)
                return Ok response.Resource
            with
            | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.NotFound ->
                return Error $"Product with id {productId} not found"
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    // ============ Query Operations - queryCosmos ============
    
    /// Query customers with SQL
    member this.QueryCustomersAsync (sqlQuery: string) (partitionKey: string option) =
        task {
            try
                let queryDefinition = QueryDefinition(sqlQuery)
                
                let requestOptions = QueryRequestOptions()
                partitionKey |> Option.iter (fun pk -> 
                    requestOptions.PartitionKey <- PartitionKey(PartitionKeys.customerPartition pk))
                
                let queryIterator = customerContainer.GetItemQueryIterator<Customer>(queryDefinition, requestOptions = requestOptions)
                let results = ResizeArray<Customer>()
                
                while queryIterator.HasMoreResults do
                    let! response = queryIterator.ReadNextAsync()
                    results.AddRange(response)
                
                return Ok (List.ofSeq results)
            with
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    /// Query orders with SQL
    member this.QueryOrdersAsync (sqlQuery: string) (yearMonth: string option) =
        task {
            try
                let queryDefinition = QueryDefinition(sqlQuery)
                
                let requestOptions = QueryRequestOptions()
                yearMonth |> Option.iter (fun ym -> 
                    requestOptions.PartitionKey <- PartitionKey(PartitionKeys.orderPartition (DateTime.Parse($"{ym}-01"))))
                
                let queryIterator = orderContainer.GetItemQueryIterator<Order>(queryDefinition, requestOptions = requestOptions)
                let results = ResizeArray<Order>()
                
                while queryIterator.HasMoreResults do
                    let! response = queryIterator.ReadNextAsync()
                    results.AddRange(response)
                
                return Ok (List.ofSeq results)
            with
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    /// Query products with SQL  
    member this.QueryProductsAsync (sqlQuery: string) (category: string option) =
        task {
            try
                let queryDefinition = QueryDefinition(sqlQuery)
                
                let requestOptions = QueryRequestOptions()
                category |> Option.iter (fun cat -> 
                    requestOptions.PartitionKey <- PartitionKey(PartitionKeys.productPartition cat))
                
                let queryIterator = productContainer.GetItemQueryIterator<Product>(queryDefinition, requestOptions = requestOptions)
                let results = ResizeArray<Product>()
                
                while queryIterator.HasMoreResults do
                    let! response = queryIterator.ReadNextAsync()
                    results.AddRange(response)
                
                return Ok (List.ofSeq results)
            with
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    // ============ queryCosmosAsyncSeq - Async Sequence Processing ============
    
    /// Query customers as async sequence for large datasets
    member this.QueryCustomersAsyncSeq (sqlQuery: string) (partitionKey: string option) =
        async {
            try
                let queryDefinition = QueryDefinition(sqlQuery)
                
                let requestOptions = QueryRequestOptions()
                requestOptions.MaxItemCount <- 100 // Process in batches of 100
                partitionKey |> Option.iter (fun pk -> 
                    requestOptions.PartitionKey <- PartitionKey(PartitionKeys.customerPartition pk))
                
                let queryIterator = customerContainer.GetItemQueryIterator<Customer>(queryDefinition, requestOptions = requestOptions)
                let results = ResizeArray<Customer>()
                
                while queryIterator.HasMoreResults do
                    let! response = queryIterator.ReadNextAsync() |> Async.AwaitTask
                    results.AddRange(response)
                    
                    // Yield batch results for processing
                    if results.Count >= 100 then
                        let batch = List.ofSeq results
                        results.Clear()
                        return batch
                
                // Return remaining items
                return List.ofSeq results
            with
            | :? CosmosException as ex ->
                return []
            | ex ->
                return []
        }
    
    /// Query orders as async sequence
    member this.QueryOrdersAsyncSeq (sqlQuery: string) (yearMonth: string option) =
        async {
            try
                let queryDefinition = QueryDefinition(sqlQuery)
                
                let requestOptions = QueryRequestOptions()
                requestOptions.MaxItemCount <- 50
                yearMonth |> Option.iter (fun ym -> 
                    requestOptions.PartitionKey <- PartitionKey(PartitionKeys.orderPartition (DateTime.Parse($"{ym}-01"))))
                
                let queryIterator = orderContainer.GetItemQueryIterator<Order>(queryDefinition, requestOptions = requestOptions)
                let results = ResizeArray<Order>()
                
                while queryIterator.HasMoreResults do
                    let! response = queryIterator.ReadNextAsync() |> Async.AwaitTask
                    results.AddRange(response)
                
                return List.ofSeq results
            with
            | ex ->
                return []
        }
    
    // ============ Advanced Query Operations ============
    
    /// Get customers by country (using partition key efficiently)
    member this.GetCustomersByCountryAsync (country: string) =
        let sqlQuery = "SELECT * FROM c WHERE c.country = @country"
        this.QueryCustomersAsync sqlQuery (Some country)
    
    /// Get active customers with high loyalty tier
    member this.GetActiveHighValueCustomersAsync () =
        let sqlQuery = "SELECT * FROM c WHERE c.isActive = true AND c.loyaltyTier IN ('Gold', 'Platinum') ORDER BY c.totalSpent DESC"
        this.QueryCustomersAsync sqlQuery None
    
    /// Get orders by status in date range
    member this.GetOrdersByStatusAndDateAsync (status: string) (startDate: DateTime) (endDate: DateTime) =
        let sqlQuery = $"SELECT * FROM o WHERE o.status = '{status}' AND o.orderDate >= '{startDate:yyyy-MM-ddTHH:mm:ss}' AND o.orderDate <= '{endDate:yyyy-MM-ddTHH:mm:ss}'"
        this.QueryOrdersAsync sqlQuery None
    
    /// Get products by category and price range
    member this.GetProductsByCategoryAndPriceAsync (category: string) (minPrice: decimal) (maxPrice: decimal) =
        let sqlQuery = $"SELECT * FROM p WHERE p.category = '{category}' AND p.price >= {minPrice} AND p.price <= {maxPrice} AND p.inStock = true ORDER BY p.price"
        this.QueryProductsAsync sqlQuery (Some category)
    
    /// Get customer order history
    member this.GetCustomerOrderHistoryAsync (customerId: string) =
        let sqlQuery = $"SELECT * FROM o WHERE o.customerId = '{customerId}' ORDER BY o.orderDate DESC"
        this.QueryOrdersAsync sqlQuery None
    
    // ============ Aggregation Queries ============
    
    /// Get order statistics by month
    member this.GetOrderStatisticsByMonthAsync (yearMonth: string) =
        task {
            try
                let sqlQuery = $"SELECT COUNT(1) as totalOrders, SUM(o.totalAmount) as totalRevenue, AVG(o.totalAmount) as averageOrderValue FROM o WHERE o.orderDate >= '{yearMonth}-01' AND o.orderDate < '{yearMonth}-31'"
                let queryDefinition = QueryDefinition(sqlQuery)
                
                let queryIterator = orderContainer.GetItemQueryIterator<obj>(queryDefinition)
                let! response = queryIterator.ReadNextAsync()
                
                return Ok (Seq.head response)
            with
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    /// Get customer analytics summary
    member this.GetCustomerAnalyticsSummaryAsync (customerId: string) =
        task {
            try
                let sqlQuery = $"SELECT TOP 1 * FROM a WHERE a.customerId = '{customerId}' ORDER BY a.lastAnalyzed DESC"
                let queryDefinition = QueryDefinition(sqlQuery)
                
                let queryIterator = analyticsContainer.GetItemQueryIterator<CustomerAnalytics>(queryDefinition)
                let! response = queryIterator.ReadNextAsync()
                
                let result = Seq.tryHead response
                return Ok result
            with
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    // ============ Batch Operations ============
    
    /// Bulk create customers
    member this.BulkCreateCustomersAsync (customers: Customer list) =
        task {
            let results = ResizeArray<Result<Customer, string>>()
            
            for customer in customers do
                let! result = this.CreateCustomerAsync customer
                results.Add(result)
            
            return List.ofSeq results
        }
    
    /// Bulk create products  
    member this.BulkCreateProductsAsync (products: Product list) =
        task {
            let results = ResizeArray<Result<Product, string>>()
            
            for product in products do
                let! result = this.CreateProductAsync product
                results.Add(result)
            
            return List.ofSeq results
        }
    
    // ============ Delete Operations ============
    
    /// Delete customer by id and partition key
    member this.DeleteCustomerAsync (customerId: string) (country: string) =
        task {
            try
                let partitionKey = PartitionKey(PartitionKeys.customerPartition country)
                let! response = customerContainer.DeleteItemAsync<Customer>(customerId, partitionKey)
                return Ok true
            with
            | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.NotFound ->
                return Error $"Customer with id {customerId} not found"
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    /// Delete order by id and partition key
    member this.DeleteOrderAsync (orderId: string) (orderDate: DateTime) =
        task {
            try
                let partitionKey = PartitionKey(PartitionKeys.orderPartition orderDate)
                let! response = orderContainer.DeleteItemAsync<Order>(orderId, partitionKey)
                return Ok true
            with
            | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.NotFound ->
                return Error $"Order with id {orderId} not found"
            | :? CosmosException as ex ->
                return Error $"Cosmos DB error: {ex.Message}"
            | ex ->
                return Error $"Unexpected error: {ex.Message}"
        }
    
    /// Dispose resources
    interface IDisposable with
        member this.Dispose() =
            cosmosClient.Dispose()