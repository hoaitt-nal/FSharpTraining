# 📊 Day 5-6: Data Structures

## 📋 Learning Objectives
- [ ] Master Tuples and Records
- [ ] Work with Lists & Arrays effectively
- [ ] Use List functions (map, filter, fold, reduce)
- [ ] Pattern matching with complex types
- [ ] Design Discriminated Unions
- [ ] Model domain data properly

### 🔍 **Chi tiết Learning Objectives:**

#### 1. **Master Tuples and Records**
```fsharp
// Tuples - nhóm nhiều giá trị tạm thời
let point = (10, 20)
let person = ("Alice", 25, "Engineer")

// Records - structured data với tên field
type Person = { Name: string; Age: int; Job: string }
let alice = { Name = "Alice"; Age = 25; Job = "Engineer" }

// Record với immutable updates
let olderAlice = { alice with Age = 26 }
```
**Mục tiêu**: Hiểu khi nào dùng Tuple (tạm thời) vs Record (có cấu trúc)

#### 2. **Work with Lists & Arrays effectively**
```fsharp
// List - immutable, linked list
let numbers = [1; 2; 3; 4; 5]
let moreNumbers = 0 :: numbers  // [0; 1; 2; 3; 4; 5]

// Array - mutable, indexed access
let arr = [|1; 2; 3; 4; 5|]
arr.[0] <- 10  // Thay đổi được

// Performance differences
let listAccess = numbers.[2]    // O(n)
let arrayAccess = arr.[2]       // O(1)
```
**Mục tiêu**: Chọn đúng data structure dựa trên performance needs

#### 3. **Use List functions (map, filter, fold, reduce)**
```fsharp
let numbers = [1; 2; 3; 4; 5; 6; 7; 8; 9; 10]

// Transform data
let doubled = numbers |> List.map (fun x -> x * 2)

// Filter data
let evens = numbers |> List.filter (fun x -> x % 2 = 0)

// Aggregate data
let sum = numbers |> List.fold (+) 0
let product = numbers |> List.reduce (*)
```
**Mục tiêu**: Thành thạo functional data processing patterns

#### 4. **Pattern matching with complex types**
```fsharp
// Pattern matching với records
let describePersonAge person =
    match person with
    | { Age = age } when age < 18 -> "Minor"
    | { Age = age } when age < 65 -> "Adult"  
    | _ -> "Senior"

// Pattern matching với tuples
let classifyPoint point =
    match point with
    | (0, 0) -> "Origin"
    | (x, 0) -> sprintf "X-axis at %d" x
    | (0, y) -> sprintf "Y-axis at %d" y
    | (x, y) when x = y -> "Diagonal"
    | _ -> "General point"
```
**Mục tiêu**: Destructuring và conditional logic với complex types

#### 5. **Design Discriminated Unions**
```fsharp
// Model business logic với DUs
type PaymentMethod =
    | Cash
    | CreditCard of string * int  // number, cvv
    | BankTransfer of string      // account number

// Pattern matching với DUs
let processPayment amount method =
    match method with
    | Cash -> sprintf "Processing cash payment of $%.2f" amount
    | CreditCard (num, cvv) -> sprintf "Charging card ***%s" (num.Substring(12))
    | BankTransfer account -> sprintf "Bank transfer from %s" account
```
**Mục tiêu**: Model domain logic một cách type-safe và expressive

#### 6. **Model domain data properly**
```fsharp
// Domain modeling với Records + DUs
type CustomerId = CustomerId of int
type ProductId = ProductId of string

type OrderStatus =
    | Pending
    | Processing
    | Shipped of DateTime
    | Delivered of DateTime

type Order = {
    Id: int
    CustomerId: CustomerId
    Items: (ProductId * int * decimal) list  // product, qty, price
    Status: OrderStatus
    Total: decimal
}
```
**Mục tiêu**: Tạo domain model rõ ràng, type-safe và maintainable

### 🎯 **Tại sao những concept này quan trọng?**
- **Data Modeling**: Tạo structure rõ ràng cho business domain
- **Type Safety**: Compiler catches errors at compile time
- **Immutability**: Predictable data flow, less bugs
- **Pattern Matching**: Expressive và exhaustive case handling
- **Functional Processing**: Elegant data transformation pipelines
- **Domain Design**: Code reflects business logic clearly

## 📝 Code Examples & Exercises

