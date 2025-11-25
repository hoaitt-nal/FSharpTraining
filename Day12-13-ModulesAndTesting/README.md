# 📦 Day 12-13: Modules & Testing

## 📋 Learning Objectives
- [ ] Master module creation and organization
- [ ] Understand namespace hierarchy
- [ ] Create internal and public modules
- [ ] Write comprehensive unit tests with xUnit
- [ ] Practice Test-Driven Development (TDD)
- [ ] Structure large F# applications

### 🔍 **Chi tiết Learning Objectives:**

#### 1. **Master module creation and organization**
```fsharp
// Module với namespace
namespace MyCompany.Utils

module MathUtils =
    let add x y = x + y
    let multiply x y = x * y
    
    // Nested module
    module Advanced =
        let factorial n = 
            [1..n] |> List.fold (*) 1
            
        let fibonacci n =
            // Implementation here
            
// Module signature để hide implementation
module StringUtils =
    // Public function
    val processText : string -> string
    
    // Private function (not in signature)  
    let private sanitizeInput text = text.Trim()
```
**Mục tiêu**: Organize code logic thành modules có namespace hierarchy

#### 2. **Understand namespace hierarchy**
```fsharp
// File structure mapping
// MyCompany/
//   Utils/
//     MathUtils.fs     -> namespace MyCompany.Utils
//     StringUtils.fs   -> namespace MyCompany.Utils  
//   Domain/
//     Customer.fs      -> namespace MyCompany.Domain
//     Order.fs         -> namespace MyCompany.Domain

// Usage trong code khác
open MyCompany.Utils
open MyCompany.Domain

let result = MathUtils.add 5 10
let customer = Customer.create "John" "john@test.com"
```
**Mục tiêu**: Structure large applications với clear namespace hierarchy

#### 3. **Create internal and public modules**
```fsharp
// Public module - available to consumers
module PublicApi =
    let createUser name email = { Name = name; Email = email }
    let validateUser user = (* validation logic *)

// Internal module - only within assembly
module internal InternalHelpers =
    let sanitizeEmail email = email.Trim().ToLower()
    let generateId () = System.Guid.NewGuid()

// Private module - only within file
module private FileHelpers =
    let readConfig () = (* file operations *)
```
**Mục tiêu**: Control visibility và encapsulation

#### 4. **Write comprehensive unit tests with xUnit**
```fsharp
module Tests.MathUtilsTests

open Xunit
open FsUnit.xUnit
open MyLibrary.MathUtils

[<Fact>]
let ``factorial of 5 should be 120`` () =
    factorial 5 |> should equal 120

[<Theory>]
[<InlineData(2, true)>]
[<InlineData(3, true)>]  
[<InlineData(4, false)>]
[<InlineData(15, false)>]
let ``isPrime should work correctly`` (number, expected) =
    isPrime number |> should equal expected

[<Fact>]
let ``factorial of negative number should throw`` () =
    (fun () -> factorial -1 |> ignore) 
    |> should throw typeof<System.ArgumentException>
```
**Mục tiêu**: Write thorough tests covering happy path, edge cases, và errors

#### 5. **Practice Test-Driven Development (TDD)**
```fsharp
// Step 1: Write failing test first
[<Fact>]
let ``calculateDiscount should apply 10% for orders over $100`` () =
    let order = { Amount = 150.0 }
    let discount = calculateDiscount order
    discount |> should equal 15.0

// Step 2: Write minimal code to make test pass
let calculateDiscount order =
    if order.Amount > 100.0 then order.Amount * 0.1
    else 0.0

// Step 3: Refactor and add more tests
[<Theory>]
[<InlineData(50.0, 0.0)>]
[<InlineData(100.0, 0.0)>]
[<InlineData(150.0, 15.0)>]
[<InlineData(200.0, 20.0)>]
let ``calculateDiscount test cases`` (amount, expectedDiscount) =
    let order = { Amount = amount }
    calculateDiscount order |> should equal expectedDiscount
```
**Mục tiêu**: Red-Green-Refactor cycle để ensure quality

