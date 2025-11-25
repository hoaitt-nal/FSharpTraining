# 🌟 Azure Cosmos DB with F# - Complete Guide

## 📋 Overview
This project demonstrates comprehensive Azure Cosmos DB usage in F# with all CRUD operations, advanced querying, and best practices following the Azure Cosmos DB guidelines.

## 🏗️ Core Azure Cosmos DB Concepts

### 🔹 **1. Databases**
- **Định nghĩa**: Logical container chứa các containers
- **Tương đương**: Database trong SQL Server, MongoDB database
- **Đặc điểm**: 
  - Một Cosmos account có thể chứa nhiều databases
  - Database không có throughput settings riêng (trừ khi shared)
  - Quản lý security và access controls

### 🔹 **2. Containers (Collections)**
- **Định nghĩa**: Logical container chứa các items (documents)
- **Tương đương**: Table trong SQL, Collection trong MongoDB
- **Đặc điểm**:
  - **Partition Key Strategy**: Cách phân tán data across partitions
  - **Throughput (RU/s)**: Request Units per second
  - **Indexing Policy**: Cách index các fields
  - **TTL (Time to Live)**: Auto-delete expired items

```fsharp
// Container examples trong project:
// - Customers container: Partition by country (geographic distribution)
// - Orders container: Partition by orderDate (time-based distribution)  
// - Products container: Partition by category (category-based distribution)
// - Analytics container: Partition by yearMonth (time-based analytics)
```

### 🔹 **3. Items (Documents)**
- **Định nghĩa**: Individual JSON documents trong containers
- **Tương đương**: Row trong SQL, Document trong MongoDB
- **Đặc điểm**:
  - **Schema-flexible**: Không cần fixed schema
  - **JSON format**: Native JSON storage
  - **Max size**: 2MB per item
  - **Auto-indexing**: Tự động index tất cả properties

```fsharp
// Example Item structure:
[<CLIMutable>]
type Customer = {
    id: string                    // Required unique identifier
    customerId: string           // Business identifier
    email: string               // Data fields
    firstName: string
    lastName: string
    country: string             // Partition key
    // ... other fields
    _etag: string option        // For optimistic concurrency
}
```

### 🔹 **4. Partition Key**
- **Định nghĩa**: Strategy để distribute data evenly across physical partitions
- **Quan trọng**: 
  - **Không thể thay đổi** sau khi container được tạo
  - **Ảnh hưởng performance**: Queries trong cùng partition = fast, cross-partition = slower
  - **Scalability**: Good partition key = better scaling

#### **Partition Key Selection Guidelines:**

```fsharp
// ✅ GOOD Partition Keys:
// 1. Geographic distribution
let customerPartition country = country.ToUpper()  // "USA", "UK", "GERMANY"

// 2. Time-based distribution  
let orderPartition orderDate = orderDate.ToString("yyyy-MM")  // "2024-11", "2024-12"

// 3. Category-based distribution
let productPartition category = category.ToLower()  // "electronics", "clothing"

// ❌ BAD Partition Keys:
// - Low cardinality: status (only "active"/"inactive")  
// - Hot partition: always same value
// - Sequential: incremental IDs causing hot spots
```

### 🔹 **5. ID (Unique Identifier)**
- **Định nghĩa**: Unique identifier cho item trong partition
- **Kết hợp**: ID + Partition Key = Composite Primary Key
- **Đặc điểm**:
  - **Unique per partition**: Chỉ cần unique trong partition, không phải globally
  - **Point reads**: Dùng cho ReadItemAsync (fastest operation)
  - **String type**: Always string trong Cosmos DB

```fsharp
// ID Generation strategies:
module IdGeneration =
    let generateCustomerId () = $"customer_{Guid.NewGuid().ToString("N")[..7]}"
    let generateOrderId () = $"order_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString("N")[..7]}"
    let generateProductId () = $"product_{Guid.NewGuid().ToString("N")[..7]}"

// Point read example (fastest):
let! customer = repo.ReadCustomerAsync "customer_abc123" "USA"  // ID + Partition Key
```

## 🚀 CRUD Operations Implementation

### 📝 **CreateItemAsync**
- **Mục đích**: Tạo item mới (fail nếu đã tồn tại)
- **Performance**: ~5-10 RUs cho small items
- **Error handling**: CosmosException khi conflict

```fsharp
member this.CreateCustomerAsync (customer: Customer) = 
    task {
        try
            let partitionKey = PartitionKey(PartitionKeys.customerPartition customer.country)
            let! response = customerContainer.CreateItemAsync(customer, partitionKey)
            return Ok response.Resource
        with
        | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.Conflict ->
            return Error $"Customer with id {customer.id} already exists"
    }
```

### 📖 **ReadItemAsync** 
- **Mục đích**: Point read với ID + Partition Key (fastest operation)
- **Performance**: ~1 RU (cheapest operation)
- **Requirement**: Cần biết chính xác ID và Partition Key

