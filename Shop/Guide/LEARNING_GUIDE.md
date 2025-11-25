# 📚 F# Shop Application - Complete Learning Guide

Hướng dẫn học tập toàn diện cho ứng dụng F# Shop, từ kiến trúc đến implementation chi tiết.

## 📋 Table of Contents

1. [🏗️ Part 1: Clean Architecture](#part1-architecture)
2. [🎯 Part 2: Domain Modeling with F# Types](#part2-domain)
3. [🔧 Part 3: Data Access Layer](#part3-dataaccess)
4. [🧠 Part 4: Business Logic Layer](#part4-business)
5. [🚀 Part 5: Application Orchestration](#part5-application)
6. [⚡ Part 6: Async Programming](#part6-async)
7. [🚂 Part 7: Error Handling Patterns](#part7-errorhandling)
8. [🔄 Part 8: Function Composition](#part8-composition)
9. [✅ Part 9: Testing Strategies](#part9-testing)
10. [🎓 Part 10: Advanced F# Concepts](#part10-advanced)

---

# 🏗️ Part 1: Clean Architecture {#part1-architecture}

## 📋 Tổng quan Clean Architecture

**Clean Architecture** là một pattern thiết kế phần mềm nhấn mạnh **separation of concerns** và **dependency inversion**. Trong F#, pattern này trở nên tự nhiên nhờ vào functional programming principles.

## 🎯 Nguyên tắc cốt lõi

### 1. **Dependency Rule**
```
DataAccess ──→ Models ←── Business ←── Program
     ↓              ↓         ↓         ↓
   (I/O)        (Domain)   (Logic)   (App)
```

**Quy tắc:** Dependencies chỉ đi từ ngoài vào trong, không bao giờ ngược lại.

### 2. **Layer Responsibilities**

| Layer | Trách nhiệm | F# Concepts |
|-------|-------------|-------------|
| **Models** | Domain types, business rules | Union types, Records |
| **DataAccess** | I/O operations, external services | Async, Result types |
| **Business** | Use cases, workflows | Function composition |
| **Program** | UI, controllers, entry points | Orchestration |

---

## 📁 Cấu trúc thư mục trong F# Shop

```
Shop/
├── Models/
│   └── Domain.fs           # 🎯 Core Domain (Innermost layer)
├── DataAccess/
│   ├── StringUtils.fs      # 🔧 Data processing
│   ├── FileOperations.fs   # 💾 File I/O
│   ├── JsonHandler.fs      # 🔄 Serialization
│   └── AsyncWorkflows.fs   # ⚡ Async operations
├── Business/
│   ├── ErrorHandling.fs    # 🚂 Railway Oriented Programming
│   └── Pipelines.fs        # 🔗 Function composition
├── Program.fs              # 🚀 Application entry point
└── Shop.fsproj            # 📋 Project configuration
```

---

## 🎯 Layer 1: Models/Domain.fs (Core Domain)

**Vai trò:** Chứa business logic thuần túy, không phụ thuộc vào bất kỳ layer nào.

### 🔑 Domain Types (F# Union & Record Types)

```fsharp
namespace Shop.Models

// Union Types - Type safety cho IDs
type ProductId = ProductId of string
type CustomerId = CustomerId of string  
type OrderId = OrderId of string

// Record Types - Immutable domain entities
type Product = {
    Id: ProductId
    Name: string
    Description: string
    Price: decimal
    Category: string
    Stock: int
    Tags: string list           // F# list type
    CreatedAt: DateTime
}

type Customer = {
    Id: CustomerId
    Name: string
    Email: string
    Address: string
    Phone: string option        // Option type - No nulls!
    RegisteredAt: DateTime
}
```

### 🎓 Tại sao thiết kế như vậy?

1. **Union Types cho IDs**: `ProductId of string` 
   - Không thể nhầm lẫn ProductId với CustomerId
   - Compile-time safety

2. **Record Types**: 
   - Immutable by default
   - Structural equality
   - Pattern matching support

3. **Option Types**: `string option`
   - Explicit về khả năng null
   - Compiler force bạn handle missing values

### 🚨 Error Types

```fsharp
// Validation errors
type ValidationError =
    | EmptyField of string
    | InvalidEmail of string
    | InvalidPhone of string
    | InvalidPrice of decimal
    | InsufficientStock of requested:int * available:int

// Shop-specific errors  
type ShopError =
    | ProductNotFound of ProductId
    | CustomerNotFound of CustomerId
    | OrderNotFound of OrderId
    | ValidationErrors of ValidationError list
    | FileNotFound of string
    | ProcessingError of string
```

**Lợi ích:** 
- Explicit error modeling
- Type-safe error handling
- No exceptions for business logic

---

## 🔧 Layer 2: DataAccess (Infrastructure)

**Vai trò:** Handle I/O operations, external services, infrastructure concerns.

### 📝 StringUtils.fs - Functional String Processing

```fsharp
namespace Shop.DataAccess

module StringUtils =
    // Pure function - no side effects
    let cleanText (text: string) =
        text.Trim().ToLower()
    
    // Function composition với pipeline operator
    let extractWords (text: string) =
        text.Split([|' '; ','; '.'; ';'|], StringSplitOptions.RemoveEmptyEntries)
        |> Array.toList              // Convert to F# list
        |> List.map cleanText        // Transform each element
    
    // Higher-order function
    let searchProducts (query: string) (products: Product list) =
        let queryWords = extractWords query
        products
        |> List.map (fun p -> (p, calculateScore queryWords p))  // Transform
        |> List.filter (fun (_, score) -> score > 0)            // Filter
        |> List.sortByDescending snd                            // Sort by score
        |> List.map fst                                         // Extract products
```

**🎓 F# Concepts:**
- **Pure Functions**: Predictable, testable
- **Pipeline Operator**: `|>` - data flows left-to-right
- **Higher-Order Functions**: Functions as parameters/return values
- **Tuple Destructuring**: `(product, score)`

### 💾 FileOperations.fs - Async I/O

```fsharp
module FileOperations =
    open System.IO
    
    // Async function returning Result
    let readTextFile (filePath: string) : Async<Result<string, ShopError>> =
        async {
            try
                let! content = File.ReadAllTextAsync(filePath) |> Async.AwaitTask
                return Ok content
            with
            | :? FileNotFoundException -> 
                return Error (FileNotFound filePath)
            | ex -> 
                return Error (ProcessingError ex.Message)
        }
    
    // Logging function with timestamp
    let logMessage (message: string) : Async<Result<unit, ShopError>> =
        async {
            let timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            let logEntry = sprintf "[%s] %s" timestamp message
            return! writeTextFile "Data/shop.log" logEntry
        }
```

**🎓 F# Concepts:**
- **Async Workflows**: `async { }` - computational expressions
- **Result Types**: Explicit success/failure
- **let!**: Bind async values (like await)
- **Exception Handling**: Pattern matching with `with`

### 🔄 JsonHandler.fs - Type-Safe Serialization

```fsharp
module JsonHandler =
    open System.Text.Json
    
    let jsonOptions = 
        let options = JsonSerializerOptions()
        options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
        options.WriteIndented <- true
        options
    
    // Generic function with Result
    let serialize<'T> (obj: 'T) : Result<string, ShopError> =
        try
            JsonSerializer.Serialize(obj, jsonOptions)
            |> Ok
        with
        | ex -> Error (ProcessingError ex.Message)
    
    // Load configuration with default fallback
    let loadConfig () : Async<Result<ShopConfig, ShopError>> =
        async {
            match! readTextFile "Data/config.json" with
            | Ok json -> return deserialize<ShopConfig> json
            | Error _ -> return Ok defaultConfig  // Fallback
        }
```

**🎓 F# Concepts:**
- **Generic Functions**: `<'T>` type parameters
- **Result Railway**: Chain operations that might fail
- **Pattern Matching**: `match! ... with`

---

## 🧠 Layer 3: Business (Application Logic)

**Vai trò:** Orchestrate domain operations, implement use cases.

### 🚂 ErrorHandling.fs - Railway Oriented Programming

```fsharp
module ErrorHandling =
    
    // Bind function - chain operations that might fail
    let bind (func: 'a -> Result<'b, 'e>) (result: Result<'a, 'e>) : Result<'b, 'e> =
        match result with
        | Ok value -> func value
        | Error err -> Error err
    
    // Map function - transform success values
    let map (func: 'a -> 'b) (result: Result<'a, 'e>) : Result<'b, 'e> =
        match result with  
        | Ok value -> Ok (func value)
        | Error err -> Error err
    
    // Custom operators for chaining
    let (>>=) result func = bind func result    // Bind operator
    let (<!>) result func = map func result     // Map operator
```

**Railway Oriented Programming:**
```
Success Track:  Input ──→ Function1 ──→ Function2 ──→ Output
                  │           │           │
Error Track:      └─────→ Error ────→ Error ────→ Error
```

**🎓 Concepts:**
- **Higher-Order Functions**: Functions that operate on functions
- **Custom Operators**: `>>=` và `<!>` 
- **Monadic Patterns**: Composing computations

### 🔗 Pipelines.fs - Function Composition

```fsharp
module Pipelines =
    
    // Forward composition operator
    let (>=>) f g x = f x |> g
    
    // Validation pipeline
    let validateProduct (product: Product) : Result<Product, ValidationError> =
        if String.IsNullOrEmpty(product.Name) then
            Error (EmptyField "Name")
        elif product.Price <= 0m then
            Error (InvalidPrice product.Price)
        else
            Ok product
    
    // Processing pipeline
    let processOrder customer items =
        items
        |> List.map validateOrderItem           // Validate each item
        |> List.fold combineResults (Ok [])     // Combine results
        |> Result.map (createOrder customer)    // Create order if all valid
    
    // Data transformation pipeline
    let productToInventoryRecord (product: Product) : InventoryRecord =
        {
            ProductId = match product.Id with ProductId id -> id
            ProductName = product.Name
            Category = product.Category
            CurrentStock = product.Stock
            ReorderLevel = if product.Stock < 10 then 20 else 10
            LastRestocked = DateTime.Now.AddDays(-7.0)
        }
```

**🎓 Concepts:**
- **Function Composition**: Building complex operations from simple ones
- **Pipeline Transformations**: Data flowing through transformations
- **Validation Patterns**: Accumulating errors vs. fail-fast

---

## 🚀 Layer 4: Program.fs (Application Entry Point)

**Vai trò:** Orchestrate all layers, handle user interface, application startup.

```fsharp
module Program =
    
    // Sample data for demonstration
    let sampleProducts = [
        { Id = ProductId "1"; Name = "Laptop Pro 15"; Price = 1299.99m; ... }
        { Id = ProductId "2"; Name = "Wireless Mouse"; Price = 29.99m; ... }
        // ...
    ]
    
    // Demo orchestration
    let demonstrateStringProcessing () =
        async {
            printfn "=== STRING PROCESSING DEMO ==="
            let query = "laptop computer"
            let results = StringUtils.searchProducts query sampleProducts
            
            results
            |> List.take 2
            |> List.iter (fun p -> 
                printfn "  - %s ($%.2f)" p.Name p.Price)
        }
    
    // Main orchestration
    let runDemos () =
        async {
            do! demonstrateStringProcessing ()
            do! demonstrateFileOperations ()
            do! demonstratePipelines ()
            do! demonstrateAsyncWorkflows ()
            
            printfn "✅ All demonstrations completed!"
        }
    
    [<EntryPoint>]
    let main args =
        // Application startup
        System.IO.Directory.CreateDirectory("Data") |> ignore
        
        try
            runDemos () |> Async.RunSynchronously
            0  // Success exit code
        with
        | ex ->
            printfn "❌ Error: %s" ex.Message
            1  // Error exit code
```

**🎓 Concepts:**
- **Async Composition**: `do!` for sequential async operations
- **Error Handling**: Try-catch at application boundary
- **Orchestration**: Combining all layers

---

## 🎯 Lợi ích của Clean Architecture trong F#

### 1. **Testability**
```fsharp
// Easy to test - pure functions
let testSearchProducts () =
    let products = [laptop; mouse]
    let results = StringUtils.searchProducts "laptop" products
    assert (results.Length = 1)
```

### 2. **Maintainability**
- Mỗi layer có trách nhiệm rõ ràng
- Thay đổi một layer không ảnh hưởng layer khác
- Dependencies rõ ràng

### 3. **Type Safety**
- F# compiler bắt lỗi sớm
- Domain types prevent invalid states
- Result types force error handling

### 4. **Functional Composition**
- Small, composable functions
- No hidden side effects
- Predictable behavior

---

## 🔄 Data Flow Example

```
User Request → Program.fs → Business Layer → DataAccess → Models
     ↓              ↓            ↓              ↓         ↓
   HTTP          Orchestrate   Validate      Read File   Domain
  Request         Use Cases    Business      Database    Types
                               Rules
```

**Ví dụ cụ thể:**
1. User muốn search products
2. `Program.fs` gọi `demonstrateStringProcessing()`  
3. `Business/Pipelines.fs` validate search query
4. `DataAccess/StringUtils.fs` thực hiện search algorithm
5. Return domain types từ `Models/Domain.fs`

---

## 🎉 Kết luận

Clean Architecture trong F# mang lại:
- **Separation of Concerns** rõ ràng
- **Type Safety** từ compiler
- **Functional Composition** thay vì inheritance
- **Explicit Error Handling** thay vì exceptions
- **Immutability** by default

---

# 🎯 Part 2: Domain Modeling with F# Types {#part2-domain}

Domain modeling là **trái tim** của F# programming. F# types cho phép chúng ta **mô hình hóa business domain** một cách chính xác và type-safe.

## 🎭 Tại Sao F# Types Mạnh Mẽ?

### 💡 **Type-Driven Development**
```fsharp
// ❌ Primitive Obsession (C#/Java style)
string customerId = "CUST123";
string productId = "PROD456"; 
// 😱 Có thể nhầm lẫn: customerId thành productId!

// ✅ F# Strong Types  
type CustomerId = CustomerId of string
type ProductId = ProductId of string

let customerId = CustomerId "CUST123"
let productId = ProductId "PROD456"
// 🎯 Compiler sẽ báo lỗi nếu dùng sai!
```

### 🛡️ **Make Illegal States Unrepresentable**
```fsharp
// ❌ Có thể có trạng thái không hợp lệ
type OrderBad = {
    Id: string
    Status: string        // "Pending", "Shipped", "Delivered"?
    ShippedDate: DateTime // Shipped date khi status = "Pending"? 😕
    DeliveredDate: DateTime
}

// ✅ F# Union Types - Chỉ cho phép states hợp lệ
type OrderStatus = 
    | Pending
    | Shipped of shippedDate: DateTime  
    | Delivered of shippedDate: DateTime * deliveredDate: DateTime

type Order = {
    Id: OrderId
    Status: OrderStatus    // Chỉ có thể là states hợp lệ!
}
```

---

## 🧩 F# Type System Deep Dive

### 1️⃣ **Union Types** - Modeling Choices

```fsharp
// 🎯 Payment Methods - Exactly one of these
type PaymentMethod =
    | Cash
    | CreditCard of cardNumber: string * expiryDate: DateTime
    | PayPal of email: string
    | BankTransfer of accountNumber: string * routingCode: string

// 📦 Product Categories với associated data
type ProductCategory =
    | Electronics of warranty: int<Months>
    | Clothing of size: string * color: string  
    | Books of isbn: string * author: string
    | Food of expiryDate: DateTime

// 🚨 Error Types - Union of all possible errors  
type ValidationError =
    | EmptyField of fieldName: string
    | InvalidEmail of email: string
    | InvalidPrice of price: decimal * reason: string
    | InsufficientStock of requested: int * available: int

// 📋 Usage với Pattern Matching
let validatePrice price =
    match price with
    | p when p <= 0m -> Error (InvalidPrice (price, "Giá phải > 0"))
    | p when p > 1000000m -> Error (InvalidPrice (price, "Giá quá cao"))
    | _ -> Ok price

let processPayment amount paymentMethod =
    match paymentMethod with
    | Cash -> processVashPayment amount
    | CreditCard (cardNum, expiry) -> 
        if expiry > DateTime.Now 
        then processCreditCard amount cardNum
        else Error "Thẻ đã hết hạn"
    | PayPal email -> processPayPalPayment amount email
    | BankTransfer (account, routing) -> processBankTransfer amount account routing
```

### 2️⃣ **Record Types** - Modeling Data Structures

```fsharp
// 🏪 Core domain entities
type Product = {
    Id: ProductId
    Name: string
    Description: string option    // Option = có thể null
    Price: decimal
    Category: ProductCategory
    Stock: int
    CreatedAt: DateTime
    UpdatedAt: DateTime option
}

// 👤 Customer với validation
type Customer = {
    Id: CustomerId  
    Name: string
    Email: string
    Phone: string option
    Address: Address option
    RegisteredAt: DateTime
    IsActive: bool
}

// 📍 Value Objects
and Address = {
    Street: string
    City: string  
    State: string
    ZipCode: string
    Country: string
}

// 🛒 Order aggregate  
type Order = {
    Id: OrderId
    CustomerId: CustomerId
    Items: OrderItem list
    Status: OrderStatus
    PaymentMethod: PaymentMethod
    OrderDate: DateTime
    Notes: string option
}

and OrderItem = {
    ProductId: ProductId
    ProductName: string    // Denormalized để tránh N+1 queries
    Quantity: int
    UnitPrice: decimal     // Price tại thời điểm order (history)
    Subtotal: decimal
}
```

### 3️⃣ **Option Types** - Explicit Null Handling

```fsharp
// ❌ Traditional nullable (C#/Java)
// Customer.Phone có thể null → NullReferenceException 💥

// ✅ F# Option Type - Explicit về nullable
type Customer = {
    // ...
    Phone: string option      // Some "0123456789" | None
    Address: Address option   // Some address | None  
}

// 🎯 Working với Option values
let formatCustomerInfo customer =
    let phoneInfo = 
        match customer.Phone with
        | Some phone -> $"📞 {phone}"
        | None -> "📞 Chưa có SĐT"
    
    let addressInfo =
        customer.Address
        |> Option.map (fun addr -> $"📍 {addr.City}, {addr.State}")
        |> Option.defaultValue "📍 Chưa có địa chỉ"
    
    $"{customer.Name} - {phoneInfo} - {addressInfo}"

// 🔄 Option combinators
let getCustomerDiscount customerId =
    findCustomer customerId          // Customer option
    |> Option.bind getCustomerTier   // CustomerTier option  
    |> Option.map calculateDiscount  // decimal option
    |> Option.defaultValue 0m        // decimal
```

---

## 🏗️ Advanced Domain Modeling Patterns

### 🎯 **Single Case Union Types** - Type Safety

```fsharp
// 🔒 Prevent primitive obsession
type CustomerId = CustomerId of string
type ProductId = ProductId of string  
type OrderId = OrderId of string
type Email = Email of string

// 📐 Units of Measure
[<Measure>] type USD
[<Measure>] type VND  
[<Measure>] type Kg
[<Measure>] type Gram

type Product = {
    // ...
    Price: decimal<USD>      // Type-safe currency
    Weight: decimal<Kg>      // Type-safe weight
}

// 🎯 Smart constructors với validation
module Email =
    let create (emailStr: string) : Result<Email, ValidationError> =
        if emailStr.Contains("@") && emailStr.Contains(".")
        then Ok (Email emailStr)
        else Error (InvalidEmail emailStr)
    
    let value (Email email) = email    // Unwrap

module ProductId =
    let create (id: string) : Result<ProductId, ValidationError> =
        if String.IsNullOrWhiteSpace(id)
        then Error (EmptyField "ProductId")
        else Ok (ProductId id)
    
    let value (ProductId id) = id
```

### 🔄 **State Machines với Union Types**

```fsharp
// 🚚 Order Lifecycle - Type-safe state transitions
type OrderState =
    | Draft of items: OrderItem list
    | Submitted of order: Order * submittedAt: DateTime
    | Processing of order: Order * startedAt: DateTime  
    | Shipped of order: Order * tracking: string * shippedAt: DateTime
    | Delivered of order: Order * deliveredAt: DateTime
    | Cancelled of order: Order * reason: string * cancelledAt: DateTime

// 🎯 Valid state transitions
type OrderCommand =
    | SubmitOrder of CustomerId
    | StartProcessing  
    | ShipOrder of trackingNumber: string
    | MarkDelivered
    | CancelOrder of reason: string

// ✅ Type-safe transitions - Compiler ensures valid states
let processOrderCommand state command =
    match state, command with
    | Draft items, SubmitOrder customerId ->
        let order = createOrder customerId items
        Ok (Submitted (order, DateTime.Now))
    
    | Submitted (order, _), StartProcessing ->
        Ok (Processing (order, DateTime.Now))
    
    | Processing (order, _), ShipOrder tracking ->
        Ok (Shipped (order, tracking, DateTime.Now))
    
    | Shipped (order, tracking, _), MarkDelivered ->
        Ok (Delivered (order, DateTime.Now))
    
    | _, CancelOrder reason ->
        // Có thể cancel từ bất kỳ state nào (trừ Delivered)
        match state with
        | Delivered _ -> Error "Không thể cancel đơn hàng đã giao"
        | _ -> 
            let order = getOrderFromState state
            Ok (Cancelled (order, reason, DateTime.Now))
    
    | invalidState, invalidCommand ->
        Error $"Invalid transition: {invalidState} -> {invalidCommand}"
```

### 🎭 **Active Patterns** - Custom Pattern Matching

```fsharp
// 🔍 Custom pattern để phân loại customers
let (|VipCustomer|RegularCustomer|NewCustomer|) customer =
    let totalOrders = getCustomerOrderCount customer.Id
    let membershipDays = (DateTime.Now - customer.RegisteredAt).Days
    
    match totalOrders, membershipDays with
    | orders, _ when orders >= 50 -> VipCustomer
    | orders, days when orders >= 5 && days >= 30 -> RegularCustomer  
    | _, _ -> NewCustomer

// 📊 Sử dụng Active Patterns
let calculateDiscount customer order =
    match customer with
    | VipCustomer -> order.Total * 0.2m      // 20% discount
    | RegularCustomer -> order.Total * 0.1m  // 10% discount
    | NewCustomer -> 0m                      // No discount

// 🎯 Partial Active Patterns
let (|HighValue|_|) order =
    if order.Total >= 1000m then Some order.Total else None

let (|BulkOrder|_|) order = 
    let totalItems = order.Items |> List.sumBy (fun item -> item.Quantity)
    if totalItems >= 20 then Some totalItems else None

let categorizeOrder order =
    match order with
    | HighValue amount -> $"💎 Đơn hàng cao cấp: ${amount}"
    | BulkOrder quantity -> $"📦 Đơn hàng số lượng lớn: {quantity} items"
    | _ -> "📝 Đơn hàng thường"
```

---

## 🎪 Real-World Domain Example

### 🛒 Complete Shopping Cart Domain

```fsharp
// 🛍️ Shopping cart states
type CartState =
    | Empty
    | Active of items: CartItem list * lastUpdated: DateTime
    | Abandoned of items: CartItem list * abandonedAt: DateTime
    | CheckedOut of orderId: OrderId * checkedOutAt: DateTime

and CartItem = {
    ProductId: ProductId
    ProductName: string
    Quantity: int
    UnitPrice: decimal<USD>
    AddedAt: DateTime
}

// 🎯 Cart operations
type CartCommand =
    | AddItem of ProductId * quantity: int
    | RemoveItem of ProductId
    | UpdateQuantity of ProductId * newQuantity: int
    | ClearCart
    | Checkout of PaymentMethod

// ⚡ Type-safe cart workflows
module ShoppingCart =
    
    let addItem productId quantity state =
        match state with
        | Empty -> 
            // Load product info and create first item
            loadProduct productId
            |> Result.map (fun product -> 
                let item = createCartItem product quantity
                Active ([item], DateTime.Now))
        
        | Active (items, _) ->
            // Check if item exists, update or add
            match items |> List.tryFind (fun i -> i.ProductId = productId) with
            | Some existingItem ->
                let updatedItems = 
                    items |> List.map (fun i -> 
                        if i.ProductId = productId 
                        then { i with Quantity = i.Quantity + quantity }
                        else i)
                Ok (Active (updatedItems, DateTime.Now))
            | None ->
                loadProduct productId
                |> Result.map (fun product ->
                    let newItem = createCartItem product quantity
                    Active (newItem :: items, DateTime.Now))
        
        | Abandoned (items, _) ->
            // Reactivate cart
            addItem productId quantity (Active (items, DateTime.Now))
        
        | CheckedOut _ ->
            Error "Cannot modify checked out cart"
    
    let calculateTotal (Active (items, _)) =
        items 
        |> List.sumBy (fun item -> 
            decimal item.Quantity * item.UnitPrice)
    
    let checkout paymentMethod state =
        match state with
        | Active (items, _) when not (List.isEmpty items) ->
            let total = calculateTotal state
            processPayment paymentMethod total
            |> Result.map (fun orderId -> 
                CheckedOut (orderId, DateTime.Now))
        | Active ([], _) -> 
            Error "Cannot checkout empty cart"
        | _ -> 
            Error "Cart is not in active state"
```

---

## 🎓 Best Practices & Patterns

### ✅ **Do's**

```fsharp
// ✅ Use descriptive union case names
type OrderStatus = 
    | PendingPayment
    | PaymentConfirmed  
    | InPreparation
    | ShippedToCustomer
    | DeliveredSuccessfully

// ✅ Group related types together
module Customer =
    type CustomerId = CustomerId of string
    type CustomerTier = Bronze | Silver | Gold | Platinum
    type Customer = { Id: CustomerId; Tier: CustomerTier; (* ... *) }

// ✅ Use Result for operations that can fail
let validateProduct product : Result<Product, ValidationError list> =
    // Validation logic
    
// ✅ Use Option for potentially missing values  
type Product = {
    Description: string option  // Explicit về potentially missing
    DiscountPrice: decimal option
}
```

### ❌ **Don'ts**

```fsharp
// ❌ Don't use primitive types for domain concepts
type Order = {
    CustomerId: string      // Should be CustomerId
    ProductId: string       // Should be ProductId  
}

// ❌ Don't use exceptions for business logic
let findCustomer id =
    // ❌ Bad
    if customerExists id 
    then getCustomer id
    else failwith "Customer not found"  // Exception!
    
    // ✅ Good  
    if customerExists id
    then Some (getCustomer id)
    else None

// ❌ Don't make everything optional
type Product = {
    Name: string option     // Name should be required!
    Price: decimal option   // Price should be required!
}
```

---

## 🚀 Next Steps

Bây giờ bạn đã hiểu cách model domain với F# types! 

**Tiếp theo:** [Part 3: Data Access Layer](#part3-dataaccess) - Học cách persist và retrieve các domain objects này một cách functional và type-safe.

**Practice:** Thử tạo domain model cho business của bạn sử dụng F# Union Types và Record Types! 🎯

---

# 🔧 Part 3: Data Access Layer {#part3-dataaccess}

Data Access Layer trong F# tập trung vào **functional I/O operations**, **async workflows**, và **type-safe error handling**. Chúng ta sẽ build một data layer hoàn toàn functional!

## 🎯 Functional I/O Philosophy

### 💡 **Pure Functions vs. Side Effects**
```fsharp
// ❌ Impure function - side effects
let saveCustomerBad customer =
    // Direct database call - side effect!
    database.Save(customer)
    customer  // Unpredictable behavior

// ✅ Pure function - returns intention
let saveCustomer customer : Async<Result<Customer, ShopError>> =
    async {
        try
            // Wrapped in async + Result for composability
            let! savedCustomer = Database.saveAsync customer
            return Ok savedCustomer
        with
        | ex -> return Error (ProcessingError ex.Message)
    }
```

### 🚂 **Railway Oriented Programming**
```fsharp
// 🎯 Data operations always return Result<'T, 'Error>
type DataResult<'T> = Result<'T, ShopError>

// 🔄 Chainable operations
let customerWorkflow customerId =
    findCustomer customerId          // DataResult<Customer>
    |> Result.bind validateCustomer  // DataResult<Customer>
    |> Result.bind updateLastSeen    // DataResult<Customer>  
    |> Result.bind saveCustomer      // DataResult<Customer>
```

---

## 📁 File-Based Data Access Implementation

### 🗂️ **FileHandler.fs** - Core I/O Operations

```fsharp
module FileHandler

open System
open System.IO
open System.Text.Json

// 📂 Ensure data directory exists
let ensureDataDirectory () =
    let dataDir = "Data"
    if not (Directory.Exists(dataDir)) then
        Directory.CreateDirectory(dataDir) |> ignore

// 📖 Generic file reader với error handling
let readTextFile (filePath: string) : Async<Result<string, ShopError>> =
    async {
        try
            ensureDataDirectory()
            let fullPath = Path.Combine("Data", filePath)
            
            if File.Exists(fullPath) then
                let! content = File.ReadAllTextAsync(fullPath) |> Async.AwaitTask
                return Ok content
            else
                return Error (FileNotFound fullPath)
        with
        | ex -> return Error (ProcessingError $"Đọc file thất bại: {ex.Message}")
    }

// ✍️ Generic file writer với atomic operations
let writeTextFile (filePath: string) (content: string) : Async<Result<unit, ShopError>> =
    async {
        try
            ensureDataDirectory()
            let fullPath = Path.Combine("Data", filePath)
            let tempPath = fullPath + ".tmp"
            
            // Atomic write: temp file -> rename
            do! File.WriteAllTextAsync(tempPath, content) |> Async.AwaitTask
            File.Move(tempPath, fullPath)
            
            return Ok ()
        with
        | ex -> return Error (ProcessingError $"Ghi file thất bại: {ex.Message}")
    }

// 📋 List files với pattern matching
let listDataFiles (pattern: string) : Async<Result<string list, ShopError>> =
    async {
        try
            ensureDataDirectory()
            let dataDir = DirectoryInfo("Data")
            let files = 
                dataDir.GetFiles(pattern)
                |> Array.map (fun f -> f.Name)
                |> Array.toList
            
            return Ok files
        with
        | ex -> return Error (ProcessingError $"List files thất bại: {ex.Message}")
    }
```

### 🎭 **JsonHandler.fs** - Type-Safe Serialization

```fsharp
module JsonHandler

open System.Text.Json
open System.Text.Json.Serialization

// ⚙️ JSON configuration với F# friendly settings
let jsonOptions = 
    let options = JsonSerializerOptions()
    options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    options.WriteIndented <- true
    options.Converters.Add(JsonFSharpConverter()) // F# types support
    options

// 📤 Generic serialization với Result handling
let serialize<'T> (obj: 'T) : Result<string, ShopError> =
    try
        let json = JsonSerializer.Serialize(obj, jsonOptions)
        Ok json
    with
    | ex -> Error (ProcessingError $"Serialize thất bại: {ex.Message}")

// 📥 Generic deserialization với Result handling  
let deserialize<'T> (json: string) : Result<'T, ShopError> =
    try
        let obj = JsonSerializer.Deserialize<'T>(json, jsonOptions)
        Ok obj
    with
    | ex -> Error (ProcessingError $"Deserialize thất bại: {ex.Message}")

// 💾 Load typed data từ JSON file
let loadFromJson<'T> (fileName: string) : Async<Result<'T, ShopError>> =
    async {
        match! FileHandler.readTextFile fileName with
        | Ok json -> return deserialize<'T> json
        | Error err -> return Error err
    }

// 💽 Save typed data to JSON file  
let saveToJson<'T> (fileName: string) (data: 'T) : Async<Result<unit, ShopError>> =
    async {
        match serialize data with
        | Ok json -> return! FileHandler.writeTextFile fileName json
        | Error err -> return Error err
    }
```

---

## 🏪 Shop Data Repositories

### 👤 **CustomerRepository.fs** - Customer CRUD Operations

```fsharp
module CustomerRepository

open Models
open JsonHandler

// 📊 Customer data container
type CustomerData = {
    Customers: Customer list
    LastUpdated: DateTime
}

let private customersFile = "customers.json"

// 🔄 Load all customers với fallback
let loadCustomers () : Async<Result<Customer list, ShopError>> =
    async {
        match! loadFromJson<CustomerData> customersFile with
        | Ok data -> return Ok data.Customers
        | Error (FileNotFound _) -> 
            // First run - return empty list
            return Ok []
        | Error err -> return Error err
    }

// 💾 Save all customers atomically
let saveCustomers (customers: Customer list) : Async<Result<unit, ShopError>> =
    async {
        let customerData = {
            Customers = customers
            LastUpdated = DateTime.Now
        }
        return! saveToJson customersFile customerData
    }

// 🔍 Find customer by ID
let findCustomer (CustomerId customerId) : Async<Result<Customer option, ShopError>> =
    async {
        match! loadCustomers() with
        | Ok customers ->
            let found = customers |> List.tryFind (fun c -> 
                let (CustomerId id) = c.Id
                id = customerId)
            return Ok found
        | Error err -> return Error err
    }

// ➕ Add new customer với duplicate check
let addCustomer (customer: Customer) : Async<Result<Customer, ShopError>> =
    async {
        match! loadCustomers() with
        | Ok customers ->
            // Check for duplicates
            let (CustomerId newId) = customer.Id
            let exists = customers |> List.exists (fun c ->
                let (CustomerId existingId) = c.Id
                existingId = newId)
            
            if exists then
                return Error (ValidationErrors [EmptyField $"Customer {newId} already exists"])
            else
                let updatedCustomers = customer :: customers
                match! saveCustomers updatedCustomers with
                | Ok () -> return Ok customer
                | Error err -> return Error err
        | Error err -> return Error err
    }

// 🔄 Update existing customer
let updateCustomer (customer: Customer) : Async<Result<Customer, ShopError>> =
    async {
        match! loadCustomers() with
        | Ok customers ->
            let (CustomerId targetId) = customer.Id
            let updated = customers |> List.map (fun c ->
                let (CustomerId existingId) = c.Id
                if existingId = targetId then customer else c)
            
            match! saveCustomers updated with
            | Ok () -> return Ok customer
            | Error err -> return Error err
        | Error err -> return Error err
    }

// 🗑️ Delete customer (soft delete - mark inactive)
let deleteCustomer (customerId: CustomerId) : Async<Result<unit, ShopError>> =
    async {
        match! findCustomer customerId with
        | Ok (Some customer) ->
            let inactiveCustomer = { customer with IsActive = false }
            match! updateCustomer inactiveCustomer with
            | Ok _ -> return Ok ()
            | Error err -> return Error err
        | Ok None -> return Error (CustomerNotFound customerId)
        | Error err -> return Error err
    }

// 📋 Get active customers only
let getActiveCustomers () : Async<Result<Customer list, ShopError>> =
    async {
        match! loadCustomers() with
        | Ok customers ->
            let activeCustomers = customers |> List.filter (fun c -> c.IsActive)
            return Ok activeCustomers
        | Error err -> return Error err
    }
```

### 🛍️ **ProductRepository.fs** - Product Management

```fsharp
module ProductRepository

open Models
open JsonHandler

type ProductData = {
    Products: Product list
    Categories: ProductCategory list
    LastUpdated: DateTime
}

let private productsFile = "products.json"

// 📦 Load products với caching potential
let loadProducts () : Async<Result<Product list, ShopError>> =
    async {
        match! loadFromJson<ProductData> productsFile with
        | Ok data -> return Ok data.Products
        | Error (FileNotFound _) -> return Ok []
        | Error err -> return Error err
    }

// 🏷️ Find products by category
let findProductsByCategory (category: ProductCategory) : Async<Result<Product list, ShopError>> =
    async {
        match! loadProducts() with
        | Ok products ->
            let filtered = products |> List.filter (fun p -> p.Category = category)
            return Ok filtered
        | Error err -> return Error err
    }

// 🔍 Search products by name/description
let searchProducts (searchTerm: string) : Async<Result<Product list, ShopError>> =
    async {
        match! loadProducts() with
        | Ok products ->
            let searchLower = searchTerm.ToLower()
            let matches = products |> List.filter (fun p ->
                p.Name.ToLower().Contains(searchLower) ||
                (p.Description |> Option.map (fun d -> d.ToLower().Contains(searchLower)) |> Option.defaultValue false))
            return Ok matches
        | Error err -> return Error err
    }

// 📊 Get low stock products
let getLowStockProducts (threshold: int) : Async<Result<Product list, ShopError>> =
    async {
        match! loadProducts() with
        | Ok products ->
            let lowStock = products |> List.filter (fun p -> p.Stock <= threshold)
            return Ok lowStock
        | Error err -> return Error err
    }

// 🔄 Update stock levels (important for e-commerce)
let updateStock (productId: ProductId) (newStock: int) : Async<Result<Product, ShopError>> =
    async {
        match! loadProducts() with
        | Ok products ->
            match products |> List.tryFind (fun p -> p.Id = productId) with
            | Some product ->
                let updatedProduct = { product with Stock = newStock; UpdatedAt = Some DateTime.Now }
                let updatedProducts = products |> List.map (fun p -> 
                    if p.Id = productId then updatedProduct else p)
                
                let productData = {
                    Products = updatedProducts
                    Categories = [] // Load from separate source
                    LastUpdated = DateTime.Now
                }
                
                match! saveToJson productsFile productData with
                | Ok () -> return Ok updatedProduct
                | Error err -> return Error err
            | None -> return Error (ProductNotFound productId)
        | Error err -> return Error err
    }
```

---

## ⚡ Async Composition Patterns

### 🔗 **Combining Multiple Data Operations**

```fsharp
module DataWorkflows

// 🎯 Parallel data loading
let loadDashboardData customerId : Async<Result<DashboardData, ShopError>> =
    async {
        // Load multiple data sources in parallel
        let! customerTask = CustomerRepository.findCustomer customerId |> Async.StartChild
        let! ordersTask = OrderRepository.getCustomerOrders customerId |> Async.StartChild  
        let! productsTask = ProductRepository.loadProducts() |> Async.StartChild
        
        // Wait for all results
        let! customerResult = customerTask
        let! ordersResult = ordersTask
        let! productsResult = productsTask
        
        // Combine results using Result computation
        return 
            result {
                let! customer = customerResult |> Result.bind (function
                    | Some c -> Ok c
                    | None -> Error (CustomerNotFound customerId))
                let! orders = ordersResult
                let! products = productsResult
                
                return {
                    Customer = customer
                    RecentOrders = orders |> List.take 5
                    FeaturedProducts = products |> List.take 10
                    LoadedAt = DateTime.Now
                }
            }
    }

// 🔄 Sequential workflows với error handling
let completeOrderWorkflow orderId : Async<Result<CompletedOrder, ShopError>> =
    async {
        // Sequential steps - each depends on previous
        let! orderResult = OrderRepository.findOrder orderId
        
        match orderResult with
        | Ok (Some order) ->
            // Validate stock for all items
            let! stockValidation = validateOrderStock order.Items
            
            match stockValidation with
            | Ok () ->
                // Update stock levels
                let! stockUpdates = updateStockLevels order.Items
                
                match stockUpdates with  
                | Ok () ->
                    // Mark order as completed
                    let completedOrder = { order with Status = Completed DateTime.Now }
                    return! OrderRepository.updateOrder completedOrder
                | Error err -> return Error err
            | Error err -> return Error err
        | Ok None -> return Error (OrderNotFound orderId)
        | Error err -> return Error err
    }

// 🚀 Batch operations với error collection
let processBulkCustomers (customers: Customer list) : Async<Result<BulkResult, ShopError>> =
    async {
        let results = ResizeArray<Result<Customer, ShopError>>()
        
        // Process each customer
        for customer in customers do
            let! result = CustomerRepository.addCustomer customer
            results.Add(result)
        
        // Separate successes and failures
        let successes = results |> Seq.choose (function Ok c -> Some c | Error _ -> None) |> Seq.toList
        let failures = results |> Seq.choose (function Error e -> Some e | Ok _ -> None) |> Seq.toList
        
        return Ok {
            Processed = results.Count
            Successes = successes.Length
            Failures = failures.Length
            FailureReasons = failures
        }
    }
```

---

## 🎯 Best Practices & Performance Tips

### ✅ **Do's - Functional Data Access**

```fsharp
// ✅ Always wrap I/O in Async<Result<'T, 'Error>>
let loadData () : Async<Result<Data, ShopError>>

// ✅ Use immutable data structures
type CustomerData = { Customers: Customer list; LastUpdated: DateTime }

// ✅ Atomic file operations (temp file + move)
let atomicSave filePath content =
    let tempPath = filePath + ".tmp"
    writeFile tempPath content
    moveFile tempPath filePath

// ✅ Validate before persist
let saveCustomer customer =
    customer
    |> validateCustomer
    |> Result.bind persistCustomer

// ✅ Use parallel async for independent operations  
let loadMultiple () = async {
    let! task1 = loadCustomers() |> Async.StartChild
    let! task2 = loadProducts() |> Async.StartChild
    let! customers = task1
    let! products = task2
    return (customers, products)
}
```

### ❌ **Don'ts - Avoid These Patterns**

```fsharp
// ❌ Don't use exceptions for business logic
let findCustomer id =
    if customerExists id then getCustomer id
    else failwith "Not found"  // BAD!

// ❌ Don't mix sync/async inconsistently
let saveCustomerBad customer =
    let json = serialize customer        // sync
    writeFileAsync "file.json" json      // async - mixing!

// ❌ Don't ignore error handling
let loadData () = async {
    let! content = File.ReadAllTextAsync("file.json") |> Async.AwaitTask
    return deserialize content  // What if file doesn't exist?
}

// ❌ Don't use mutable global state
let mutable customers = []  // BAD - global mutable state
```

---

## 🚀 Testing Your Data Layer

```fsharp
module DataLayerTests

// 🧪 Test utilities
let createTestCustomer name email =
    {
        Id = CustomerId (Guid.NewGuid().ToString())
        Name = name
        Email = email
        Phone = None
        Address = None
        RegisteredAt = DateTime.Now
        IsActive = true
    }

// ✅ Test happy path
let ``Should save and load customer successfully`` () = async {
    // Arrange
    let customer = createTestCustomer "John Doe" "john@test.com"
    
    // Act
    let! saveResult = CustomerRepository.addCustomer customer
    let! loadResult = CustomerRepository.findCustomer customer.Id
    
    // Assert
    match saveResult, loadResult with
    | Ok savedCustomer, Ok (Some loadedCustomer) ->
        Assert.Equal(savedCustomer.Name, loadedCustomer.Name)
    | _ -> Assert.Fail("Test failed")
}

// ✅ Test error scenarios  
let ``Should handle duplicate customer gracefully`` () = async {
    // Arrange
    let customer = createTestCustomer "Jane Doe" "jane@test.com"
    
    // Act
    let! firstSave = CustomerRepository.addCustomer customer
    let! secondSave = CustomerRepository.addCustomer customer
    
    // Assert
    match firstSave, secondSave with
    | Ok _, Error (ValidationErrors _) -> () // Expected
    | _ -> Assert.Fail("Should reject duplicate")
}
```

**Part 3 Summary:** Bạn đã học cách build một **functional data access layer** với **type safety**, **error handling**, và **async composition**! 

**Ready cho Part 4?** 🚀

---

# 🧠 Part 4: Business Logic Layer {#part4-business}

Business Logic Layer là **trái tim** của ứng dụng - nơi implement **business rules**, **domain workflows**, và **use cases**. Trong F#, chúng ta sử dụng **pure functions**, **function composition**, và **domain modeling** để tạo business logic rõ ràng và dễ test.

## 🎯 Business Logic Philosophy

### 💡 **Pure Business Functions**
```fsharp
// ✅ Pure function - no side effects, predictable
let calculateDiscount (customer: Customer) (order: Order) : decimal =
    match customer with
    | c when c.RegisteredAt < DateTime.Now.AddYears(-1) -> order.Total * 0.1m  // 10% loyal
    | _ -> 0m

// ✅ Business rule as pure function
let canPlaceOrder (customer: Customer) (requestedItems: CartItem list) : Result<unit, BusinessError> =
    if not customer.IsActive then 
        Error (CustomerInactive customer.Id)
    elif List.isEmpty requestedItems then 
        Error EmptyOrder
    else 
        Ok ()

// 🎯 Compose pure functions for complex workflows
let processOrderWorkflow customer cartItems =
    canPlaceOrder customer cartItems
    |> Result.bind (fun () -> validateStockAvailability cartItems)
    |> Result.bind (fun () -> calculatePricing customer cartItems)
    |> Result.map (createOrder customer)
```

### 🏗️ **Business Error Modeling**
```fsharp
// 🚨 Domain-specific errors (not technical errors)
type BusinessError =
    | CustomerInactive of CustomerId
    | InsufficientStock of ProductId * requested: int * available: int
    | EmptyOrder
    | InvalidDiscount of reason: string
    | PaymentDeclined of reason: string
    | OrderAlreadyShipped of OrderId
    | PriceChanged of ProductId * oldPrice: decimal * newPrice: decimal

// 📊 Business validation results
type BusinessResult<'T> = Result<'T, BusinessError>
```

---

## 🛒 Order Management Workflows

### 🎯 **OrderService.fs** - Core Order Logic

```fsharp
module OrderService

open Models
open CustomerRepository
open ProductRepository

// 📋 Order creation workflow
type CreateOrderRequest = {
    CustomerId: CustomerId
    Items: (ProductId * int) list  // (productId, quantity)
    PaymentMethod: PaymentMethod
    ShippingAddress: Address option
}

// ✅ Step 1: Validate order request
let validateOrderRequest (request: CreateOrderRequest) : Async<BusinessResult<CreateOrderRequest>> =
    async {
        // Business rule: minimum order value
        if List.isEmpty request.Items then
            return Error EmptyOrder
        
        // Business rule: maximum items per order  
        elif request.Items |> List.sumBy snd > 50 then
            return Error (InvalidOrder "Tối đa 50 items per order")
        
        else
            return Ok request
    }

// 🔍 Step 2: Validate stock availability
let validateStockAvailability (items: (ProductId * int) list) : Async<BusinessResult<(Product * int) list>> =
    async {
        let results = ResizeArray<BusinessResult<Product * int>>()
        
        for (productId, quantity) in items do
            match! ProductRepository.findProduct productId with
            | Ok (Some product) ->
                if product.Stock >= quantity then
                    results.Add(Ok (product, quantity))
                else
                    results.Add(Error (InsufficientStock (productId, quantity, product.Stock)))
            | Ok None ->
                results.Add(Error (ProductNotFound productId))
            | Error dataError ->
                results.Add(Error (SystemError dataError))
        
        // Collect all errors or return all successes
        let errors = results |> Seq.choose (function Error e -> Some e | Ok _ -> None) |> Seq.toList
        let successes = results |> Seq.choose (function Ok p -> Some p | Error _ -> None) |> Seq.toList
        
        if not errors.IsEmpty then
            return Error (MultipleErrors errors)
        else
            return Ok successes
    }

// 💰 Step 3: Calculate pricing với business rules
let calculateOrderPricing (customer: Customer) (items: (Product * int) list) : BusinessResult<OrderPricing> =
    let subtotal = items |> List.sumBy (fun (product, qty) -> product.Price * decimal qty)
    
    // Business rules for discount
    let discount = 
        match customer with
        | c when c.RegisteredAt < DateTime.Now.AddYears(-2) -> subtotal * 0.15m  // 15% VIP
        | c when c.RegisteredAt < DateTime.Now.AddYears(-1) -> subtotal * 0.10m  // 10% loyal
        | _ when subtotal > 1000m -> subtotal * 0.05m  // 5% bulk discount
        | _ -> 0m
    
    // Business rules for shipping
    let shipping = 
        if subtotal > 500m then 0m  // Free shipping over $500
        elif subtotal > 100m then 15m  // $15 shipping
        else 25m  // $25 standard shipping
    
    let tax = (subtotal - discount) * 0.08m  // 8% tax
    let total = subtotal - discount + shipping + tax
    
    Ok {
        Subtotal = subtotal
        Discount = discount
        Shipping = shipping
        Tax = tax
        Total = total
    }

// 🎯 Main order creation workflow
let createOrder (request: CreateOrderRequest) : Async<BusinessResult<Order>> =
    async {
        // Validate customer exists and is active
        match! CustomerRepository.findCustomer request.CustomerId with
        | Error dataError -> return Error (SystemError dataError)
        | Ok None -> return Error (CustomerNotFound request.CustomerId)
        | Ok (Some customer) ->
            
            // Sequential workflow with early termination on errors
            match! validateOrderRequest request with
            | Error businessError -> return Error businessError
            | Ok validRequest ->
                
                match! validateStockAvailability validRequest.Items with
                | Error businessError -> return Error businessError  
                | Ok validatedItems ->
                    
                    match calculateOrderPricing customer validatedItems with
                    | Error businessError -> return Error businessError
                    | Ok pricing ->
                        
                        // Create order domain object
                        let orderId = OrderId (Guid.NewGuid().ToString())
                        let orderItems = validatedItems |> List.map (fun (product, qty) -> {
                            ProductId = product.Id
                            ProductName = product.Name
                            Quantity = qty
                            UnitPrice = product.Price
                            Subtotal = product.Price * decimal qty
                        })
                        
                        let order = {
                            Id = orderId
                            CustomerId = customer.Id
                            Items = orderItems
                            Status = Pending
                            PaymentMethod = request.PaymentMethod
                            OrderDate = DateTime.Now
                            Notes = None
                            Pricing = pricing
                        }
                        
                        return Ok order
    }
```

### 🔄 **Order State Management**

```fsharp
// 🎭 Order state transitions với business rules
module OrderStateMachine =
    
    type OrderTransition =
        | SubmitPayment of PaymentDetails
        | ConfirmPayment
        | StartProcessing
        | ShipOrder of trackingNumber: string
        | DeliverOrder  
        | CancelOrder of reason: string
        | RefundOrder of amount: decimal
    
    // ✅ Valid state transitions
    let applyTransition (order: Order) (transition: OrderTransition) : BusinessResult<Order> =
        match order.Status, transition with
        
        // From Pending
        | Pending, SubmitPayment paymentDetails ->
            Ok { order with Status = PaymentProcessing paymentDetails }
        | Pending, CancelOrder reason ->
            Ok { order with Status = Cancelled (reason, DateTime.Now) }
        
        // From PaymentProcessing  
        | PaymentProcessing _, ConfirmPayment ->
            Ok { order with Status = PaymentConfirmed DateTime.Now }
        | PaymentProcessing _, CancelOrder reason ->
            Ok { order with Status = Cancelled (reason, DateTime.Now) }
        
        // From PaymentConfirmed
        | PaymentConfirmed _, StartProcessing ->
            Ok { order with Status = Processing DateTime.Now }
        | PaymentConfirmed _, CancelOrder reason ->
            // Can still cancel before processing
            Ok { order with Status = Cancelled (reason, DateTime.Now) }
        
        // From Processing
        | Processing _, ShipOrder tracking ->
            Ok { order with Status = Shipped (tracking, DateTime.Now) }
        
        // From Shipped
        | Shipped (tracking, shippedDate), DeliverOrder ->
            Ok { order with Status = Delivered (tracking, shippedDate, DateTime.Now) }
        
        // From any state (except Delivered)
        | Delivered _, _ ->
            Error (OrderAlreadyCompleted order.Id)
        
        // Invalid transitions
        | currentStatus, invalidTransition ->
            Error (InvalidOrderTransition (currentStatus, invalidTransition))
    
    // 🎯 Business workflow for order processing
    let processOrderPayment orderId paymentDetails : Async<BusinessResult<Order>> =
        async {
            match! OrderRepository.findOrder orderId with
            | Error dataError -> return Error (SystemError dataError)
            | Ok None -> return Error (OrderNotFound orderId)
            | Ok (Some order) ->
                
                match applyTransition order (SubmitPayment paymentDetails) with
                | Error businessError -> return Error businessError
                | Ok updatedOrder ->
                    
                    // Simulate payment processing
                    let! paymentResult = PaymentService.processPayment paymentDetails updatedOrder.Pricing.Total
                    
                    match paymentResult with
                    | Ok paymentConfirmation ->
                        match applyTransition updatedOrder ConfirmPayment with
                        | Ok confirmedOrder -> 
                            return! OrderRepository.updateOrder confirmedOrder
                        | Error err -> return Error err
                    
                    | Error paymentError ->
                        let cancelledOrder = applyTransition updatedOrder (CancelOrder "Payment failed")
                        match cancelledOrder with
                        | Ok cancelled -> 
                            let! _ = OrderRepository.updateOrder cancelled
                            return Error (PaymentDeclined paymentError)
                        | Error _ -> return Error (PaymentDeclined paymentError)
        }
```

---

## 🎯 Customer Business Services

### 👤 **CustomerService.fs** - Customer Management Logic

```fsharp
module CustomerService

// 📊 Customer analytics và business intelligence
type CustomerAnalytics = {
    TotalOrders: int
    TotalSpent: decimal
    AverageOrderValue: decimal
    CustomerTier: CustomerTier
    LastOrderDate: DateTime option
    FavoriteCategories: ProductCategory list
}

and CustomerTier = 
    | Bronze
    | Silver  
    | Gold
    | Platinum

// 📈 Calculate customer tier based on business rules
let calculateCustomerTier (analytics: CustomerAnalytics) : CustomerTier =
    match analytics.TotalSpent, analytics.TotalOrders with
    | spent, orders when spent >= 10000m && orders >= 50 -> Platinum
    | spent, orders when spent >= 5000m && orders >= 25 -> Gold
    | spent, orders when spent >= 1000m && orders >= 10 -> Silver
    | _, _ -> Bronze

// 🎯 Customer registration workflow với validation
let registerNewCustomer (registrationData: CustomerRegistration) : Async<BusinessResult<Customer>> =
    async {
        // Business validation
        let validationErrors = ResizeArray<ValidationError>()
        
        // Email validation
        if not (registrationData.Email.Contains("@")) then
            validationErrors.Add(InvalidEmail registrationData.Email)
        
        // Name validation  
        if String.IsNullOrWhiteSpace(registrationData.Name) then
            validationErrors.Add(EmptyField "Name")
        
        // Phone validation (if provided)
        registrationData.Phone |> Option.iter (fun phone ->
            if phone.Length < 10 then
                validationErrors.Add(InvalidPhone phone))
        
        if validationErrors.Count > 0 then
            return Error (ValidationErrors (validationErrors |> Seq.toList))
        
        else
            // Check for existing customer với same email
            match! CustomerRepository.findByEmail registrationData.Email with
            | Error dataError -> return Error (SystemError dataError)
            | Ok (Some existingCustomer) -> 
                return Error (DuplicateCustomer registrationData.Email)
            | Ok None ->
                
                // Create new customer
                let customerId = CustomerId (Guid.NewGuid().ToString())
                let customer = {
                    Id = customerId
                    Name = registrationData.Name
                    Email = registrationData.Email
                    Phone = registrationData.Phone
                    Address = registrationData.Address
                    RegisteredAt = DateTime.Now
                    IsActive = true
                }
                
                match! CustomerRepository.addCustomer customer with
                | Ok savedCustomer -> return Ok savedCustomer
                | Error dataError -> return Error (SystemError dataError)
    }

// 📊 Generate customer analytics
let generateCustomerAnalytics (customerId: CustomerId) : Async<BusinessResult<CustomerAnalytics>> =
    async {
        match! OrderRepository.getCustomerOrders customerId with
        | Error dataError -> return Error (SystemError dataError)
        | Ok orders ->
            
            let completedOrders = orders |> List.filter (fun o -> 
                match o.Status with
                | Delivered _ -> true
                | _ -> false)
            
            let totalSpent = completedOrders |> List.sumBy (fun o -> o.Pricing.Total)
            let averageOrderValue = 
                if completedOrders.IsEmpty then 0m
                else totalSpent / decimal completedOrders.Length
            
            let lastOrderDate = 
                completedOrders 
                |> List.map (fun o -> o.OrderDate)
                |> List.tryHead
            
            // Find favorite categories
            let categoryFrequency = 
                completedOrders
                |> List.collect (fun o -> o.Items)
                |> List.groupBy (fun item -> item.ProductCategory)
                |> List.map (fun (category, items) -> category, items.Length)
                |> List.sortByDescending snd
                |> List.map fst
                |> List.take 3
            
            let analytics = {
                TotalOrders = completedOrders.Length
                TotalSpent = totalSpent
                AverageOrderValue = averageOrderValue
                CustomerTier = Bronze // Will be calculated
                LastOrderDate = lastOrderDate
                FavoriteCategories = categoryFrequency
            }
            
            let finalAnalytics = { analytics with CustomerTier = calculateCustomerTier analytics }
            
            return Ok finalAnalytics
    }
```

---

## 🎪 Product Business Services

### 🛍️ **ProductService.fs** - Product Management Logic

```fsharp
module ProductService

// 📈 Inventory management với business rules
let updateProductStock (productId: ProductId) (stockChange: int) (reason: string) : Async<BusinessResult<Product>> =
    async {
        match! ProductRepository.findProduct productId with
        | Error dataError -> return Error (SystemError dataError)
        | Ok None -> return Error (ProductNotFound productId)
        | Ok (Some product) ->
            
            let newStock = product.Stock + stockChange
            
            // Business rule: stock cannot go negative
            if newStock < 0 then
                return Error (InsufficientStock (productId, abs stockChange, product.Stock))
            
            // Business rule: low stock warning
            elif newStock <= 5 && newStock > 0 then
                // Log low stock warning
                let! _ = LoggingService.logWarning $"Low stock for {product.Name}: {newStock} remaining"
                let updatedProduct = { product with Stock = newStock; UpdatedAt = Some DateTime.Now }
                return! ProductRepository.updateProduct updatedProduct
            
            else
                let updatedProduct = { product with Stock = newStock; UpdatedAt = Some DateTime.Now }
                return! ProductRepository.updateProduct updatedProduct
    }

// 🎯 Product recommendation engine
let getRecommendedProducts (customerId: CustomerId) : Async<BusinessResult<Product list>> =
    async {
        match! CustomerService.generateCustomerAnalytics customerId with
        | Error businessError -> return Error businessError
        | Ok analytics ->
            
            // Business logic: recommend based on favorite categories
            let recommendationTasks = 
                analytics.FavoriteCategories
                |> List.map (ProductRepository.findProductsByCategory)
            
            let! categoryResults = 
                recommendationTasks 
                |> Async.Parallel
            
            let recommendations = 
                categoryResults
                |> Array.collect (function Ok products -> Array.ofList products | Error _ -> [||])
                |> Array.distinctBy (fun p -> p.Id)
                |> Array.sortByDescending (fun p -> p.Price)  // Sort by price desc
                |> Array.take 10
                |> Array.toList
            
            return Ok recommendations
    }

// 🔍 Advanced product search với business logic
let searchProductsAdvanced (searchCriteria: ProductSearchCriteria) : Async<BusinessResult<ProductSearchResult>> =
    async {
        match! ProductRepository.loadProducts() with
        | Error dataError -> return Error (SystemError dataError)
        | Ok allProducts ->
            
            let filtered = 
                allProducts
                |> List.filter (fun p ->
                    // Text search
                    let matchesText = 
                        searchCriteria.SearchTerm
                        |> Option.map (fun term -> 
                            let termLower = term.ToLower()
                            p.Name.ToLower().Contains(termLower) ||
                            (p.Description |> Option.map (fun d -> d.ToLower().Contains(termLower)) |> Option.defaultValue false))
                        |> Option.defaultValue true
                    
                    // Category filter
                    let matchesCategory = 
                        searchCriteria.Category
                        |> Option.map (fun cat -> p.Category = cat)
                        |> Option.defaultValue true
                    
                    // Price range filter
                    let matchesPrice = 
                        let withinMin = searchCriteria.MinPrice |> Option.map (fun min -> p.Price >= min) |> Option.defaultValue true
                        let withinMax = searchCriteria.MaxPrice |> Option.map (fun max -> p.Price <= max) |> Option.defaultValue true
                        withinMin && withinMax
                    
                    // Stock filter
                    let inStock = 
                        if searchCriteria.InStockOnly then p.Stock > 0 else true
                    
                    matchesText && matchesCategory && matchesPrice && inStock)
            
            // Apply sorting
            let sorted = 
                match searchCriteria.SortBy with
                | Some PriceAsc -> filtered |> List.sortBy (fun p -> p.Price)
                | Some PriceDesc -> filtered |> List.sortByDescending (fun p -> p.Price)
                | Some NameAsc -> filtered |> List.sortBy (fun p -> p.Name)
                | Some Relevance | None -> filtered  // Default order
            
            let result = {
                Products = sorted
                TotalFound = sorted.Length
                SearchCriteria = searchCriteria
                SearchedAt = DateTime.Now
            }
            
            return Ok result
    }
```

---

## 🎯 Business Rules Engine

### ⚖️ **BusinessRules.fs** - Centralized Business Logic

```fsharp
module BusinessRules

// 🎯 Centralized discount calculation
module DiscountRules =
    
    let calculateCustomerDiscount (customer: Customer) (orderTotal: decimal) : decimal =
        let membershipDays = (DateTime.Now - customer.RegisteredAt).Days
        
        match membershipDays, orderTotal with
        | days, total when days >= 730 && total >= 1000m -> total * 0.20m  // 20% VIP + bulk
        | days, total when days >= 730 -> total * 0.15m                    // 15% VIP
        | days, total when days >= 365 && total >= 500m -> total * 0.12m   // 12% loyal + bulk  
        | days, total when days >= 365 -> total * 0.10m                    // 10% loyal
        | _, total when total >= 1000m -> total * 0.05m                    // 5% bulk only
        | _, _ -> 0m
    
    let calculateSeasonalDiscount (orderDate: DateTime) : decimal =
        match orderDate.Month with
        | 11 | 12 -> 0.10m  // 10% holiday season
        | 6 | 7 | 8 -> 0.05m  // 5% summer sale
        | _ -> 0m

// 📦 Shipping calculation rules
module ShippingRules =
    
    let calculateShippingCost (orderTotal: decimal) (destination: Address) : decimal =
        let baseCost = 
            match destination.Country.ToLower() with
            | "vietnam" | "vn" -> 25000m  // VND
            | _ -> 50m  // USD for international
        
        // Free shipping rules
        match orderTotal with
        | total when total >= 1000m -> 0m  // Free over $1000
        | total when total >= 500m -> baseCost * 0.5m  // 50% off over $500
        | _ -> baseCost

// 🔒 Inventory rules
module InventoryRules =
    
    let canReserveStock (product: Product) (requestedQuantity: int) : BusinessResult<unit> =
        if product.Stock >= requestedQuantity then
            Ok ()
        else
            Error (InsufficientStock (product.Id, requestedQuantity, product.Stock))
    
    let calculateReorderPoint (product: Product) (salesHistory: int list) : int =
        let averageMonthlySales = 
            if salesHistory.IsEmpty then 5
            else salesHistory |> List.average |> int
        
        // Safety stock = 2 weeks of average sales
        averageMonthlySales / 2

// ✅ Order validation rules
module OrderValidationRules =
    
    let validateOrderLimits (customer: Customer) (orderValue: decimal) : BusinessResult<unit> =
        // New customers have order limits
        let daysSinceRegistration = (DateTime.Now - customer.RegisteredAt).Days
        
        match daysSinceRegistration, orderValue with
        | days, value when days <= 30 && value > 5000m ->
            Error (OrderLimitExceeded (customer.Id, value, 5000m))
        | days, value when days <= 7 && value > 1000m ->
            Error (OrderLimitExceeded (customer.Id, value, 1000m))
        | _, _ -> Ok ()
    
    let validatePaymentMethod (paymentMethod: PaymentMethod) (orderValue: decimal) : BusinessResult<unit> =
        match paymentMethod, orderValue with
        | Cash, value when value > 500m ->
            Error (PaymentMethodNotAllowed "Cash payments limited to $500")
        | BankTransfer _, value when value < 100m ->
            Error (PaymentMethodNotAllowed "Bank transfers minimum $100")
        | _, _ -> Ok ()
```

---

## 🧪 Testing Business Logic

### ✅ **Business Logic Tests**

```fsharp
module BusinessLogicTests

open Xunit
open BusinessRules

// 🎯 Test discount calculations
[<Fact>]
let ``VIP customer should get 15% discount`` () =
    // Arrange
    let vipCustomer = {
        Id = CustomerId "vip123"
        Name = "VIP Customer"
        Email = "vip@test.com"
        RegisteredAt = DateTime.Now.AddYears(-3)  // 3 years ago
        // ... other properties
    }
    
    // Act  
    let discount = DiscountRules.calculateCustomerDiscount vipCustomer 1000m
    
    // Assert
    Assert.Equal(150m, discount)  // 15% of 1000

[<Theory>]
[<InlineData(11, 0.10)>]  // November - holiday season
[<InlineData(6, 0.05)>]   // June - summer sale  
[<InlineData(3, 0.0)>]    // March - no seasonal discount
let ``Seasonal discount should be calculated correctly`` (month: int) (expectedRate: decimal) =
    // Arrange
    let testDate = DateTime(2024, month, 15)
    
    // Act
    let discount = DiscountRules.calculateSeasonalDiscount testDate
    
    // Assert
    Assert.Equal(expectedRate, discount)

// 🔒 Test business rule validation
[<Fact>]
let ``New customer should not place large orders`` () =
    // Arrange
    let newCustomer = {
        Id = CustomerId "new123"
        RegisteredAt = DateTime.Now.AddDays(-5)  // 5 days ago
        // ... other properties
    }
    
    // Act
    let result = OrderValidationRules.validateOrderLimits newCustomer 2000m
    
    // Assert
    match result with
    | Error (OrderLimitExceeded _) -> ()  // Expected
    | _ -> Assert.Fail("Should reject large order for new customer")

// 📦 Test inventory rules
[<Theory>]
[<InlineData(10, 5, true)>]   // Sufficient stock
[<InlineData(3, 5, false)>]   // Insufficient stock  
let ``Stock reservation should validate availability`` (availableStock: int) (requestedQty: int) (shouldSucceed: bool) =
    // Arrange
    let product = { 
        Id = ProductId "prod123"
        Stock = availableStock
        // ... other properties 
    }
    
    // Act
    let result = InventoryRules.canReserveStock product requestedQty
    
    // Assert
    match result, shouldSucceed with
    | Ok _, true -> ()  // Expected success
    | Error _, false -> ()  // Expected failure
    | _ -> Assert.Fail($"Test failed: expected {shouldSucceed}")
```

---

## 🎯 Best Practices Summary

### ✅ **Business Logic Best Practices**

```fsharp
// ✅ Pure functions for business rules
let calculatePrice customer product quantity = (* pure calculation *)

// ✅ Explicit error modeling
type BusinessError = | CustomerInactive | InsufficientStock | (* ... *)

// ✅ Composition over inheritance
let processOrder = validateOrder >> calculatePricing >> createOrder

// ✅ Centralized business rules
module BusinessRules = 
    let canPlaceOrder customer order = (* centralized logic *)

// ✅ Type-safe state machines
type OrderStatus = | Pending | Processing | Shipped | Delivered

// ✅ Comprehensive testing
[<Fact>] let ``Should calculate VIP discount correctly`` () = (* test *)
```

**Part 4 Summary:** Business logic layer với **pure functions**, **domain rules**, và **comprehensive validation**! 

**Ready cho Part 5: Application Orchestration?** 🚀

---

# 🚀 Part 5: Application Orchestration {#part5-application}

Application Orchestration layer là **conductor** của toàn bộ hệ thống - nơi kết hợp **data access**, **business logic**, và **external services** thành các **use cases** hoàn chỉnh. Trong F#, chúng ta sử dụng **function composition**, **dependency injection**, và **workflow orchestration**.

## 🎯 Application Layer Philosophy

### 💡 **Use Cases as Functions**
```fsharp
// 🎯 Use case = Pure function signature
type UseCase<'Input, 'Output> = 'Input -> Async<Result<'Output, ApplicationError>>

// ✅ Each use case is a composable function
let registerCustomerUseCase : UseCase<CustomerRegistration, Customer>
let createOrderUseCase : UseCase<CreateOrderRequest, Order>  
let processPaymentUseCase : UseCase<PaymentRequest, PaymentConfirmation>

// 🔄 Compose use cases into workflows
let completeOrderWorkflow orderRequest =
    createOrderUseCase orderRequest
    |> AsyncResult.bind processPaymentUseCase
    |> AsyncResult.bind sendOrderConfirmationUseCase
```

### 🏗️ **Application Error Modeling**
```fsharp
// 🚨 Application-level errors (wrapping all lower layers)
type ApplicationError =
    | BusinessError of BusinessError      // From business layer
    | DataError of ShopError             // From data layer  
    | ValidationError of ValidationError list
    | ExternalServiceError of string     // From 3rd party APIs
    | SystemError of exn                 // Unexpected system errors
    
// 📊 Application result type
type ApplicationResult<'T> = Result<'T, ApplicationError>
type AsyncApplicationResult<'T> = Async<ApplicationResult<'T>>
```

---

## 🎪 Use Cases Implementation

### 🛒 **OrderUseCases.fs** - Complete Order Management

```fsharp
module OrderUseCases

open Models
open OrderService
open CustomerService  
open ProductService

// 📋 Use case input models (DTOs from external world)
type CreateOrderRequest = {
    CustomerId: string              // String from API/UI
    Items: CreateOrderItem list
    PaymentMethod: string           // "CreditCard", "PayPal", etc.
    ShippingAddressId: string option
    Notes: string option
}

and CreateOrderItem = {
    ProductId: string
    Quantity: int
}

// 🎯 Create Order Use Case - Complete workflow
let createOrderUseCase (request: CreateOrderRequest) : AsyncApplicationResult<Order> =
    async {
        try
            // Step 1: Convert and validate input
            let customerId = CustomerId request.CustomerId
            let productRequests = request.Items |> List.map (fun item -> 
                (ProductId item.ProductId, item.Quantity))
            
            // Parse payment method
            let paymentMethodResult = 
                match request.PaymentMethod.ToLower() with
                | "cash" -> Ok Cash
                | "creditcard" -> Ok (CreditCard ("****-****-****-1234", DateTime.Now.AddYears(2)))
                | "paypal" -> Ok (PayPal "user@example.com")  // Would get from user context
                | other -> Error (ValidationError [InvalidPaymentMethod other])
            
            match paymentMethodResult with
            | Error appError -> return Error appError
            | Ok paymentMethod ->
                
                // Step 2: Execute business workflow
                let businessRequest = {
                    CustomerId = customerId
                    Items = productRequests  
                    PaymentMethod = paymentMethod
                    ShippingAddress = None  // Load separately if needed
                }
                
                match! OrderService.createOrder businessRequest with
                | Ok order -> 
                    // Step 3: Persist the order
                    match! OrderRepository.saveOrder order with
                    | Ok savedOrder -> return Ok savedOrder
                    | Error dataError -> return Error (DataError dataError)
                
                | Error businessError -> return Error (BusinessError businessError)
        
        with
        | ex -> return Error (SystemError ex)
    }

// 🔍 Get Order Details Use Case
let getOrderDetailsUseCase (orderId: string) : AsyncApplicationResult<OrderDetails> =
    async {
        try
            let orderIdTyped = OrderId orderId
            
            match! OrderRepository.findOrder orderIdTyped with
            | Error dataError -> return Error (DataError dataError)
            | Ok None -> return Error (BusinessError (OrderNotFound orderIdTyped))
            | Ok (Some order) ->
                
                // Enrich with additional data
                match! CustomerRepository.findCustomer order.CustomerId with
                | Error dataError -> return Error (DataError dataError) 
                | Ok customerOpt ->
                    
                    // Load product details for each order item
                    let productTasks = 
                        order.Items 
                        |> List.map (fun item -> ProductRepository.findProduct item.ProductId)
                    
                    let! productResults = productTasks |> Async.Parallel
                    
                    let products = 
                        productResults 
                        |> Array.choose (function Ok (Some p) -> Some p | _ -> None)
                        |> Array.toList
                    
                    let orderDetails = {
                        Order = order
                        Customer = customerOpt
                        ProductDetails = products
                        LoadedAt = DateTime.Now
                    }
                    
                    return Ok orderDetails
        with
        | ex -> return Error (SystemError ex)
    }

// 📦 Process Order Shipment Use Case
let processOrderShipmentUseCase (orderId: string) (trackingNumber: string) : AsyncApplicationResult<Order> =
    async {
        try
            let orderIdTyped = OrderId orderId
            
            match! OrderRepository.findOrder orderIdTyped with
            | Error dataError -> return Error (DataError dataError)
            | Ok None -> return Error (BusinessError (OrderNotFound orderIdTyped))  
            | Ok (Some order) ->
                
                // Business logic: validate order can be shipped
                match order.Status with
                | Processing _ ->
                    let shippedOrder = { order with Status = Shipped (trackingNumber, DateTime.Now) }
                    
                    match! OrderRepository.updateOrder shippedOrder with
                    | Ok updated -> 
                        // Send notification (fire and forget)
                        let! _ = NotificationService.sendShippingNotification updated |> Async.StartChild
                        return Ok updated
                    | Error dataError -> return Error (DataError dataError)
                
                | currentStatus -> 
                    return Error (BusinessError (InvalidOrderTransition (currentStatus, "Ship")))
        with
        | ex -> return Error (SystemError ex)
    }
```

### 👤 **CustomerUseCases.fs** - Customer Management Workflows

```fsharp
module CustomerUseCases

// 📝 Customer registration workflow
let registerCustomerUseCase (registration: CustomerRegistrationRequest) : AsyncApplicationResult<CustomerRegistrationResult> =
    async {
        try
            // Step 1: Input validation
            let validationErrors = ResizeArray<ValidationError>()
            
            if String.IsNullOrWhiteSpace(registration.Name) then
                validationErrors.Add(EmptyField "Name")
            
            if not (registration.Email.Contains("@")) then
                validationErrors.Add(InvalidEmail registration.Email)
                
            registration.Phone |> Option.iter (fun phone ->
                if phone.Length < 10 then
                    validationErrors.Add(InvalidPhone phone))
            
            if validationErrors.Count > 0 then
                return Error (ValidationError (validationErrors |> Seq.toList))
            
            else
                // Step 2: Business logic execution
                let businessRegistration = {
                    Name = registration.Name.Trim()
                    Email = registration.Email.Trim().ToLower()
                    Phone = registration.Phone |> Option.map (fun p -> p.Trim())
                    Address = registration.Address
                }
                
                match! CustomerService.registerNewCustomer businessRegistration with
                | Ok customer ->
                    // Step 3: Send welcome email (async, don't wait)
                    let! _ = EmailService.sendWelcomeEmail customer |> Async.StartChild
                    
                    let result = {
                        Customer = customer
                        RegistrationDate = DateTime.Now
                        WelcomeEmailSent = true
                    }
                    
                    return Ok result
                
                | Error businessError -> return Error (BusinessError businessError)
        
        with
        | ex -> return Error (SystemError ex)
    }

// 📊 Get Customer Dashboard Use Case
let getCustomerDashboardUseCase (customerId: string) : AsyncApplicationResult<CustomerDashboard> =
    async {
        try
            let customerIdTyped = CustomerId customerId
            
            // Load data in parallel
            let! customerTask = CustomerRepository.findCustomer customerIdTyped |> Async.StartChild
            let! ordersTask = OrderRepository.getCustomerOrders customerIdTyped |> Async.StartChild  
            let! analyticsTask = CustomerService.generateCustomerAnalytics customerIdTyped |> Async.StartChild
            
            // Wait for all results
            let! customerResult = customerTask
            let! ordersResult = ordersTask
            let! analyticsResult = analyticsTask
            
            match customerResult, ordersResult, analyticsResult with
            | Ok (Some customer), Ok orders, Ok analytics ->
                
                // Get recommended products based on customer history
                match! ProductService.getRecommendedProducts customerIdTyped with
                | Ok recommendations ->
                    let dashboard = {
                        Customer = customer
                        RecentOrders = orders |> List.take (min 5 orders.Length)
                        Analytics = analytics
                        Recommendations = recommendations |> List.take 8
                        GeneratedAt = DateTime.Now
                    }
                    return Ok dashboard
                
                | Error businessError -> return Error (BusinessError businessError)
            
            | Ok None, _, _ -> return Error (BusinessError (CustomerNotFound customerIdTyped))
            | Error dataError, _, _ -> return Error (DataError dataError)
            | _, Error dataError, _ -> return Error (DataError dataError)
            | _, _, Error businessError -> return Error (BusinessError businessError)
        
        with
        | ex -> return Error (SystemError ex)
    }
```

---

## 🔄 Workflow Orchestration Patterns

### ⚡ **AsyncResult Computation Expression**

```fsharp
// 🎯 AsyncResult builder for clean workflow composition
module AsyncResult =
    
    let bind (f: 'T -> Async<Result<'U, 'E>>) (asyncResult: Async<Result<'T, 'E>>) : Async<Result<'U, 'E>> =
        async {
            match! asyncResult with
            | Ok value -> return! f value
            | Error error -> return Error error
        }
    
    let map (f: 'T -> 'U) (asyncResult: Async<Result<'T, 'E>>) : Async<Result<'U, 'E>> =
        async {
            match! asyncResult with
            | Ok value -> return Ok (f value)
            | Error error -> return Error error
        }
    
    let mapError (f: 'E -> 'F) (asyncResult: Async<Result<'T, 'E>>) : Async<Result<'T, 'F>> =
        async {
            match! asyncResult with
            | Ok value -> return Ok value
            | Error error -> return Error (f error)
        }

// 🎭 Computation expression for clean syntax
type AsyncResultBuilder() =
    member __.Bind(x, f) = AsyncResult.bind f x
    member __.Return(x) = async { return Ok x }
    member __.ReturnFrom(x) = x
    member __.Zero() = async { return Ok () }

let asyncResult = AsyncResultBuilder()

// ✅ Usage: Clean workflow syntax
let complexOrderWorkflow (request: CreateOrderRequest) : AsyncApplicationResult<OrderConfirmation> =
    asyncResult {
        // Each step automatically handles errors
        let! validatedRequest = validateOrderRequest request
        let! customer = loadCustomer validatedRequest.CustomerId  
        let! order = createOrder customer validatedRequest
        let! savedOrder = saveOrder order
        let! paymentResult = processPayment savedOrder
        let! notification = sendOrderConfirmation paymentResult
        
        return {
            Order = savedOrder
            PaymentConfirmation = paymentResult
            NotificationSent = notification
            ProcessedAt = DateTime.Now
        }
    }
```

### 🎪 **Saga Pattern for Long-Running Workflows**

```fsharp
// 🎭 Saga for complex multi-step workflows với compensation
module OrderSaga =
    
    type SagaStep<'Input, 'Output> = {
        Execute: 'Input -> AsyncApplicationResult<'Output>
        Compensate: 'Output -> AsyncApplicationResult<unit>
        Name: string
    }
    
    type SagaState = {
        CompletedSteps: (string * obj) list  // Step name and output
        FailedAt: string option
    }
    
    // 🔄 Execute saga với automatic compensation on failure
    let executeSaga (steps: SagaStep<obj, obj> list) (initialInput: obj) : AsyncApplicationResult<obj> =
        async {
            let mutable state = { CompletedSteps = []; FailedAt = None }
            let mutable currentInput = initialInput
            
            try
                // Execute each step
                for step in steps do
                    match! step.Execute currentInput with
                    | Ok output ->
                        state <- { state with CompletedSteps = (step.Name, output) :: state.CompletedSteps }
                        currentInput <- output
                    | Error error ->
                        state <- { state with FailedAt = Some step.Name }
                        
                        // Compensate completed steps in reverse order
                        for (stepName, stepOutput) in state.CompletedSteps do
                            let compensationStep = steps |> List.find (fun s -> s.Name = stepName)
                            let! _ = compensationStep.Compensate stepOutput  // Log but don't fail saga
                            ()
                        
                        return Error error
                
                return Ok currentInput
                
            with
            | ex -> return Error (SystemError ex)
        }
    
    // 🎯 Complete order saga với compensation logic
    let completeOrderSaga = [
        {
            Name = "CreateOrder"
            Execute = fun input -> 
                let request = input :?> CreateOrderRequest
                createOrderUseCase request |> AsyncResult.map box
            Compensate = fun output ->
                let order = output :?> Order
                cancelOrderUseCase order.Id
        }
        
        {
            Name = "ProcessPayment"  
            Execute = fun input ->
                let order = input :?> Order
                processPaymentUseCase order |> AsyncResult.map box
            Compensate = fun output ->
                let payment = output :?> PaymentConfirmation
                refundPaymentUseCase payment.TransactionId
        }
        
        {
            Name = "UpdateInventory"
            Execute = fun input ->
                let payment = input :?> PaymentConfirmation
                updateInventoryUseCase payment.Order |> AsyncResult.map box  
            Compensate = fun output ->
                let inventory = output :?> InventoryUpdate
                restoreInventoryUseCase inventory
        }
        
        {
            Name = "SendConfirmation"
            Execute = fun input ->
                let inventory = input :?> InventoryUpdate
                sendOrderConfirmationUseCase inventory.Order |> AsyncResult.map box
            Compensate = fun output -> 
                async { return Ok () }  // Cannot unsend email
        }
    ]
```

---

## 🏗️ Application Services & Dependency Management

### 🔧 **AppServices.fs** - Service Composition

```fsharp
module AppServices

// 📦 Application service container
type AppServices = {
    // Repositories
    CustomerRepository: CustomerRepository.T
    ProductRepository: ProductRepository.T  
    OrderRepository: OrderRepository.T
    
    // External services
    EmailService: EmailService.T
    PaymentService: PaymentService.T
    NotificationService: NotificationService.T
    
    // Configuration
    Config: AppConfig
    Logger: ILogger
}

// ⚙️ Service factory functions
let createServices (config: AppConfig) (logger: ILogger) : AppServices =
    {
        CustomerRepository = CustomerRepository.create config.ConnectionString
        ProductRepository = ProductRepository.create config.ConnectionString  
        OrderRepository = OrderRepository.create config.ConnectionString
        
        EmailService = EmailService.create config.EmailSettings
        PaymentService = PaymentService.create config.PaymentSettings
        NotificationService = NotificationService.create config.NotificationSettings
        
        Config = config
        Logger = logger
    }

// 🎯 Use case factory với dependency injection
let createUseCases (services: AppServices) = 
    let wrapUseCase name useCase input =
        async {
            services.Logger.Information($"Executing use case: {name}")
            let! result = useCase input
            
            match result with
            | Ok output -> 
                services.Logger.Information($"Use case {name} completed successfully")
                return result
            | Error error ->
                services.Logger.Error($"Use case {name} failed: {error}")
                return result
        }
    
    {|
        // Order use cases
        CreateOrder = wrapUseCase "CreateOrder" (OrderUseCases.createOrderUseCase services)
        GetOrderDetails = wrapUseCase "GetOrderDetails" (OrderUseCases.getOrderDetailsUseCase services)
        ProcessShipment = wrapUseCase "ProcessShipment" (OrderUseCases.processOrderShipmentUseCase services)
        
        // Customer use cases  
        RegisterCustomer = wrapUseCase "RegisterCustomer" (CustomerUseCases.registerCustomerUseCase services)
        GetCustomerDashboard = wrapUseCase "GetCustomerDashboard" (CustomerUseCases.getCustomerDashboardUseCase services)
        
        // Product use cases
        SearchProducts = wrapUseCase "SearchProducts" (ProductUseCases.searchProductsUseCase services)
        GetRecommendations = wrapUseCase "GetRecommendations" (ProductUseCases.getRecommendationsUseCase services)
    |}
```

### 🎯 **ApplicationWorkflows.fs** - High-Level Orchestration

```fsharp
module ApplicationWorkflows

// 🛒 Complete e-commerce workflows
let completeShoppingWorkflow (services: AppServices) = 
    
    // Customer journey workflow
    let customerJourneyWorkflow customerId = asyncResult {
        let! dashboard = CustomerUseCases.getCustomerDashboardUseCase services customerId
        let! recommendations = ProductUseCases.getRecommendationsUseCase services customerId
        let! cart = ShoppingCartUseCases.getActiveCartUseCase services customerId
        
        return {
            Dashboard = dashboard
            Recommendations = recommendations
            ActiveCart = cart
            GeneratedAt = DateTime.Now
        }
    }
    
    // Order fulfillment workflow
    let orderFulfillmentWorkflow orderId = asyncResult {
        let! order = OrderUseCases.getOrderDetailsUseCase services orderId
        let! shipment = OrderUseCases.processOrderShipmentUseCase services orderId "TRACK123"
        let! notification = NotificationUseCases.sendShippingNotificationUseCase services shipment
        
        return {
            Order = order
            Shipment = shipment
            NotificationSent = notification
            ProcessedAt = DateTime.Now
        }
    }
    
    {|
        CustomerJourney = customerJourneyWorkflow
        OrderFulfillment = orderFulfillmentWorkflow
    |}

// 📊 Analytics và reporting workflows
let analyticsWorkflows (services: AppServices) =
    
    let generateDailyReport date = asyncResult {
        let! orders = OrderRepository.getOrdersByDate services date
        let! revenue = RevenueService.calculateDailyRevenue services orders
        let! customers = CustomerService.getNewCustomers services date
        
        return {
            Date = date
            OrderCount = orders.Length
            Revenue = revenue
            NewCustomers = customers.Length
            GeneratedAt = DateTime.Now
        }
    }
    
    let generateCustomerInsights customerId = asyncResult {
        let! analytics = CustomerService.generateCustomerAnalytics services customerId  
        let! predictions = MLService.predictCustomerBehavior services analytics
        let! recommendations = ProductService.getPersonalizedRecommendations services customerId
        
        return {
            CustomerId = customerId
            Analytics = analytics
            Predictions = predictions
            Recommendations = recommendations
            GeneratedAt = DateTime.Now
        }
    }
    
    {|
        DailyReport = generateDailyReport
        CustomerInsights = generateCustomerInsights
    |}
```

---

## 🚀 Application Startup & Configuration

### ⚙️ **Program.fs** - Application Entry Point

```fsharp
module Program

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open Serilog

// 📄 Configuration setup
let buildConfiguration () =
    ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile("appsettings.Development.json", true, true)
        .AddEnvironmentVariables()
        .Build()

// 📝 Logging setup
let setupLogging (config: IConfiguration) =
    Log.Logger <- 
        LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .Enrich.FromLogContext()
            .CreateLogger()

// 🔧 Service registration
let configureServices (services: IServiceCollection) (config: IConfiguration) =
    let appConfig = config.Get<AppConfig>()
    
    services
        .AddSingleton<AppConfig>(appConfig)
        .AddSingleton<ILogger>(Log.Logger)
        .AddSingleton<AppServices>(fun provider -> 
            let config = provider.GetService<AppConfig>()
            let logger = provider.GetService<ILogger>()
            AppServices.createServices config logger)
        .AddTransient(fun provider ->
            let services = provider.GetService<AppServices>()
            AppServices.createUseCases services)
    |> ignore

// 🚀 Application startup
[<EntryPoint>]
let main argv =
    try
        let config = buildConfiguration()
        setupLogging config
        
        Log.Information("Starting F# Shop Application")
        
        let host = 
            Host.CreateDefaultBuilder(argv)
                .ConfigureServices(fun context services -> 
                    configureServices services config)
                .UseSerilog()
                .Build()
        
        // Run application
        host.RunAsync() |> Async.AwaitTask |> Async.RunSynchronously
        
        Log.Information("Application stopped cleanly")
        0
        
    with
    | ex ->
        Log.Fatal(ex, "Application terminated unexpectedly")
        1
    
    finally
        Log.CloseAndFlush()
```

---

## 🎯 Best Practices & Patterns Summary

### ✅ **Application Layer Best Practices**

```fsharp
// ✅ Use case functions với clear signatures
type UseCase<'Input, 'Output> = 'Input -> AsyncApplicationResult<'Output>

// ✅ Comprehensive error handling
type ApplicationError = BusinessError | DataError | SystemError | ValidationError

// ✅ Async composition với AsyncResult
let workflow = asyncResult {
    let! step1 = executeStep1()
    let! step2 = executeStep2 step1  
    return finalResult
}

// ✅ Dependency injection through function parameters
let createUseCase (services: AppServices) (input: Input) = (* implementation *)

// ✅ Centralized logging và monitoring
let wrapUseCase logger name useCase = (* add logging wrapper *)

// ✅ Configuration-driven behavior
let createServices (config: AppConfig) = (* build from config *)
```

### 🎪 **Orchestration Patterns**

1. **Simple Workflows** - AsyncResult computation expression
2. **Complex Workflows** - Saga pattern với compensation  
3. **Parallel Processing** - Async.Parallel với error collection
4. **Event-Driven** - Fire-and-forget background tasks
5. **Monitoring** - Centralized logging và metrics

---

## 🎓 Part 5 Summary

Bạn đã học cách xây dựng **Application Orchestration Layer** với:

- **Use Cases** as composable functions
- **AsyncResult** workflows cho clean error handling  
- **Saga Pattern** cho complex multi-step processes
- **Dependency Injection** functional style
- **Application startup** và configuration management

**🎉 Chúc mừng!** Bạn đã hoàn thành 3 phần core của F# application architecture. Các layers này tạo nên foundation vững chắc cho bất kỳ F# application nào.

**Next:** Parts 6-10 sẽ deep dive vào các advanced topics như **Async Programming**, **Error Handling**, **Function Composition**, **Testing**, và **Performance**! 🚀

---

# ⚡ Part 6: Async Programming {#part6-async}

Async Programming trong F# là **first-class citizen** với **computation expressions**, **parallel processing**, và **performance optimization**. F# async có syntax sạch hơn C# và built-in support cho functional composition.

## 🎯 F# Async vs. C# Task

### 💡 **Fundamental Differences**

```fsharp
// ❌ C# Task approach - imperative
public async Task<Customer> GetCustomerAsync(int id)
{
    var customer = await _repository.GetByIdAsync(id);
    if (customer == null) 
        throw new NotFoundException($"Customer {id}");
    
    var orders = await _orderService.GetOrdersAsync(customer.Id);
    customer.Orders = orders;
    return customer;
}

// ✅ F# Async approach - functional + composable
let getCustomerAsync (customerId: CustomerId) : Async<Result<CustomerWithOrders, ShopError>> =
    async {
        match! CustomerRepository.findCustomer customerId with
        | Error err -> return Error err
        | Ok None -> return Error (CustomerNotFound customerId)
        | Ok (Some customer) ->
            match! OrderRepository.getCustomerOrders customerId with
            | Error err -> return Error err
            | Ok orders -> 
                return Ok { Customer = customer; Orders = orders }
    }
```

### 🔄 **Async Computation Expression**

```fsharp
// 🎯 Async builder provides clean syntax
let processOrderWorkflow orderId = async {
    let! order = loadOrder orderId              // let! = await + bind
    let! customer = loadCustomer order.CustomerId
    let! payment = processPayment order
    do! sendConfirmation customer payment       // do! = await + ignore result
    return { Order = order; Payment = payment }
}

// 🔧 Manual async without computation expression (verbose)
let processOrderWorkflowManual orderId =
    loadOrder orderId
    |> Async.bind (fun order ->
        loadCustomer order.CustomerId
        |> Async.bind (fun customer ->
            processPayment order  
            |> Async.bind (fun payment ->
                sendConfirmation customer payment
                |> Async.map (fun () -> { Order = order; Payment = payment }))))
```

---

## 🚀 Parallel & Concurrent Patterns

### ⚡ **Parallel Execution**

```fsharp
// 🔥 Parallel async operations
let loadDashboardDataParallel (customerId: CustomerId) : Async<Result<DashboardData, ShopError>> =
    async {
        // Start all operations concurrently
        let! customerTask = CustomerRepository.findCustomer customerId |> Async.StartChild
        let! ordersTask = OrderRepository.getCustomerOrders customerId |> Async.StartChild  
        let! recommendationsTask = ProductService.getRecommendations customerId |> Async.StartChild
        let! analyticsTask = AnalyticsService.getCustomerAnalytics customerId |> Async.StartChild
        
        // Await all results (they run in parallel)
        let! customerResult = customerTask
        let! ordersResult = ordersTask
        let! recommendationsResult = recommendationsTask
        let! analyticsResult = analyticsTask
        
        // Combine results
        return result {
            let! customer = customerResult |> Result.bind (function 
                | Some c -> Ok c 
                | None -> Error (CustomerNotFound customerId))
            let! orders = ordersResult
            let! recommendations = recommendationsResult
            let! analytics = analyticsResult
            
            return {
                Customer = customer
                RecentOrders = orders |> List.take 5
                Recommendations = recommendations
                Analytics = analytics
                LoadedAt = DateTime.Now
            }
        }
    }

// 🎯 Async.Parallel for independent operations
let processMultipleOrders (orderIds: OrderId list) : Async<ProcessOrderResult list> =
    async {
        let processTasks = orderIds |> List.map processOrderAsync
        let! results = processTasks |> Async.Parallel
        return results |> Array.toList
    }

// ⚡ Async.Choice for first-wins scenarios
let getProductFromMultipleSources (productId: ProductId) : Async<Product option> =
    async {
        let sources = [
            ProductRepository.findProduct productId
            ExternalAPI.findProduct productId  
            CacheService.getProduct productId
        ]
        
        // Return first successful result
        let! result = sources |> Async.Choice
        return result
    }
```

### 🔄 **Async Composition Utilities**

```fsharp
module AsyncExtensions =
    
    // Map over async value
    let map (f: 'T -> 'U) (asyncValue: Async<'T>) : Async<'U> =
        async {
            let! value = asyncValue
            return f value
        }
    
    // Bind async computations
    let bind (f: 'T -> Async<'U>) (asyncValue: Async<'T>) : Async<'U> =
        async {
            let! value = asyncValue
            return! f value
        }
    
    // Apply function in async context
    let apply (asyncF: Async<'T -> 'U>) (asyncValue: Async<'T>) : Async<'U> =
        async {
            let! f = asyncF
            let! value = asyncValue
            return f value
        }
    
    // Traverse list với async operations
    let traverse (f: 'T -> Async<'U>) (list: 'T list) : Async<'U list> =
        async {
            let results = ResizeArray<'U>()
            for item in list do
                let! result = f item
                results.Add(result)
            return results |> Seq.toList
        }
    
    // Sequence list of async operations
    let sequence (asyncList: Async<'T> list) : Async<'T list> =
        traverse id asyncList

// 📊 Usage examples
let loadMultipleCustomers customerIds = 
    customerIds 
    |> AsyncExtensions.traverse CustomerRepository.findCustomer
    |> AsyncExtensions.map (List.choose id)  // Filter out None values
```

---

## ⏱️ Timeout & Cancellation

### ⏰ **Timeout Patterns**

```fsharp
// 🚨 Timeout với Async.RunSynchronously
let loadCustomerWithTimeout customerId timeout =
    async {
        try
            return! CustomerRepository.findCustomer customerId
        with
        | :? System.TimeoutException -> return Error (TimeoutError "Customer load timeout")
    }
    |> fun asyncOp -> Async.RunSynchronously(asyncOp, timeout)

// ⏱️ Async.Choice for timeout racing
let loadWithTimeout (asyncOperation: Async<'T>) (timeoutMs: int) : Async<Result<'T, string>> =
    async {
        let! result = 
            Async.Choice [
                asyncOperation |> Async.map Ok
                async {
                    do! Async.Sleep timeoutMs
                    return Error $"Operation timed out after {timeoutMs}ms"
                }
            ]
        return result
    }

// 🎯 Usage với timeout
let safeCustomerLoad customerId = async {
    let! result = loadCustomerWithTimeout (CustomerRepository.findCustomer customerId) 5000
    match result with
    | Ok customer -> return customer
    | Error timeout -> 
        // Fallback to cache
        return! CacheService.getCustomer customerId
}
```

### 🛑 **Cancellation Support**

```fsharp
// 🚫 Cancellation token support
let longRunningOperation (cancellationToken: CancellationToken) = async {
    for i in 1..1000 do
        // Check for cancellation periodically  
        cancellationToken.ThrowIfCancellationRequested()
        
        // Do some work
        do! processItem i
        
        // Cooperative cancellation check
        if cancellationToken.IsCancellationRequested then
            return Error "Operation was cancelled"
    
    return Ok "Completed successfully"
}

// 🎯 Cancellation với timeout
let processWithCancellation operation timeoutMs = async {
    use cts = new CancellationTokenSource(timeoutMs)
    
    try
        let! result = operation cts.Token
        return result
    with
    | :? OperationCanceledException -> return Error "Operation cancelled or timed out"
}
```

---

## 🎪 Advanced Async Patterns

### 🔄 **Producer-Consumer Pattern**

```fsharp
// 📦 Producer-Consumer với MailboxProcessor
type OrderProcessor() =
    
    let processor = MailboxProcessor<Order * AsyncReplyChannel<Result<ProcessedOrder, ShopError>>>.Start(fun inbox ->
        let rec messageLoop() = async {
            let! (order, replyChannel) = inbox.Receive()
            
            try
                // Process order sequentially to avoid conflicts
                let! result = processOrderInternal order
                replyChannel.Reply(Ok result)
            with
            | ex -> replyChannel.Reply(Error (ProcessingError ex.Message))
            
            return! messageLoop()
        }
        messageLoop()
    )
    
    member _.ProcessOrderAsync(order: Order) : Async<Result<ProcessedOrder, ShopError>> =
        processor.PostAndAsyncReply(fun replyChannel -> (order, replyChannel))
    
    interface IDisposable with
        member _.Dispose() = (processor :> IDisposable).Dispose()

// 🎯 Usage
let orderProcessor = new OrderProcessor()

let processMultipleOrdersSafely orders = async {
    let results = ResizeArray<Result<ProcessedOrder, ShopError>>()
    
    for order in orders do
        let! result = orderProcessor.ProcessOrderAsync(order)
        results.Add(result)
    
    return results |> Seq.toList
}
```

### 🌊 **Async Streams & Sequences**

```fsharp
// 🌊 Async sequences for streaming data
let streamCustomerOrders (customerId: CustomerId) : AsyncSeq<Order> = asyncSeq {
    let pageSize = 50
    let mutable page = 0
    let mutable hasMore = true
    
    while hasMore do
        let! orderPage = OrderRepository.getCustomerOrdersPaged customerId page pageSize
        
        match orderPage with
        | Ok orders when not orders.IsEmpty ->
            for order in orders do
                yield order
            page <- page + 1
            hasMore <- orders.Length = pageSize
        | Ok [] | Error _ ->
            hasMore <- false
}

// 🎯 Processing async streams
let processOrderStream customerId = async {
    let orderStream = streamCustomerOrders customerId
    let processedCount = ref 0
    
    do! orderStream 
        |> AsyncSeq.iterAsync (fun order -> async {
            let! result = processOrderAsync order
            incr processedCount
            if !processedCount % 10 = 0 then
                printfn $"Processed {!processedCount} orders"
        })
    
    return !processedCount
}
```

### 🎭 **Async Error Handling Patterns**

```fsharp
// 🛡️ Retry pattern với exponential backoff
let rec asyncRetry (maxAttempts: int) (delay: int) (operation: unit -> Async<Result<'T, 'E>>) : Async<Result<'T, 'E>> =
    async {
        if maxAttempts <= 0 then
            return Error (SystemError "Max retry attempts exceeded")
        else
            match! operation() with
            | Ok result -> return Ok result
            | Error _ when maxAttempts > 1 ->
                do! Async.Sleep delay
                return! asyncRetry (maxAttempts - 1) (delay * 2) operation  // Exponential backoff
            | Error error -> return Error error
    }

// 🔄 Circuit breaker pattern
type CircuitBreakerState = Closed | Open | HalfOpen

type CircuitBreaker<'T, 'E>(threshold: int, timeout: TimeSpan) =
    let mutable failures = 0
    let mutable state = Closed
    let mutable lastFailureTime = DateTime.MinValue
    
    member _.ExecuteAsync(operation: unit -> Async<Result<'T, 'E>>) : Async<Result<'T, 'E>> = async {
        match state with
        | Open when DateTime.Now - lastFailureTime < timeout ->
            return Error (SystemError "Circuit breaker is open")
        
        | Open -> 
            state <- HalfOpen
            match! operation() with
            | Ok result -> 
                failures <- 0
                state <- Closed
                return Ok result
            | Error err ->
                state <- Open
                lastFailureTime <- DateTime.Now
                return Error err
        
        | _ -> // Closed or HalfOpen
            match! operation() with  
            | Ok result ->
                failures <- 0
                if state = HalfOpen then state <- Closed
                return Ok result
            | Error err ->
                failures <- failures + 1
                if failures >= threshold then
                    state <- Open
                    lastFailureTime <- DateTime.Now
                return Error err
    }

// 🎯 Usage
let customerServiceWithCircuitBreaker = new CircuitBreaker<Customer, ShopError>(3, TimeSpan.FromMinutes(1.0))

let safeGetCustomer customerId = 
    customerServiceWithCircuitBreaker.ExecuteAsync(fun () -> CustomerRepository.findCustomer customerId)
```

---

## 📈 Performance Optimization

### ⚡ **Memory & Resource Management**

```fsharp
// 🔋 Resource management với use!
let processLargeFile filePath = async {
    use! fileStream = File.OpenAsync(filePath) |> Async.AwaitTask
    use reader = new StreamReader(fileStream)
    
    let results = ResizeArray<ProcessedLine>()
    let mutable line = ""
    
    while (line <- reader.ReadLine(); line <> null) do
        let! processed = processLineAsync line
        results.Add(processed)
    
    return results |> Seq.toList
}

// 💾 Memory-efficient streaming
let processLargeDataset (data: AsyncSeq<DataItem>) = asyncSeq {
    let batchSize = 100
    let batch = ResizeArray<DataItem>()
    
    do! data |> AsyncSeq.iterAsync (fun item -> async {
        batch.Add(item)
        
        if batch.Count >= batchSize then
            let! processedBatch = processBatch (batch.ToArray())
            for result in processedBatch do
                yield result
            batch.Clear()
    })
    
    // Process remaining items
    if batch.Count > 0 then
        let! finalBatch = processBatch (batch.ToArray())
        for result in finalBatch do
            yield result
}
```

### 🎯 **Async Performance Best Practices**

```fsharp
// ✅ ConfigureAwait equivalent - run on thread pool
let computeIntensiveOperation data = async {
    do! Async.SwitchToThreadPool()  // Move to thread pool
    
    let result = expensiveComputation data
    
    do! Async.SwitchToContext()     // Switch back to original context
    return result
}

// ✅ Batch operations for better throughput
let batchProcessor<'T, 'U> (batchSize: int) (processor: 'T[] -> Async<'U[]>) (items: 'T list) : Async<'U list> =
    async {
        let batches = items |> List.chunkBySize batchSize
        let results = ResizeArray<'U>()
        
        for batch in batches do
            let! batchResult = processor (Array.ofList batch)
            results.AddRange(batchResult)
        
        return results |> Seq.toList
    }

// ✅ Async caching to avoid repeated operations
let memoizeAsync<'T, 'U when 'T : comparison> (f: 'T -> Async<'U>) : ('T -> Async<'U>) =
    let cache = System.Collections.Concurrent.ConcurrentDictionary<'T, Async<'U>>()
    
    fun input ->
        cache.GetOrAdd(input, fun key -> 
            async {
                let! result = f key
                return result
            })

// 🎯 Usage
let cachedCustomerLoad = memoizeAsync CustomerRepository.findCustomer

let loadMultipleCustomersEfficiently customerIds = async {
    let! results = 
        customerIds
        |> batchProcessor 10 (fun batch -> async {
            let tasks = batch |> Array.map cachedCustomerLoad
            return! tasks |> Async.Parallel
        })
    
    return results |> Array.choose id |> Array.toList
}
```

---

## 🧪 Testing Async Code

### ✅ **Async Testing Patterns**

```fsharp
module AsyncTests =
    open Xunit
    open FsUnit.Xunit
    
    // 🎯 Basic async test
    [<Fact>]
    let ``Should load customer async successfully`` () = async {
        // Arrange
        let customerId = CustomerId "test123"
        
        // Act  
        let! result = CustomerRepository.findCustomer customerId
        
        // Assert
        match result with
        | Ok (Some customer) -> 
            customer.Id |> should equal customerId
        | _ -> Assert.Fail("Expected customer to be found")
    }
    
    // ⏱️ Testing với timeout
    [<Fact>]
    let ``Should timeout on slow operation`` () = async {
        // Arrange
        let slowOperation = async {
            do! Async.Sleep 10000  // 10 seconds
            return "completed"
        }
        
        // Act & Assert
        let! result = 
            Async.Choice [
                slowOperation |> Async.map Some
                async {
                    do! Async.Sleep 1000  // 1 second timeout
                    return None
                }
            ]
        
        result |> should equal None
    }
    
    // 🔄 Testing parallel operations
    [<Fact>]  
    let ``Should process multiple items in parallel`` () = async {
        // Arrange
        let items = [1..10]
        let processItem x = async {
            do! Async.Sleep 100  // Simulate work
            return x * 2
        }
        
        let startTime = DateTime.Now
        
        // Act - parallel processing
        let! results = items |> List.map processItem |> Async.Parallel
        
        let duration = DateTime.Now - startTime
        
        // Assert
        results |> should equal [|2; 4; 6; 8; 10; 12; 14; 16; 18; 20|]
        duration.TotalMilliseconds |> should be (lessThan 500.0)  // Should be much faster than sequential
    }
    
    // 🧪 Mock async dependencies
    type MockCustomerRepository() =
        interface ICustomerRepository with
            member _.FindCustomerAsync(customerId) = async {
                if customerId = CustomerId "valid" then
                    return Ok (Some testCustomer)
                else
                    return Ok None
            }
    
    [<Fact>]
    let ``Should handle customer not found`` () = async {
        // Arrange
        let mockRepo = MockCustomerRepository()
        let invalidId = CustomerId "invalid"
        
        // Act
        let! result = mockRepo.FindCustomerAsync(invalidId)
        
        // Assert  
        result |> should equal (Ok None)
    }
```

---

## 🎯 Part 6 Summary

**Async Programming** trong F# cung cấp:

- **Clean Syntax** với computation expressions
- **Composable** async operations với bind/map
- **Parallel Processing** với Async.Parallel và Async.StartChild  
- **Error Handling** integration với Result types
- **Performance Patterns** cho memory efficiency
- **Advanced Patterns** như Circuit Breaker, Retry, Producer-Consumer
- **Testable Code** với async-friendly test patterns

**Key Takeaways:**
- F# async is **more composable** than C# Task
- **let!** cho async binding, **do!** cho async side effects
- **Async.Parallel** cho independent operations
- **Resource management** với use! 
- **Performance optimization** với batching và caching

**Next:** Part 7 sẽ deep dive vào **Error Handling Patterns**! 🚀

---

# 🚂 Part 7: Error Handling Patterns {#part7-errorhandling}

Error Handling trong F# sử dụng **Railway Oriented Programming (ROP)**, **Result types**, và **functional composition** để tạo ra error handling **predictable**, **composable**, và **type-safe**. Không còn exceptions bất ngờ!

## 🎯 Philosophy: Errors as Data

### 💡 **Exceptions vs. Result Types**

```fsharp
// ❌ Exception-based (unpredictable)
let loadCustomer customerId =
    if customerExists customerId then
        getCustomer customerId
    else
        failwith "Customer not found"  // 💥 Exception!

// Có thể ném nhiều loại exceptions:
// - FileNotFoundException
// - NetworkException  
// - DatabaseException
// - ArgumentException
// - NullReferenceException

// ✅ Result-based (predictable)
type CustomerError = 
    | CustomerNotFound of CustomerId
    | DatabaseError of string
    | NetworkError of string  
    | ValidationError of string list

let loadCustomer (customerId: CustomerId) : Result<Customer, CustomerError> =
    if customerExists customerId then
        Ok (getCustomer customerId)
    else
        Error (CustomerNotFound customerId)

// 🎯 Compiler forces you to handle ALL error cases!
match loadCustomer customerId with
| Ok customer -> processCustomer customer
| Error (CustomerNotFound id) -> handleNotFound id
| Error (DatabaseError msg) -> handleDbError msg
| Error (NetworkError msg) -> handleNetworkError msg  
| Error (ValidationError errors) -> handleValidation errors
```

### 🚂 **Railway Oriented Programming**

```fsharp
// 🛤️ Two tracks: Success track & Failure track
//
//     Input ──→ [Function] ──→ Success Output
//                   │
//                   └──→ Error Output
//
// Functions are "railway switches" - success continues on success track,
// errors are routed to error track

// 🎯 Basic railway functions
let bind (f: 'T -> Result<'U, 'E>) (result: Result<'T, 'E>) : Result<'U, 'E> =
    match result with
    | Ok value -> f value           // Continue on success track
    | Error err -> Error err        // Stay on error track

let map (f: 'T -> 'U) (result: Result<'T, 'E>) : Result<'U, 'E> =
    match result with  
    | Ok value -> Ok (f value)      // Transform success value
    | Error err -> Error err        // Pass error through

// 🔧 Railway operators  
let (>>=) result f = bind f result    // Infix bind
let (<!>) f result = map f result     // Infix map

// ✅ Railway composition
let processCustomerWorkflow customerId =
    loadCustomer customerId           // Result<Customer, CustomerError>
    >>= validateCustomer              // Continue only if successful  
    >>= enrichCustomerData            // Chain more operations
    >>= saveCustomer                  // Final save
    // If ANY step fails, error flows through automatically!
```

---

## 🏗️ Hierarchical Error Modeling

### 🎭 **Domain-Specific Error Hierarchies**

```fsharp
// 🎯 Granular error modeling by domain
module ErrorTypes =
    
    // 📊 Validation errors
    type ValidationError =
        | EmptyField of fieldName: string
        | InvalidEmail of email: string  
        | InvalidPhone of phone: string
        | InvalidAge of age: int * reason: string
        | InvalidPrice of price: decimal * reason: string
        | DuplicateValue of fieldName: string * value: string
    
    // 👤 Customer domain errors
    type CustomerError =
        | CustomerNotFound of CustomerId
        | CustomerInactive of CustomerId  
        | CustomerValidationError of ValidationError list
        | CustomerDuplicateEmail of email: string
    
    // 🛍️ Product domain errors  
    type ProductError =
        | ProductNotFound of ProductId
        | ProductOutOfStock of ProductId * requested: int * available: int
        | ProductDiscontinued of ProductId
        | ProductValidationError of ValidationError list
        | ProductPriceChanged of ProductId * oldPrice: decimal * newPrice: decimal
    
    // 🛒 Order domain errors
    type OrderError =
        | OrderNotFound of OrderId
        | OrderAlreadyShipped of OrderId
        | OrderCancelled of OrderId * reason: string
        | InvalidOrderState of OrderId * currentState: string * attemptedAction: string
        | OrderValidationError of ValidationError list
        | InsufficientStock of (ProductId * int * int) list  // productId, requested, available
    
    // 💳 Payment errors
    type PaymentError =
        | PaymentDeclined of reason: string
        | PaymentTimeout of transactionId: string
        | InvalidPaymentMethod of method: string
        | InsufficientFunds of required: decimal * available: decimal
        | PaymentProcessingError of errorCode: string * message: string
    
    // 🌐 Infrastructure errors
    type InfrastructureError =
        | DatabaseError of operation: string * message: string
        | NetworkError of endpoint: string * statusCode: int * message: string
        | FileSystemError of path: string * operation: string * message: string
        | ExternalServiceError of serviceName: string * errorCode: string * message: string
        | ConfigurationError of setting: string * message: string
    
    // 🎪 Application-level error union
    type ShopError =
        | CustomerError of CustomerError
        | ProductError of ProductError  
        | OrderError of OrderError
        | PaymentError of PaymentError
        | InfrastructureError of InfrastructureError
        | ValidationError of ValidationError list
        | BusinessRuleViolation of rule: string * message: string
        | SystemError of exn
```

### 🔄 **Error Conversion & Propagation**

```fsharp
module ErrorHandling =
    
    // 🔄 Convert specific errors to general ShopError
    let mapCustomerError (customerError: CustomerError) : ShopError =
        ShopError.CustomerError customerError
    
    let mapProductError (productError: ProductError) : ShopError =
        ShopError.ProductError productError
    
    let mapOrderError (orderError: OrderError) : ShopError =
        ShopError.OrderError orderError
    
    // 🎯 Result mappers for layer boundaries
    let mapCustomerResult (result: Result<'T, CustomerError>) : Result<'T, ShopError> =
        result |> Result.mapError mapCustomerError
    
    let mapProductResult (result: Result<'T, ProductError>) : Result<'T, ShopError> =
        result |> Result.mapError mapProductError
    
    let mapOrderResult (result: Result<'T, OrderError>) : Result<'T, ShopError> =
        result |> Result.mapError mapOrderError
    
    // 🚂 Cross-domain operations với error mapping
    let processOrderWithCustomerValidation (orderId: OrderId) : Result<ProcessedOrder, ShopError> =
        result {
            let! order = OrderService.loadOrder orderId |> mapOrderResult
            let! customer = CustomerService.loadCustomer order.CustomerId |> mapCustomerResult
            let! validatedCustomer = CustomerService.validateForOrdering customer |> mapCustomerResult
            let! products = ProductService.validateOrderProducts order.Items |> mapProductResult
            
            return {
                Order = order
                ValidatedCustomer = validatedCustomer
                ValidatedProducts = products
                ProcessedAt = DateTime.Now
            }
        }
```

---

## 🔧 Advanced Railway Patterns

### ⚡ **Parallel Railway Processing**

```fsharp
// 🎯 Process multiple operations, collect ALL errors
module ParallelRailway =
    
    // 📊 Applicative Result - accumulate errors
    type ValidationResult<'T> = 
        | Valid of 'T
        | Invalid of ValidationError list
    
    let map f = function
        | Valid value -> Valid (f value)
        | Invalid errors -> Invalid errors
    
    let apply fResult xResult =
        match fResult, xResult with
        | Valid f, Valid x -> Valid (f x)
        | Valid _, Invalid errors -> Invalid errors  
        | Invalid errors, Valid _ -> Invalid errors
        | Invalid errors1, Invalid errors2 -> Invalid (errors1 @ errors2)  // ACCUMULATE!
    
    // 🔧 Operators for applicative style
    let (<!>) = map
    let (<*>) = apply
    
    // 🎯 Validate customer registration (collect all validation errors)
    let validateCustomerRegistration (registration: CustomerRegistration) : ValidationResult<ValidatedCustomer> =
        let validateName name =
            if String.IsNullOrWhiteSpace(name) then 
                Invalid [EmptyField "Name"]
            else Valid name
        
        let validateEmail email =
            if email.Contains("@") && email.Contains(".") then 
                Valid email
            else Invalid [InvalidEmail email]
        
        let validatePhone phone =
            match phone with
            | Some p when p.Length >= 10 -> Valid phone
            | Some p -> Invalid [InvalidPhone p]  
            | None -> Valid phone
        
        let validateAge age =
            if age >= 0 && age <= 150 then 
                Valid age
            else Invalid [InvalidAge (age, "Age must be 0-150")]
        
        // 🚂 Applicative composition - runs ALL validations
        let createValidatedCustomer name email phone age = {
            Name = name; Email = email; Phone = phone; Age = age
        }
        
        createValidatedCustomer
        <!> validateName registration.Name      // Validate name
        <*> validateEmail registration.Email    // Validate email (parallel)  
        <*> validatePhone registration.Phone    // Validate phone (parallel)
        <*> validateAge registration.Age        // Validate age (parallel)
        
        // Kết quả: Valid customer HOẶC Invalid với TẤT CẢ errors được thu thập!

// 🎯 Usage
let registrationResult = ParallelRailway.validateCustomerRegistration {
    Name = ""                    // Error: EmptyField "Name"
    Email = "invalid-email"      // Error: InvalidEmail "invalid-email"  
    Phone = Some "123"           // Error: InvalidPhone "123"
    Age = 200                    // Error: InvalidAge (200, "Age must be 0-150")
}
// Result: Invalid [EmptyField "Name"; InvalidEmail "invalid-email"; InvalidPhone "123"; InvalidAge (200, "Age must be 0-150")]
```

### 🎪 **Advanced Railway Combinators**

```fsharp
module RailwayExtensions =
    
    // 🔄 Try-catch bridge to railway
    let attempt (f: unit -> 'T) : Result<'T, exn> =
        try
            Ok (f())
        with
        | ex -> Error ex
    
    // 🎯 Tee function - side effects on success track
    let tee (f: 'T -> unit) (result: Result<'T, 'E>) : Result<'T, 'E> =
        match result with
        | Ok value -> 
            f value        // Side effect (logging, etc.)
            Ok value       // Continue on success track
        | Error err -> Error err
    
    // 🔧 Double-bind for nested Results
    let bindAsync (f: 'T -> Async<Result<'U, 'E>>) (result: Result<'T, 'E>) : Async<Result<'U, 'E>> =
        async {
            match result with
            | Ok value -> return! f value
            | Error err -> return Error err
        }
    
    // ⚡ Parallel result processing
    let parallelMap (f: 'T -> Async<Result<'U, 'E>>) (items: 'T list) : Async<Result<'U list, 'E>> =
        async {
            let! results = items |> List.map f |> Async.Parallel
            
            let successes = results |> Array.choose (function Ok x -> Some x | Error _ -> None)
            let errors = results |> Array.choose (function Error e -> Some e | Ok _ -> None)
            
            if Array.isEmpty errors then
                return Ok (Array.toList successes)  
            else
                return Error (Array.head errors)  // Return first error
        }
    
    // 🎭 Choice between multiple railway operations  
    let choice (operations: (unit -> Result<'T, 'E>) list) : Result<'T, 'E list> =
        let rec tryNext ops errors =
            match ops with
            | [] -> Error (List.rev errors)
            | op :: rest ->
                match op() with
                | Ok value -> Ok value
                | Error err -> tryNext rest (err :: errors)
        
        tryNext operations []
    
    // 📊 Aggregation with error collection
    let collect (f: 'T -> Result<'U, 'E>) (items: 'T list) : Result<'U list, 'E list> =
        let results = items |> List.map f
        let successes = results |> List.choose (function Ok x -> Some x | Error _ -> None)
        let errors = results |> List.choose (function Error e -> Some e | Ok _ -> None)
        
        if List.isEmpty errors then
            Ok successes
        else
            Error errors

// 🎯 Usage examples
let processOrdersWithLogging orders =
    orders
    |> List.map (fun order -> 
        OrderService.processOrder order
        |> RailwayExtensions.tee (fun processedOrder -> 
            Logger.info $"Processed order {processedOrder.Id}"))
    |> RailwayExtensions.collect id
```

---

## 🛡️ Defensive Programming Patterns

### 🔒 **Input Validation Railway**

```fsharp
module InputValidation =
    
    // 🎯 Smart constructors với validation
    module Email =
        type Email = private Email of string
        
        let create (emailStr: string) : Result<Email, ValidationError> =
            if String.IsNullOrWhiteSpace(emailStr) then
                Error (EmptyField "Email")
            elif not (emailStr.Contains("@") && emailStr.Contains(".")) then
                Error (InvalidEmail emailStr)  
            elif emailStr.Length > 254 then
                Error (InvalidEmail "Email too long")
            else
                Ok (Email emailStr)
        
        let value (Email email) = email
    
    module ProductPrice =
        type ProductPrice = private ProductPrice of decimal
        
        let create (price: decimal) : Result<ProductPrice, ValidationError> =
            if price < 0m then
                Error (InvalidPrice (price, "Price cannot be negative"))
            elif price > 1000000m then  
                Error (InvalidPrice (price, "Price too high"))
            else
                Ok (ProductPrice price)
        
        let value (ProductPrice price) = price
        let add (ProductPrice p1) (ProductPrice p2) = ProductPrice (p1 + p2)
        let multiply (ProductPrice price) factor = ProductPrice (price * factor)
    
    // 🏗️ Validated domain model
    type ValidatedCustomer = {
        Id: CustomerId
        Name: string        // Already validated non-empty
        Email: Email        // Already validated format
        Phone: string option // Already validated format if present
        Age: int           // Already validated range
    }
    
    type ValidatedProduct = {
        Id: ProductId
        Name: string
        Price: ProductPrice  // Already validated positive
        Stock: int          // Already validated non-negative
    }
    
    // 🎯 Validation pipeline
    let validateAndCreateCustomer (raw: RawCustomerData) : Result<ValidatedCustomer, ValidationError list> =
        result {
            let! email = Email.create raw.Email |> Result.mapError List.singleton
            
            let validationErrors = ResizeArray<ValidationError>()
            
            if String.IsNullOrWhiteSpace(raw.Name) then
                validationErrors.Add(EmptyField "Name")
            
            if raw.Age < 0 || raw.Age > 150 then
                validationErrors.Add(InvalidAge (raw.Age, "Must be 0-150"))
            
            raw.Phone |> Option.iter (fun phone ->
                if phone.Length < 10 then
                    validationErrors.Add(InvalidPhone phone))
            
            if validationErrors.Count > 0 then
                return! Error (validationErrors |> Seq.toList)
            else
                return {
                    Id = CustomerId (Guid.NewGuid().ToString())
                    Name = raw.Name.Trim()
                    Email = email
                    Phone = raw.Phone |> Option.map (fun p -> p.Trim())
                    Age = raw.Age
                }
        }
```

### 🎪 **Retry & Circuit Breaker Railway**

```fsharp
module ResiliencePatterns =
    
    // 🔄 Retry with exponential backoff
    let retryWithBackoff<'T, 'E> (maxAttempts: int) (initialDelay: TimeSpan) (operation: unit -> Result<'T, 'E>) : Result<'T, 'E> =
        let rec retry attempt delay =
            match operation() with
            | Ok result -> Ok result
            | Error _ when attempt < maxAttempts ->
                Thread.Sleep(delay)
                retry (attempt + 1) (TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2.0))
            | Error err -> Error err
        
        retry 1 initialDelay
    
    // 🔌 Circuit breaker for external services  
    type CircuitState = Closed | Open of DateTime | HalfOpen
    
    type CircuitBreaker<'T, 'E>(threshold: int, timeout: TimeSpan) =
        let mutable state = Closed
        let mutable failures = 0
        
        member _.Execute(operation: unit -> Result<'T, 'E>) : Result<'T, 'E> =
            match state with
            | Open openTime when DateTime.Now - openTime < timeout ->
                Error (SystemError (Exception("Circuit breaker is open")))
            
            | Open _ -> 
                state <- HalfOpen
                match operation() with
                | Ok result -> 
                    failures <- 0
                    state <- Closed
                    Ok result
                | Error err ->
                    state <- Open DateTime.Now
                    Error err
            
            | Closed | HalfOpen ->
                match operation() with
                | Ok result ->
                    failures <- 0
                    if state = HalfOpen then state <- Closed
                    Ok result
                | Error err ->
                    failures <- failures + 1
                    if failures >= threshold then
                        state <- Open DateTime.Now
                    Error err
    
    // 🎯 Resilient operation composition
    let resilientOperation<'T, 'E> (circuitBreaker: CircuitBreaker<'T, 'E>) (operation: unit -> Result<'T, 'E>) : Result<'T, 'E> =
        retryWithBackoff 3 (TimeSpan.FromSeconds(1.0)) (fun () ->
            circuitBreaker.Execute(operation))
    
    // 📡 Example: Resilient external API call
    let callExternalAPI endpoint = 
        let circuitBreaker = CircuitBreaker<APIResponse, APIError>(3, TimeSpan.FromMinutes(1.0))
        
        resilientOperation circuitBreaker (fun () ->
            try
                let response = HTTP.get endpoint
                Ok response
            with
            | :? TimeoutException as ex -> Error (APITimeout ex.Message)
            | :? HttpRequestException as ex -> Error (APIError ex.Message)
        )
```

---

## 📊 Error Reporting & Observability

### 📈 **Structured Error Logging**

```fsharp
module ErrorReporting =
    
    // 📊 Error context for debugging
    type ErrorContext = {
        OperationId: string
        UserId: string option  
        Timestamp: DateTime
        RequestPath: string option
        Parameters: Map<string, obj>
        StackTrace: string option
    }
    
    // 📝 Enhanced error with context
    type ContextualError<'E> = {
        Error: 'E
        Context: ErrorContext
        Severity: ErrorSeverity
    }
    
    and ErrorSeverity = Low | Medium | High | Critical
    
    // 🎯 Error logging with structured data
    let logError (logger: ILogger) (contextualError: ContextualError<ShopError>) =
        let errorMessage = formatShopError contextualError.Error
        let context = contextualError.Context
        
        let logLevel = 
            match contextualError.Severity with
            | Low -> LogLevel.Information
            | Medium -> LogLevel.Warning  
            | High -> LogLevel.Error
            | Critical -> LogLevel.Critical
        
        logger.Log(logLevel, 
            "Operation {OperationId} failed: {ErrorMessage}. User: {UserId}, Path: {RequestPath}",
            context.OperationId, errorMessage, context.UserId, context.RequestPath)
    
    // 📈 Error metrics collection
    type ErrorMetrics = {
        mutable TotalErrors: int64
        mutable ErrorsByType: Map<string, int64>
        mutable ErrorsBySeverity: Map<ErrorSeverity, int64>
    }
    
    let updateErrorMetrics (metrics: ErrorMetrics) (error: ContextualError<ShopError>) =
        Interlocked.Increment(&metrics.TotalErrors) |> ignore
        
        let errorType = error.Error.GetType().Name
        metrics.ErrorsByType <- 
            metrics.ErrorsByType 
            |> Map.change errorType (function 
                | Some count -> Some (count + 1L)
                | None -> Some 1L)
        
        metrics.ErrorsBySeverity <-
            metrics.ErrorsBySeverity
            |> Map.change error.Severity (function
                | Some count -> Some (count + 1L)  
                | None -> Some 1L)
    
    // 🎭 Error handling middleware
    let errorHandlingMiddleware (logger: ILogger) (metrics: ErrorMetrics) (operation: unit -> Result<'T, ShopError>) (context: ErrorContext) : Result<'T, ShopError> =
        match operation() with
        | Ok result -> Ok result
        | Error shopError ->
            let severity = determineErrorSeverity shopError
            let contextualError = {
                Error = shopError
                Context = context  
                Severity = severity
            }
            
            logError logger contextualError
            updateErrorMetrics metrics contextualError
            
            Error shopError
    
    // 🎯 Severity determination logic
    let determineErrorSeverity = function
        | CustomerError (CustomerNotFound _) -> Low
        | ProductError (ProductOutOfStock _) -> Medium
        | PaymentError (PaymentDeclined _) -> Medium  
        | InfrastructureError (DatabaseError _) -> High
        | SystemError _ -> Critical
        | _ -> Medium
```

### 🎪 **Error Recovery Strategies**

```fsharp
module ErrorRecovery =
    
    // 🔄 Fallback strategies  
    type FallbackStrategy<'T, 'E> =
        | NoFallback
        | DefaultValue of 'T
        | AlternativeOperation of (unit -> Result<'T, 'E>)
        | RetryWithDifferentParameters of (unit -> Result<'T, 'E>)
    
    let executeWithFallback (strategy: FallbackStrategy<'T, 'E>) (operation: unit -> Result<'T, 'E>) : Result<'T, 'E> =
        match operation() with
        | Ok result -> Ok result
        | Error _ ->
            match strategy with
            | NoFallback -> operation()  // Let it fail
            | DefaultValue value -> Ok value
            | AlternativeOperation altOp -> altOp()
            | RetryWithDifferentParameters retryOp -> retryOp()
    
    // 🎯 Example: Product service với fallbacks
    let getProductWithFallback (productId: ProductId) : Result<Product, ProductError> =
        let primaryOperation () = ProductRepository.findProduct productId
        
        let fallbackStrategies = [
            AlternativeOperation (fun () -> CacheService.getProduct productId)
            AlternativeOperation (fun () -> ExternalAPI.getProduct productId)  
            DefaultValue {
                Id = productId
                Name = "Product Temporarily Unavailable"
                Price = 0m
                Stock = 0
                // ... default values
            }
        ]
        
        let rec tryFallbacks strategies =
            match strategies with
            | [] -> Error (ProductNotFound productId)
            | strategy :: rest ->
                match executeWithFallback strategy primaryOperation with
                | Ok product -> Ok product
                | Error _ -> tryFallbacks rest
        
        tryFallbacks fallbackStrategies
    
    // 🛡️ Graceful degradation
    type ServiceHealth = Healthy | Degraded | Unavailable
    
    let getServiceHealth (serviceName: string) : ServiceHealth = 
        // Implementation would check actual service health
        Healthy  // Placeholder
    
    let adaptToServiceHealth<'T, 'E> (serviceName: string) (fullOperation: unit -> Result<'T, 'E>) (degradedOperation: unit -> Result<'T, 'E>) : Result<'T, 'E> =
        match getServiceHealth serviceName with
        | Healthy -> fullOperation()
        | Degraded -> degradedOperation()  
        | Unavailable -> Error (InfrastructureError (ExternalServiceError (serviceName, "503", "Service unavailable")))
```

---

## 🎯 Part 7 Summary

**Error Handling Patterns** trong F# cung cấp:

- **Railway Oriented Programming** cho predictable error flow
- **Hierarchical Error Modeling** với domain-specific errors  
- **Result Types** thay vì exceptions
- **Parallel Error Collection** với Applicative patterns
- **Advanced Combinators** cho complex error scenarios
- **Defensive Programming** với smart constructors
- **Resilience Patterns** như Retry và Circuit Breaker  
- **Structured Error Reporting** với context và metrics
- **Error Recovery Strategies** với fallbacks

**Lợi ích chính:**
- **Xử lý lỗi rõ ràng** - compiler bắt buộc bạn phải handle errors
- **Có thể kết hợp** - errors flow through function composition
- **An toàn kiểu** - không có exceptions bất ngờ
- **Dễ test** - pure functions với predictable error cases
- **Có thể quan sát** - structured error reporting

**Next:** Part 8 sẽ explore **Function Composition** patterns! 🚀

---

# 🔄 Part 8: Function Composition {#part8-composition}

Function Composition trong F# là **heart của functional programming**. Chúng ta sẽ xây dựng **complex applications** từ **simple, composable functions**. Think LEGO blocks cho code! 🎯

## 🎭 Philosophy: Functions as Building Blocks

### 💡 **Composition Over Inheritance**

```fsharp
// ❌ OOP Approach - Inheritance hierarchy
type Animal() =
    abstract member MakeSound: unit -> string
    abstract member Move: unit -> string

type Dog() =
    inherit Animal()
    override this.MakeSound() = "Woof!"  
    override this.Move() = "Running"

type Bird() =
    inherit Animal()  
    override this.MakeSound() = "Tweet!"
    override this.Move() = "Flying"

// Vấn đề:
// - Hierarchy cứng nhắc
// - Khó mở rộng
// - Xung đột multiple inheritance
// - Behavioral coupling

// ✅ FP Approach - Function composition
type Animal = {
    Name: string
    MakeSound: unit -> string
    Move: unit -> string  
}

// 🎯 Behavior functions (composable!)
let barkBehavior () = "Woof!"
let tweetBehavior () = "Tweet!"  
let runBehavior () = "Running"
let flyBehavior () = "Flying"
let swimBehavior () = "Swimming"

// 🧩 Compose behaviors dynamically!
let createDog name = {
    Name = name
    MakeSound = barkBehavior
    Move = runBehavior
}

let createBird name = {
    Name = name  
    MakeSound = tweetBehavior
    Move = flyBehavior
}

let createDuck name = {
    Name = name
    MakeSound = tweetBehavior    // Tweet like bird
    Move = swimBehavior          // But swims like fish!
}

// 🎪 Mix and match any behaviors!
// Không có ràng buộc inheritance!
```

### 🔧 **Basic Composition Operators**

```fsharp
// 🎯 Core composition operators
let (>>) f g x = g (f x)        // Forward composition: f >> g
let (<<) f g x = f (g x)        // Backward composition: f << g  
let (|>) x f = f x              // Pipe forward: x |> f
let (<|) f x = f x              // Pipe backward: f <| x

// 📊 Biểu diễn trực quan:
//
// f >> g:  Input ──→ [f] ──→ [g] ──→ Output
// f << g:  Input ──→ [g] ──→ [f] ──→ Output  
// x |> f:  Input ──→ [f] ──→ Output
// f <| x:  Input ──→ [f] ──→ Output

// 🎯 Ví dụ thực tế
let addOne x = x + 1
let multiplyByTwo x = x * 2
let toString x = x.ToString()

// Các kiểu Composition:
let processNumber1 = addOne >> multiplyByTwo >> toString    // Function composition
let processNumber2 x = x |> addOne |> multiplyByTwo |> toString  // Pipeline

processNumber1 5    // "12" 
processNumber2 5    // "12"

// 🎪 Pipeline phức tạp hơn
let processCustomerOrder customerId =
    customerId
    |> CustomerService.loadCustomer           // CustomerId -> Customer  
    |> Result.bind validateCustomerStatus     // Customer -> Result<Customer, Error>
    |> Result.bind loadCustomerOrders         // Customer -> Result<Order list, Error>
    |> Result.map filterActiveOrders          // Order list -> Order list
    |> Result.map (List.sortByDescending (_.OrderDate))  // Sort by date
    |> Result.map (List.take 10)              // Take top 10
```

---

## 🏗️ Higher-Order Function Patterns

### 🎯 **Decorators và Middleware**

```fsharp
module FunctionDecorators =
    
    // 🕒 Timing decorator
    let withTiming (f: 'T -> 'U) : 'T -> 'U * TimeSpan =
        fun input ->
            let stopwatch = Stopwatch.StartNew()
            let result = f input
            stopwatch.Stop()
            (result, stopwatch.Elapsed)
    
    // 📝 Logging decorator  
    let withLogging (logger: ILogger) (operationName: string) (f: 'T -> 'U) : 'T -> 'U =
        fun input ->
            logger.LogInformation($"Starting {operationName}")
            try
                let result = f input
                logger.LogInformation($"Completed {operationName} successfully")
                result
            with
            | ex -> 
                logger.LogError($"Failed {operationName}: {ex.Message}")
                reraise()
    
    // 🧠 Memoization decorator (caching)
    let memoize (f: 'T -> 'U) : 'T -> 'U =
        let cache = ConcurrentDictionary<'T, 'U>()
        fun input ->
            cache.GetOrAdd(input, fun key -> f key)
    
    // 🔄 Retry decorator
    let withRetry (maxAttempts: int) (f: 'T -> 'U) : 'T -> 'U =
        fun input ->
            let rec attempt count =
                try
                    f input
                with
                | _ when count < maxAttempts -> 
                    Thread.Sleep(100 * count)  // Simple backoff
                    attempt (count + 1)
                | ex -> raise ex
            attempt 1
    
    // 🛡️ Exception handling decorator
    let withExceptionHandling (handleError: exn -> 'U) (f: 'T -> 'U) : 'T -> 'U =
        fun input ->
            try
                f input  
            with
            | ex -> handleError ex
    
    // 🎯 Compose multiple decorators
    let decoratedFunction = 
        CustomerService.processOrder
        |> withLogging logger "ProcessOrder"
        |> withRetry 3
        |> withTiming
        |> memoize
    
    // Sử dụng: decoratedFunction order
    // Kết quả: (ProcessedOrder * TimeSpan, với logging, retry và caching!)

// 🎪 Middleware pipeline pattern
module MiddlewarePipeline =
    
    type Middleware<'Context, 'Result> = 
        ('Context -> 'Result) -> ('Context -> 'Result)
    
    // 🔧 Authentication middleware
    let authenticationMiddleware : Middleware<HttpContext, HttpResult> =
        fun next context ->
            if context.User.IsAuthenticated then
                next context
            else
                Unauthorized "Authentication required"
    
    // 🔐 Authorization middleware  
    let authorizationMiddleware (requiredRole: string) : Middleware<HttpContext, HttpResult> =
        fun next context ->
            if context.User.IsInRole(requiredRole) then
                next context
            else
                Forbidden "Insufficient permissions"
    
    // 📊 Request logging middleware
    let requestLoggingMiddleware (logger: ILogger) : Middleware<HttpContext, HttpResult> =
        fun next context ->
            logger.LogInformation($"Processing request: {context.Request.Path}")
            let result = next context
            logger.LogInformation($"Request completed: {context.Request.Path}")
            result
    
    // 🧩 Compose middleware pipeline
    let buildPipeline (middlewares: Middleware<'Context, 'Result> list) (finalHandler: 'Context -> 'Result) : 'Context -> 'Result =
        middlewares
        |> List.fold (fun handler middleware -> middleware handler) finalHandler
    
    // 🎯 Example API endpoint with middleware
    let processOrderEndpoint =
        buildPipeline [
            requestLoggingMiddleware logger
            authenticationMiddleware  
            authorizationMiddleware "OrderManager"
        ] (fun context ->
            let order = parseOrderFromRequest context.Request
            OrderService.processOrder order
            |> Ok
        )
```

### 🎭 **Partial Application Patterns**

```fsharp
module PartialApplicationPatterns =
    
    // 🎯 Configuration injection via partial application
    let createEmailSender (smtpConfig: SmtpConfig) (templateEngine: ITemplateEngine) 
                          (recipient: EmailAddress) (template: EmailTemplate) (data: 'T) : Result<unit, EmailError> =
        try
            let renderedContent = templateEngine.Render(template, data)
            let message = {
                To = recipient
                Subject = template.Subject  
                Body = renderedContent
                From = smtpConfig.FromAddress
            }
            SmtpClient.send smtpConfig message
            Ok ()
        with
        | ex -> Error (EmailSendError ex.Message)
    
    // 🔧 Pre-configured email functions (dependency injection!)
    let emailSender = createEmailSender smtpConfig templateEngine
    
    let sendWelcomeEmail = emailSender welcomeTemplate
    let sendOrderConfirmation = emailSender orderConfirmationTemplate  
    let sendPasswordReset = emailSender passwordResetTemplate
    
    // Sử dụng: 
    sendWelcomeEmail customer.Email { CustomerName = customer.Name }
    sendOrderConfirmation customer.Email { Order = order; Customer = customer }
    
    // 🎪 Validation with configurable rules
    let validateField (rules: ValidationRule list) (fieldName: string) (value: 'T) : ValidationResult =
        rules
        |> List.fold (fun acc rule -> 
            match acc, rule.Validate(value) with
            | Valid, Valid -> Valid
            | Valid, Invalid errors -> Invalid [(fieldName, errors)]
            | Invalid existing, Invalid newErrors -> Invalid ((fieldName, newErrors) :: existing)
            | Invalid existing, Valid -> Invalid existing
        ) Valid
    
    // 🔧 Pre-configured validators
    let validateEmail = validateField [
        { Name = "Required"; Validate = fun email -> if String.IsNullOrWhiteSpace(email) then Invalid ["Required"] else Valid }
        { Name = "Format"; Validate = fun email -> if email.Contains("@") then Valid else Invalid ["Invalid format"] }
        { Name = "Length"; Validate = fun email -> if email.Length <= 254 then Valid else Invalid ["Too long"] }
    ]
    
    let validateAge = validateField [
        { Name = "Range"; Validate = fun age -> if age >= 0 && age <= 150 then Valid else Invalid ["Must be 0-150"] }
        { Name = "Adult"; Validate = fun age -> if age >= 18 then Valid else Invalid ["Must be adult"] }
    ]
    
    // 🧩 Compose validation pipeline  
    let validateCustomerRegistration registration =
        [
            validateEmail "Email" registration.Email
            validateAge "Age" registration.Age  
            // ... more validations
        ]
        |> ValidationResult.combine
```

---

## 🚂 Pipeline Architecture Patterns

### 🔄 **Data Transformation Pipelines**

```fsharp
module DataPipelines =
    
    // 🎯 ETL (Extract, Transform, Load) pipeline
    type PipelineStep<'Input, 'Output> = 'Input -> Result<'Output, PipelineError>
    
    and PipelineError = 
        | ExtractionError of string
        | TransformationError of step: string * error: string
        | LoadError of string
        | ValidationError of string list
    
    // 🔧 Pipeline composition operator
    let (>=>) (step1: PipelineStep<'A, 'B>) (step2: PipelineStep<'B, 'C>) : PipelineStep<'A, 'C> =
        fun input ->
            step1 input
            |> Result.bind step2
    
    // 📊 Customer data processing pipeline
    let extractCustomerData (source: DataSource) : PipelineStep<unit, RawCustomerData list> =
        fun () ->
            try
                let rawData = DataExtractor.extract source
                Ok rawData
            with
            | ex -> Error (ExtractionError ex.Message)
    
    let validateCustomerData : PipelineStep<RawCustomerData list, RawCustomerData list> =
        fun rawData ->
            let validationErrors = ResizeArray<string>()
            
            rawData
            |> List.iteri (fun i customer -> 
                if String.IsNullOrWhiteSpace(customer.Email) then
                    validationErrors.Add($"Row {i}: Missing email")
                if customer.Age < 0 then  
                    validationErrors.Add($"Row {i}: Invalid age"))
            
            if validationErrors.Count > 0 then
                Error (ValidationError (validationErrors |> Seq.toList))
            else
                Ok rawData
    
    let transformCustomerData : PipelineStep<RawCustomerData list, ProcessedCustomer list> =
        fun rawData ->
            try
                let processedData = 
                    rawData
                    |> List.map (fun raw -> {
                        Id = CustomerId (Guid.NewGuid().ToString())
                        Name = raw.Name.Trim().ToTitleCase()
                        Email = Email.create raw.Email |> Result.defaultWith (fun _ -> failwith "Invalid email")
                        NormalizedPhone = PhoneNumber.normalize raw.Phone
                        AgeGroup = AgeGroup.fromAge raw.Age
                        CreatedAt = DateTime.Now
                    })
                Ok processedData
            with
            | ex -> Error (TransformationError ("CustomerTransformation", ex.Message))
    
    let enrichCustomerData (externalService: ICustomerEnrichmentService) : PipelineStep<ProcessedCustomer list, EnrichedCustomer list> =
        fun processedData ->
            try
                let enrichedData =
                    processedData  
                    |> List.map (fun customer ->
                        let demographics = externalService.GetDemographics(customer.Email)
                        let preferences = externalService.GetPreferences(customer.Email)
                        {
                            Customer = customer
                            Demographics = demographics
                            Preferences = preferences
                            EnrichmentScore = calculateEnrichmentScore demographics preferences
                        })
                Ok enrichedData
            with
            | ex -> Error (TransformationError ("CustomerEnrichment", ex.Message))
    
    let loadCustomerData (repository: ICustomerRepository) : PipelineStep<EnrichedCustomer list, unit> =
        fun enrichedData ->
            try
                enrichedData |> List.iter repository.Save
                Ok ()
            with
            | ex -> Error (LoadError ex.Message)
    
    // 🎪 Complete pipeline composition
    let customerImportPipeline (source: DataSource) (enrichmentService: ICustomerEnrichmentService) (repository: ICustomerRepository) =
        extractCustomerData source
        >=> validateCustomerData
        >=> transformCustomerData  
        >=> enrichCustomerData enrichmentService
        >=> loadCustomerData repository
    
    // Sử dụng:
    // let pipeline = customerImportPipeline csvSource enrichmentService customerRepo
    // let result = pipeline ()
```

### ⚡ **Parallel Processing Pipelines**

```fsharp
module ParallelPipelines =
    
    // 🎯 Parallel map-reduce pattern
    let parallelMapReduce (mapFn: 'T -> 'U) (reduceFn: 'U -> 'U -> 'U) (identity: 'U) (items: 'T list) : 'U =
        items
        |> List.toArray
        |> Array.Parallel.map mapFn        // Parallel mapping
        |> Array.reduce reduceFn           // Sequential reduction
    
    // 📊 Parallel batch processing
    let processBatches<'T, 'U> (batchSize: int) (processor: 'T list -> Async<'U list>) (items: 'T list) : Async<'U list> =
        async {
            let batches = 
                items
                |> List.chunkBySize batchSize
            
            let! results = 
                batches
                |> List.map processor
                |> Async.Parallel        // Process batches in parallel
            
            return results |> Array.concat |> Array.toList
        }
    
    // 🚂 Stream processing pipeline  
    type StreamProcessor<'T, 'U> = {
        BufferSize: int
        Processor: 'T array -> Async<'U array>
        OutputHandler: 'U array -> Async<unit>
    }
    
    let processStream (processor: StreamProcessor<'T, 'U>) (inputStream: IAsyncEnumerable<'T>) : Async<unit> =
        async {
            let buffer = ResizeArray<'T>(processor.BufferSize)
            
            let processBuffer () = async {
                if buffer.Count > 0 then
                    let items = buffer.ToArray()
                    buffer.Clear()
                    let! results = processor.Processor items
                    do! processor.OutputHandler results
            }
            
            // Process stream items
            do! inputStream.ForEachAsync(fun item -> async {
                buffer.Add(item)
                
                if buffer.Count >= processor.BufferSize then
                    do! processBuffer()
            })
            
            // Process remaining items
            do! processBuffer()
        }
    
    // 🎯 Example: Order processing pipeline
    let orderProcessor = {
        BufferSize = 100
        Processor = fun orders -> async {
            return orders |> Array.Parallel.map processOrder  
        }
        OutputHandler = fun processedOrders -> async {
            do! OrderRepository.saveBatch processedOrders
            do! NotificationService.sendBatchConfirmations processedOrders
        }
    }
    
    // Sử dụng: processStream orderProcessor incomingOrders
```

---

## 🎯 Part 8 Summary

**Function Composition Patterns** trong F# cung cấp:

- **Basic Composition** với operators `>>`, `<<`, `|>`, `<|`
- **Higher-Order Functions** cho decorators, middleware, và partial application
- **Pipeline Architecture** cho data transformation và parallel processing
- **Advanced Composition** với Monad Transformers và Reader pattern  
- **Function Factories** cho dynamic function generation và plugin systems

**Lợi ích chính:**
- **Tính module** - functions nhỏ, tập trung, có thể kết hợp
- **Tái sử dụng** - functions là building blocks
- **Dễ test** - pure functions dễ dàng test  
- **Dễ bảo trì** - data flow rõ ràng và separation of concerns
- **Linh hoạt** - dynamic composition và configuration

**Triết lý Composition:**
- Xây dựng hệ thống phức tạp từ các phần đơn giản
- Dữ liệu chảy qua transformation pipelines
- Functions là first-class citizens  
- Composition thay vì inheritance
- Pure functions cho phép fearless composition

**Next:** Part 9 sẽ explore **Testing Strategies** for functional code! 🧪

---

# ✅ Part 9: Testing Strategies {#part9-testing}

Testing trong F# là **fundamentally different** từ OOP testing. Với **pure functions**, **immutable data**, và **explicit error handling**, testing trở nên **simpler**, **more reliable**, và **more predictable**! 🧪

## 🎯 Philosophy: Pure Functions = Easy Testing

### 💡 **Pure vs. Impure Function Testing**

```fsharp
// ❌ Impure function - Hard to test
type CustomerService(database: IDatabase, emailService: IEmailService, logger: ILogger) =
    member this.ProcessCustomer(customerId: string) =
        try
            logger.LogInformation($"Processing customer {customerId}")
            
            let customer = database.Customers.Find(customerId)  // Database dependency!
            if customer = null then
                raise (CustomerNotFoundException(customerId))
            
            customer.LastProcessed <- DateTime.Now              // Mutation!
            database.Customers.Update(customer)                // Database side effect!
            
            emailService.SendWelcomeEmail(customer.Email)       // Email side effect!
            logger.LogInformation($"Customer {customerId} processed successfully")
            
            customer
        with
        | ex -> 
            logger.LogError($"Failed to process customer {customerId}: {ex.Message}")
            reraise()

// Ác mộng testing:
// - Cần mock IDatabase, IEmailService, ILogger
// - DateTime.Now làm tests không deterministic  
// - Exception handling trộn lẫn business logic
// - Side effects làm verification phức tạp

// ✅ Pure function approach - Easy to test!
module CustomerProcessing =
    
    type ProcessingContext = {
        Customer: Customer
        CurrentTime: DateTime
        WelcomeEmailTemplate: EmailTemplate
    }
    
    type ProcessingResult = {
        UpdatedCustomer: Customer
        EmailToSend: Email option
        LogMessages: LogMessage list
    }
    
    // 🎯 Pure business logic (no dependencies!)
    let processCustomer (context: ProcessingContext) : Result<ProcessingResult, ProcessingError> =
        if context.Customer.IsActive then
            let updatedCustomer = { 
                context.Customer with 
                LastProcessed = Some context.CurrentTime 
            }
            
            let emailToSend = {
                To = context.Customer.Email
                Template = context.WelcomeEmailTemplate
                Data = { CustomerName = context.Customer.Name }
            }
            
            let logMessages = [
                LogMessage.Info $"Processing customer {context.Customer.Id}"
                LogMessage.Info $"Customer {context.Customer.Id} processed successfully"
            ]
            
            Ok {
                UpdatedCustomer = updatedCustomer
                EmailToSend = Some emailToSend
                LogMessages = logMessages  
            }
        else
            Error (CustomerInactive context.Customer.Id)

// 🧪 Testing pure functions is trivial!
[<Test>]
let ``processCustomer should update last processed time`` () =
    let context = {
        Customer = { Id = "123"; Name = "John"; Email = "john@test.com"; IsActive = true; LastProcessed = None }
        CurrentTime = DateTime(2024, 1, 15, 10, 30, 0)
        WelcomeEmailTemplate = welcomeTemplate
    }
    
    let result = CustomerProcessing.processCustomer context
    
    result |> should be (ofCase <@ Ok @>)
    match result with
    | Ok processedResult ->
        processedResult.UpdatedCustomer.LastProcessed |> should equal (Some context.CurrentTime)
        processedResult.EmailToSend |> should not' (equal None)
        processedResult.LogMessages |> should haveLength 2
    | Error _ -> failwith "Không nên có lỗi"
    
// Không cần mocking! Không side effects! Deterministic!
```

### 🧩 **Functional Test Composition**

```fsharp
module TestComposition =
    
    // 🎯 Composable test data builders
    type CustomerBuilder = {
        Id: string
        Name: string
        Email: string
        Age: int
        IsActive: bool
        Orders: Order list
    }
    
    module CustomerBuilder =
        let create () = {
            Id = "default-id"
            Name = "John Doe"  
            Email = "john@example.com"
            Age = 30
            IsActive = true
            Orders = []
        }
        
        let withId id builder = { builder with Id = id }
        let withName name builder = { builder with Name = name }
        let withEmail email builder = { builder with Email = email }
        let withAge age builder = { builder with Age = age }
        let inactive builder = { builder with IsActive = false }
        let withOrders orders builder = { builder with Orders = orders }
        
        let build builder : Customer = {
            Id = CustomerId builder.Id
            Name = builder.Name
            Email = builder.Email
            Age = builder.Age
            IsActive = builder.IsActive
            Orders = builder.Orders
        }
    
    // 🔧 Fluent test data creation
    let validCustomer = 
        CustomerBuilder.create()
        |> CustomerBuilder.withId "valid-123"
        |> CustomerBuilder.withEmail "valid@example.com"
        |> CustomerBuilder.build
    
    let inactiveCustomer =
        CustomerBuilder.create()
        |> CustomerBuilder.withId "inactive-456"
        |> CustomerBuilder.inactive
        |> CustomerBuilder.build
    
    let premiumCustomer =
        CustomerBuilder.create()
        |> CustomerBuilder.withId "premium-789"
        |> CustomerBuilder.withOrders [order1; order2; order3]  // High order count
        |> CustomerBuilder.build
    
    // 🎪 Property-based test data generation
    let generateCustomerId () = 
        "CUST-" + Guid.NewGuid().ToString("N").[..7].ToUpper()
    
    let generateEmail () =
        let domains = ["test.com"; "example.org"; "sample.net"]  
        let names = ["john"; "jane"; "bob"; "alice"; "charlie"]
        let domain = domains.[Random().Next(domains.Length)]
        let name = names.[Random().Next(names.Length)]
        $"{name}@{domain}"
    
    let generateRandomCustomer () =
        CustomerBuilder.create()
        |> CustomerBuilder.withId (generateCustomerId())
        |> CustomerBuilder.withEmail (generateEmail())
        |> CustomerBuilder.withAge (Random().Next(18, 80))
        |> CustomerBuilder.build
```

---

## 🏗️ Unit Testing Pure Functions

### ✅ **Testing Domain Logic**

```fsharp
module DomainTests =
    
    // 🎯 Testing validation functions
    [<Test>]
    let ``validateCustomerAge should accept valid ages`` () =
        [ 0; 18; 30; 65; 100 ]
        |> List.iter (fun age ->
            let result = CustomerValidation.validateAge age
            result |> should be (ofCase <@ Valid @>))
    
    [<Test>]  
    let ``validateCustomerAge should reject invalid ages`` () =
        [ -1; -10; 151; 200 ]
        |> List.iter (fun age ->
            let result = CustomerValidation.validateAge age
            result |> should be (ofCase <@ Invalid @>))
    
    // 🧩 Testing computation expressions
    [<Test>]
    let ``order processing workflow should succeed with valid data`` () =
        let validOrder = {
            Id = OrderId "ORDER-123"
            CustomerId = CustomerId "CUST-456"  
            Items = [
                { ProductId = ProductId "PROD-1"; Quantity = 2; Price = 10.0m }
                { ProductId = ProductId "PROD-2"; Quantity = 1; Price = 15.0m }
            ]
            Status = Pending
        }
        
        let validCustomer = validCustomer
        let validProducts = Map [
            ProductId "PROD-1", { Id = ProductId "PROD-1"; Name = "Product 1"; Stock = 10; Price = 10.0m }
            ProductId "PROD-2", { Id = ProductId "PROD-2"; Name = "Product 2"; Stock = 5; Price = 15.0m }
        ]
        
        let context = {
            Order = validOrder
            Customer = validCustomer
            Products = validProducts
            CurrentTime = DateTime(2024, 1, 15)
        }
        
        let result = OrderProcessing.processOrderWorkflow context
        
        match result with
        | Ok processedOrder ->
            processedOrder.Status |> should equal Confirmed
            processedOrder.TotalAmount |> should equal 35.0m
            processedOrder.ProcessedAt |> should equal context.CurrentTime
        | Error error -> failwith $"Lỗi bất ngờ: {error}"
    
    // 🎭 Testing error cases
    [<Test>]
    let ``order processing should fail with insufficient stock`` () =
        let orderWithHighQuantity = {
            validOrder with 
            Items = [{ ProductId = ProductId "PROD-1"; Quantity = 20; Price = 10.0m }]  // Only 10 in stock!
        }
        
        let context = { validContext with Order = orderWithHighQuantity }
        
        let result = OrderProcessing.processOrderWorkflow context
        
        match result with
        | Error (InsufficientStock _) -> () // Mong đợi!
        | Ok _ -> failwith "Nên thất bại vì insufficient stock"
        | Error otherError -> failwith $"Loại lỗi bất ngờ: {otherError}"
    
    // 🔧 Testing Railway Oriented Programming
    [<Test>]
    let ``customer registration pipeline should collect all validation errors`` () =
        let invalidCustomerData = {
            Name = ""                    // Invalid: empty
            Email = "not-an-email"       // Invalid: format
            Age = 200                    // Invalid: range  
            Phone = Some "123"           // Invalid: too short
        }
        
        let result = CustomerRegistration.validateRegistration invalidCustomerData
        
        match result with
        | Valid _ -> failwith "Nên thất bại validation"
        | Invalid errors -> 
            errors |> should contain "Name is required"
            errors |> should contain "Invalid email format"  
            errors |> should contain "Age must be between 0 and 150"
            errors |> should contain "Phone number too short"
            errors |> should haveLength 4  // Tất cả errors được thu thập!
```

### 🎪 **Testing Higher-Order Functions**

```fsharp
module HigherOrderFunctionTests =
    
    // 🎯 Testing function decorators
    [<Test>]
    let ``memoize decorator should cache function results`` () =
        let mutable callCount = 0
        let expensiveFunction x = 
            callCount <- callCount + 1
            x * x
        
        let memoizedFunction = FunctionDecorators.memoize expensiveFunction
        
        // Lần gọi đầu tiên
        let result1 = memoizedFunction 5
        result1 |> should equal 25
        callCount |> should equal 1
        
        // Lần gọi thứ hai với cùng input - nên sử dụng cache
        let result2 = memoizedFunction 5  
        result2 |> should equal 25
        callCount |> should equal 1  // Vẫn là 1!
        
        // Input khác - nên gọi function lại
        let result3 = memoizedFunction 10
        result3 |> should equal 100  
        callCount |> should equal 2
    
    [<Test>]
    let ``retry decorator should retry on failure`` () =
        let mutable attemptCount = 0
        let flakyFunction () =
            attemptCount <- attemptCount + 1
            if attemptCount < 3 then
                failwith "Temporary failure"
            else
                "Success!"
        
        let retryFunction = FunctionDecorators.withRetry 5 flakyFunction
        
        let result = retryFunction ()
        result |> should equal "Success!"
        attemptCount |> should equal 3
    
    // 🧩 Testing function composition
    [<Test>]
    let ``function pipeline should compose correctly`` () =
        let addOne x = x + 1
        let double x = x * 2
        let toString x = x.ToString()
        
        let pipeline = addOne >> double >> toString
        
        let result = pipeline 5
        result |> should equal "12"  // (5 + 1) * 2 = "12"
    
    // 🔄 Testing partial application
    [<Test>]  
    let ``partial application should work correctly`` () =
        let multiply a b = a * b
        
        let double = multiply 2    // Partially applied
        let triple = multiply 3    // Partially applied
        
        double 5 |> should equal 10
        triple 5 |> should equal 15
        
        // Vẫn có thể sử dụng fully applied
        multiply 4 5 |> should equal 20
```

---

## 🎲 Property-Based Testing

### 🔬 **FsCheck for Property Testing**

```fsharp
module PropertyBasedTests =
    
    // 🎯 Basic property testing
    [<Property>]
    let ``adding zero should not change the value`` (x: int) =
        x + 0 = x
    
    [<Property>]
    let ``list reverse twice equals original`` (xs: int list) =
        List.rev (List.rev xs) = xs
    
    [<Property>]
    let ``map preserves list length`` (xs: int list) (f: int -> string) =
        List.length (List.map f xs) = List.length xs
    
    // 🧩 Domain-specific properties
    [<Property>]
    let ``customer validation is consistent`` (name: string) (email: string) (age: int) =
        let customer = { Name = name; Email = email; Age = age }
        
        // Property: kết quả validation phải deterministic
        let result1 = CustomerValidation.validate customer
        let result2 = CustomerValidation.validate customer
        result1 = result2
    
    [<Property>] 
    let ``order total calculation is associative`` (items: OrderItem list) =
        let calculateTotal items = 
            items |> List.sumBy (fun item -> item.Quantity * item.Price)
        
        // Property: grouping items không nên thay đổi total
        let shuffledItems = items |> List.sortBy (fun _ -> System.Random().Next())
        calculateTotal items = calculateTotal shuffledItems
    
    // 🎭 Custom generators for complex types
    type CustomerGenerator =
        static member Customer() =
            gen {
                let! name = Arb.generate<NonEmptyString> |> Gen.map (fun s -> s.Get)
                let! email = 
                    gen {
                        let! username = Gen.alphaNumStr |> Gen.filter (not << String.IsNullOrEmpty)
                        let! domain = Gen.elements ["test.com"; "example.org"; "sample.net"]
                        return $"{username}@{domain}"
                    }
                let! age = Gen.choose(0, 120)
                let! isActive = Arb.generate<bool>
                
                return {
                    Id = CustomerId (Guid.NewGuid().ToString())
                    Name = name
                    Email = email  
                    Age = age
                    IsActive = isActive
                    Orders = []
                }
            }
    
    [<Property(Arbitrary = [| typeof<CustomerGenerator> |])>]
    let ``customer processing should never throw exceptions`` (customer: Customer) =
        try
            let context = {
                Customer = customer
                CurrentTime = DateTime.Now
                WelcomeEmailTemplate = defaultTemplate
            }
            
            let result = CustomerProcessing.processCustomer context
            
            // Property: function nên luôn return Result, không bao giờ throw
            match result with
            | Ok _ -> true
            | Error _ -> true  // Errors là ổn, exceptions thì không!
        with
        | _ -> false  // Nếu đến đây, property đã thất bại
    
    // 🔧 Conditional properties
    [<Property>]
    let ``active customers can be processed`` (customer: Customer) =
        customer.IsActive ==> lazy (
            let context = {
                Customer = customer
                CurrentTime = DateTime.Now  
                WelcomeEmailTemplate = defaultTemplate
            }
            
            let result = CustomerProcessing.processCustomer context
            
            match result with
            | Ok processedResult -> 
                processedResult.UpdatedCustomer.LastProcessed.IsSome
            | Error _ -> false
        )
```

---

## 🎯 Part 9 Summary

**Testing Strategies** trong F# cung cấp:

- **Pure Function Testing** - no mocks needed, deterministic, simple
- **Functional Test Composition** với builders và property generators
- **Unit Testing** cho domain logic, higher-order functions, và composition
- **Property-Based Testing** với FsCheck và custom generators
- **Integration Testing** với test-specific implementations
- **Contract Testing** cho API compliance và compatibility

**Lợi ích chính:**
- **Đơn giản** - pure functions rất dễ test
- **Tin cậy** - không cần mocking phức tạp, kết quả deterministic
- **Coverage** - property-based testing khám phá edge cases  
- **Tự tin** - immutability loại bỏ nhiều loại bugs
- **Nhanh** - không cần I/O setup, test chạy nhanh

**Triết lý Testing:**
- Test business logic, không phải infrastructure  
- Sử dụng pure functions để loại bỏ mocking
- Property-based testing cho coverage toàn diện
- Integration tests cho end-to-end workflows
- Contract tests cho API compatibility

**Next:** Part 10 sẽ explore **Advanced F# Concepts** và production considerations! 🚀

---

# 🎓 Part 10: Advanced F# Concepts {#part10-advanced}

Advanced F# Concepts bao gồm **performance optimization**, **interoperability**, **production patterns**, và **advanced language features**. Đây là kiến thức cần thiết để build **production-ready**, **high-performance** F# applications! 🚀

## ⚡ Performance Optimization

### 🔧 **Memory Management & Allocation**

```fsharp
module PerformanceOptimization =
    
    // 🎯 Struct types for performance-critical code
    [<Struct>]
    type Point3D = {
        X: float32
        Y: float32  
        Z: float32
    }
    
    [<Struct>]
    type ProductPrice = 
        | Price of decimal
        
        member this.Value = match this with Price p -> p
        static member (+) (Price a, Price b) = Price (a + b)
        static member (*) (Price p, factor: decimal) = Price (p * factor)
    
    // 📊 Value vs Reference type comparison
    let performanceTest() =
        // ❌ Reference type (heap allocation)
        type RefPoint = { X: float; Y: float; Z: float }
        
        // ✅ Struct type (stack allocation) 
        [<Struct>]
        type StructPoint = { X: float; Y: float; Z: float }
        
        let refPoints = Array.init 1000000 (fun i -> { RefPoint.X = float i; Y = float i; Z = float i })
        let structPoints = Array.init 1000000 (fun i -> { StructPoint.X = float i; Y = float i; Z = float i })
        
        // Phiên bản Struct:
        // - Sử dụng bộ nhớ ít hơn ~3x
        // - Cache locality tốt hơn
        // - Không GC pressure
        // - Truy cập array nhanh hơn
    
    // 🧠 Memory-efficient collections
    let efficientCollections() =
        // ❌ Inefficient: Creating many intermediate lists
        let inefficient data =
            data
            |> List.filter (fun x -> x > 0)      // Creates intermediate list
            |> List.map (fun x -> x * 2)         // Creates another intermediate list  
            |> List.filter (fun x -> x < 100)    // Creates another intermediate list
            |> List.sum                          // Final result
        
        // ✅ Efficient: Using Seq for lazy evaluation
        let efficient data =
            data
            |> Seq.filter (fun x -> x > 0)       // Lazy - no intermediate collection
            |> Seq.map (fun x -> x * 2)          // Lazy - no intermediate collection
            |> Seq.filter (fun x -> x < 100)     // Lazy - no intermediate collection  
            |> Seq.sum                           // Materializes only once
        
        // ✅ Most efficient: Single pass with fold
        let mostEfficient data =
            data |> List.fold (fun acc x ->
                if x > 0 then
                    let doubled = x * 2
                    if doubled < 100 then acc + doubled else acc
                else acc
            ) 0
    
    // 🔥 Tail-call optimization
    let rec factorialNotOptimized n =
        if n <= 1 then 1
        else n * factorialNotOptimized (n - 1)  // ❌ Not tail-recursive (stack overflow risk!)
    
    let factorialOptimized n =
        let rec loop acc n =
            if n <= 1 then acc
            else loop (acc * n) (n - 1)          // ✅ Tail-recursive (optimized to loop)
        loop 1 n
    
    // 🎯 List processing optimization
    let processLargeDataset data =
        // ❌ Multiple passes over data
        let filtered = data |> List.filter isValid
        let transformed = filtered |> List.map transform  
        let sorted = transformed |> List.sortBy getKey
        sorted
        
        // ✅ Single pass with choose + sort at end
        data
        |> List.choose (fun x -> if isValid x then Some (transform x) else None)
        |> List.sortBy getKey
```

### 🏎️ **High-Performance Patterns**

```fsharp
module HighPerformancePatterns =
    
    // 🎯 Span<T> và Memory<T> cho zero-copy operations
    open System
    open System.Buffers
    
    let processTextEfficiently (text: ReadOnlySpan<char>) : int =
        let mutable count = 0
        for i = 0 to text.Length - 1 do
            if Char.IsLetter(text.[i]) then
                count <- count + 1
        count
        
    // Usage with zero allocations:
    let analyzeString (input: string) =
        let span = input.AsSpan()
        let firstHalf = span.Slice(0, span.Length / 2)     // Zero-copy slice
        let secondHalf = span.Slice(span.Length / 2)       // Zero-copy slice
        
        let firstCount = processTextEfficiently firstHalf
        let secondCount = processTextEfficiently secondHalf
        
        firstCount + secondCount
    
    // 🧠 Object pooling for expensive objects
    type ObjectPool<'T>(factory: unit -> 'T, resetAction: 'T -> unit) =
        let pool = ConcurrentQueue<'T>()
        
        member _.Get() =
            match pool.TryDequeue() with
            | true, item -> item
            | false, _ -> factory()
        
        member _.Return(item: 'T) =
            resetAction item
            pool.Enqueue(item)
    
    // Ví dụ: StringBuilder pooling
    let stringBuilderPool = ObjectPool<StringBuilder>(
        (fun () -> StringBuilder(1024)),
        (fun sb -> sb.Clear() |> ignore)
    )
    
    let buildStringEfficiently (parts: string list) =
        let sb = stringBuilderPool.Get()
        try
            parts |> List.iter (sb.Append >> ignore)
            sb.ToString()
        finally
            stringBuilderPool.Return(sb)
    
    // ⚡ Vectorization with SIMD
    open System.Numerics
    
    let sumArrayVectorized (data: float32[]) : float32 =
        let mutable sum = 0.0f
        let vectorSize = Vector<float32>.Count
        let mutable i = 0
        
        // Xử lý vectors (SIMD)
        while i <= data.Length - vectorSize do
            let vector = Vector<float32>(data, i)
            sum <- sum + Vector.Sum(vector)
            i <- i + vectorSize
        
        // Xử lý các phần tử còn lại
        while i < data.Length do
            sum <- sum + data.[i]
            i <- i + 1
            
        sum
    
    // 🔄 Parallel processing with partitioning
    let parallelMapReduce<'T, 'U> 
        (mapFn: 'T -> 'U) 
        (reduceFn: 'U -> 'U -> 'U) 
        (identity: 'U) 
        (data: 'T[]) : 'U =
        
        let partitionSize = max 1 (data.Length / Environment.ProcessorCount)
        
        data
        |> Array.chunkBySize partitionSize
        |> Array.Parallel.map (fun chunk ->
            chunk |> Array.map mapFn |> Array.reduce reduceFn)
        |> Array.reduce reduceFn
```

---

## 🌐 Interoperability Patterns

### 🔗 **C# Interop Best Practices**

```fsharp
module CSharpInterop =
    
    // 🎯 F# types that work well with C#
    [<CLIMutable>]  // Enables C# object initializer syntax
    type CustomerDto = {
        Id: string
        Name: string
        Email: string
        Age: int
        Orders: OrderDto[]
    }
    
    and [<CLIMutable>] OrderDto = {
        Id: string
        Total: decimal
        OrderDate: DateTime
        Items: OrderItemDto[]
    }
    
    and [<CLIMutable>] OrderItemDto = {
        ProductId: string
        Quantity: int
        Price: decimal
    }
    
    // 🔧 Exposing F# functions to C#
    type CustomerService() =
        
        // ✅ C#-friendly method signatures
        member _.GetCustomer(customerId: string) : CustomerDto option =
            CustomerRepository.findById (CustomerId customerId)
            |> Option.map CustomerMapping.toDto
        
        // ❌ F# signature that's awkward in C#: 'a -> Result<'b, 'c>
        // ✅ C# friendly alternative:
        member _.TryCreateCustomer(request: CreateCustomerRequest, [<Out>] customer: CustomerDto byref) : bool =
            match CustomerService.createCustomer request with
            | Ok createdCustomer -> 
                customer <- CustomerMapping.toDto createdCustomer
                true
            | Error _ -> 
                customer <- Unchecked.defaultof<CustomerDto>
                false
        
        // 🎯 Async methods for C#
        member _.GetCustomerAsync(customerId: string) : Task<CustomerDto option> =
            async {
                let! customerResult = CustomerRepository.findByIdAsync (CustomerId customerId)
                return customerResult |> Option.map CustomerMapping.toDto
            } |> Async.StartAsTask
        
        // 🔄 Collection types C# understands
        member _.GetAllCustomers() : IReadOnlyList<CustomerDto> =
            CustomerRepository.findAll()
            |> List.map CustomerMapping.toDto
            |> List.toArray :> IReadOnlyList<CustomerDto>
    
    // 🎭 Bridging F# discriminated unions to C#
    type OrderStatus =
        | Pending = 0
        | Confirmed = 1  
        | Shipped = 2
        | Delivered = 3
        | Cancelled = 4
        
    // Alternative: Class hierarchy approach
    [<AbstractClass>]
    type PaymentMethod() = class end
    
    type CreditCard(cardNumber: string, expiryDate: DateTime) =
        inherit PaymentMethod()
        member _.CardNumber = cardNumber
        member _.ExpiryDate = expiryDate
    
    type PayPal(email: string) =
        inherit PaymentMethod()
        member _.Email = email
    
    type BankTransfer(accountNumber: string, routingNumber: string) =
        inherit PaymentMethod()  
        member _.AccountNumber = accountNumber
        member _.RoutingNumber = routingNumber
```

---

## 🏭 Production Patterns

### 🔒 **Configuration & Dependency Injection**

```fsharp
module ProductionPatterns =
    
    // 🎯 Strongly-typed configuration
    [<CLIMutable>]
    type DatabaseConfig = {
        ConnectionString: string
        CommandTimeout: int
        MaxRetries: int
        EnableLogging: bool
    }
    
    [<CLIMutable>]
    type EmailConfig = {
        SmtpHost: string
        SmtpPort: int
        FromAddress: string
        ApiKey: string
    }
    
    [<CLIMutable>]
    type AppConfig = {
        Database: DatabaseConfig
        Email: EmailConfig
        Environment: string
        LogLevel: string
    }
    
    // 🔧 Dependency injection setup
    open Microsoft.Extensions.DependencyInjection
    open Microsoft.Extensions.Configuration
    
    let configureServices (services: IServiceCollection) (configuration: IConfiguration) =
        
        // Bind configuration sections
        let appConfig = configuration.Get<AppConfig>()
        services.AddSingleton<AppConfig>(appConfig) |> ignore
        
        // Register F# services
        services.AddScoped<ICustomerRepository, DatabaseCustomerRepository>() |> ignore
        services.AddScoped<IOrderRepository, DatabaseOrderRepository>() |> ignore  
        services.AddScoped<IEmailService, SmtpEmailService>() |> ignore
        
        // Register F# domain services
        services.AddScoped<CustomerService>() |> ignore
        services.AddScoped<OrderService>() |> ignore
        
        // Register with factory functions
        services.AddScoped<IOrderProcessor>(fun provider ->
            let customerRepo = provider.GetService<ICustomerRepository>()
            let orderRepo = provider.GetService<IOrderRepository>()
            let emailService = provider.GetService<IEmailService>()
            
            OrderProcessor.create customerRepo orderRepo emailService
        ) |> ignore
        
        services
```

### 📊 **Monitoring & Observability**

```fsharp
module Observability =
    
    open System.Diagnostics
    open Microsoft.Extensions.Logging
    open Microsoft.Extensions.Metrics
    
    // 🎯 Structured logging
    type LogEvents =
        static member OrderCreated = EventId(1001, "OrderCreated")
        static member OrderProcessed = EventId(1002, "OrderProcessed")  
        static member OrderFailed = EventId(1003, "OrderFailed")
        static member CustomerValidationFailed = EventId(2001, "CustomerValidationFailed")
        static member PaymentProcessed = EventId(3001, "PaymentProcessed")
        static member PaymentFailed = EventId(3002, "PaymentFailed")
    
    let logOrderCreated (logger: ILogger) (order: Order) =
        logger.LogInformation(LogEvents.OrderCreated, 
            "Order created: {OrderId} for customer {CustomerId} with total {Total}",
            order.Id, order.CustomerId, order.Total)
    
    let logOrderProcessed (logger: ILogger) (order: ProcessedOrder) (processingTime: TimeSpan) =
        logger.LogInformation(LogEvents.OrderProcessed,
            "Order processed: {OrderId} in {ProcessingTime}ms",  
            order.Id, processingTime.TotalMilliseconds)
    
    let logOrderFailed (logger: ILogger) (orderId: OrderId) (error: OrderProcessingError) =
        logger.LogError(LogEvents.OrderFailed,
            "Order processing failed: {OrderId} - {ErrorType}: {ErrorMessage}",
            orderId, error.GetType().Name, error.ToString())
    
    // 📈 Metrics and telemetry
    type OrderMetrics(meterFactory: IMeterFactory) =
        let meter = meterFactory.Create("MyCompany.OrderProcessing")
        
        let ordersProcessed = 
            meter.CreateCounter<int>("orders_processed_total", "count", "Total number of orders processed")
        
        let orderProcessingDuration = 
            meter.CreateHistogram<double>("order_processing_duration_seconds", "seconds", "Order processing duration")
        
        let activeOrders = 
            meter.CreateUpDownCounter<int>("orders_active", "count", "Number of orders currently being processed")
        
        member _.RecordOrderProcessed(orderType: string, success: bool) =
            ordersProcessed.Add(1, KeyValuePair("order_type", orderType), KeyValuePair("success", string success))
        
        member _.RecordProcessingDuration(duration: TimeSpan, orderType: string) =
            orderProcessingDuration.Record(duration.TotalSeconds, KeyValuePair("order_type", orderType))
        
        member _.IncrementActiveOrders() = activeOrders.Add(1)
        member _.DecrementActiveOrders() = activeOrders.Add(-1)
```

---

## 🎯 Part 10 Summary

**Advanced F# Concepts** cung cấp:

- **Performance Optimization** với struct types, memory management, SIMD, parallel processing
- **Interoperability** với C# best practices, clean API surfaces  
- **Production Patterns** với configuration, DI, monitoring, observability
- **Enterprise Features** với health checks, distributed tracing, structured logging, metrics

**Cân nhắc quan trọng cho Production:**
- **Hiệu quả bộ nhớ** - sử dụng struct types cho performance-critical code
- **Thiết kế Interop** - design APIs hoạt động tốt từ C#
- **Khả năng quan sát** - logging, metrics và tracing toàn diện
- **Cấu hình** - strongly-typed configuration với validation
- **Testing** - test coverage toàn diện bao gồm integration tests

**Khuyến nghị Kiến trúc:**
- **Pure Core** - giữ business logic pure và testable
- **Functional Shell** - xử lý I/O và side effects ở boundaries  
- **Type Safety** - tận dụng F# type system cho correctness
- **Performance** - tối ưu hot paths với data structures phù hợp
- **Monitoring** - instrument mọi thứ cho production visibility

**F# trong Production:**
- Đặc tính performance xuất sắc
- Type safety mạnh giảm runtime errors  
- Tương tác tốt với .NET ecosystems hiện có
- Khả năng concurrency và async mạnh mẽ
- Functional programming cho phép code đáng tin cậy, dễ bảo trì

**Congratulations!** 🎉 Bạn đã hoàn thành comprehensive F# learning guide từ basic concepts đến advanced production patterns. Time to build amazing applications! 🚀

---

## 🎉 Learning Path Recommendation

1. **Start here:** Part 1 (Architecture) - Understand the big picture
2. **Foundation:** Part 2 (Domain) - Learn F# types  
3. **Core Skills:** Parts 3-5 - Master each layer
4. **Advanced:** Parts 6-8 - Async, errors, composition
5. **Mastery:** Parts 9-10 - Testing and advanced concepts

**Next step:** Read Part 1 thoroughly, then we'll expand each section! 🚀