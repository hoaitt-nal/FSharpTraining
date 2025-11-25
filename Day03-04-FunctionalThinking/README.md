# 🧠 Day 3-4: Functional Thinking

## 📋 Learning Objectives
- [ ] Master function definitions and recursive functions
- [ ] Understand higher-order functions
- [ ] Learn pipelining operators (`|>`, `>>`, `<<`)
- [ ] Work with Option type (Some, None)
- [ ] Practice pattern matching extensively

### 🔍 **Chi tiết Learning Objectives:**

#### 1. **Master function definitions and recursive functions**
```fsharp
// Function definition cơ bản
let add x y = x + y

// Function với type annotation
let multiply (x: int) (y: int) : int = x * y

// Recursive function - Hàm gọi chính nó
let rec factorial n =
    match n with
    | 0 | 1 -> 1  // Base case - điều kiện dừng
    | _ -> n * factorial (n - 1)  // Recursive case
```
**Mục tiêu**: Hiểu cách định nghĩa hàm, đặc biệt là đệ quy với `let rec`

#### 2. **Understand higher-order functions**
```fsharp
// Higher-order function: nhận function làm parameter
let applyTwice f x = f (f x)

// Function trả về function khác
let makeMultiplier n = fun x -> x * n

// Sử dụng
let double = makeMultiplier 2
let result = applyTwice double 5  // (5 * 2) * 2 = 20
```
**Mục tiêu**: Function có thể nhận/trả về function khác - nền tảng functional programming

#### 3. **Learn pipelining operators (`|>`, `>>`, `<<`)**
```fsharp
// Forward pipe |> - chuyển kết quả sang function tiếp theo
let result = 
    [1; 2; 3; 4; 5]
    |> List.filter (fun x -> x > 2)  // [3; 4; 5]
    |> List.map (fun x -> x * 2)     // [6; 8; 10]
    |> List.sum                      // 24

// Function composition >> - ghép function
let processNumbers = 
    List.filter (fun x -> x > 2)
    >> List.map (fun x -> x * 2)
    >> List.sum
```
**Mục tiêu**: Tạo pipeline xử lý dữ liệu dễ đọc và maintainable

##### 🔍 **`|>` vs `>>` - Pipeline vs Composition:**

| **Aspect** | **`\|>` Pipeline** | **`>>` Composition** |
|------------|-------------------|---------------------|
| **Purpose** | Truyền DATA qua functions | Ghép FUNCTIONS thành function mới |
| **Input** | Data + Function | Function + Function |
| **Output** | Result value | New function |
| **Usage** | Immediate processing | Reusable function creation |

```fsharp
// |> - Xử lý data ngay lập tức
let processTextNow text =
    text
    |> String.trim                     // "  hello  " -> "hello"
    |> String.toLower                  // "hello"
    |> String.split ' '                // [|"hello"|]
    |> Array.length                    // 1

// >> - Tạo function để reuse
let textProcessor = 
    String.trim
    >> String.toLower
    >> String.split ' '
    >> Array.length

// Apply nhiều lần
let result1 = "  Hello World  " |> textProcessor    // 2
let result2 = "F# Programming" |> textProcessor     // 2

// Kết hợp cả hai
let data |> (func1 >> func2 >> func3)  // Pipeline of composed functions
```

**Khi nào dùng:**
- **`|>`**: One-time data processing, immediate results
- **`>>`**: Reusable processing pipelines, function building

#### 4. **Work with Option type (Some, None)**
```fsharp
// Option type - xử lý giá trị có thể null/missing
let safeDivide x y =
    if y <> 0 then Some (x / y)  // Có kết quả
    else None                    // Không có kết quả

// Pattern matching với Option
match safeDivide 10 2 with
| Some result -> printfn "Result: %.2f" result  // Result: 5.00
| None -> printfn "Division by zero!"
```
**Mục tiêu**: Xử lý null-safety một cách elegant, tránh NullReferenceException

#### 5. **Practice pattern matching extensively**
```fsharp
// Pattern matching với lists
let rec processList lst =
    match lst with
    | [] -> "Empty list"
    | [x] -> sprintf "Single item: %d" x
    | head :: tail -> sprintf "Head: %d, processing rest..." head

// Pattern matching với tuples
let processCoordinate coord =
    match coord with
    | (0, 0) -> "Origin"
    | (x, 0) -> sprintf "On X-axis at %d" x
    | (0, y) -> sprintf "On Y-axis at %d" y
    | (x, y) -> sprintf "Point at (%d, %d)" x y
```
**Mục tiêu**: Sử dụng pattern matching thay cho if/else chains, code cleaner và safer

### 🎯 **Tại sao những concept này quan trọng?**
- **Functional Programming Foundation**: Hiểu cách tư duy functional thay vì imperative
- **Code Composition**: Ghép các function nhỏ thành logic phức tạp
- **Error Handling**: Option type thay cho null checking
- **Data Transformation**: Pipeline processing giúp code dễ đọc
- **Pattern Recognition**: Pattern matching giúp handle các cases một cách explicit

## 📝 Code Examples & Exercises

### Exercise 1: Basic Functions
Create `Functions.fs`:
```fsharp
// Simple function
let square x = x * x

// Function with type annotation
let multiply (x: int) (y: int) : int = x * y

// Function composition
let addOne x = x + 1
let double x = x * 2
let addOneThenDouble = addOne >> double

// Test functions
printfn "Square of 5: %d" (square 5)
printfn "Multiply 3 * 4: %d" (multiply 3 4)
printfn "Add one then double 5: %d" (addOneThenDouble 5)
```