#### 6. **Structure large F# applications**
```fsharp
// Project structure
// MyApp/
//   Domain/           // Business logic
//     Customer.fs
//     Order.fs
//   Infrastructure/   // External concerns
//     Database.fs
//     FileSystem.fs
//   Application/      // Use cases
//     CustomerService.fs
//     OrderService.fs
//   Api/             // HTTP endpoints
//     CustomerController.fs
//   Tests/
//     Domain/
//     Application/
//     Integration/

// Dependency direction: Api -> Application -> Domain <- Infrastructure
```
**Mục tiêu**: Clean architecture với proper dependency management

### 🎯 **Tại sao những concept này quan trọng?**
- **Scalability**: Large applications cần proper organization
- **Maintainability**: Modules giúp separate concerns
- **Reusability**: Well-designed modules có thể reuse
- **Quality**: Unit tests ensure code correctness
- **Confidence**: TDD approach reduces bugs
- **Team Collaboration**: Clear structure giúp team work together
- **Documentation**: Tests serve as living documentation

## 🛠️ Setup Requirements
```bash
# Add testing packages
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package FsUnit.xUnit
```

## 📝 Code Examples & Exercises

### Exercise 1: Module Basics
Create `MathUtilities.fs`:
```fsharp
namespace MyLibrary

// Public module
module MathUtils =
    /// Calculate the factorial of a number
    let factorial n =
        let rec loop acc i =
            if i <= 1 then acc
            else loop (acc * i) (i - 1)
        if n < 0 then invalidArg "n" "Factorial is not defined for negative numbers"
        else loop 1 n
    
    /// Check if a number is prime
    let isPrime n =
        if n < 2 then false
        elif n = 2 then true
        elif n % 2 = 0 then false
        else
            let limit = int (sqrt (float n))
            [3..2..limit]
            |> List.forall (fun i -> n % i <> 0)
    
    /// Generate fibonacci sequence up to n terms
    let fibonacciSequence n =
        let rec fib a b count acc =
            if count = 0 then List.rev acc
            else fib b (a + b) (count - 1) (a :: acc)
        if n <= 0 then []
        elif n = 1 then [0]
        else fib 0 1 n []

// Internal module (only accessible within this assembly)
module internal MathHelpers =
    let gcd a b =
        let rec loop x y =
            if y = 0 then x
            else loop y (x % y)
        loop (abs a) (abs b)
    
    let lcm a b =
        if a = 0 || b = 0 then 0
        else abs (a * b) / (gcd a b)

// Public module using internal helpers
module NumberTheory =
    open MathHelpers
    
    let greatestCommonDivisor = gcd
    let leastCommonMultiple = lcm
    
    let areRelativelyPrime a b =
        gcd a b = 1
```

### Exercise 2: Namespace Organization
Create `DataStructures/Collections.fs`:
```fsharp
namespace MyLibrary.DataStructures

// Stack implementation
module Stack =
    type Stack<'T> = Stack of 'T list
    
    let empty = Stack []
    
    let push item (Stack items) = Stack (item :: items)
    
    let pop (Stack items) =
        match items with
        | [] -> failwith "Stack is empty"
        | head :: tail -> (head, Stack tail)
    
    let peek (Stack items) =
        match items with
        | [] -> failwith "Stack is empty"
        | head :: _ -> head
    
    let isEmpty (Stack items) = List.isEmpty items
    
    let size (Stack items) = List.length items

// Queue implementation
module Queue =
    type Queue<'T> = Queue of 'T list * 'T list
    
    let empty = Queue ([], [])
    
    let enqueue item (Queue (front, back)) =
        Queue (front, item :: back)
    
    let dequeue (Queue (front, back)) =
        match front with
        | head :: tail -> (head, Queue (tail, back))
        | [] ->
            match List.rev back with
            | [] -> failwith "Queue is empty"
            | head :: tail -> (head, Queue (tail, []))
    
    let isEmpty (Queue (front, back)) =
        List.isEmpty front && List.isEmpty back
```

