# 🔄 F# Async Workflows - Complete Guide

> **Hướng dẫn toàn diện về F# Async Workflows** - Từ cơ bản đến nâng cao với các ví dụ thực tế

## 🚀 Quick Start
```fsharp
// Tạo async workflow đầu tiên của bạn
let myFirstAsync = async {
    printfn "Bắt đầu..."
    do! Async.Sleep(1000)  // Đợi 1 giây
    printfn "Hoàn thành!"
    return "Success"
}

// Chạy nó
let result = myFirstAsync |> Async.RunSynchronously
```

## 📋 Table of Contents

### 🎯 **Fundamentals**
1. [Introduction & Benefits](#introduction--benefits) - Tại sao chọn F# Async
2. [Basic Syntax & Core Elements](#basic-syntax--core-elements) - Cú pháp cơ bản
3. [Execution Methods](#execution-methods) - Cách chạy async operations

### ⚡ **Execution Patterns**  
4. [Sequential Chaining](#sequential-chaining) - Khi operations phụ thuộc nhau
5. [Parallel Execution](#parallel-execution) - Khi operations độc lập
6. [Mixed Types & Advanced Patterns](#mixed-types--advanced-patterns) - Xử lý các kiểu khác nhau

### 🛡️ **Practical Usage**
7. [Error Handling Patterns](#error-handling-patterns) - Xử lý lỗi an toàn
8. [File Operations & I/O](#file-operations--io) - Thao tác file bất đồng bộ

### 🌐 **Integration & Comparison**
9. [JavaScript/Angular Comparison](#javascriptangular-comparison) - So sánh với JS
10. [Best Practices & Performance](#best-practices--performance) - Thực hành tốt nhất

### 📚 **Quick Reference**
11. [Async Module Reference](#async-module-reference) - Tất cả functions quan trọng
12. [Quick Reference Card](#quick-reference-card) - Patterns thông dụng  
13. [Common Patterns Cheatsheet](#common-patterns-cheatsheet) - Decision tree

## Introduction & Benefits

**F# Async Workflows** là hệ thống lập trình bất đồng bộ mạnh mẽ và type-safe của F#, cung cấp cú pháp clean tương tự JavaScript async/await nhưng với nhiều ưu điểm vượt trội.

### 🎯 **Tại sao chọn F# Async?**

| 🔥 **F# Async** | 🌐 **JavaScript Promise** | 💡 **Ưu điểm** |
|----------------|-------------------------|----------------|
| `async { }` | `async function() {}` | ✅ Type safety |
| `let! result = op()` | `const result = await op()` | ✅ Compile-time check |
| `Async.Parallel` | `Promise.all()` | ✅ Better error handling |
| `Async.Choice` | `Promise.race()` | ✅ Built-in cancellation |

### 🚀 **Core Benefits**
- **🎯 Clean Syntax**: Không có callback hell, code tuyến tính dễ đọc
- **🛡️ Type Safety**: Compiler kiểm tra async types, tránh runtime errors  
- **⚡ High Performance**: Parallel execution dễ dàng với `Async.Parallel`
- **🔧 Rich Tooling**: IntelliSense đầy đủ, debugging mạnh mẽ
- **🌐 .NET Integration**: Seamless interop với .NET Task ecosystem

## Basic Syntax & Core Elements

### 📝 **Essential Syntax**

| **Cú pháp** | **Mục đích** | **JavaScript tương đương** |
|-------------|--------------|---------------------------|
| `async { }` | Tạo async computation | `async function() {}` |
| `let! result = op()` | Await với kết quả | `const result = await op()` |
| `do! op()` | Await không kết quả | `await op()` |
| `return value` | Trả về giá trị | `return value` |

### ⚡ **Example Progression**

#### 🎯 **Level 1: Basic Async**
```fsharp
let simpleAsync = async {
    do! Async.Sleep(1000)      // Đợi 1 giây
    return "Hello Async!"       // Trả về kết quả
}

// Chạy: let result = simpleAsync |> Async.RunSynchronously
```

#### 🎯 **Level 2: With Operations**
```fsharp
let fetchDataAsync url = async {
    printfn "🚀 Downloading: %s" url
    do! Async.Sleep(500)                    // Simulate network delay
    let content = $"Content from {url}"     // Process data
    printfn "✅ Downloaded: %s" content
    return content
}
```

#### 🎯 **Level 3: Chaining Operations**
```fsharp
let chainedExample = async {
    let! data1 = fetchDataAsync "api1.com"     // Await first
    printfn "Got: %s" data1
    
    let! data2 = fetchDataAsync "api2.com"     // Await second
    printfn "Got: %s" data2
    
    return $"Combined: {data1} + {data2}"      // Combine results
}
```

## Execution Methods

### 🎮 **How to Run Async Operations**

| **Method** | **Use Case** | **Blocking?** | **Returns** |
|------------|--------------|---------------|-------------|
| `RunSynchronously` | Console apps, testing | ✅ Yes | Direct result |
| `Start` | Fire & forget tasks | ❌ No | `unit` |
| `StartAsTask` | .NET/C# interop | ❌ No | `Task<T>` |

#### **Examples**
```fsharp
let myAsync = async { return "Hello" }

// 1. Blocking execution (for console apps)
let result = myAsync |> Async.RunSynchronously   // "Hello"

// 2. Fire and forget (for background tasks)  
Async.Start (async { 
    do! Async.Sleep(5000)
    printfn "Background task done!" 
})

// 3. Convert to Task (for C# interop)
let task = myAsync |> Async.StartAsTask  // Task<string>
```

---

## Sequential Chaining

### 🔗 **When Results Flow Between Operations**

Khi kết quả của operation này là input cho operation tiếp theo - **phải chạy tuần tự**, không thể parallel.

#### **Pattern: Authentication Pipeline**
```fsharp
type AuthResult = { UserId: string; Token: string; Role: string }
type UserProfile = { Name: string; Email: string; Department: string }

let authenticationPipeline username password = async {
    // Step 1: Login (must succeed first)
    let! authResult = loginAsync username password
    match authResult with
    | Error msg -> return Error $"Login failed: {msg}"
    | Ok auth ->
        
    // Step 2: Get profile (using token from step 1)
    let! profile = fetchProfileAsync auth.Token
    
    // Step 3: Get permissions (using userId from step 1)  
    let! permissions = fetchPermissionsAsync auth.UserId
    
    return Ok {| Auth = auth; Profile = profile; Permissions = permissions |}
}
```

#### **Pattern: E-commerce Order Flow**
```fsharp
let processOrder cartItems = async {
    let! validItems = validateCartAsync cartItems        // Step 1
    let! totalPrice = calculatePriceAsync validItems     // Step 2: uses validItems
    let! reservation = reserveInventoryAsync validItems  // Step 3: uses validItems  
    let! payment = processPaymentAsync totalPrice        // Step 4: uses totalPrice
    let! order = createOrderAsync payment validItems     // Step 5: uses everything
    
    return order
}
```

---

## Parallel Execution

### 🚀 **When Operations Are Independent**

Khi các operations **không phụ thuộc** lẫn nhau - có thể chạy **song song** để tăng performance.

#### **⚡ Performance Comparison**
```fsharp
// ❌ Sequential: 3 seconds total
let slowSequential = async {
    let! result1 = downloadAsync "site1.com"  // 1s
    let! result2 = downloadAsync "site2.com"  // 1s
    let! result3 = downloadAsync "site3.com"  // 1s
    return [result1; result2; result3]
}

// ✅ Parallel: 1 second total  
let fastParallel = async {
    let! results = [
        downloadAsync "site1.com"
        downloadAsync "site2.com" 
        downloadAsync "site3.com"
    ] |> Async.Parallel
    return results
}
```

#### **📊 When to Use Each**
| **Scenario** | **Approach** | **Reason** |
|--------------|--------------|------------|
| Login → Profile → Permissions | Sequential | Each step needs previous result |
| Load User + Products + Categories | Parallel | All independent |
| Validate → Price + Inventory | Mixed | Validate first, then parallel |

---

## Mixed Types & Advanced Patterns

### 🎭 **Parallel Execution with Different Return Types**

**Problem**: `Async.Parallel` requires same return types. When you have different types:

#### **❌ Won't Compile**
```fsharp
let! results = [
    getUserAsync()      // returns Async<User>
    getOrdersAsync()    // returns Async<Order[]>  
    getSettingsAsync()  // returns Async<Settings>
] |> Async.Parallel  // ❌ Type mismatch!
```

#### **✅ Solution 1: StartChild (Recommended)**
```fsharp
let loadDashboard = async {
    // Start all operations in parallel
    let! userAsync = Async.StartChild(getUserAsync())
    let! ordersAsync = Async.StartChild(getOrdersAsync())  
    let! settingsAsync = Async.StartChild(getSettingsAsync())
    
    // Await each with correct types
    let! user = userAsync        // User
    let! orders = ordersAsync    // Order[]
    let! settings = settingsAsync // Settings
    
    return {| User = user; Orders = orders; Settings = settings |}
}
```

#### **✅ Solution 2: Union Types**
```fsharp
type DashboardData = 
    | UserData of User
    | OrderData of Order[]
    | SettingsData of Settings

let loadWithUnion = async {
    let! results = [
        async { let! u = getUserAsync() in return UserData u }
        async { let! o = getOrdersAsync() in return OrderData o }
        async { let! s = getSettingsAsync() in return SettingsData s }
    ] |> Async.Parallel
    
    // Pattern match to extract
    let user = results |> Array.pick (function UserData u -> Some u | _ -> None)
    // etc...
}
```

### ⚡ **Advanced Async Patterns**

#### **🎯 Timeout Pattern**
```fsharp
let withTimeout timeoutMs operation = async {
    try
        let! child = Async.StartChild(operation, timeoutMs)
        return! child
    with
    | :? System.TimeoutException -> return failwith "Operation timed out"
}

// Usage: let! result = withTimeout 5000 (longRunningAsync())
```

#### **🔄 Retry Pattern**  
```fsharp
let retryAsync maxAttempts operation = 
    let rec retry attempt = async {
        try
            return! operation()
        with
        | ex when attempt < maxAttempts ->
            do! Async.Sleep(1000 * attempt)  // Exponential backoff
            return! retry (attempt + 1)
        | ex -> return failwith $"Failed after {maxAttempts} attempts"
    }
    retry 1
```

---

## Error Handling Patterns

### 🛡️ **Safe Async with Result Types**

#### **Basic Pattern**
```fsharp
let safeAsync operation = async {
    try
        let! result = operation
        return Ok result
    with
    | ex -> return Error ex.Message
}
```

#### **Practical Example**
```fsharp
let safeDivisionAsync x y = async {
    try
        do! Async.Sleep(100)  // Simulate work
        if y = 0 then
            return Error "Cannot divide by zero"
        else
            return Ok (x / y)
    with
    | ex -> return Error $"Calculation error: {ex.Message}"
}

// Usage with pattern matching
let handleResult = async {
    let! result = safeDivisionAsync 10 2
    match result with
    | Ok value -> printfn "Result: %d" value
    | Error msg -> printfn "Error: %s" msg
}
```

---

## File Operations & I/O

### 📁 **Async File Operations**

```fsharp
open System.IO

let writeFileAsync path content = async {
    try
        do! File.WriteAllTextAsync(path, content) |> Async.AwaitTask
        return Ok ()
    with
    | ex -> return Error ex.Message
}

let readFileAsync path = async {
    try
        let! content = File.ReadAllTextAsync(path) |> Async.AwaitTask
        return Ok content
    with
    | ex -> return Error ex.Message
}

// Usage
let fileExample = async {
    let! writeResult = writeFileAsync "test.txt" "Hello F#!"
    match writeResult with
    | Error msg -> printfn "Write failed: %s" msg
    | Ok _ ->
        let! readResult = readFileAsync "test.txt" 
        match readResult with
        | Ok content -> printfn "Read: %s" content
        | Error msg -> printfn "Read failed: %s" msg
}
```

### 💡 Khái niệm quan trọng (Key Concept): Async.AwaitTask

`Async.AwaitTask` chuyển đổi .NET `Task` thành F# `Async`:

```fsharp
// .NET Task → F# Async
do! File.WriteAllTextAsync(path, content) |> Async.AwaitTask
```

## 🔄 Async Methods Comparison - So sánh các phương thức Async

### 📊 **Bảng so sánh tổng quan**

| **Method** | **Input** | **Output** | **Blocking** | **Use Case** | **Performance** |
|------------|-----------|------------|--------------|--------------|-----------------|
| `RunSynchronously` | `Async<'T>` | `'T` | ✅ Yes | Console apps, testing | ⚠️ Blocks thread |
| `Start` | `Async<unit>` | `unit` | ❌ No | Fire & forget | ✅ Non-blocking |
| `StartAsTask` | `Async<'T>` | `Task<'T>` | ❌ No | .NET interop | ✅ Non-blocking |
| `StartChild` | `Async<'T>` | `Async<Async<'T>>` | ❌ No | Parallel control | ✅ Advanced control |
| `Parallel` | `Async<'T>[]` | `Async<'T[]>` | ❌ No | Same type parallel | ✅ High performance |
| `Choice` | `Async<'T>[]` | `Async<'T option>` | ❌ No | Race conditions | ✅ First wins |
| `AwaitTask` | `Task<'T>` | `Async<'T>` | ❌ No | Task → Async | ✅ Interop |

### 🎯 **Chi tiết từng phương thức**

#### **1. Async.RunSynchronously - Chạy đồng bộ**
```fsharp
// 🎯 Mục đích: Chuyển async thành synchronous call
let result = someAsync |> Async.RunSynchronously

// ✅ Khi nào dùng:
// - Console applications
// - Unit testing  
// - Script execution
// - Khi bạn PHẢI có kết quả ngay

// ❌ Tránh dùng khi:
// - Web applications (ASP.NET)
// - WPF/WinForms UI threads
// - Bên trong async context khác

// 📝 Ví dụ thực tế:
[<EntryPoint>]
let main argv =
    let downloadContent = async {
        use client = new HttpClient()
        let! response = client.GetStringAsync("https://api.com") |> Async.AwaitTask
        return response
    }
    
    let content = downloadContent |> Async.RunSynchronously
    printfn "Downloaded: %s" content
    0  // Return exit code
```

#### **2. Async.Start - Fire and Forget**
```fsharp
// 🎯 Mục đích: Chạy background task không cần đợi kết quả
Async.Start someAsync

// ✅ Khi nào dùng:
// - Logging operations
// - Cache warming
// - Background cleanup
// - Analytics tracking

// ❌ Tránh dùng khi:
// - Cần kết quả trả về
// - Cần error handling
// - Cần biết khi nào completed

// 📝 Ví dụ thực tế:
let logAnalytics userId action = async {
    do! Async.Sleep(100)  // Simulate API call
    // Log to analytics service
}

let handleUserClick userId = 
    // Main business logic (synchronous)
    updateUI()
    
    // Background analytics (fire & forget)
    Async.Start (logAnalytics userId "click")
    
    // Continue immediately, don't wait for logging
```

#### **3. Async.StartAsTask - .NET Interop**
```fsharp
// 🎯 Mục đích: Chuyển F# Async thành .NET Task
let task = someAsync |> Async.StartAsTask

// ✅ Khi nào dùng:
// - Gọi từ C# code
// - ASP.NET Core controllers
// - SignalR hubs
// - Entity Framework async methods

// 📝 Ví dụ thực tế - ASP.NET Core:
[<ApiController>]
[<Route("api/[controller]")>]
type ProductsController() =
    inherit ControllerBase()
    
    // F# async function
    let getProductsAsync() = async {
        let! products = ProductRepository.loadProductsAsync()
        return products
    }
    
    // Expose as Task for ASP.NET
    [<HttpGet>]
    member this.GetProducts(): Task<IActionResult> =
        async {
            let! products = getProductsAsync()
            return this.Ok(products) :> IActionResult
        } |> Async.StartAsTask

// ✅ Interop với C# libraries:
let callCSharpLibrary() = async {
    let someAsyncOp = async { return "F# result" }
    let task = someAsyncOp |> Async.StartAsTask
    
    // C# library có thể await task này
    CSharpLibrary.ProcessAsync(task) |> ignore
}
```

#### **4. Async.StartChild - Advanced Parallel Control**
```fsharp
// 🎯 Mục đích: Parallel execution với fine-grained control
let! childAsync = Async.StartChild(operation, ?millisecondsTimeout)
let! result = childAsync

// ✅ Khi nào dùng:
// - Different return types parallel
// - Timeout control
// - Complex orchestration
// - Conditional waiting

// 📝 Ví dụ 1: Different Types Parallel
let loadUserDashboard userId = async {
    // Start all operations in parallel
    let! userAsync = Async.StartChild(fetchUser userId)
    let! ordersAsync = Async.StartChild(fetchOrders userId)
    let! prefsAsync = Async.StartChild(fetchPreferences userId)
    
    // Wait for each with proper types
    let! user = userAsync        // User type
    let! orders = ordersAsync    // Order[] type  
    let! prefs = prefsAsync      // Preferences type
    
    return {| User = user; Orders = orders; Prefs = prefs |}
}

// 📝 Ví dụ 2: Timeout Control
let withTimeout timeoutMs operation = async {
    try
        let! childAsync = Async.StartChild(operation, timeoutMs)
        let! result = childAsync
        return Ok result
    with
    | :? TimeoutException -> return Error "Operation timed out"
}

// Usage: let! result = withTimeout 5000 longRunningOperation
```

#### **5. Async.Parallel - High Performance Same Types**
```fsharp
// 🎯 Mục đích: Parallel execution của cùng return type
let! results = operations |> Async.Parallel

// ✅ Khi nào dùng:
// - Multiple API calls cùng type
// - Batch processing
// - Maximum performance cần thiết
// - Simple parallelization

// ❌ Hạn chế:
// - Phải cùng return type
// - Ít control over individual operations

// 📝 Ví dụ thực tế:
let downloadMultipleFiles urls = async {
    let downloadFile url = async {
        use client = new HttpClient()
        let! content = client.GetStringAsync(url) |> Async.AwaitTask
        return (url, content.Length)
    }
    
    // Parallel download tất cả URLs
    let! results = 
        urls 
        |> List.map downloadFile
        |> Async.Parallel
    
    // Tất cả có cùng type: (string * int)[]
    return results
}

// Performance comparison:
let urls = ["url1.com"; "url2.com"; "url3.com"; "url4.com"]

// ❌ Sequential: 4 seconds
let sequential = async {
    let! r1 = downloadFile urls.[0]  // 1s
    let! r2 = downloadFile urls.[1]  // 1s  
    let! r3 = downloadFile urls.[2]  // 1s
    let! r4 = downloadFile urls.[3]  // 1s
    return [r1; r2; r3; r4]
}

// ✅ Parallel: 1 second  
let parallel = async {
    let! results = urls |> List.map downloadFile |> Async.Parallel
    return results
}
```

#### **6. Async.Choice - Race Conditions**
```fsharp
// 🎯 Mục đích: Trả về kết quả của operation hoàn thành đầu tiên
let! winner = operations |> Async.Choice

// ✅ Khi nào dùng:
// - Multiple data sources (fallback)
// - Performance racing
// - Timeout implementations
// - Load balancing

// 📝 Ví dụ 1: Multiple API Endpoints
let fetchFromMultipleAPIs query = async {
    let api1 = async {
        do! Async.Sleep(300)  // Slow API
        return $"API1: {query}"
    }
    
    let api2 = async {
        do! Async.Sleep(100)  // Fast API  
        return $"API2: {query}"
    }
    
    let api3 = async {
        do! Async.Sleep(500)  // Very slow API
        return $"API3: {query}"
    }
    
    // Trả về kết quả của API nhanh nhất
    let! result = Async.Choice [api1; api2; api3]
    match result with
    | Some data -> return data      // API2 wins (100ms)
    | None -> return "All APIs failed"
}

// 📝 Ví dụ 2: Timeout with Choice
let operationWithTimeout timeoutMs operation = async {
    let timeout = async {
        do! Async.Sleep(timeoutMs)
        return None
    }
    
    let work = async {
        let! result = operation
        return Some result
    }
    
    let! result = Async.Choice [work; timeout]
    match result with
    | Some (Some value) -> return Ok value
    | _ -> return Error "Timeout or failed"
}
```

#### **7. Async.AwaitTask - Task Interoperability**
```fsharp
// 🎯 Mục đích: Chuyển .NET Task thành F# Async
let! result = someTask |> Async.AwaitTask

// ✅ Khi nào dùng:
// - Working với .NET libraries
// - Entity Framework
// - HttpClient
// - File I/O operations

// 📝 Ví dụ thực tế:
let entityFrameworkExample() = async {
    use context = new MyDbContext()
    
    // EF Core trả về Task<T>
    let! users = context.Users.ToListAsync() |> Async.AwaitTask
    let! orders = context.Orders.Where(fun o -> o.UserId = 1)
                                .ToListAsync() |> Async.AwaitTask
    
    return (users, orders)
}

let httpClientExample() = async {
    use client = new HttpClient()
    
    // HttpClient methods return Task<T>
    let! response = client.GetAsync("https://api.com") |> Async.AwaitTask
    let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
    
    return content
}
```

### 🚀 **Performance Comparison - So sánh hiệu năng**

```fsharp
// Scenario: Download 5 URLs, mỗi URL mất 1 giây

// ❌ Sequential: 5 giây
let sequentialDownload urls = async {
    let mutable results = []
    for url in urls do
        let! content = downloadAsync url  // 1s each
        results <- content :: results
    return List.rev results
}

// ✅ Parallel (Same Types): 1 giây
let parallelDownload urls = async {
    let! results = 
        urls 
        |> List.map downloadAsync
        |> Async.Parallel
    return results
}

// ✅ StartChild (Different Types): 1 giây
let startChildDownload urls = async {
    // Start all in parallel
    let! children = 
        urls 
        |> List.map (downloadAsync >> Async.StartChild)
        |> Async.sequential  // Start all children
    
    // Wait for all results
    let! results = children |> List.map id |> Async.Parallel
    return results
}

// ✅ Choice (First Wins): ~1 giây hoặc ít hơn
let raceDownload urls = async {
    let! winner = 
        urls 
        |> List.map downloadAsync
        |> Async.Choice
    
    match winner with
    | Some result -> return [result]  // Only the fastest
    | None -> return []
}
```

### 🎯 **Decision Matrix - Khi nào dùng gì?**

| **Scenario** | **Recommended Method** | **Alternative** | **Avoid** |
|--------------|----------------------|----------------|-----------|
| **Console app main** | `RunSynchronously` | - | `Start` |
| **Web API endpoint** | `StartAsTask` | - | `RunSynchronously` |
| **Background logging** | `Start` | - | `RunSynchronously` |
| **Same type parallel** | `Parallel` | `StartChild` | Sequential |
| **Different type parallel** | `StartChild` | Union types | `Parallel` |
| **Race condition** | `Choice` | `StartChild` + timeout | `Parallel` |
| **EF Core / HttpClient** | `AwaitTask` | - | Direct Task usage |
| **Timeout needed** | `StartChild` with timeout | `Choice` | `Parallel` |

### 💡 **Best Practices cho từng method**

#### **RunSynchronously Best Practices**
```fsharp
// ✅ Good: Console application
[<EntryPoint>]
let main argv =
    let work = async { (* async work *) }
    let result = work |> Async.RunSynchronously
    0

// ❌ Bad: Inside async context
let badPattern = async {
    let result = otherAsync |> Async.RunSynchronously  // Deadlock risk!
    return result
}

// ✅ Good: Inside async context  
let goodPattern = async {
    let! result = otherAsync  // Proper async chaining
    return result
}
```

#### **StartAsTask Best Practices**
```fsharp
// ✅ Good: ASP.NET Core
[<HttpGet>]
member this.GetData(): Task<IActionResult> =
    async {
        let! data = loadDataAsync()
        return this.Ok(data) :> IActionResult
    } |> Async.StartAsTask

// ✅ Good: C# interop
type FSharpLibrary() =
    member _.ProcessAsync(input: string): Task<string> =
        async {
            do! Async.Sleep(100)
            return input.ToUpper()
        } |> Async.StartAsTask
```

#### **Parallel vs StartChild Guidelines**
```fsharp
// ✅ Use Parallel when: Same types, simple parallelization
let loadSameTypeData ids = async {
    let! results = ids |> List.map fetchUserAsync |> Async.Parallel
    return results  // User[]
}

// ✅ Use StartChild when: Different types, timeout control
let loadMixedData userId = async {
    let! userAsync = Async.StartChild(fetchUserAsync userId)
    let! ordersAsync = Async.StartChild(fetchOrdersAsync userId, 5000) // 5s timeout
    let! settingsAsync = Async.StartChild(fetchSettingsAsync userId)
    
    let! user = userAsync      // User
    let! orders = ordersAsync  // Order[]  
    let! settings = settingsAsync // Settings
    
    return (user, orders, settings)  // Mixed types
}
```

### 🔍 Chi tiết về Async.AwaitTask vs Async.Parallel

#### 🎯 **Async.AwaitTask** - Chuyển đổi Task sang Async
```fsharp
// Mục đích: Interop với .NET Task-based APIs
let downloadFromApi url = async {
    use client = new HttpClient()
    
    // Chuyển Task<HttpResponseMessage> → Async<HttpResponseMessage>
    let! response = client.GetAsync(url) |> Async.AwaitTask
    
    // Chuyển Task<string> → Async<string>  
    let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
    
    return content
}
```

#### 🚀 **Async.Parallel** - Thực thi đồng thời nhiều Async
```fsharp
// Mục đích: Chạy nhiều async operations cùng lúc
let downloadMultipleUrls urls = async {
    let! results = 
        urls
        |> List.map downloadFromApi  // Tạo list các async operations
        |> Async.Parallel           // Chạy tất cả cùng lúc!
        
    return results
}
```

### 📚 Các Function thông dụng khác trong Async Module

#### ⚡ **1. Async.Sleep** - Delay không blocking
```fsharp
let delayExample = async {
    printfn "Bắt đầu..."
    do! Async.Sleep(2000)  // Đợi 2 giây (không block thread)
    printfn "Kết thúc sau 2 giây"
}
```

#### 🎮 **2. Async.RunSynchronously** - Chạy đồng bộ
```fsharp
// Chuyển async thành synchronous (blocking)
let result = downloadFromApi "https://api.com" |> Async.RunSynchronously
printfn "Kết quả: %s" result
```

#### 🔥 **3. Async.Start** - Fire and forget
```fsharp
// Chạy background task không cần đợi kết quả
let backgroundTask = async {
    do! Async.Sleep(5000)
    printfn "Background task hoàn thành"
}

Async.Start backgroundTask  // Không block, chạy ngầm
printfn "Tiếp tục main thread"
```

#### 🎯 **4. Async.StartAsTask** - Interop với C#
```fsharp
// Chuyển F# Async thành .NET Task (để C# có thể await)
let asyncOperation = async { return "Hello from F#" }
let task = asyncOperation |> Async.StartAsTask

// C# code có thể: await task
```

#### 🔄 **5. Async.Choice** - Race condition
```fsharp
// Chạy nhiều operations, trả về cái nào hoàn thành trước
let raceExample = async {
    let fast = async {
        do! Async.Sleep(100)
        return "Fast result"
    }
    
    let slow = async {
        do! Async.Sleep(1000) 
        return "Slow result"
    }
    
    let! winner = Async.Choice [fast; slow]
    match winner with
    | Some result -> return result
    | None -> return "Không có kết quả"
}
```

#### ⏱️ **6. Async.StartChild** - Timeout và Background
```fsharp
// Chạy operation với timeout
let withTimeoutExample = async {
    try
        let operation = async {
            do! Async.Sleep(3000)
            return "Hoàn thành"
        }
        
        // Timeout sau 2 giây
        let! childAsync = Async.StartChild(operation, 2000)
        let! result = childAsync
        return Ok result
    with
    | :? TimeoutException -> 
        return Error "Timeout sau 2 giây"
}
```

#### 🎪 **7. Async.Catch** - Exception handling
```fsharp
// Bắt exception và wrap trong Result
let safeAsyncOperation operation = async {
    let! result = Async.Catch operation
    match result with
    | Choice1Of2 success -> return Ok success
    | Choice2Of2 ex -> return Error ex.Message
}
```

### 🔄 **Bảng so sánh các Function chính**

| Function | Mục đích | Input | Output | Use Case |
|----------|----------|-------|---------|----------|
| `AwaitTask` | Task → Async | `Task<'T>` | `Async<'T>` | .NET interop |
| `Parallel` | Chạy đồng thời | `Async<'T>[]` | `Async<'T[]>` | Performance |
| `Sleep` | Delay | `int` (ms) | `Async<unit>` | Timing |
| `RunSynchronously` | Async → sync | `Async<'T>` | `'T` | Console apps |
| `Start` | Fire & forget | `Async<unit>` | `unit` | Background |
| `StartAsTask` | Async → Task | `Async<'T>` | `Task<'T>` | C# interop |
| `Choice` | Race condition | `Async<'T>[]` | `Async<'T option>` | First wins |
| `StartChild` | Timeout/Background | `Async<'T> * timeout` | `Async<Async<'T>>` | Advanced control |

### 🎭 **Xử lý Async.Parallel với các kiểu khác nhau**

Vấn đề: `Async.Parallel` yêu cầu tất cả async operations có cùng kiểu trả về. Khi có các kiểu khác nhau:

#### ❌ **Không hoạt động - Different Return Types**
```fsharp
// Lỗi compile - các kiểu trả về khác nhau
let! results = [
    downloadReturnTypeAAsync "site1.com"  // returns Async<TypeA>
    downloadReturnTypeBAsync "site2.com"  // returns Async<TypeB> 
    downloadReturnTypeCAsync "site3.com"  // returns Async<TypeC>
] |> Async.Parallel  // ❌ COMPILE ERROR!
```

#### ✅ **Giải pháp 1: Union Types (Discriminated Unions)**
```fsharp
// 1. Định nghĩa Union Type để wrap các kiểu khác nhau
type AsyncResult = 
    | ResultA of TypeA
    | ResultB of TypeB  
    | ResultC of TypeC

// 2. Wrap mỗi async operation
let wrappedOperations = [
    async { 
        let! result = downloadReturnTypeAAsync "site1.com"
        return ResultA result 
    }
    async { 
        let! result = downloadReturnTypeBAsync "site2.com"
        return ResultB result 
    }
    async { 
        let! result = downloadReturnTypeCAsync "site3.com"
        return ResultC result 
    }
]

// 3. Chạy parallel
let! results = wrappedOperations |> Async.Parallel

// 4. Pattern match để xử lý từng loại
results |> Array.iter (fun result ->
    match result with
    | ResultA dataA -> printfn "Got TypeA: %A" dataA
    | ResultB dataB -> printfn "Got TypeB: %A" dataB  
    | ResultC dataC -> printfn "Got TypeC: %A" dataC
)
```

#### ✅ **Giải pháp 2: Async.StartChild (Recommended)**
```fsharp
// Chạy song song với StartChild - không cần cùng kiểu
let parallelDifferentTypes = async {
    // 1. Start các operations song song
    let! asyncA = Async.StartChild(downloadReturnTypeAAsync "site1.com")
    let! asyncB = Async.StartChild(downloadReturnTypeBAsync "site2.com") 
    let! asyncC = Async.StartChild(downloadReturnTypeCAsync "site3.com")
    
    // 2. Await từng kết quả (vẫn song song)
    let! resultA = asyncA  // TypeA
    let! resultB = asyncB  // TypeB
    let! resultC = asyncC  // TypeC
    
    // 3. Xử lý với đúng kiểu của từng result
    printfn "A: %A, B: %A, C: %A" resultA resultB resultC
    
    return (resultA, resultB, resultC)  // Tuple với các kiểu khác nhau
}
```

#### ✅ **Giải pháp 3: Tasks với Async.AwaitTask**
```fsharp
// Sử dụng Task.WhenAll cho multiple types
let parallelWithTasks = async {
    // 1. Convert sang Tasks
    let taskA = downloadReturnTypeAAsync "site1.com" |> Async.StartAsTask
    let taskB = downloadReturnTypeBAsync "site2.com" |> Async.StartAsTask
    let taskC = downloadReturnTypeCAsync "site3.com" |> Async.StartAsTask
    
    // 2. Wait for all (song song)
    do! Task.WhenAll([| taskA :> Task; taskB :> Task; taskC :> Task |]) 
        |> Async.AwaitTask
    
    // 3. Get results với đúng kiểu
    let resultA = taskA.Result  // TypeA
    let resultB = taskB.Result  // TypeB  
    let resultC = taskC.Result  // TypeC
    
    return (resultA, resultB, resultC)
}
```

#### ✅ **Giải pháp 4: Generic với Object**
```fsharp
// Khi cần flexibility cao (nhưng mất type safety)
let parallelAsObjects = async {
    let! results = [
        async { 
            let! result = downloadReturnTypeAAsync "site1.com"
            return box result  // Convert thành obj
        }
        async { 
            let! result = downloadReturnTypeBAsync "site2.com"  
            return box result  // Convert thành obj
        }
        async { 
            let! result = downloadReturnTypeCAsync "site3.com"
            return box result  // Convert thành obj
        }
    ] |> Async.Parallel
    
    // Unbox với pattern matching hoặc type checking
    let resultA = results.[0] :?> TypeA
    let resultB = results.[1] :?> TypeB
    let resultC = results.[2] :?> TypeC
    
    return (resultA, resultB, resultC)
}
```

#### 🎯 **So sánh các giải pháp**

| Phương pháp | Type Safety | Performance | Complexity | Recommendation |
|-------------|-------------|-------------|------------|----------------|
| **Union Types** | ✅ Cao | ✅ Tốt | ⚠️ Trung bình | Khi có ít kiểu (2-5) |
| **StartChild** | ✅ Cao | ✅ Tốt | ✅ Đơn giản | **🌟 Recommended** |
| **Tasks** | ✅ Cao | ✅ Tốt | ⚠️ Phức tạp | Khi cần .NET interop |
| **Object Boxing** | ❌ Thấp | ⚠️ Chậm hơn | ✅ Đơn giản | Tránh nếu có thể |

### 💡 **Ví dụ thực tế hoàn chỉnh**

```fsharp
// Định nghĩa các types khác nhau
type UserProfile = { Name: string; Email: string }
type OrderHistory = { Orders: string list; Total: decimal }
type Preferences = { Theme: string; Language: string }

// Các async functions với kiểu khác nhau
let fetchUserProfile userId = async {
    do! Async.Sleep(100)
    return { Name = "John"; Email = "john@email.com" }
}

let fetchOrderHistory userId = async {
    do! Async.Sleep(200)  
    return { Orders = ["Order1"; "Order2"]; Total = 150.0m }
}

let fetchUserPreferences userId = async {
    do! Async.Sleep(150)
    return { Theme = "Dark"; Language = "Vietnamese" }
}

// ✅ Giải pháp tốt nhất: StartChild
let loadUserDashboard userId = async {
    printfn "🚀 Loading dashboard for user %s..." userId
    
    // Start tất cả operations song song
    let! profileAsync = Async.StartChild(fetchUserProfile userId)
    let! historyAsync = Async.StartChild(fetchOrderHistory userId)  
    let! prefsAsync = Async.StartChild(fetchUserPreferences userId)
    
    // Await tất cả results (vẫn song song)
    let! profile = profileAsync      // UserProfile
    let! history = historyAsync      // OrderHistory
    let! prefs = prefsAsync          // Preferences
    
    printfn "✅ Dashboard loaded!"
    printfn "User: %s, Orders: %d, Theme: %s" 
        profile.Name history.Orders.Length prefs.Theme
    
    return {| Profile = profile; History = history; Preferences = prefs |}
}

// Sử dụng
let! dashboard = loadUserDashboard "user123"
printfn "Total spent: %M" dashboard.History.Total
```

### 🎭 **Pattern: Conditional Parallel Execution**

```fsharp
// Khi cần chạy các operations khác nhau based on conditions
let smartParallelExecution condition = async {
    match condition with
    | "full" ->
        // Load everything song song
        let! profileAsync = Async.StartChild(fetchUserProfile "user1")
        let! historyAsync = Async.StartChild(fetchOrderHistory "user1")
        let! prefsAsync = Async.StartChild(fetchUserPreferences "user1")
        
        let! profile = profileAsync
        let! history = historyAsync  
        let! prefs = prefsAsync
        
        return Some (profile, Some history, Some prefs)
        
    | "basic" ->
        // Chỉ load profile
        let! profile = fetchUserProfile "user1"
        return Some (profile, None, None)
        
    | _ ->
        return None
}
```

### 🔗 **Sequential Chaining - Kết quả này làm input của kết quả kia**

Khác với parallel execution, đôi khi bạn cần chạy **tuần tự** vì kết quả của operation này là input cho operation tiếp theo.

#### 📝 **Ví dụ cơ bản: Authentication Flow**

```fsharp
// Các functions phụ thuộc lẫn nhau
let loginAsync username password = async {
    do! Async.Sleep(100)
    if username = "admin" && password = "secret" then
        return Ok { UserId = "123"; Token = "abc-token"; Role = "admin" }
    else
        return Error "Invalid credentials"
}

let fetchUserProfileAsync token = async {
    do! Async.Sleep(200)
    return { 
        Name = "John Admin"
        Email = "admin@company.com" 
        Department = "IT"
    }
}

let fetchUserPermissionsAsync userId role = async {
    do! Async.Sleep(150)
    match role with
    | "admin" -> return ["read"; "write"; "delete"; "manage"]
    | "user" -> return ["read"; "write"]
    | _ -> return ["read"]
}

// ✅ Sequential Chaining - Mỗi bước phụ thuộc vào bước trước
let authenticateAndLoadProfile username password = async {
    printfn "🔐 Đăng nhập..."
    
    // Bước 1: Login (phải thành công mới tiếp tục)
    let! loginResult = loginAsync username password
    
    match loginResult with
    | Error msg -> 
        printfn "❌ Đăng nhập thất bại: %s" msg
        return Error msg
        
    | Ok authInfo ->
        printfn "✅ Đăng nhập thành công với token: %s" authInfo.Token
        
        // Bước 2: Fetch profile (dùng token từ bước 1)
        printfn "👤 Tải thông tin profile..."
        let! profile = fetchUserProfileAsync authInfo.Token
        
        // Bước 3: Fetch permissions (dùng userId và role từ bước 1)
        printfn "🔑 Tải quyền hạn..."
        let! permissions = fetchUserPermissionsAsync authInfo.UserId authInfo.Role
        
        printfn "🎉 Hoàn thành! User: %s, Permissions: %A" profile.Name permissions
        
        return Ok {| 
            Auth = authInfo
            Profile = profile  
            Permissions = permissions 
        |}
}

// Sử dụng
let! result = authenticateAndLoadProfile "admin" "secret"
```

#### 🛒 **Ví dụ thực tế: E-commerce Order Flow**

```fsharp
// Định nghĩa types
type Product = { Id: string; Name: string; Price: decimal; Stock: int }
type CartItem = { ProductId: string; Quantity: int }
type Order = { Id: string; Items: CartItem list; Total: decimal; Status: string }

// Các async functions phụ thuộc tuần tự
let validateCartAsync (items: CartItem list) = async {
    printfn "🛒 Validating cart với %d items..." items.Length
    do! Async.Sleep(100)
    
    // Giả sử validate thành công
    let validItems = items |> List.filter (fun item -> item.Quantity > 0)
    return Ok validItems
}

let calculatePriceAsync (items: CartItem list) = async {
    printfn "💰 Tính tổng tiền cho %d items..." items.Length
    do! Async.Sleep(150)
    
    // Giả sử tính giá
    let total = items |> List.sumBy (fun item -> decimal item.Quantity * 10.0m)
    return total
}

let reserveInventoryAsync (items: CartItem list) = async {
    printfn "📦 Reserve inventory cho %d items..." items.Length  
    do! Async.Sleep(200)
    
    // Giả sử reserve thành công
    return Ok "inventory-reservation-123"
}

let processPaymentAsync (amount: decimal) (reservationId: string) = async {
    printfn "💳 Xử lý thanh toán %M với reservation %s..." amount reservationId
    do! Async.Sleep(300)
    
    if amount > 0m then
        return Ok { PaymentId = "pay-456"; Amount = amount; Status = "Success" }
    else
        return Error "Invalid amount"
}

let createOrderAsync paymentInfo items total = async {
    printfn "📝 Tạo order với payment %s..." paymentInfo.PaymentId
    do! Async.Sleep(100)
    
    return {
        Id = "order-789"
        Items = items
        Total = total
        Status = "Confirmed"
    }
}

// 🔗 Sequential Chain - Mỗi bước cần kết quả của bước trước
let processOrderAsync (cartItems: CartItem list) = async {
    try
        printfn "🚀 Bắt đầu xử lý order..."
        
        // Bước 1: Validate cart
        let! validationResult = validateCartAsync cartItems
        match validationResult with
        | Error msg -> return Error $"Cart validation failed: {msg}"
        | Ok validItems ->
            
        // Bước 2: Calculate price (dùng validItems từ bước 1)
        let! totalPrice = calculatePriceAsync validItems
        
        // Bước 3: Reserve inventory (dùng validItems từ bước 1)  
        let! reservationResult = reserveInventoryAsync validItems
        match reservationResult with
        | Error msg -> return Error $"Inventory reservation failed: {msg}"
        | Ok reservationId ->
            
        // Bước 4: Process payment (dùng totalPrice từ bước 2 và reservationId từ bước 3)
        let! paymentResult = processPaymentAsync totalPrice reservationId
        match paymentResult with
        | Error msg -> return Error $"Payment failed: {msg}"  
        | Ok paymentInfo ->
            
        // Bước 5: Create order (dùng tất cả thông tin từ các bước trước)
        let! order = createOrderAsync paymentInfo validItems totalPrice
        
        printfn "🎉 Order thành công! Order ID: %s, Total: %M" order.Id order.Total
        return Ok order
        
    with
    | ex -> 
        printfn "💥 Lỗi trong quá trình xử lý: %s" ex.Message
        return Error ex.Message
}
```

#### ⚡ **So sánh Parallel vs Sequential**

```fsharp
// ❌ PARALLEL - Không phù hợp vì có dependencies
let incorrectParallelOrder cartItems = async {
    // Tất cả chạy cùng lúc - SẼ LỖI!
    let! results = [
        validateCartAsync cartItems |> Async.map box
        calculatePriceAsync cartItems |> Async.map box  // Cần validItems!
        reserveInventoryAsync cartItems |> Async.map box // Cần validItems!  
        // processPaymentAsync ??? // Cần totalPrice và reservationId!
    ] |> Async.Parallel
    
    // Không thể xử lý vì thiếu dependencies
    return Error "Cannot process in parallel due to dependencies"
}

// ✅ SEQUENTIAL - Đúng cách vì có dependencies
let correctSequentialOrder cartItems = async {
    // Mỗi bước dùng kết quả của bước trước
    let! step1 = validateCartAsync cartItems
    match step1 with | Error e -> return Error e | Ok validItems ->
    
    let! step2 = calculatePriceAsync validItems  // Dùng validItems
    let! step3 = reserveInventoryAsync validItems // Dùng validItems
    match step3 with | Error e -> return Error e | Ok reservationId ->
    
    let! step4 = processPaymentAsync step2 reservationId // Dùng cả 2 kết quả trước
    match step4 with | Error e -> return Error e | Ok paymentInfo ->
    
    let! step5 = createOrderAsync paymentInfo validItems step2 // Dùng tất cả
    return Ok step5
}
```

#### 🎯 **Khi nào dùng Sequential vs Parallel?**

| Trường hợp | Approach | Lý do | Ví dụ |
|------------|----------|-------|--------|
| **Dependencies** | Sequential | Bước sau cần kết quả bước trước | Login → Profile → Permissions |
| **Independent** | Parallel | Không phụ thuộc lẫn nhau | Load User + Products + Categories |
| **Mixed** | Hybrid | Một số song song, một số tuần tự | Auth (sequential) + Load data (parallel) |

#### 🔄 **Pattern: Hybrid (Sequential + Parallel)**

```fsharp
let hybridOrderProcessing cartItems userId = async {
    // Phase 1: Sequential (có dependencies)
    let! validItems = validateCartAsync cartItems
    match validItems with | Error e -> return Error e | Ok items ->
    
    // Phase 2: Parallel (independent operations sử dụng kết quả từ Phase 1)
    let! userAsync = Async.StartChild(fetchUserProfile userId)
    let! priceAsync = Async.StartChild(calculatePriceAsync items)
    let! inventoryAsync = Async.StartChild(reserveInventoryAsync items)
    
    let! user = userAsync
    let! price = priceAsync  
    let! inventoryResult = inventoryAsync
    match inventoryResult with | Error e -> return Error e | Ok reservation ->
    
    // Phase 3: Sequential (cần tất cả kết quả từ Phase 2)
    let! paymentResult = processPaymentAsync price reservation
    match paymentResult with | Error e -> return Error e | Ok payment ->
    
    let! order = createOrderAsync payment items price
    
    return Ok {| Order = order; User = user |}
}
```

#### 💡 **Best Practices cho Sequential Chaining**

```fsharp
// ✅ Sử dụng Result/Option để handle errors gracefully
let safeSequentialChain input = async {
    let! result1 = step1Async input
    match result1 with
    | Error e -> return Error e
    | Ok value1 ->
        
    let! result2 = step2Async value1  // Dùng value1
    match result2 with  
    | Error e -> return Error e
    | Ok value2 ->
        
    let! result3 = step3Async value1 value2  // Dùng cả value1 và value2
    return result3
}

// ✅ Hoặc sử dụng computation expression để clean hơn
let cleanSequentialChain input = async {
    use! result1 = step1Async input |> AsyncResult.ofAsync
    use! result2 = step2Async result1 |> AsyncResult.ofAsync  
    use! result3 = step3Async result1 result2 |> AsyncResult.ofAsync
    return result3
}
```

## Các mẫu thực tế (Practical Patterns)

### 🔄 Mẫu thử lại (Retry Pattern)

```fsharp
let retryAsync maxAttempts operation = 
    let rec retry attempt = async {
        try
            let! result = operation()
            return Ok result
        with
        | ex when attempt < maxAttempts ->
            printfn "Attempt %d failed, retrying..." attempt
            do! Async.Sleep(1000 * attempt)  // Exponential backoff
            return! retry (attempt + 1)
        | ex ->
            return Error $"Failed after {maxAttempts} attempts: {ex.Message}"
    }
    retry 1
```

### ⏱️ Mẫu Timeout (Timeout Pattern)

```fsharp
let timeoutAsync (milliseconds: int) operation = async {
    try
        let! result = Async.StartChild(operation, milliseconds)
        return! result
    with
    | :? TimeoutException ->
        printfn "Operation timed out after %d ms" milliseconds
        return failwith "Timeout"
}
```

### 📊 Báo cáo tiến trình (Progress Reporting)

```fsharp
let longRunningTaskAsync progressCallback = async {
    for i in 1..5 do
        printfn "Step %d/5..." i
        progressCallback(i * 20)  // Report progress percentage
        do! Async.Sleep(200)
    
    return "Task completed!"
}

// Usage
let progressCallback progress = 
    printfn "Progress: %d%%" progress
    
let! result = longRunningTaskAsync progressCallback
```

## So sánh với JavaScript/Angular (Comparison with JavaScript/Angular)

### 🔄 So sánh trực tiếp (Side-by-Side Comparison)

| Khái niệm | F# Async | JavaScript/TypeScript | Angular |
|---------|----------|----------------------|---------|
| **Define async function** | `async { }` | `async function() {}` | `async method(): Promise<T>` |
| **Await operation** | `let! result = op()` | `const result = await op()` | `const result = await this.http.get().toPromise()` |
| **Await void operation** | `do! op()` | `await op()` | `await this.service.action()` |
| **Parallel execution** | `Async.Parallel` | `Promise.all()` | `forkJoin()` |
| **Delay** | `Async.Sleep(ms)` | `setTimeout()` (promisified) | `timer(ms).toPromise()` |

### 🌐 Ví dụ thực tế (Real-world Examples)

#### F# HTTP Request
```fsharp
let fetchDataAsync url = async {
    use client = new HttpClient()
    let! response = client.GetAsync(url) |> Async.AwaitTask
    let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
    return content
}
```

#### JavaScript Equivalent
```javascript
async function fetchData(url) {
    const response = await fetch(url);
    const content = await response.text();
    return content;
}
```

#### Angular Service
```typescript
@Injectable()
export class DataService {
    constructor(private http: HttpClient) {}
    
    async fetchData(url: string): Promise<string> {
        const response = await this.http.get(url, { responseType: 'text' }).toPromise();
        return response;
    }
}
```

### 🎯 Ưu điểm chính so với JavaScript (Key Advantages Over JavaScript)

| Tính năng | F# Async | JavaScript Promise |
|---------|----------|-------------------|
| **Type Safety** | ✅ Compile-time type checking | ❌ Runtime errors possible |
| **Composability** | ✅ Easy function composition | ⚠️ Requires careful chaining |
| **Error Handling** | ✅ Structured with Result types | ⚠️ Try-catch or .catch() |
| **Cancellation** | ✅ Built-in support | ⚠️ Manual AbortController |
| **Resource Management** | ✅ Automatic disposal | ⚠️ Manual cleanup |

## Phương thức thực thi (Execution Methods)

### 🎮 Chạy các thao tác Async (Running Async Operations)

```fsharp
// Method 1: Async.RunSynchronously (blocking)
let syncResult = asyncOperation |> Async.RunSynchronously

// Method 2: Async.Start (fire and forget, non-blocking)
let fireAndForget = async {
    do! Async.Sleep(1000)
    printfn "Background task completed"
}
Async.Start fireAndForget

// Method 3: Async.StartAsTask (interop with .NET Tasks)
let taskResult = asyncOperation |> Async.StartAsTask
// Can be awaited in C# code: await taskResult
```

### ⚡ Khi nào sử dụng từng phương thức (When to Use Each Method)

| Phương thức | Trường hợp sử dụng | Blocking | Giá trị trả về |
|--------|----------|----------|--------------|
| `RunSynchronously` | Console apps, testing | ✅ Yes | Direct result |
| `Start` | Background tasks | ❌ No | Unit (fire & forget) |
| `StartAsTask` | .NET interop | ❌ No | Task<T> |

## Thực hành tốt nhất (Best Practices)

### ✅ Nên làm (Do's)

1. **Use Result types for error handling**
   ```fsharp
   let safeOperation = async {
       try
           let! result = riskyOperation()
           return Ok result
       with
       | ex -> return Error ex.Message
   }
   ```

2. **Prefer Async.Parallel for independent operations**
   ```fsharp
   let! results = [op1(); op2(); op3()] |> Async.Parallel
   ```

3. **Use proper resource management**
   ```fsharp
   let useResourceAsync = async {
       use resource = new SomeResource()
       let! result = resource.ProcessAsync()
       return result
   } // Resource automatically disposed
   ```

### ❌ Không nên làm (Don'ts)

1. **Don't mix async and sync code carelessly**
   ```fsharp
   // ❌ Bad - blocking in async context
   let badAsync = async {
       let result = syncOperation()  // Blocks thread
       return result
   }
   
   // ✅ Good - use async version
   let goodAsync = async {
       let! result = asyncOperation()
       return result
   }
   ```

2. **Don't ignore cancellation**
   ```fsharp
   // ✅ Good - support cancellation
   let cancellableAsync cancellationToken = async {
       for i in 1..1000 do
           do! Async.Sleep(10)
           // Check for cancellation periodically
   }
   ```

### 🎯 Mẹo về hiệu năng (Performance Tips)

1. **Use `ConfigureAwait(false)` equivalent**
   ```fsharp
   // F# automatically handles context switching efficiently
   let! result = operation() // No need for ConfigureAwait
   ```

2. **Avoid creating unnecessary async wrappers**
   ```fsharp
   // ❌ Unnecessary wrapper
   let wrapperAsync x = async { return x }
   
   // ✅ Direct usage
   let directResult = someValue
   ```

## 🚀 Bắt đầu (Getting Started)

### Step 1: Create Your First Async Function
```fsharp
let myFirstAsync = async {
    printfn "Starting async operation..."
    do! Async.Sleep(1000)
    printfn "Operation completed!"
    return "Success"
}
```

### Step 2: Run It
```fsharp
// In F# Interactive or console app
let result = myFirstAsync |> Async.RunSynchronously
printfn "Result: %s" result
```

### Step 3: Experiment with Patterns
```fsharp
// Try parallel execution
let! results = [
    myFirstAsync
    myFirstAsync  
    myFirstAsync
] |> Async.Parallel

printfn "All completed: %A" results
```

## Async Module Reference

### 📚 **Essential Functions**

| **Function** | **Purpose** | **Example** |
|--------------|-------------|-------------|
| `Async.Sleep(ms)` | Non-blocking delay | `do! Async.Sleep(1000)` |
| `Async.Parallel` | Run multiple async ops | `Async.Parallel [op1; op2; op3]` |
| `Async.StartChild` | Start background async | `let! child = Async.StartChild(op)` |
| `Async.AwaitTask` | Task → Async | `task |> Async.AwaitTask` |
| `Async.RunSynchronously` | Run blocking | `async { return 42 } |> Async.RunSynchronously` |
| `Async.Start` | Fire & forget | `Async.Start backgroundTask` |
| `Async.Choice` | Race condition | `Async.Choice [fast; slow]` |
| `Async.Catch` | Safe execution | `Async.Catch riskyOperation` |

---

## Quick Reference Card

### 🎯 **Common Patterns**

#### **Sequential Flow**
```fsharp
let pipeline = async {
    let! step1 = firstAsync()
    let! step2 = secondAsync(step1)  // Uses step1 result
    let! step3 = thirdAsync(step2)   // Uses step2 result
    return step3
}
```

#### **Parallel Independent Operations**  
```fsharp
let parallel = async {
    let! results = [
        fetchUser()
        fetchProducts() 
        fetchCategories()
    ] |> Async.Parallel
    return results
}
```

#### **Mixed Types Parallel**
```fsharp
let mixedParallel = async {
    let! userAsync = Async.StartChild(fetchUser())
    let! productsAsync = Async.StartChild(fetchProducts())
    
    let! user = userAsync      // User type
    let! products = productsAsync // Product[] type
    return (user, products)
}
```

#### **Safe Operations**
```fsharp
let safeOperation = async {
    try
        let! result = riskyAsync()
        return Ok result
    with
    | ex -> return Error ex.Message
}
```

### 🚀 **Performance Tips**
- ✅ Use `Async.Parallel` for independent operations
- ✅ Use sequential chaining when operations depend on each other  
- ✅ Use `StartChild` for different return types
- ✅ Always handle errors with Result types
- ❌ Don't block async operations with synchronous calls

---

## Common Patterns Cheatsheet

### 📋 **Decision Tree: Which Pattern to Use?**

```
Do operations depend on each other?
├─ YES → Sequential Chaining
│   └─ let! a = op1()
│      let! b = op2(a)
│
└─ NO → Are return types the same?
    ├─ YES → Async.Parallel
    │   └─ [op1(); op2(); op3()] |> Async.Parallel
    │
    └─ NO → StartChild Pattern  
        └─ let! child1 = Async.StartChild(op1())
           let! child2 = Async.StartChild(op2())
```

### 🎯 **Real-world Examples Summary**

| **Scenario** | **Pattern** | **Code Template** |
|--------------|-------------|-------------------|
| **Login Flow** | Sequential | `login → profile → permissions` |
| **Dashboard Load** | Mixed Parallel | `StartChild` for different types |
| **File Processing** | Sequential | `read → process → write` |
| **API Aggregation** | Parallel | `Async.Parallel` for same types |
| **Background Tasks** | Fire & Forget | `Async.Start` |

---

## 📚 Summary

**F# Async Workflows** mang lại:

- 🎯 **Clean Syntax**: Tương tự async/await nhưng type-safe hơn
- ⚡ **High Performance**: Parallel execution dễ dàng
- 🛡️ **Error Safety**: Structured error handling với Result types  
- 🔧 **Rich Ecosystem**: Seamless .NET integration
- 🌐 **Future-proof**: Scales từ simple scripts đến enterprise apps

### **🚀 Next Steps**
1. **Thực hành** với F# Interactive (`dotnet fsi`)
2. **Áp dụng** vào project thực tế
3. **Kết hợp** với F# pipelines và pattern matching
4. **Khám phá** advanced scenarios như cancellation và progress reporting

**Happy F# Async Coding!** 🎉