### Exercise 2: Recursive Functions
Create `Recursion.fs`:
```fsharp
// Classic factorial
let rec factorial n =
    match n with
    | 0 | 1 -> 1
    | _ -> n * factorial (n - 1)

// Fibonacci sequence
let rec fibonacci n =
    match n with
    | 0 -> 0
    | 1 -> 1
    | _ -> fibonacci (n - 1) + fibonacci (n - 2)

// List sum using recursion
let rec sumList lst =
    match lst with
    | [] -> 0
    | head :: tail -> head + sumList tail

// Test recursive functions
printfn "Factorial of 5: %d" (factorial 5)
printfn "Fibonacci of 7: %d" (fibonacci 7)
printfn "Sum of [1;2;3;4;5]: %d" (sumList [1;2;3;4;5])
```

### Exercise 3: Higher-Order Functions
Create `HigherOrderFunctions.fs`:
```fsharp
// Function that takes another function
let applyTwice f x = f (f x)

// Function that returns a function
let makeAdder n = fun x -> x + n

// Using List functions (higher-order)
let numbers = [1; 2; 3; 4; 5; 6; 7; 8; 9; 10]

let doubled = List.map (fun x -> x * 2) numbers
let evens = List.filter (fun x -> x % 2 = 0) numbers  
let sum = List.fold (+) 0 numbers
let product = List.reduce (*) numbers

// Test higher-order functions
let addFive = makeAdder 5
printfn "Apply double twice to 3: %d" (applyTwice double 3)
printfn "Add 5 to 10: %d" (addFive 10)
printfn "Doubled: %A" doubled
printfn "Even numbers: %A" evens
printfn "Sum: %d, Product: %d" sum product
```

### Exercise 4: Pipeline Operations
Create `Pipelines.fs`:
```fsharp
// Forward pipe operator |>
let processNumbers nums =
    nums
    |> List.filter (fun x -> x > 0)      // Keep positive numbers
    |> List.map (fun x -> x * x)         // Square them
    |> List.filter (fun x -> x < 100)    // Keep squares less than 100
    |> List.sum                          // Sum them up

// Composition operators >> and <<
let processData = 
    List.filter (fun x -> x > 0)
    >> List.map (fun x -> x * x)
    >> List.sum

// Backward composition
let processDataBackward = 
    List.sum
    << List.map (fun x -> x * x)
    << List.filter (fun x -> x > 0)

// Test pipelines
let testData = [-2; -1; 0; 1; 2; 3; 4; 5; 6; 7; 8; 9; 10]
printfn "Pipeline result: %d" (processNumbers testData)
printfn "Composition result: %d" (processData testData)
```

### Exercise 5: Option Type & Pattern Matching
Create `OptionTypes.fs`:
```fsharp
// Safe division function
let safeDivide x y =
    if y <> 0.0 then Some (x / y)
    else None

// Pattern matching with Options
let printDivisionResult x y =
    match safeDivide x y with
    | Some result -> printfn "%.2f / %.2f = %.2f" x y result
    | None -> printfn "Cannot divide %.2f by zero!" x

// Option.map and Option.bind examples
let maybeNumber = Some 10
let maybeResult = 
    maybeNumber
    |> Option.map (fun x -> x * 2)
    |> Option.map (fun x -> x + 5)

// Chaining operations with Option
let chainedCalculation x =
    Some x
    |> Option.bind (fun n -> if n > 0 then Some (n * 2) else None)
    |> Option.bind (fun n -> if n < 100 then Some (n + 10) else None)

// Test Option operations
printDivisionResult 10.0 2.0
printDivisionResult 10.0 0.0
printfn "Maybe result: %A" maybeResult
printfn "Chained calculation (5): %A" (chainedCalculation 5)
printfn "Chained calculation (50): %A" (chainedCalculation 50)
```

## 🏃‍♂️ Practice Tasks

### Task 1: Math Library
Create functions for:
1. Power function (recursive)
2. GCD (Greatest Common Divisor) using Euclidean algorithm  
3. Check if number is prime
4. Generate list of primes up to N

### Task 2: List Processing
Create functions that:
1. Find maximum element in list (using recursion)
2. Reverse a list (using recursion)
3. Remove duplicates from list
4. Implement custom map and filter functions

### Task 3: String Processing Pipeline
Create a text processing pipeline that:
1. Takes a sentence
2. Splits into words
3. Filters out short words (< 3 chars)
4. Converts to uppercase
5. Joins with " | " separator

### Task 4: Advanced Pattern Matching
Create a calculator that uses pattern matching for:
1. Basic operations (+, -, *, /)
2. Handles invalid operations
3. Returns Option<float> for results
4. Chains multiple operations

## ✅ Completion Checklist
- [ ] Understand recursive function patterns
- [ ] Comfortable with higher-order functions
- [ ] Can use pipeline operators effectively
- [ ] Master Option type and pattern matching
- [ ] Completed all 5 exercises
- [ ] Finished all 4 practice tasks
- [ ] Can compose functions using >> and <<

## 🔍 Key Concepts Mastered
- **Recursion**: Base case + recursive case pattern
- **Higher-order functions**: Functions as parameters/return values
- **Pipelines**: Data transformation chains with |>
- **Options**: Handling null/missing values safely
- **Pattern matching**: Destructuring and conditional logic

## 🎯 Next Steps
Ready for **Day 5-6: Data Structures** to learn about Records, Tuples, and complex data modeling!