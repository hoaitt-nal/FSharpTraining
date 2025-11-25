# 🔄 F# Pipeline Transformations - Complete Guide

> **Hướng dẫn toàn diện về F# Pipeline Transformations** - Xử lý dữ liệu theo cách functional và elegant

## 🚀 Quick Start
```fsharp
// Transform data với pipeline operator |>
let result = 
    [1; 2; 3; 4; 5]
    |> List.map (fun x -> x * 2)      // [2; 4; 6; 8; 10]
    |> List.filter (fun x -> x > 5)   // [6; 8; 10] 
    |> List.sum                       // 24
```

## 📋 Table of Contents

### 🎯 **Fundamentals**
1. [Introduction & Benefits](#introduction--benefits) - Tại sao dùng pipelines
2. [Basic Pipeline Operators](#basic-pipeline-operators) - Các operators cơ bản
3. [Core Transformation Functions](#core-transformation-functions) - Map, Filter, Fold

### 🔄 **Data Processing Patterns**
4. [List Processing Pipelines](#list-processing-pipelines) - Xử lý lists
5. [Sequence Transformations](#sequence-transformations) - Lazy sequences  
6. [Array Operations](#array-operations) - High-performance arrays
7. [String Processing](#string-processing) - Text manipulation

### 🎯 **Advanced Patterns**
8. [Composition & Chaining](#composition--chaining) - Kết hợp functions
9. [Error Handling in Pipelines](#error-handling-in-pipelines) - Railway-oriented programming
10. [Async Pipelines](#async-pipelines) - Combine với Async workflows
11. [Custom Operators](#custom-operators) - Tạo operators riêng

### 🛠️ **Real-world Applications**
12. [Data Processing Examples](#data-processing-examples) - CSV, JSON, APIs
13. [Business Logic Pipelines](#business-logic-pipelines) - Domain modeling
14. [Performance Optimization](#performance-optimization) - Tips & tricks

### 📚 **Reference**
15. [Pipeline Operators Reference](#pipeline-operators-reference) - Tất cả operators
16. [Common Patterns Cheatsheet](#common-patterns-cheatsheet) - Templates thông dụng

---

## Introduction & Benefits

**F# Pipeline Transformations** là cách functional và elegant để xử lý dữ liệu, sử dụng pipeline operator `|>` để tạo ra data flow rõ ràng và dễ đọc.

### 🎯 **Tại sao dùng Pipelines?**

| 🔥 **F# Pipeline** | 🌐 **Imperative Style** | 💡 **Ưu điểm** |
|-------------------|-------------------------|----------------|
| `data |> transform |> filter` | `filter(transform(data))` | ✅ Readable left-to-right |
| `|> List.map f` | `List.map f list` | ✅ Natural data flow |
| `|> List.filter p` | Nested function calls | ✅ Easy to modify |
| Functional composition | Mutable variables | ✅ Immutable by design |

### 🚀 **Core Benefits**
- **🎯 Readability**: Data flows từ trái sang phải, natural reading
- **🔧 Composability**: Dễ dàng thêm/xóa/sửa transformations
- **⚡ Performance**: Compiler optimizations cho functional chains
- **🛡️ Safety**: Immutable data, không có side effects
- **🧪 Testability**: Mỗi transformation function dễ test riêng

### **💡 Mental Model: Assembly Line**
```fsharp
// Tưởng tượng như dây chuyền sản xuất
let processOrders orders =
    orders
    |> validateOrders        // Quality check station
    |> calculateTotals       // Pricing station  
    |> applyDiscounts        // Discount station
    |> generateInvoices      // Invoice station
    |> sendNotifications     // Shipping station
```

---

## Basic Pipeline Operators

### 🔄 **Essential Operators**

| **Operator** | **Name** | **Purpose** | **Example** |
|--------------|----------|-------------|-------------|
| `\|>` | Forward pipe | Data flows left to right | `x \|> f` = `f x` |
| `<\|` | Backward pipe | Function application | `f <\| x` = `f x` |
| `>>` | Forward composition | Combine functions | `(f >> g) x` = `g(f x)` |
| `<<` | Backward composition | Combine functions | `(f << g) x` = `f(g x)` |

### 📝 **Basic Examples**

#### **🎯 Forward Pipeline (`|>`)**
```fsharp
// Traditional function application
let result1 = List.sum (List.filter (fun x -> x > 3) (List.map (fun x -> x * 2) [1; 2; 3; 4; 5]))

// Pipeline style - much cleaner!
let result2 = 
    [1; 2; 3; 4; 5]
    |> List.map (fun x -> x * 2)      // Transform: [2; 4; 6; 8; 10]
    |> List.filter (fun x -> x > 3)   // Filter: [4; 6; 8; 10]  
    |> List.sum                       // Aggregate: 28

// result1 = result2 = 28
```

#### **🔄 Function Composition (`>>`)**
```fsharp
// Create reusable transformation pipeline
let processNumbers = 
    List.map (fun x -> x * 2)
    >> List.filter (fun x -> x > 3) 
    >> List.sum

// Use it multiple times
let result1 = [1; 2; 3] |> processNumbers  // 10
let result2 = [4; 5; 6] |> processNumbers  // 30
```

---

## Core Transformation Functions

### 🔄 **The Big Three: Map, Filter, Fold**

#### **🎯 Map - Transform Each Element**
```fsharp
// Transform every element
let doubles = [1; 2; 3; 4] |> List.map (fun x -> x * 2)        // [2; 4; 6; 8]
let strings = [1; 2; 3; 4] |> List.map (fun x -> $"Item {x}")  // ["Item 1"; "Item 2"; ...]

// Real-world example: Process user data  
type User = { Id: int; Name: string; Email: string }
let formatUsers users =
    users 
    |> List.map (fun user -> $"{user.Name} ({user.Email})")
```

#### **🔍 Filter - Select Elements**
```fsharp
// Keep only elements that match condition
let evens = [1; 2; 3; 4; 5; 6] |> List.filter (fun x -> x % 2 = 0)  // [2; 4; 6]
let longNames = ["An"; "Bob"; "Charlie"; "D"] |> List.filter (fun s -> s.Length > 3)  // ["Charlie"]

// Real-world: Filter active orders
type Order = { Id: int; Status: string; Amount: decimal }
let activeOrders orders = 
    orders |> List.filter (fun order -> order.Status = "Active")
```

#### **📊 Fold/Reduce - Aggregate to Single Value**
```fsharp
// Fold: Start with initial value, combine each element
let sum = [1; 2; 3; 4; 5] |> List.fold (+) 0                    // 15
let product = [1; 2; 3; 4; 5] |> List.fold (*) 1               // 120
let concat = ["Hello"; " "; "World"] |> List.fold (+) ""        // "Hello World"

// Real-world: Calculate total order amount
let totalAmount orders =
    orders 
    |> List.fold (fun total order -> total + order.Amount) 0m
```

### **🎭 Advanced Transformations**

#### **🔄 Collect - Map and Flatten**
```fsharp
// Map each element to a list, then flatten
let words = ["hello world"; "foo bar"] 
let allWords = words |> List.collect (fun s -> s.Split(' ') |> Array.toList)
// Result: ["hello"; "world"; "foo"; "bar"]

// Real-world: Extract all tags from posts
type Post = { Title: string; Tags: string list }
let allTags posts = 
    posts |> List.collect (fun post -> post.Tags)
```

#### **🎯 Choose - Map and Filter Combined**  
```fsharp
// Transform and filter in one step
let parseNumbers strings =
    strings 
    |> List.choose (fun s -> 
        match System.Int32.TryParse(s) with
        | true, n -> Some n
        | false, _ -> None)

let result = ["1"; "abc"; "42"; "xyz"; "7"] |> parseNumbers  // [1; 42; 7]
```

#### **📦 GroupBy - Organize Data**
```fsharp
// Group elements by a key function
let numbers = [1; 2; 3; 4; 5; 6; 7; 8; 9; 10]
let grouped = numbers |> List.groupBy (fun x -> x % 3)
// Result: [(1, [1; 4; 7; 10]); (2, [2; 5; 8]); (0, [3; 6; 9])]

// Real-world: Group orders by customer
let ordersByCustomer orders =
    orders |> List.groupBy (fun order -> order.CustomerId)
```

---

## List Processing Pipelines

### 📋 **Complete List Pipeline Examples**

#### **🛒 E-commerce Order Processing**
```fsharp
type Customer = { Id: int; Name: string; IsVip: bool }
type Product = { Id: int; Name: string; Price: decimal; Category: string }
type OrderItem = { Product: Product; Quantity: int }
type Order = { Id: int; Customer: Customer; Items: OrderItem list; Status: string }

let processOrders (orders: Order list) =
    orders
    |> List.filter (fun order -> order.Status = "Pending")           // Only pending orders
    |> List.filter (fun order -> order.Items |> List.length > 0)     // Has items
    |> List.map (fun order ->                                        // Calculate totals
        let total = 
            order.Items 
            |> List.map (fun item -> item.Product.Price * decimal item.Quantity)
            |> List.sum
        {| Order = order; Total = total |})
    |> List.filter (fun x -> x.Total > 0m)                          // Valid total
    |> List.sortByDescending (fun x -> x.Total)                     // Sort by amount
    |> List.map (fun x ->                                           // Apply VIP discount
        let discount = if x.Order.Customer.IsVip then 0.1m else 0m
        let finalTotal = x.Total * (1m - discount)
        {| x with Total = finalTotal |})
```

#### **📊 Sales Analytics Pipeline**
```fsharp
let analyzeSales (orders: Order list) =
    let salesByCategory =
        orders
        |> List.collect (fun order -> order.Items)                   // Flatten all items
        |> List.groupBy (fun item -> item.Product.Category)          // Group by category  
        |> List.map (fun (category, items) ->                       // Calculate category totals
            let totalRevenue = 
                items 
                |> List.map (fun item -> item.Product.Price * decimal item.Quantity)
                |> List.sum
            let totalQuantity = items |> List.map (fun item -> item.Quantity) |> List.sum
            {| Category = category; Revenue = totalRevenue; Quantity = totalQuantity |})
        |> List.sortByDescending (fun x -> x.Revenue)                // Top categories first
    
    let topCustomers =
        orders
        |> List.groupBy (fun order -> order.Customer)                // Group by customer
        |> List.map (fun (customer, customerOrders) ->              // Calculate customer value
            let totalSpent = 
                customerOrders
                |> List.collect (fun order -> order.Items)
                |> List.map (fun item -> item.Product.Price * decimal item.Quantity)  
                |> List.sum
            let orderCount = customerOrders |> List.length
            {| Customer = customer; TotalSpent = totalSpent; OrderCount = orderCount |})
        |> List.sortByDescending (fun x -> x.TotalSpent)
        |> List.take 10                                             // Top 10 customers
    
    {| SalesByCategory = salesByCategory; TopCustomers = topCustomers |}
```

---

## Sequence Transformations

### ⚡ **Lazy Evaluation với Sequences**

Sequences (`seq`) là lazy - chỉ tính khi cần, rất hiệu quả cho large datasets.

#### **🎯 Basic Sequence Operations**
```fsharp
// Create infinite sequence (lazy!)
let infiniteNumbers = Seq.initInfinite id  // 0, 1, 2, 3, 4, ...

let processedNumbers = 
    infiniteNumbers
    |> Seq.filter (fun x -> x % 2 = 0)      // Even numbers only
    |> Seq.map (fun x -> x * x)             // Square them
    |> Seq.take 5                           // Take first 5
    |> Seq.toList                           // Materialize: [0; 4; 16; 36; 64]
```

#### **📁 File Processing Pipeline**
```fsharp
open System.IO

let processLargeLogFile filePath =
    File.ReadLines(filePath)                           // Lazy file reading
    |> Seq.filter (fun line -> line.Contains("ERROR")) // Only error lines
    |> Seq.map (fun line ->                           // Parse log entry
        let parts = line.Split('\t')
        {| Timestamp = parts.[0]; Message = parts.[1] |})
    |> Seq.groupBy (fun entry -> entry.Timestamp.Substring(0, 10)) // Group by date
    |> Seq.map (fun (date, entries) ->                // Count errors per day
        {| Date = date; ErrorCount = Seq.length entries |})
    |> Seq.sortBy (fun x -> x.Date)
    |> Seq.toList

// Memory efficient - processes one line at a time!
```

#### **🔄 Streaming Data Pipeline**
```fsharp
// Simulate real-time data stream
let generateSensorData() = 
    Seq.initInfinite (fun i -> 
        {| Id = i; Temperature = Random().NextDouble() * 100.0; Timestamp = DateTime.Now |})

let monitorTemperature() =
    generateSensorData()
    |> Seq.filter (fun reading -> reading.Temperature > 80.0)    // High temperature alert
    |> Seq.map (fun reading ->                                   // Format alert
        $"ALERT: Sensor {reading.Id} - {reading.Temperature:F1}°C at {reading.Timestamp}")
    |> Seq.take 10                                              // First 10 alerts
    |> Seq.iter (printfn "%s")                                  // Print each alert
```

---

## Array Operations

### ⚡ **High-Performance Array Processing**

Arrays provide better performance for computational intensive operations.

#### **🎯 Parallel Array Processing**
```fsharp
let largeDataProcessing (data: float[]) =
    data
    |> Array.Parallel.map (fun x -> Math.Sin(x) * Math.Cos(x))   // CPU intensive operation
    |> Array.Parallel.filter (fun x -> x > 0.0)                  // Parallel filter
    |> Array.sort                                                 // Sort results
    |> Array.take 1000                                           // Take top 1000
```

#### **📊 Statistical Analysis Pipeline**  
```fsharp
let analyzeDataset (numbers: float[]) =
    let mean = numbers |> Array.average
    let stdDev = 
        numbers 
        |> Array.map (fun x -> (x - mean) ** 2.0)
        |> Array.average
        |> sqrt
    
    let outliers = 
        numbers
        |> Array.filter (fun x -> abs(x - mean) > 2.0 * stdDev)
        
    let percentiles =
        numbers
        |> Array.sort
        |> fun sorted -> 
            let len = sorted.Length
            {| P25 = sorted.[len/4]; P50 = sorted.[len/2]; P75 = sorted.[3*len/4] |}
    
    {| Mean = mean; StdDev = stdDev; Outliers = outliers; Percentiles = percentiles |}
```

---

## String Processing

### 📝 **Text Manipulation Pipelines**

#### **🔍 Text Analysis Pipeline**
```fsharp
let analyzeText (text: string) =
    text.Split([|'.'; '!'; '?'|], StringSplitOptions.RemoveEmptyEntries)  // Split to sentences
    |> Array.map (fun sentence -> sentence.Trim())                         // Clean whitespace
    |> Array.filter (fun sentence -> sentence.Length > 0)                 // Remove empty
    |> Array.collect (fun sentence ->                                      // Extract words
        sentence.Split([|' '; '\t'; '\n'|], StringSplitOptions.RemoveEmptyEntries))
    |> Array.map (fun word -> word.ToLower().Trim([|','; ';'; ':'|]))     // Normalize
    |> Array.filter (fun word -> word.Length > 2)                         // Filter short words
    |> Array.groupBy id                                                    // Group same words
    |> Array.map (fun (word, occurrences) -> {| Word = word; Count = occurrences.Length |})
    |> Array.sortByDescending (fun x -> x.Count)                          // Most frequent first
    |> Array.take 20                                                      // Top 20 words
```

#### **🌐 URL Processing Pipeline**
```fsharp
open System

let extractDomains (urls: string list) =
    urls
    |> List.choose (fun url ->                           // Safe URL parsing
        try Some (Uri(url)) with _ -> None)
    |> List.filter (fun uri -> uri.Scheme = "https")     // HTTPS only
    |> List.map (fun uri -> uri.Host.ToLower())          // Extract domain
    |> List.distinct                                     // Remove duplicates  
    |> List.sort                                         // Alphabetical order
```

---

## Composition & Chaining

### 🔗 **Building Reusable Pipelines**

#### **🎯 Function Composition**
```fsharp
// Build smaller, composable functions
let multiplyBy2 = List.map (fun x -> x * 2)
let filterGreaterThan5 = List.filter (fun x -> x > 5)
let sumAll = List.sum

// Compose them into bigger pipeline
let processNumbersPipeline = multiplyBy2 >> filterGreaterThan5 >> sumAll

// Use multiple times
let result1 = [1; 2; 3; 4; 5] |> processNumbersPipeline  // 18
let result2 = [3; 4; 5; 6; 7] |> processNumbersPipeline  // 30
```

#### **🏭 Reusable Business Logic**
```fsharp
// Domain functions
let validateOrder order = 
    if order.Items |> List.isEmpty then Error "No items" 
    else Ok order

let calculateTotal order =
    let total = order.Items |> List.map (fun item -> item.Price * decimal item.Quantity) |> List.sum
    Ok { order with Total = total }

let applyDiscount order =
    let discount = if order.Customer.IsVip then 0.1m else 0m
    let discountedTotal = order.Total * (1m - discount)
    Ok { order with Total = discountedTotal }

// Compose business pipeline
let processOrderPipeline order =
    order
    |> validateOrder
    |> Result.bind calculateTotal  
    |> Result.bind applyDiscount
```

---

## Error Handling in Pipelines

### 🛡️ **Railway-Oriented Programming**

#### **🎯 Safe Pipeline with Result Types**
```fsharp
// Safe transformation functions that return Result
let safeDivide x y = 
    if y = 0.0 then Error "Division by zero" 
    else Ok (x / y)

let safeSquareRoot x = 
    if x < 0.0 then Error "Negative number"
    else Ok (sqrt x)

let safeLog x = 
    if x <= 0.0 then Error "Non-positive number"
    else Ok (log x)

// Result bind operator for chaining
let (>>=) result f = Result.bind f result

// Chain safe operations
let safeCalculation x y =
    Ok x
    >>= fun x' -> safeDivide x' y
    >>= safeSquareRoot  
    >>= safeLog

// Usage
let result1 = safeCalculation 100.0 2.0  // Ok 3.912...
let result2 = safeCalculation 100.0 0.0  // Error "Division by zero"
```

#### **🔄 List of Results Pattern**
```fsharp
// Process list with potential failures
let processItems items =
    items
    |> List.map (fun item ->
        try Ok (processItem item)
        with ex -> Error ex.Message)
    |> List.partition (function Ok _ -> true | Error _ -> false)
    |> fun (successes, failures) ->
        let values = successes |> List.choose (function Ok v -> Some v | _ -> None)
        let errors = failures |> List.choose (function Error e -> Some e | _ -> None)
        {| Successes = values; Errors = errors |}
```

---

## Async Pipelines

### ⚡ **Combining Pipelines with Async**

#### **🎯 Async Data Pipeline**
```fsharp
let fetchAndProcessData urls = async {
    let! responses = 
        urls
        |> List.map (fun url -> async {
            use client = new HttpClient()
            let! response = client.GetStringAsync(url) |> Async.AwaitTask
            return (url, response)
        })
        |> Async.Parallel
    
    return 
        responses
        |> Array.map (fun (url, content) -> 
            {| Url = url; WordCount = content.Split(' ').Length |})
        |> Array.sortByDescending (fun x -> x.WordCount)
}
```

#### **📊 Async Processing with Error Handling**
```fsharp
let safeAsyncPipeline data = async {
    try
        let! step1Results = 
            data
            |> List.map processStepOneAsync
            |> Async.Parallel
        
        let! step2Results = 
            step1Results
            |> Array.choose (function Ok v -> Some v | Error _ -> None)
            |> Array.map processStepTwoAsync
            |> Async.Parallel
            
        let finalResults = 
            step2Results
            |> Array.map (fun result -> processStepThree result)
            |> Array.toList
            
        return Ok finalResults
    with
    | ex -> return Error ex.Message
}
```

---

## Real-world Applications

### 🛠️ **Complete Business Examples**

#### **📊 CSV Data Processing**
```fsharp
open System.IO

type SalesRecord = {
    Date: DateTime
    Product: string  
    Category: string
    Amount: decimal
    SalesRep: string
}

let processSalesData csvPath =
    File.ReadAllLines(csvPath)
    |> Array.skip 1                                    // Skip header
    |> Array.choose (fun line ->                       // Safe parsing
        try
            let parts = line.Split(',')
            Some {
                Date = DateTime.Parse(parts.[0])
                Product = parts.[1].Trim()
                Category = parts.[2].Trim()  
                Amount = decimal parts.[3]
                SalesRep = parts.[4].Trim()
            }
        with _ -> None)
    |> Array.groupBy (fun record -> record.Category)    // Group by category
    |> Array.map (fun (category, records) ->           // Analyze each category
        let totalSales = records |> Array.map (fun r -> r.Amount) |> Array.sum
        let avgSale = records |> Array.map (fun r -> r.Amount) |> Array.average
        let topRep = 
            records 
            |> Array.groupBy (fun r -> r.SalesRep)
            |> Array.maxBy (fun (rep, sales) -> sales |> Array.map (fun s -> s.Amount) |> Array.sum)
            |> fst
        {| 
            Category = category
            TotalSales = totalSales
            AverageSale = avgSale  
            TopSalesRep = topRep
            RecordCount = records.Length
        |})
    |> Array.sortByDescending (fun x -> x.TotalSales)
```

#### **🌐 API Data Aggregation**
```fsharp
type ApiResponse<'T> = { Data: 'T; Status: string; Message: string }
type User = { Id: int; Name: string; Email: string; Department: string }
type Project = { Id: int; Name: string; Status: string; OwnerId: int }

let aggregateUserProjects() = async {
    // Fetch data from multiple APIs in parallel
    let! usersTask = fetchUsersAsync() |> Async.StartChild
    let! projectsTask = fetchProjectsAsync() |> Async.StartChild
    
    let! users = usersTask
    let! projects = projectsTask
    
    // Process and combine data
    return 
        users
        |> List.map (fun user ->
            let userProjects = 
                projects
                |> List.filter (fun project -> project.OwnerId = user.Id)
                |> List.groupBy (fun project -> project.Status)
                |> List.map (fun (status, projectList) -> 
                    {| Status = status; Count = projectList.Length |})
            
            {| 
                User = user
                ProjectStats = userProjects
                TotalProjects = userProjects |> List.map (fun x -> x.Count) |> List.sum
            |})
        |> List.filter (fun x -> x.TotalProjects > 0)          // Users with projects
        |> List.sortByDescending (fun x -> x.TotalProjects)    // Most active first
}
```

---

## Pipeline Operators Reference

### 📚 **Complete Operators Guide**

| **Operator** | **Signature** | **Description** | **Example** |
|--------------|---------------|-----------------|-------------|
| `\|>` | `'a -> ('a -> 'b) -> 'b` | Forward pipe | `5 \|> (+) 3` = `8` |
| `<\|` | `('a -> 'b) -> 'a -> 'b` | Backward pipe | `(+) 3 <\| 5` = `8` |
| `>>` | `('a -> 'b) -> ('b -> 'c) -> ('a -> 'c)` | Forward compose | `((*) 2 >> (+) 1) 5` = `11` |
| `<<` | `('b -> 'c) -> ('a -> 'b) -> ('a -> 'c)` | Backward compose | `((+) 1 << (*) 2) 5` = `11` |
| `\|>>` | Custom | Map pipe | `[1;2] \|>> (*) 2` = `[2;4]` |
| `>>=` | Monadic | Bind operator | `Some 5 >>= fun x -> Some(x*2)` |

### 🔄 **Collection Functions Reference**

#### **List Module**
```fsharp
// Core transformations
List.map       : ('a -> 'b) -> 'a list -> 'b list
List.filter    : ('a -> bool) -> 'a list -> 'a list  
List.fold      : ('state -> 'a -> 'state) -> 'state -> 'a list -> 'state
List.reduce    : ('a -> 'a -> 'a) -> 'a list -> 'a

// Advanced operations  
List.collect   : ('a -> 'b list) -> 'a list -> 'b list
List.choose    : ('a -> 'b option) -> 'a list -> 'b list
List.groupBy   : ('a -> 'key) -> 'a list -> ('key * 'a list) list
List.sortBy    : ('a -> 'key) -> 'a list -> 'a list
List.distinct  : 'a list -> 'a list
List.partition : ('a -> bool) -> 'a list -> ('a list * 'a list)
```

---

## Common Patterns Cheatsheet

### 📋 **Pipeline Templates**

#### **🎯 Basic Data Processing**
```fsharp
let processData data =
    data
    |> List.filter (/* condition */)      // Select relevant items
    |> List.map (/* transformation */)    // Transform each item  
    |> List.groupBy (/* key function */)  // Group by some criteria
    |> List.map (fun (key, items) ->     // Process each group
        (* aggregate logic *))
    |> List.sortBy (/* sort criteria */)  // Order results
```

#### **🔍 Search and Analysis**
```fsharp
let analyzeAndSearch criteria data =
    data
    |> List.filter (matchesCriteria criteria)     // Filter by criteria
    |> List.collect extractRelevantFields         // Extract fields of interest
    |> List.groupBy categorizeData                // Group for analysis
    |> List.map computeStatistics                 // Calculate metrics
    |> List.sortByDescending (fun x -> x.Score)   // Rank by relevance
    |> List.take 10                              // Top results
```

#### **💰 Business Logic Pipeline**
```fsharp
let processBusinessData input =
    input
    |> validateInput                              // Business rules validation
    |> Result.map enrichWithExternalData          // Add external data
    |> Result.map applyBusinessLogic              // Core business processing  
    |> Result.map calculateDerivedFields          // Compute calculated fields
    |> Result.map formatForOutput                 // Prepare for presentation
```

### 🎯 **Decision Tree: Which Function to Use?**

```
What do you want to do with each element?
├─ Transform it → List.map
├─ Keep or remove it → List.filter  
├─ Transform to multiple items → List.collect
├─ Transform with potential failure → List.choose
├─ Combine all into one value → List.fold/List.reduce
├─ Group by some property → List.groupBy
└─ Sort by some criteria → List.sortBy
```

### 🚀 **Performance Tips**
- ✅ Use `Seq` for large datasets (lazy evaluation)
- ✅ Use `Array.Parallel` for CPU-intensive operations
- ✅ Chain operations to minimize intermediate collections
- ✅ Use `List.collect` instead of `List.map >> List.concat`
- ❌ Don't materialize sequences too early
- ❌ Avoid deeply nested function calls

---

## 📚 Summary

**F# Pipeline Transformations** cung cấp:

- 🎯 **Readable Code**: Data flows naturally từ trái sang phải
- ⚡ **High Performance**: Compiler optimizations và parallel processing
- 🛡️ **Type Safety**: Compile-time guarantees, không runtime surprises
- 🔧 **Composability**: Build complex logic từ simple functions
- 🧪 **Testability**: Mỗi transformation dễ test independently

### **🚀 Key Takeaways**
1. **Start Simple**: `|>` operator là foundation của mọi thứ
2. **Think in Transformations**: Map → Filter → Aggregate pattern
3. **Compose Functions**: Build reusable pipeline components
4. **Handle Errors**: Use Result types cho safe pipelines
5. **Choose Right Collection**: List vs Seq vs Array based on use case

### **💡 Next Steps**
- **Practice** với real datasets trong F# Interactive
- **Build** your own custom operators
- **Combine** với Async workflows cho powerful data processing
- **Explore** advanced libraries like FSharp.Data

**Happy Pipeline Programming!** 🎉