### Exercise 3: Unit Testing with xUnit
Create `Tests/MathUtilitiesTests.fs`:
```fsharp
module Tests.MathUtilitiesTests

open Xunit
open FsUnit.Xunit
open MyLibrary.MathUtils

[<Fact>]
let ``factorial of 0 should be 1`` () =
    factorial 0 |> should equal 1

[<Fact>]
let ``factorial of 5 should be 120`` () =
    factorial 5 |> should equal 120

[<Fact>]
let ``factorial of negative number should throw exception`` () =
    (fun () -> factorial -1 |> ignore) |> should throw typeof<System.ArgumentException>

[<Theory>]
[<InlineData(2, true)>]
[<InlineData(3, true)>]
[<InlineData(4, false)>]
[<InlineData(17, true)>]
[<InlineData(25, false)>]
let ``isPrime should correctly identify prime numbers`` (number, expected) =
    isPrime number |> should equal expected

[<Fact>]
let ``fibonacci sequence of 5 terms should be correct`` () =
    let expected = [0; 1; 1; 2; 3]
    fibonacciSequence 5 |> should equal expected

[<Fact>]
let ``fibonacci sequence of 0 terms should be empty`` () =
    fibonacciSequence 0 |> should be Empty

// Property-based testing
[<Theory>]
[<InlineData(1)>]
[<InlineData(5)>]
[<InlineData(10)>]
let ``factorial is always positive for non-negative inputs`` (n) =
    factorial n |> should be (greaterThan 0)
```

### Exercise 4: Advanced Testing Patterns
Create `Tests/DataStructuresTests.fs`:
```fsharp
module Tests.DataStructuresTests

open Xunit
open FsUnit.Xunit
open MyLibrary.DataStructures

// Stack tests
module StackTests =
    [<Fact>]
    let ``new stack should be empty`` () =
        Stack.empty |> Stack.isEmpty |> should be True
    
    [<Fact>]
    let ``push and peek should work correctly`` () =
        let stack = 
            Stack.empty
            |> Stack.push 1
            |> Stack.push 2
            |> Stack.push 3
        
        Stack.peek stack |> should equal 3
        Stack.size stack |> should equal 3
    
    [<Fact>]
    let ``pop should return LIFO order`` () =
        let stack = 
            Stack.empty
            |> Stack.push 1
            |> Stack.push 2
            |> Stack.push 3
        
        let (item1, stack1) = Stack.pop stack
        let (item2, stack2) = Stack.pop stack1
        let (item3, stack3) = Stack.pop stack2
        
        item1 |> should equal 3
        item2 |> should equal 2
        item3 |> should equal 1
        Stack.isEmpty stack3 |> should be True

// Queue tests  
module QueueTests =
    [<Fact>]
    let ``new queue should be empty`` () =
        Queue.empty |> Queue.isEmpty |> should be True
    
    [<Fact>]
    let ``enqueue and dequeue should work in FIFO order`` () =
        let queue = 
            Queue.empty
            |> Queue.enqueue 1
            |> Queue.enqueue 2
            |> Queue.enqueue 3
        
        let (item1, queue1) = Queue.dequeue queue
        let (item2, queue2) = Queue.dequeue queue1
        let (item3, queue3) = Queue.dequeue queue2
        
        item1 |> should equal 1
        item2 |> should equal 2
        item3 |> should equal 3
        Queue.isEmpty queue3 |> should be True

// Integration tests
[<Fact>]
let ``stack and queue integration test`` () =
    let data = [1; 2; 3; 4; 5]
    
    // Push to stack
    let stack = 
        data |> List.fold (fun acc item -> Stack.push item acc) Stack.empty
    
    // Pop from stack and enqueue to queue
    let rec stackToQueue s q =
        if Stack.isEmpty s then q
        else
            let (item, newStack) = Stack.pop s
            stackToQueue newStack (Queue.enqueue item q)
    
    let queue = stackToQueue stack Queue.empty
    
    // Dequeue and verify order (should be reversed)
    let rec queueToList q acc =
        if Queue.isEmpty q then List.rev acc
        else
            let (item, newQueue) = Queue.dequeue q
            queueToList newQueue (item :: acc)
    
    let result = queueToList queue []
    result |> should equal [5; 4; 3; 2; 1]
```