```fsharp
member this.ReadCustomerAsync (customerId: string) (country: string) =
    task {
        try
            let partitionKey = PartitionKey(PartitionKeys.customerPartition country)
            let! response = customerContainer.ReadItemAsync<Customer>(customerId, partitionKey)
            return Ok response.Resource
        with
        | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.NotFound ->
            return Error $"Customer with id {customerId} not found"
    }
```

### 🔄 **UpsertItemAsync**
- **Mục đích**: Create nếu không tồn tại, Update nếu đã tồn tại
- **Performance**: ~5-15 RUs tùy thuộc item size
- **Use case**: Khi không chắc item đã tồn tại hay chưa

```fsharp
member this.UpsertCustomerAsync (customer: Customer) =
    task {
        try
            let partitionKey = PartitionKey(PartitionKeys.customerPartition customer.country)
            let! response = customerContainer.UpsertItemAsync(customer, partitionKey)
            return Ok response.Resource
        with
        | :? CosmosException as ex ->
            return Error $"Cosmos DB error: {ex.Message}"
    }
```

### ⚡ **PatchItemAsync**
- **Mục đích**: Partial updates (chỉ update specific fields)
- **Performance**: ~2-5 RUs (efficient hơn replace toàn bộ item)
- **Ưu điểm**: 
  - Atomic operations
  - Bandwidth efficient  
  - Support increment/decrement operations

```fsharp
member this.PatchCustomerAsync (customerId: string) (country: string) (patchOps: PatchOperation list) =
    task {
        try
            let partitionKey = PartitionKey(PartitionKeys.customerPartition country)
            let! response = customerContainer.PatchItemAsync<Customer>(customerId, partitionKey, patchOps)
            return Ok response.Resource

// Usage example:
let patchOps = [
    PatchOperation.Set("/loyaltyTier", "Platinum")        // Set field
    PatchOperation.Increment("/totalOrders", 5)          // Increment number  
    PatchOperation.Add("/preferences/newsletter", true)   // Add to nested object
]
```

## 🔍 Query Operations

### 📊 **queryCosmos (SQL Queries)**
- **Mục đích**: SQL-like queries với flexibility cao
- **Performance**: Varies based on query complexity và partition strategy
- **Syntax**: SQL-like với JSON path expressions

```fsharp
// Partition-specific query (efficient)
let sqlQuery = "SELECT * FROM c WHERE c.country = @country AND c.isActive = true"
let! customers = repo.QueryCustomersAsync sqlQuery (Some "USA")

// Cross-partition query (more expensive)
let sqlQuery = "SELECT * FROM c WHERE c.loyaltyTier = 'Platinum' ORDER BY c.totalSpent DESC"
let! customers = repo.QueryCustomersAsync sqlQuery None

// Aggregation query
let sqlQuery = "SELECT COUNT(1) as total, AVG(c.totalSpent) as avgSpent FROM c WHERE c.isActive = true"
```

### ⚡ **queryCosmosAsyncSeq (Async Sequence)**
- **Mục đích**: Process large datasets trong batches
- **Performance**: Memory efficient, streaming processing
- **Use case**: Analytics, bulk processing, ETL operations

```fsharp
member this.QueryCustomersAsyncSeq (sqlQuery: string) (partitionKey: string option) =
    async {
        let queryDefinition = QueryDefinition(sqlQuery)
        let requestOptions = QueryRequestOptions()
        requestOptions.MaxItemCount <- 100 // Process in batches of 100
        
        let queryIterator = customerContainer.GetItemQueryIterator<Customer>(queryDefinition, requestOptions = requestOptions)
        let results = ResizeArray<Customer>()
        
        while queryIterator.HasMoreResults do
            let! response = queryIterator.ReadNextAsync() |> Async.AwaitTask
            results.AddRange(response)
            
            // Process batch và yield results
            if results.Count >= 100 then
                let batch = List.ofSeq results
                results.Clear()
                return batch
        
        return List.ofSeq results
    }
```

## 🎯 Best Practices & Performance Tips

### 🔹 **1. Partition Key Design**
```fsharp
// ✅ Good: Even distribution
let customerPartition country = country.ToUpper()  // Geographic spread

// ✅ Good: Time-based for time-series data  
let orderPartition orderDate = orderDate.ToString("yyyy-MM")  // Monthly partitions

// ❌ Bad: Hot partition
let badPartition () = "single_value"  // All data in one partition

// ❌ Bad: Sequential IDs
let badPartition orderId = orderId.ToString()  // Creates hot spots
```

### 🔹 **2. Query Optimization**
```fsharp
// ✅ Efficient: Point read
let! customer = repo.ReadCustomerAsync customerId country

// ✅ Efficient: Single partition query
let! customers = repo.QueryCustomersAsync "SELECT * FROM c WHERE c.country = 'USA'" (Some "USA")

// ⚠️  Expensive: Cross-partition query
let! customers = repo.QueryCustomersAsync "SELECT * FROM c WHERE c.email LIKE '%@gmail.com'" None

// ✅ Efficient: Use composite index
let! customers = repo.QueryCustomersAsync "SELECT * FROM c WHERE c.country = 'USA' AND c.loyaltyTier = 'Gold'" (Some "USA")
```