### Exercise 1: Tuples
Create `Tuples.fs`:
```fsharp
// Simple tuples
let coordinates = (10, 20)
let person = ("Alice", 25, "Engineer")
let rgb = (255, 128, 0)

// Destructuring tuples
let (x, y) = coordinates
let (name, age, job) = person
let (red, green, blue) = rgb

// Functions returning tuples
let getMinMax list =
    let min = List.min list
    let max = List.max list
    (min, max)

// Functions taking tuples
let distance (x1, y1) (x2, y2) =
    let dx = x2 - x1
    let dy = y2 - y1
    sqrt (float (dx * dx + dy * dy))

// Test tuples
printfn "Coordinates: (%d, %d)" x y
printfn "Person: %s, %d years old, %s" name age job
printfn "RGB: (%d, %d, %d)" red green blue

let numbers = [1; 5; 3; 9; 2; 7]
let (minVal, maxVal) = getMinMax numbers
printfn "Min: %d, Max: %d" minVal maxVal

let dist = distance (0, 0) (3, 4)
printfn "Distance: %.2f" dist
```

### Exercise 2: Records
Create `Records.fs`:
```fsharp
// Record type definition
type Person = {
    FirstName: string
    LastName: string
    Age: int
    Email: string
}

// Creating records
let alice = {
    FirstName = "Alice"
    LastName = "Smith"
    Age = 30
    Email = "alice@example.com"
}

// Record with copy-and-update
let olderAlice = { alice with Age = 31 }

// Records in functions
let getFullName person =
    sprintf "%s %s" person.FirstName person.LastName

let isAdult person = person.Age >= 18

let updateEmail newEmail person =
    { person with Email = newEmail }

// Test records
printfn "Full name: %s" (getFullName alice)
printfn "Is adult: %b" (isAdult alice)
printfn "Original email: %s" alice.Email
let updatedAlice = updateEmail "alice.smith@newcompany.com" alice
printfn "Updated email: %s" updatedAlice.Email
```

### Exercise 3: Discriminated Unions
Create `DiscriminatedUnions.fs`:
```fsharp
// Simple discriminated union
type Shape =
    | Circle of radius: float
    | Rectangle of width: float * height: float
    | Square of side: float

// Union with data
type PaymentMethod =
    | Cash
    | CreditCard of number: string * expiry: string
    | BankTransfer of account: string * routingNumber: string

// Option-like custom type
type Result<'T, 'E> =
    | Success of 'T
    | Error of 'E

// Pattern matching with unions
let calculateArea shape =
    match shape with
    | Circle radius -> System.Math.PI * radius * radius
    | Rectangle (width, height) -> width * height
    | Square side -> side * side

let processPayment amount method =
    match method with
    | Cash -> sprintf "Processing cash payment of $%.2f" amount
    | CreditCard (number, expiry) -> 
        sprintf "Processing card payment of $%.2f (Card: ****%s)" amount (number.Substring(number.Length - 4))
    | BankTransfer (account, routing) ->
        sprintf "Processing bank transfer of $%.2f to account %s" amount account

// Test discriminated unions
let shapes = [
    Circle 5.0
    Rectangle (10.0, 20.0)
    Square 7.0
]

shapes |> List.iter (fun shape ->
    let area = calculateArea shape
    printfn "Area: %.2f" area
)

let payment = CreditCard ("1234567890123456", "12/25")
printfn "%s" (processPayment 99.99 payment)
```

### Exercise 4: Lists & Arrays
Create `ListsAndArrays.fs`:
```fsharp
// Lists (immutable, linked list)
let numbers = [1; 2; 3; 4; 5]
let moreNumbers = [6; 7; 8; 9; 10]
let allNumbers = numbers @ moreNumbers  // Concatenation
let evenMore = 0 :: allNumbers         // Prepend

// List functions
let doubled = List.map (fun x -> x * 2) numbers
let evens = List.filter (fun x -> x % 2 = 0) allNumbers
let sum = List.sum numbers
let product = List.fold (*) 1 numbers
let max = List.max numbers

// Custom list functions
let rec customLength lst =
    match lst with
    | [] -> 0
    | _ :: tail -> 1 + customLength tail

let rec customReverse lst =
    match lst with
    | [] -> []
    | head :: tail -> customReverse tail @ [head]

// Arrays (mutable, indexed)
let arr = [| 1; 2; 3; 4; 5 |]
arr.[2] <- 99  // Mutation allowed

// Array operations
let doubledArr = Array.map (fun x -> x * 2) arr
let evenArr = Array.filter (fun x -> x % 2 = 0) arr

// Test collections
printfn "Original numbers: %A" numbers
printfn "All numbers: %A" allNumbers
printfn "Doubled: %A" doubled
printfn "Evens: %A" evens
printfn "Sum: %d, Product: %d, Max: %d" sum product max
printfn "Custom length: %d" (customLength numbers)
printfn "Array after mutation: %A" arr
```

