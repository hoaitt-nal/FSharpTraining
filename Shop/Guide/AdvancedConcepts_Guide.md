# 🎓 Advanced F# Concepts - Complete Guide

## 📋 Table of Contents
1. [Higher-Order Functions](#higher-order-functions)
2. [Custom Operators](#custom-operators)
3. [Monadic Patterns](#monadic-patterns)
4. [Function Composition](#function-composition)
5. [Pipeline Transformations](#pipeline-transformations)
6. [Validation Patterns](#validation-patterns)
7. [Real-World Examples](#real-world-examples)
8. [Angular Developer Perspective](#angular-developer-perspective)

---

## Higher-Order Functions - Hàm Bậc Cao

### 🎯 Định Nghĩa
**Higher-Order Functions (HOF)** là các hàm có khả năng:
- Nhận các hàm khác làm tham số đầu vào
- Trả về một hàm như kết quả
- Hoặc cả hai điều trên

### 📝 Ví Dụ Cơ Bản

```fsharp
// 1. Hàm nhận một hàm khác làm tham số
let applyTwice f x = f (f x)  // Áp dụng hàm f hai lần lên x

let increment x = x + 1
let result = applyTwice increment 5  // Kết quả: 7 (5 -> 6 -> 7)

// 2. Hàm trả về một hàm khác
let createMultiplier factor = 
    fun x -> x * factor  // Tạo hàm nhân với factor

let double = createMultiplier 2   // Hàm nhân đôi
let triple = createMultiplier 3   // Hàm nhân ba
let doubled = double 10    // 20
let tripled = triple 10    // 30
```

### 🔧 Các Higher-Order Functions Có Sẵn

| Hàm | Mục Đích | Ví Dụ |
|-----|----------|--------|
| `List.map` | Biến đổi từng phần tử | `[1;2;3] \|> List.map ((*) 2)` → `[2;4;6]` |
| `List.filter` | Lọc các phần tử | `[1;2;3;4] \|> List.filter (fun x -> x % 2 = 0)` → `[2;4]` |
| `List.fold` | Tích lũy giá trị | `[1;2;3] \|> List.fold (+) 0` → `6` |
| `List.reduce` | Fold không có giá trị ban đầu | `[1;2;3] \|> List.reduce (+)` → `6` |

### 🌟 Các Mẫu HOF Nâng Cao

#### Currying và Partial Application
```fsharp
// Curried function (nhiều danh sách tham số)
let add x y = x + y
let addFive = add 5        // Partial application - đã cố định tham số đầu
let result = addFive 10    // 15

// Currying tường minh
let multiply = fun x -> fun y -> x * y  // Hàm trả về hàm
let double = multiply 2    // Hàm nhân đôi
let quadruple = double >> double  // Kết hợp hàm: nhân 4
```

#### Function Factories - Nhà Máy Hàm
```fsharp
// Tạo các hàm validation
let createValidator minLength maxLength = 
    fun (input: string) ->
        if input.Length < minLength then Error $"Quá ngắn (tối thiểu {minLength})"
        elif input.Length > maxLength then Error $"Quá dài (tối đa {maxLength})"
        else Ok input

// Tạo các validator cụ thể
let validateUsername = createValidator 3 20  // Validator cho username
let validatePassword = createValidator 8 50  // Validator cho password

// Cách sử dụng
let usernameResult = validateUsername "john"     // Ok "john" - hợp lệ
let passwordResult = validatePassword "12"       // Error "Quá ngắn (tối thiểu 8)"
```

### 💡 Ví Dụ HOF Thực Tế
```fsharp
// Hệ thống giảm giá e-commerce
type DiscountRule = decimal -> decimal  // Type cho quy tắc giảm giá

// Giảm giá theo phần trăm
let percentageDiscount percent : DiscountRule = 
    fun price -> price * (1.0m - percent / 100.0m)

// Giảm giá số tiền cố định
let fixedDiscount amount : DiscountRule = 
    fun price -> max 0m (price - amount)  // Không để giá âm

// Giảm giá thành viên thân thiết
let loyaltyDiscount years : DiscountRule =
    let discountPercent = min 20.0m (decimal years * 2.0m)  // Tối đa 20%
    percentageDiscount discountPercent

// Áp dụng nhiều giảm giá liên tiếp
let applyDiscounts (discounts: DiscountRule list) price =
    discounts |> List.fold (fun acc discount -> discount acc) price

// Cách sử dụng
let customerDiscounts = [
    percentageDiscount 10.0m    // Giảm 10%
    fixedDiscount 5.0m          // Giảm 5$ cố định
    loyaltyDiscount 3           // Giảm 6% thành viên 3 năm
]

let finalPrice = applyDiscounts customerDiscounts 100.0m  // Tính giá cuối cùng
```

---

## Custom Operators - Toán Tử Tùy Chỉnh

### 🛠️ Tại Sao Cần Custom Operators?
Custom operators giúp code dễ đọc và biểu cảm hơn, đặc biệt cho:
- Các phép toán đặc thù theo domain
- Monadic compositions (kết hợp monadic)
- Tính toán toán học
- Biến đổi pipeline

### 📝 Định Nghĩa Toán Tử Cơ Bản

```fsharp
// Định nghĩa custom operators
let (++) x y = x + y + 1          // Phép cộng có tăng thêm 1
let (|>) x f = f x                // Forward pipe (có sẵn)
let (<|) f x = f x                // Backward pipe (có sẵn)

// Cách sử dụng
let result1 = 5 ++ 3              // 9 (5 + 3 + 1)
let result2 = 10 |> (*) 2         // 20 - truyền 10 vào hàm nhân 2
let result3 = (*) 2 <| 10         // 20 - áp dụng hàm nhân 2 lên 10
```

### 🚀 Monadic Operators

#### Toán Tử Bind (>>=)
```fsharp
// Result bind operator - kết nối các phép tính có thể thất bại
let (>>=) result f =
    match result with
    | Ok value -> f value      // Thành công -> tiếp tục xử lý
    | Error err -> Error err   // Lỗi -> truyền lỗi tiếp

// Option bind operator - kết nối các phép tính có thể null
let (>>=) option f =
    match option with
    | Some value -> f value    // Có giá trị -> tiếp tục xử lý
    | None -> None            // None -> giữ None

// Sử dụng với Result để validate
let validateAge age =
    if age >= 0 && age <= 150 then Ok age
    else Error "Tuổi không hợp lệ"

let validateName name =
    if String.IsNullOrEmpty(name) then Error "Tên là bắt buộc"
    else Ok name

// Kết hợp validation - chỉ thành công khi cả hai đều hợp lệ
let validatePerson ageInput nameInput =
    validateAge ageInput
    >>= (fun age ->                    // Nếu tuổi hợp lệ
        validateName nameInput 
        >>= (fun name -> Ok (name, age)))  // Và tên hợp lệ

// Cách sử dụng
let person1 = validatePerson 25 "John"        // Ok ("John", 25)
let person2 = validatePerson -5 "Jane"        // Error "Tuổi không hợp lệ"
```

#### Toán Tử Map (<!>)
```fsharp
// Result map operator (functor) - biến đổi giá trị bên trong
let (<!>) f result =
    match result with
    | Ok value -> Ok (f value)    // Áp dụng function f lên giá trị
    | Error err -> Error err      // Giữ nguyên lỗi

// Option map operator - biến đổi giá trị trong Option
let (<!>) f option =
    match option with
    | Some value -> Some (f value)  // Áp dụng f lên giá trị
    | None -> None                  // Giữ nguyên None

// Cách sử dụng
let toUpper (s: string) = s.ToUpper()  // Hàm chuyển thành chữ hoa

let result1 = toUpper <!> Ok "hello"         // Ok "HELLO" - thành công
let result2 = toUpper <!> Error "failed"     // Error "failed" - giữ lỗi
let result3 = toUpper <!> Some "world"       // Some "WORLD" - có giá trị
let result4 = toUpper <!> None               // None - giữ None
```

### 🎨 Advanced Custom Operators

#### Composition Operators
```fsharp
// Function composition operators
let (>>) f g x = g (f x)    // Forward composition (built-in)
let (<<) f g x = f (g x)    // Backward composition (built-in)

// Custom composition với xử lý lỗi
let (>=>) f g x =           // Kleisli composition cho Result
    match f x with
    | Ok y -> g y           // f thành công -> tiếp tục g
    | Error err -> Error err // f thất bại -> truyền lỗi

// Cách sử dụng
let parseNumber (s: string) =
    match System.Int32.TryParse(s) with
    | true, n -> Ok n
    | false, _ -> Error "Không phải số"

let checkPositive n =
    if n > 0 then Ok n
    else Error "Phải là số dương"

// Kết hợp hai hàm: parse rồi check positive
let parsePositiveNumber = parseNumber >=> checkPositive

let test1 = parsePositiveNumber "42"    // Ok 42 - hợp lệ
let test2 = parsePositiveNumber "-5"    // Error "Phải là số dương"
let test3 = parsePositiveNumber "abc"   // Error "Không phải số"
```

#### Toán Tử Toán Học
```fsharp
// Phép toán vector 2D
type Vector2D = { X: float; Y: float }

let (+.) v1 v2 = { X = v1.X + v2.X; Y = v1.Y + v2.Y }  // Cộng vector
let (-.) v1 v2 = { X = v1.X - v2.X; Y = v1.Y - v2.Y }  // Trừ vector  
let (*.) scalar v = { X = scalar * v.X; Y = scalar * v.Y }  // Nhân với scalar

// Cách sử dụng
let v1 = { X = 1.0; Y = 2.0 }
let v2 = { X = 3.0; Y = 4.0 }
let sum = v1 +. v2              // { X = 4.0; Y = 6.0 } - tổng hai vector
let scaled = 2.0 *. v1          // { X = 2.0; Y = 4.0 } - nhân đôi vector
```

---

## Monadic Patterns - Các Mẫu Monadic

### 🎯 Monad Là Gì?
Monads là các design pattern để kết hợp các phép tính trong một ngữ cảnh:
- `Option` - Phép tính có thể thất bại (nullable)
- `Result` - Phép tính có thông tin lỗi  
- `Async` - Phép tính bất đồng bộ
- `List` - Phép tính không xác định (nhiều kết quả)

### 📝 The Monad Laws

```fsharp
// 1. Left Identity: return a >>= f ≡ f a
let leftIdentity a f = 
    (Ok a >>= f) = (f a)

// 2. Right Identity: m >>= return ≡ m  
let rightIdentity m = 
    (m >>= Ok) = m

// 3. Associativity: (m >>= f) >>= g ≡ m >>= (fun x -> f x >>= g)
let associativity m f g = 
    ((m >>= f) >>= g) = (m >>= (fun x -> f x >>= g))
```

### 🔧 Result Monad Implementation - 2 Cách Tiếp Cận

```fsharp
module Result =
    let bind f result =
        match result with
        | Ok value -> f value
        | Error err -> Error err
    
    let map f result =
        match result with
        | Ok value -> Ok (f value)
        | Error err -> Error err
    
    let return' value = Ok value
    
    // Applicative functor - cho phép kết hợp nhiều Result
    let apply fResult xResult =
        match fResult, xResult with
        | Ok f, Ok x -> Ok (f x)        // Cả hai thành công
        | Error e, _ -> Error e          // Function lỗi - trả lỗi đầu tiên
        | _, Error e -> Error e          // Value lỗi - trả lỗi đầu tiên

// Custom operators
let (>>=) = Result.bind    // Monadic bind - tuần tự
let (<!>) = Result.map     // Functor map
let (<*>) = Result.apply   // Applicative apply - song song

// Ví dụ: Parsing và validating user input
type User = { Name: string; Age: int; Email: string }

let validateName name =
    if String.IsNullOrEmpty(name) then Error "Tên là bắt buộc"
    else Ok name

let validateAge ageStr =
    match System.Int32.TryParse(ageStr) with
    | true, age when age >= 0 && age <= 150 -> Ok age
    | true, _ -> Error "Tuổi phải từ 0 đến 150"
    | false, _ -> Error "Tuổi phải là số"

let validateEmail email =
    if email.Contains("@") then Ok email
    else Error "Email không hợp lệ"

// 🔄 CÁCH 1: Monadic Composition (Tuần Tự - Fail Fast)
let createUserMonadic nameStr ageStr emailStr =
    validateName nameStr              // Validate tên trước
    >>= fun name ->                   // ✅ Nếu tên OK -> tiếp tục
        validateAge ageStr            // Validate tuổi  
        >>= fun age ->                // ✅ Nếu tuổi OK -> tiếp tục
            validateEmail emailStr    // Validate email
            >>= fun email ->          // ✅ Nếu email OK -> tạo user
                Ok { Name = name; Age = age; Email = email }

// ⚡ CÁCH 2: Applicative Style (Song Song - Thu Thập Tất Cả Lỗi)  
let createUserApplicative nameStr ageStr emailStr =
    let createUser' name age email = { Name = name; Age = age; Email = email }
    
    createUser'                       // Constructor function
    <!> validateName nameStr          // Validate tên (độc lập)
    <*> validateAge ageStr           // Validate tuổi (độc lập)
    <*> validateEmail emailStr       // Validate email (độc lập)

// 📊 So sánh kết quả:
let testMonadic = createUserMonadic "" "abc" "invalid"     
// Error "Tên là bắt buộc" - DỪNG TẠI LỖI ĐẦU TIÊN

let testApplicative = createUserApplicative "" "abc" "invalid"  
// Error "Tên là bắt buộc" - VẪN CHỈ HIỆN 1 LỖI (Result không tích lũy lỗi)
```

### 🎯 Phân Biệt 2 Cách Monad Implementation

#### 🔄 **Monadic Composition (`>>=`)** - Thực Hiện Tuần Tự
```fsharp
// 🏃‍♂️ LUỒNG THỰC HIỆN:
// Step 1: validateName → nếu OK → Step 2 → nếu OK → Step 3
// ❌ Fail-Fast: Dừng ngay khi gặp lỗi đầu tiên

// 💡 KHI NÀO DÙNG:
// - Các bước phụ thuộc lẫn nhau
// - Muốn dừng ngay khi có lỗi (tiết kiệm CPU)
// - Logic nghiệp vụ yêu cầu thứ tự

// Ví dụ: Đăng nhập → Lấy profile → Kiểm tra quyền
let loginFlow username password =
    authenticateUser username password    // Phải thành công trước
    >>= fun user -> getUserProfile user.Id  // Mới lấy được profile  
    >>= fun profile -> checkPermissions profile // Mới check được quyền
```

#### ⚡ **Applicative Style (`<*>`)** - Thực Hiện Song Song
```fsharp  
// 🔥 LUỒNG THỰC HIỆN:
// Tất cả validations chạy đồng thời (có thể parallel)
// ❌ Với Result: vẫn chỉ hiện 1 lỗi đầu tiên

// 💡 KHI NÀO DÙNG:
// - Các validation độc lập nhau  
// - Muốn hiệu suất tốt hơn (parallel)
// - Form validation (user cần biết tất cả lỗi)

// 🛠️ ĐỂ THU THẬP TẤT CẢ LỖI: Cần custom type
type ValidationResult<'T> = 
    | Valid of 'T
    | Invalid of string list    // 📝 Danh sách lỗi

let combineResults fResult xResult =
    match fResult, xResult with
    | Valid f, Valid x -> Valid (f x)
    | Valid _, Invalid errors -> Invalid errors
    | Invalid errors, Valid _ -> Invalid errors
    | Invalid e1, Invalid e2 -> Invalid (e1 @ e2)  // 🎯 TÍCH LŨY LỖI!

// Bây giờ có thể hiện tất cả lỗi cùng lúc!
let validateUserComplete name age email =
    createUser
    <!> validateNameV name      // ValidationResult
    <*> validateAgeV age        // ValidationResult  
    <*> validateEmailV email    // ValidationResult
    // → Invalid ["Tên trống"; "Tuổi không hợp lệ"; "Email sai format"]
```

#### 📊 Bảng So Sánh Chi Tiết

| Khía Cạnh | Monadic (`>>=`) | Applicative (`<*>`) |
|-----------|----------------|---------------------|
| **Thực hiện** | Tuần tự (step-by-step) | Song song (parallel) |
| **Phụ thuộc** | Bước sau cần kết quả bước trước | Các bước độc lập |
| **Hiệu suất** | Chậm hơn (sequential) | Nhanh hơn (concurrent) | 
| **Lỗi hiển thị** | Chỉ lỗi đầu tiên | Có thể tất cả (nếu custom type) |
| **Use case** | Login flow, Database transactions | Form validation, Config parsing |

### 🌟 Option Monad Patterns

```fsharp
module Option =
    let bind f option =
        match option with
        | Some value -> f value
        | None -> None
    
    let map f option =
        match option with  
        | Some value -> Some (f value)
        | None -> None

// Safe navigation - duyệt an toàn
let tryGetProperty (obj: 'T option) (getter: 'T -> 'U option) =
    obj >>= getter  // Chỉ thực hiện nếu obj có giá trị

// Ví dụ: Chuỗi lookup an toàn
let tryGetUser userId database =
    database.Users.TryFind userId  // Tìm user, trả Option

let tryGetProfile user =
    user.Profile    // Lấy profile của user

let tryGetAddress profile =
    profile.Address // Lấy địa chỉ từ profile

// Kết hợp an toàn - chỉ thành công khi tất cả đều tồn tại
let getUserAddress userId database =
    tryGetUser userId database
    >>= tryGetProfile    // Nếu tìm thấy user
    >>= tryGetAddress    // Và có profile, và có address
```

---

## Function Composition - Kết Hợp Hàm

### 🔗 Kết Hợp Cơ Bản

```fsharp
// Kết hợp tiến (>>): thực hiện từ trái sang phải
let addOne x = x + 1
let multiplyByTwo x = x * 2
let addOneThenDouble = addOne >> multiplyByTwo  // Cộng 1 rồi nhân 2

let result1 = addOneThenDouble 5    // 12 (5 + 1 = 6, 6 * 2 = 12)

// Kết hợp lùi (<<): thực hiện từ phải sang trái
let doubleFirstThenAddOne = addOne << multiplyByTwo  // Nhân 2 rồi cộng 1
let result2 = doubleFirstThenAddOne 5   // 11 (5 * 2 = 10, 10 + 1 = 11)
```

### 🎭 Complex Function Composition

```fsharp
// String processing pipeline
let cleanString = 
    String.filter (fun c -> c <> ' ')
    >> String.map Char.ToLower
    >> (fun s -> s.Trim())

// Mathematical computation pipeline  
let statisticalAnalysis =
    List.filter (fun x -> x > 0.0)          // Remove negative values
    >> List.map (fun x -> x * x)            // Square each value
    >> List.sort                            // Sort ascending
    >> (fun xs -> 
        let sum = List.sum xs
        let count = List.length xs
        sum / float count)                   // Calculate mean

// Usage
let numbers = [1.0; -2.0; 3.0; 4.0; -1.0]
let result = statisticalAnalysis numbers    // Mean of squared positive numbers
```

### 🏭 Function Factories with Composition

```fsharp
// Create configurable processing pipelines
let createTextProcessor (options: string list) =
    let processors = [
        if options |> List.contains "trim" then yield fun (s: string) -> s.Trim()
        if options |> List.contains "lower" then yield fun (s: string) -> s.ToLower()
        if options |> List.contains "reverse" then yield fun (s: string) -> 
            s.ToCharArray() |> Array.rev |> String
    ]
    
    processors |> List.reduce (>>)

// Usage
let processor1 = createTextProcessor ["trim"; "lower"]
let processor2 = createTextProcessor ["lower"; "reverse"]

let result1 = processor1 "  HELLO WORLD  "     // "hello world"
let result2 = processor2 "Hello"               // "olleh"
```

---

## Pipeline Transformations - Biến Đổi Pipeline

### 🚰 Toán Tử Pipeline (|>)

```fsharp
// Pipeline cơ bản - dễ đọc từ trên xuống
let result = 
    [1; 2; 3; 4; 5]
    |> List.filter (fun x -> x % 2 = 0)     // Lọc số chẵn: [2; 4]
    |> List.map (fun x -> x * x)            // Bình phương: [4; 16]
    |> List.sum                             // Tính tổng: 20

// Tương đương không dùng pipeline (khó đọc hơn)
let result' = List.sum (List.map (fun x -> x * x) (List.filter (fun x -> x % 2 = 0) [1; 2; 3; 4; 5]))
```

### 🌊 Các Mẫu Pipeline Nâng Cao

#### Pipeline Có Điều Kiện
```fsharp
let processData includeFilter includeSort data =
    data
    |> (if includeFilter then List.filter (fun x -> x > 0) else id)  // Lọc điều kiện
    |> List.map (fun x -> x * 2)                                      // Nhân đôi tất cả
    |> (if includeSort then List.sort else id)                        // Sắp xếp điều kiện

// Cách sử dụng
let data = [3; -1; 4; -2; 5]
let result1 = processData true true data      // [6; 8; 10] (lọc và sắp xếp)
let result2 = processData false false data    // [6; -2; 8; -4; 10] (không lọc, không sắp xếp)
```

#### Pipeline Phân Nhánh
```fsharp
// Chia xử lý dữ liệu thành nhiều nhánh
let analyzeNumbers numbers =
    let positives = numbers |> List.filter (fun x -> x > 0)  // Lọc số dương
    let negatives = numbers |> List.filter (fun x -> x < 0)  // Lọc số âm
    
    let positiveStats = 
        positives 
        |> List.map float      // Chuyển sang float
        |> List.average        // Tính trung bình số dương
        
    let negativeCount = negatives |> List.length  // Đếm số âm
    
    {| PositiveAverage = positiveStats; NegativeCount = negativeCount |}
```

#### Pipeline Bất Đồng Bộ
```fsharp
let processFileAsync filename =
    filename
    |> File.ReadAllTextAsync                    // Đọc file async
    |> Async.AwaitTask                          // Chuyển Task thành Async
    |> Async.map (fun content -> content.Split('\n'))  // Chia thành các dòng
    |> Async.map (Array.filter (fun line -> not (String.IsNullOrEmpty(line))))  // Lọc dòng rỗng
    |> Async.map Array.length                   // Đếm số dòng

// Custom async operators
module Async =
    let map f asyncValue = async {
        let! value = asyncValue
        return f value
    }
    
    let bind f asyncValue = async {
        let! value = asyncValue
        return! f value
    }
```

---

## Validation Patterns - Các Mẫu Validation

### 🎯 Fail-Fast vs. Thu Thập Lỗi

#### Mẫu Fail-Fast (Result với >>=)
```fsharp
type ValidationError = 
    | NameRequired
    | InvalidAge of string
    | InvalidEmail of string

let validateUserFailFast name ageStr email =
    let validateName n = 
        if String.IsNullOrEmpty(n) then Error NameRequired else Ok n
    
    let validateAge a = 
        match System.Int32.TryParse(a) with
        | true, age when age >= 0 -> Ok age
        | _ -> Error (InvalidAge a)
    
    let validateEmail e = 
        if e.Contains("@") then Ok e else Error (InvalidEmail e)
    
    validateName name
    >>= fun validName ->
        validateAge ageStr
        >>= fun validAge ->
            validateEmail email
            >>= fun validEmail ->
                Ok (validName, validAge, validEmail)

// Stops at first error
let result1 = validateUserFailFast "" "abc" "invalid"  // Error NameRequired
```

#### Accumulating Errors Pattern
```fsharp
type ValidationResult<'T> = 
    | Valid of 'T
    | Invalid of ValidationError list

module ValidationResult =
    let map f = function
        | Valid value -> Valid (f value)
        | Invalid errors -> Invalid errors
    
    let apply fResult xResult =
        match fResult, xResult with
        | Valid f, Valid x -> Valid (f x)
        | Valid _, Invalid errors -> Invalid errors
        | Invalid errors, Valid _ -> Invalid errors
        | Invalid errors1, Invalid errors2 -> Invalid (errors1 @ errors2)
    
    let bind f = function
        | Valid value -> f value
        | Invalid errors -> Invalid errors

// Custom operators for accumulating validation
let (<!>) = ValidationResult.map
let (<*>) = ValidationResult.apply

let validateUserAccumulating name ageStr email =
    let validateName n = 
        if String.IsNullOrEmpty(n) then Invalid [NameRequired] else Valid n
    
    let validateAge a = 
        match System.Int32.TryParse(a) with
        | true, age when age >= 0 -> Valid age
        | _ -> Invalid [InvalidAge a]
    
    let validateEmail e = 
        if e.Contains("@") then Valid e else Invalid [InvalidEmail e]
    
    let createUser name age email = (name, age, email)
    
    createUser 
    <!> validateName name
    <*> validateAge ageStr  
    <*> validateEmail email

// Collects all errors
let result2 = validateUserAccumulating "" "abc" "invalid"  
// Invalid [NameRequired; InvalidAge "abc"; InvalidEmail "invalid"]
```

### 🏗️ Các Trường Hợp Validation Phức Tạp

#### Validation Object Lồng Nhau
```fsharp
type Address = { Street: string; City: string; ZipCode: string }
type Person = { Name: string; Age: int; Address: Address }

// Validate địa chỉ với nhiều trường
let validateAddress street city zipCode =
    let validateStreet s = 
        if String.IsNullOrEmpty(s) then Invalid ["Cần nhập đường"] else Valid s
    let validateCity c = 
        if String.IsNullOrEmpty(c) then Invalid ["Cần nhập thành phố"] else Valid c
    let validateZip z = 
        if System.Text.RegularExpressions.Regex.IsMatch(z, @"^\d{5}$") 
        then Valid z else Invalid ["Mã zip không hợp lệ"]
    
    let createAddress street city zip = { Street = street; City = city; ZipCode = zip }
    
    createAddress
    <!> validateStreet street    // Áp dụng validateStreet
    <*> validateCity city        // Kết hợp validateCity  
    <*> validateZip zipCode      // Kết hợp validateZip

// Validate person với địa chỉ lồng nhau
let validatePerson name ageStr street city zipCode =
    let validateName n = 
        if String.IsNullOrEmpty(n) then Invalid ["Cần nhập tên"] else Valid n
    let validateAge a = 
        match System.Int32.TryParse(a) with
        | true, age when age >= 0 -> Valid age
        | _ -> Invalid ["Tuổi không hợp lệ"]
    
    let createPerson name age address = { Name = name; Age = age; Address = address }
    
    createPerson
    <!> validateName name           // Validate tên
    <*> validateAge ageStr          // Validate tuổi
    <*> validateAddress street city zipCode  // Validate địa chỉ (lồng)
```

---

## Ví Dụ Thực Tế

### 🛍️ Xử Lý Đơn Hàng E-Commerce

```fsharp
type Product = { Id: string; Name: string; Price: decimal }  // Sản phẩm
type OrderItem = { Product: Product; Quantity: int }        // Một item trong đơn
type Order = { Items: OrderItem list; Customer: string }    // Đơn hàng

// Higher-order functions for order processing
let applyDiscount discountFn order =
    { order with Items = order.Items |> List.map discountFn }

let calculateTotal order =
    order.Items 
    |> List.map (fun item -> item.Product.Price * decimal item.Quantity)
    |> List.sum

// Custom operators for order transformations
let (|+|) order item = { order with Items = item :: order.Items }
let (|*|) order multiplier = 
    { order with Items = order.Items |> List.map (fun i -> { i with Quantity = i.Quantity * multiplier }) }

// Pipeline processing
let processOrder customer items =
    { Items = []; Customer = customer }
    |> (fun order -> items |> List.fold (|+|) order)
    |> applyDiscount (fun item -> 
        if item.Quantity >= 5 then 
            { item with Product = { item.Product with Price = item.Product.Price * 0.9m } }
        else item)
    |> (fun order -> (order, calculateTotal order))
```

### 🌐 Web API Response Processing

```fsharp
type ApiResponse<'T> = 
    | Success of 'T
    | NotFound
    | ServerError of string
    | ValidationError of string list

module ApiResponse =
    let map f = function
        | Success value -> Success (f value)
        | NotFound -> NotFound
        | ServerError msg -> ServerError msg  
        | ValidationError errors -> ValidationError errors
    
    let bind f = function
        | Success value -> f value
        | NotFound -> NotFound
        | ServerError msg -> ServerError msg
        | ValidationError errors -> ValidationError errors

// Custom operators cho API
let (>>=) = ApiResponse.bind  // Kết nối API calls
let (<!>) = ApiResponse.map   // Biến đổi kết quả API

// Pipeline xử lý API calls liên tiếp
let processApiCall userId =
    fetchUser userId                    // Gọi API lấy user
    >>= fun user ->                     // Nếu thành công
        fetchUserProfile user.Id        // Gọi API lấy profile  
        >>= fun profile ->              // Nếu profile thành công
            fetchUserPreferences user.Id // Gọi API lấy preferences
            <!> fun preferences ->      // Biến đổi kết quả cuối
                (user, profile, preferences)  // Trả về tuple
```

---

## Góc Nhìn Angular Developer

### 🔄 So Sánh F# vs Angular/RxJS Patterns

| Khái Niệm | F# | Angular/RxJS | Mục Đích |
|-----------|----|--------------|-----------|
| **Higher-Order Functions** | `List.map`, `List.filter` | `array.map()`, `array.filter()` | Biến đổi dữ liệu |
| **Custom Operators** | `>>=`, `<!>` | Custom RxJS operators | Phép toán đặc thù domain |
| **Monads** | `Result`, `Option`, `Async` | `Observable`, `Promise` | Kết hợp các phép tính |
| **Function Composition** | `>>`, `<<` | `pipe()`, method chaining | Xây dựng phép toán phức tạp |
| **Pipelines** | `\|>` | `.pipe()` trong RxJS | Luồng dữ liệu |
| **Validation** | Applicative functors | Form validators | Validation đầu vào |

### 🎯 Practical Comparisons

#### Pipeline Xử Lý Dữ Liệu

**Phong Cách F#:**
```fsharp
let processUsers users =
    users
    |> List.filter (fun u -> u.Age >= 18)                        // Lọc user >= 18 tuổi
    |> List.map (fun u -> { u with Name = u.Name.ToUpper() })     // Chuyển tên thành chữ hoa
    |> List.sortBy (fun u -> u.Name)                             // Sắp xếp theo tên
    |> List.take 10                                              // Lấy 10 người đầu
```

**Phong Cách Angular/RxJS:**
```typescript
processUsers(users$: Observable<User[]>): Observable<User[]> {
  return users$.pipe(
    map(users => users.filter(u => u.age >= 18)),           // Lọc user >= 18 tuổi
    map(users => users.map(u => ({ ...u, name: u.name.toUpperCase() }))),  // Chuyển tên hoa
    map(users => users.sort((a, b) => a.name.localeCompare(b.name))),
    map(users => users.slice(0, 10))
  );
}
```

#### Error Handling

**F# Result Pattern:**
```fsharp
let validateAndSave user =
    validateUser user
    >>= saveToDatabase
    >>= sendWelcomeEmail
```

**Angular Promise/Observable Pattern:**
```typescript
validateAndSave(user: User): Observable<void> {
  return this.validateUser(user).pipe(
    switchMap(validUser => this.saveToDatabase(validUser)),
    switchMap(savedUser => this.sendWelcomeEmail(savedUser))
  );
}
```

### 💡 Key Takeaways for Angular Developers

1. **F# pipelines** are like RxJS `.pipe()` but for any data type
2. **F# custom operators** are similar to custom RxJS operators  
3. **F# monads** provide the same composability as Observables but with different semantics
4. **F# function composition** is like method chaining but more flexible
5. **F# validation patterns** offer structured error handling like Angular reactive forms

---

## 🚀 Getting Started Exercises

### Exercise 1: Build Your First Higher-Order Function
```fsharp
// Create a function that applies a transformation twice
let applyTwice transform value = 
    // Your implementation here
    transform (transform value)

// Test with different functions
let addOne x = x + 1
let double x = x * 2

let result1 = applyTwice addOne 5     // Should be 7
let result2 = applyTwice double 3     // Should be 12
```

### Exercise 2: Create Custom Operators
```fsharp
// Define a custom operator for safe division
let (/?) x y = 
    if y = 0 then None 
    else Some (x / y)

// Usage
let result1 = 10 /? 2    // Some 5
let result2 = 10 /? 0    // None
```

### Exercise 3: Build a Validation Pipeline
```fsharp
// Create a user registration validator
type User = { Username: string; Email: string; Age: int }

let validateUser username email ageStr =
    // Implement using applicative validation pattern
    // Should collect all validation errors
    
let result = validateUser "john" "john@email.com" "25"
```

---

## 📚 Tóm Tắt

Những concepts F# nâng cao này cung cấp các công cụ mạnh mẽ cho:

- 🎯 **Tái Sử Dụng Code** - Higher-order functions cho phép các phép toán generic
- 🔧 **Domain Modeling** - Custom operators tạo APIs biểu cảm  
- 🛡️ **Xử Lý Lỗi** - Monadic patterns cung cấp quản lý lỗi có cấu trúc
- 🚰 **Luồng Dữ Liệu** - Pipelines làm cho biến đổi dữ liệu rõ ràng và dễ đọc
- ✅ **Validation** - Applicative patterns cho phép validation đầu vào tinh vi
- 🏗️ **Kết Hợp** - Function composition xây dựng phép toán phức tạp từ những phần đơn giản

**Bước Tiếp Theo:**
1. Thực hành với các bài tập trên
2. Thử implement monads của riêng bạn
3. Tạo các operators đặc thù domain cho projects
4. Thử nghiệm với các validation patterns khác nhau

Chúc bạn functional programming vui vẻ! 🎉