### 🔹 **3. Indexing Strategy**
```json
// Container indexing policy example:
{
    "indexingMode": "consistent",
    "automatic": true,
    "includedPaths": [
        {
            "path": "/*"
        }
    ],
    "excludedPaths": [
        {
            "path": "/largeBlobField/*"  // Exclude large fields
        }
    ],
    "compositeIndexes": [
        [
            {
                "path": "/country",
                "order": "ascending"
            },
            {
                "path": "/loyaltyTier", 
                "order": "ascending"
            }
        ]
    ]
}
```

### 🔹 **4. Error Handling Patterns**
```fsharp
// Comprehensive error handling
member this.CreateCustomerAsync (customer: Customer) = 
    task {
        try
            let partitionKey = PartitionKey(PartitionKeys.customerPartition customer.country)
            let! response = customerContainer.CreateItemAsync(customer, partitionKey)
            return Ok response.Resource
        with
        | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.Conflict ->
            return Error $"Customer already exists: {customer.id}"
        | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.TooManyRequests ->
            return Error $"Rate limited (429): {ex.RetryAfter}"
        | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.RequestEntityTooLarge ->
            return Error $"Item too large (>2MB): {customer.id}"
        | :? CosmosException as ex ->
            return Error $"Cosmos DB error ({ex.StatusCode}): {ex.Message}"
        | ex ->
            return Error $"Unexpected error: {ex.Message}"
    }
```

## 🏃‍♂️ Running the Demo

### Prerequisites
1. **Azure Cosmos DB Emulator** hoặc **Azure Cosmos DB account**
2. **.NET 8.0 SDK**
3. **F# development environment**

### Setup Steps

1. **Install Cosmos DB Emulator** (for local development):
   ```bash
   # Download and install from: https://docs.microsoft.com/azure/cosmos-db/local-emulator
   # Or use Docker:
   docker run -p 8081:8081 -p 10251:10251 -p 10252:10252 -p 10253:10253 -p 10254:10254 mcr.microsoft.com/cosmosdb/emulator:latest
   ```

2. **Update connection string** (if using Azure):
   ```fsharp
   let cosmosConfig = {
       EndpointUrl = "https://your-account.documents.azure.com:443/"
       PrimaryKey = "your-primary-key"
       DatabaseId = "FSharpCosmosDemo"
       // ... containers
   }
   ```

3. **Run the demo**:
   ```bash
   cd CosmosDB-Demo
   dotnet run
   ```

### Demo Output
The program will demonstrate:
- ✅ **CREATE**: Creating customers, products, orders
- ✅ **READ**: Point reads by ID + partition key  
- ✅ **UPSERT**: Create/update operations
- ✅ **PATCH**: Partial field updates
- ✅ **QUERY**: SQL queries (single & cross-partition)
- ✅ **ASYNC SEQ**: Batch processing với async sequences
- ✅ **BULK OPS**: Bulk create operations
- ✅ **AGGREGATION**: Statistics và analytics queries

## 📊 Performance Monitoring

### Request Units (RU) Consumption
```fsharp
// Monitor RU consumption in responses
let! response = customerContainer.CreateItemAsync(customer, partitionKey)
printfn "RU Consumed: %.2f" response.RequestCharge

// Set RU budget limits
let requestOptions = ItemRequestOptions()
requestOptions.ConsistencyLevel <- ConsistencyLevel.Session
```

### Diagnostic Information  
```fsharp
// Enable detailed diagnostics
let! response = customerContainer.ReadItemAsync<Customer>(customerId, partitionKey)
printfn "Diagnostics: %s" response.Diagnostics.ToString()
```

## 🔗 Resources

- [Azure Cosmos DB Documentation](https://docs.microsoft.com/azure/cosmos-db/)
- [Cosmos DB .NET SDK](https://docs.microsoft.com/azure/cosmos-db/sql/sql-api-sdk-dotnet-standard)
- [Partition Key Design](https://docs.microsoft.com/azure/cosmos-db/partitioning-overview)
- [Query Optimization](https://docs.microsoft.com/azure/cosmos-db/sql/how-to-sql-query)
- [Best Practices](https://docs.microsoft.com/azure/cosmos-db/sql/best-practice-dotnet)

## 🎯 Key Takeaways

1. **Partition Key** là quan trọng nhất cho performance và scalability
2. **Point reads** (ReadItemAsync) là fastest và cheapest operations  
3. **Cross-partition queries** expensive hơn single-partition queries
4. **PatchItemAsync** efficient hơn full item replacement
5. **Async sequences** tốt cho large dataset processing
6. **Error handling** cần handle các CosmosException types khác nhau
7. **Indexing strategy** ảnh hưởng lớn đến query performance