### Exercise 5: Complex Data Modeling
Create `DataModeling.fs`:
```fsharp
// Domain modeling example: E-commerce system
type Customer = {
    Id: int
    Name: string
    Email: string
    Phone: string option
}

type Product = {
    Id: int
    Name: string
    Price: decimal
    Category: string
    InStock: int
}

type OrderItem = {
    Product: Product
    Quantity: int
    Discount: decimal option
}

type OrderStatus =
    | Pending
    | Processing  
    | Shipped of trackingNumber: string
    | Delivered of deliveryDate: System.DateTime
    | Cancelled of reason: string

type Order = {
    Id: int
    Customer: Customer
    Items: OrderItem list
    Status: OrderStatus
    OrderDate: System.DateTime
    TotalAmount: decimal
}

// Business logic functions
let calculateItemTotal item =
    let basePrice = item.Product.Price * decimal item.Quantity
    match item.Discount with
    | Some discountPercent -> basePrice * (1M - discountPercent / 100M)
    | None -> basePrice

let calculateOrderTotal order =
    order.Items
    |> List.map calculateItemTotal
    |> List.sum

let getOrderStatusMessage order =
    match order.Status with
    | Pending -> "Your order is pending approval"
    | Processing -> "Your order is being processed"
    | Shipped tracking -> sprintf "Your order has been shipped (Tracking: %s)" tracking
    | Delivered date -> sprintf "Order delivered on %s" (date.ToString("yyyy-MM-dd"))
    | Cancelled reason -> sprintf "Order cancelled: %s" reason

// Test data modeling
let laptop = { Id = 1; Name = "Gaming Laptop"; Price = 1299.99M; Category = "Electronics"; InStock = 5 }
let mouse = { Id = 2; Name = "Wireless Mouse"; Price = 29.99M; Category = "Electronics"; InStock = 20 }

let customer = { Id = 1; Name = "John Doe"; Email = "john@example.com"; Phone = Some "+1234567890" }

let orderItems = [
    { Product = laptop; Quantity = 1; Discount = Some 10M }
    { Product = mouse; Quantity = 2; Discount = None }
]

let order = {
    Id = 1001
    Customer = customer
    Items = orderItems
    Status = Shipped "ABC123456"
    OrderDate = System.DateTime.Now
    TotalAmount = calculateOrderTotal { Id = 0; Customer = customer; Items = orderItems; Status = Pending; OrderDate = System.DateTime.Now; TotalAmount = 0M }
}

printfn "Order total: $%.2f" (calculateOrderTotal order)
printfn "Order status: %s" (getOrderStatusMessage order)
```

## 🏃‍♂️ Practice Tasks

### Task 1: Library Management System
Design types for:
1. Book (Title, Author, ISBN, Genre, Available)
2. Member (ID, Name, Email, MembershipType)
3. Loan (Book, Member, LoanDate, DueDate, Status)
4. Functions to check availability, calculate fines

### Task 2: Advanced List Operations
Implement custom functions:
1. `groupBy` - group list elements by a key function
2. `partition` - split list into two based on predicate
3. `zip` - combine two lists into tuple list
4. `flatMap` - map and flatten results

### Task 3: Game Character System
Model:
1. Character stats (Health, Mana, Strength, etc.)
2. Equipment (Weapon, Armor, Accessories)  
3. Skills and abilities
4. Combat calculation functions

### Task 4: Financial Portfolio
Create system for:
1. Different investment types (Stocks, Bonds, Cash)
2. Portfolio with multiple investments
3. Performance calculations
4. Risk assessment functions

## ✅ Completion Checklist
- [ ] Comfortable with tuples and destructuring
- [ ] Can create and use records effectively  
- [ ] Understand discriminated unions
- [ ] Master list and array operations
- [ ] Can model complex domain data
- [ ] Completed all 5 exercises
- [ ] Finished all 4 practice tasks

## 🔍 Key Concepts Mastered
- **Tuples**: Lightweight data grouping
- **Records**: Structured data with named fields
- **Discriminated Unions**: Type-safe alternatives
- **Pattern Matching**: Destructuring complex types
- **Collection Functions**: Transforming and processing data

## 🎯 Next Steps
Ready for **Day 7: Mini Project - Calculator** to combine all learned concepts into a practical application!