### Exercise 5: Test-Driven Development Example
Create `Tests/BankAccountTDD.fs`:
```fsharp
module Tests.BankAccountTDD

open Xunit
open FsUnit.Xunit

// First, write the tests
type BankAccount = {
    AccountNumber: string
    Balance: decimal
    IsActive: bool
}

type TransactionResult =
    | Success of BankAccount
    | InsufficientFunds
    | AccountClosed
    | InvalidAmount

// Tests written first (TDD approach)
module BankAccountTests =
    [<Fact>]
    let ``new account should have zero balance`` () =
        let account = BankAccount.create "12345"
        account.Balance |> should equal 0m
        account.AccountNumber |> should equal "12345"
        account.IsActive |> should be True
    
    [<Fact>]
    let ``deposit positive amount should increase balance`` () =
        let account = BankAccount.create "12345"
        match BankAccount.deposit 100m account with
        | Success newAccount -> newAccount.Balance |> should equal 100m
        | _ -> failwith "Deposit should succeed"
    
    [<Fact>]
    let ``withdraw more than balance should fail`` () =
        let account = BankAccount.create "12345"
        let result = BankAccount.withdraw 50m account
        result |> should equal InsufficientFunds
    
    [<Fact>]
    let ``withdraw from closed account should fail`` () =
        let account = BankAccount.create "12345" |> BankAccount.close
        let result = BankAccount.withdraw 10m account
        result |> should equal AccountClosed

// Now implement the module to make tests pass
module BankAccount =
    let create accountNumber = {
        AccountNumber = accountNumber
        Balance = 0m
        IsActive = true
    }
    
    let deposit amount account =
        if amount <= 0m then InvalidAmount
        elif not account.IsActive then AccountClosed
        else Success { account with Balance = account.Balance + amount }
    
    let withdraw amount account =
        if amount <= 0m then InvalidAmount
        elif not account.IsActive then AccountClosed
        elif account.Balance < amount then InsufficientFunds
        else Success { account with Balance = account.Balance - amount }
    
    let close account =
        { account with IsActive = false }
```

## 🏃‍♂️ Practice Tasks

### Task 1: Library Management System
Create a complete library system with:
1. Proper module organization
2. Domain models in separate modules
3. Business logic modules
4. Comprehensive test suite
5. Integration tests

### Task 2: Calculator Library
Refactor your Day 7 calculator into:
1. Separate modules for operations
2. Memory management module
3. UI abstraction module
4. Full test coverage
5. Performance benchmarks

### Task 3: Data Processing Pipeline
Build a data processing system with:
1. Input/output modules
2. Transformation modules
3. Validation modules
4. Error handling modules
5. End-to-end tests

### Task 4: Configuration Management
Implement a config system with:
1. Configuration loading modules
2. Validation modules
3. Environment-specific modules
4. Test configurations
5. Integration with DI container

## ✅ Completion Checklist
- [ ] Can create and organize modules effectively
- [ ] Understand namespace hierarchy
- [ ] Master public/internal accessibility
- [ ] Write comprehensive unit tests
- [ ] Practice TDD methodology
- [ ] Completed all 5 exercises
- [ ] Finished all 4 practice tasks

## 🎯 Next Steps
Ready for **Day 14: Mini Project - CSV Processor** to apply all learned concepts in a real-world application!