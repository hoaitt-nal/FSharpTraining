# 🛡️ F# Validation Patterns - Complete Guide

> **Hướng dẫn toàn diện về F# Validation Patterns** - Xây dựng validation logic mạnh mẽ, type-safe và maintainable

## 🚀 Quick Start
```fsharp
// Simple validation với Result type
let validateEmail email =
    if String.IsNullOrWhiteSpace(email) then Error "Email không được rỗng"
    elif not (email.Contains("@")) then Error "Email không hợp lệ"
    else Ok email

// Usage
match validateEmail "user@domain.com" with
| Ok validEmail -> printfn "Email hợp lệ: %s" validEmail
| Error msg -> printfn "Lỗi: %s" msg
```

## 📋 Table of Contents

### 🎯 **Fundamentals**
1. [Introduction & Philosophy](#introduction--philosophy) - Tại sao validation quan trọng
2. [Basic Validation Types](#basic-validation-types) - Result, Option, Choice
3. [Validation Functions](#validation-functions) - Core building blocks

### 🔧 **Core Patterns**
4. [Single Field Validation](#single-field-validation) - Validate từng field
5. [Composite Validation](#composite-validation) - Combine multiple validations
6. [Applicative Validation](#applicative-validation) - Collect all errors
7. [Railway-Oriented Programming](#railway-oriented-programming) - Error pipeline

### 🏗️ **Advanced Techniques**
8. [Domain Modeling](#domain-modeling) - Make illegal states unrepresentable
9. [Custom Validation Types](#custom-validation-types) - Specialized validators
10. [Async Validation](#async-validation) - Database & external service checks
11. [Validation Combinators](#validation-combinators) - Composable validators

### 🛠️ **Real-world Applications**
12. [Form Validation](#form-validation) - Web forms & UI
13. [API Input Validation](#api-input-validation) - REST API validation
14. [Business Rules Validation](#business-rules-validation) - Domain logic
15. [Data Import Validation](#data-import-validation) - File & batch processing

### 📚 **Reference & Best Practices**
16. [Validation Patterns Reference](#validation-patterns-reference) - All patterns
17. [Performance & Optimization](#performance--optimization) - Efficient validation
18. [Testing Validation Logic](#testing-validation-logic) - Test strategies

---

## Introduction & Philosophy

**F# Validation Patterns** tận dụng type system mạnh mẽ của F# để tạo ra validation logic an toàn, composable và expressive.

### 🎯 **Core Philosophy: Make Illegal States Unrepresentable**

```fsharp
// ❌ Weak: String có thể là anything
type User = { Email: string; Age: string }

// ✅ Strong: Types enforce constraints  
type Email = Email of string
type Age = Age of int
type User = { Email: Email; Age: Age }
```

### **🛡️ Benefits của F# Validation**

| **F# Approach** | **Traditional Approach** | **Advantage** |
|-----------------|-------------------------|---------------|
| Type-driven validation | Runtime string checking | ✅ Compile-time safety |
| Composable functions | Scattered if/else | ✅ Reusable & testable |
| Railway-oriented | Nested try/catch | ✅ Clean error flow |
| Applicative validation | Fail-fast validation | ✅ Collect all errors |

### **🚀 Key Principles**
- **🎯 Type Safety**: Use types để enforce business rules
- **🔧 Composability**: Small functions combine thành complex validation
- **⚡ Performance**: Fail fast nhưng collect errors khi cần
- **🧪 Testability**: Mỗi validation rule dễ test independently
- **📝 Expressiveness**: Validation code reads like business requirements

---

## Basic Validation Types

### 🔧 **Core Types for Validation**

#### **1. Result<'Success, 'Error> - Success or Error**
```fsharp
type ValidationResult<'T> = Result<'T, string>

// Basic usage
let validatePositive x =
    if x > 0 then Ok x
    else Error "Số phải dương"

// Example
validatePositive 5   // Ok 5
validatePositive -1  // Error "Số phải dương"
```

#### **2. Option<'T> - Present or Absent**
```fsharp
let tryParseInt str =
    match System.Int32.TryParse(str) with
    | true, value -> Some value
    | false, _ -> None

// Usage với Option.bind
let validateAge str =
    str
    |> tryParseInt
    |> Option.bind (fun age -> if age >= 0 && age <= 120 then Some age else None)
```

#### **3. Choice<'Success, 'Error> - Alternative to Result**
```fsharp
let validateEmail email =
    if String.IsNullOrWhiteSpace(email) then Choice2Of2 "Email rỗng"
    elif email.Contains("@") then Choice1Of2 email
    else Choice2Of2 "Email không có @"
```

### **🎯 Choosing the Right Type**

| **Type** | **Use Case** | **When to Use** |
|----------|--------------|-----------------|
| `Result<'T, string>` | Single error message | Simple validation with descriptive errors |
| `Result<'T, 'Error list>` | Multiple errors | Collect all validation errors |
| `Option<'T>` | Optional validation | When absence is not necessarily an error |
| Custom discriminated union | Domain-specific errors | Complex business validation |

---

## Validation Functions

### 📝 **Building Block Functions**

#### **🎯 Basic Validators**
```fsharp
// String validators
let notEmpty str = 
    if String.IsNullOrWhiteSpace(str) then Error "Không được rỗng"
    else Ok str

let minLength min str =
    if String.length str >= min then Ok str
    else Error $"Tối thiểu {min} ký tự"

let maxLength max str =
    if String.length str <= max then Ok str
    else Error $"Tối đa {max} ký tự"

let containsChar char str =
    if str |> String.exists ((=) char) then Ok str
    else Error $"Phải chứa '{char}'"

// Numeric validators
let between min max value =
    if value >= min && value <= max then Ok value
    else Error $"Phải từ {min} đến {max}"

let positive value =
    if value > 0 then Ok value
    else Error "Phải là số dương"
```

#### **🔄 Composition Helpers**
```fsharp
// Result combinators
let (>>=) result f = Result.bind f result
let (<!>) f result = Result.map f result
let (<*>) resultF resultX = 
    match resultF, resultX with
    | Ok f, Ok x -> Ok (f x)
    | Error e, Ok _ -> Error e
    | Ok _, Error e -> Error e
    | Error e1, Error e2 -> Error e1 // Or combine errors

// Validation pipeline
let (>=>) f g = fun x -> f x >>= g

// Usage examples
let validateUserName = 
    notEmpty 
    >=> minLength 3 
    >=> maxLength 20
```

---

## Single Field Validation

### 🎯 **Individual Field Validators**

#### **📧 Email Validation**
```fsharp
open System.Text.RegularExpressions

type EmailError = 
    | EmailEmpty
    | EmailInvalid
    | EmailTooLong

let validateEmail (email: string) =
    let emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
    
    if String.IsNullOrWhiteSpace(email) then Error EmailEmpty
    elif email.Length > 254 then Error EmailTooLong
    elif not (Regex.IsMatch(email, emailPattern)) then Error EmailInvalid
    else Ok email

// Friendly error messages
let emailErrorMessage = function
    | EmailEmpty -> "Email không được rỗng"
    | EmailInvalid -> "Định dạng email không hợp lệ"
    | EmailTooLong -> "Email quá dài (tối đa 254 ký tự)"
```

#### **🔒 Password Validation**
```fsharp
type PasswordError =
    | PasswordTooShort
    | PasswordMissingUpper
    | PasswordMissingLower  
    | PasswordMissingNumber
    | PasswordMissingSpecial

let validatePassword (password: string) =
    let errors = [
        if password.Length < 8 then yield PasswordTooShort
        if not (password |> String.exists Char.IsUpper) then yield PasswordMissingUpper
        if not (password |> String.exists Char.IsLower) then yield PasswordMissingLower
        if not (password |> String.exists Char.IsDigit) then yield PasswordMissingNumber
        if not (password |> String.exists (fun c -> "!@#$%^&*()".Contains(c))) then 
            yield PasswordMissingSpecial
    ]
    
    match errors with
    | [] -> Ok password
    | errs -> Error errs

let passwordErrorMessages errors =
    errors |> List.map (function
        | PasswordTooShort -> "Mật khẩu tối thiểu 8 ký tự"
        | PasswordMissingUpper -> "Phải có ít nhất 1 chữ hoa"
        | PasswordMissingLower -> "Phải có ít nhất 1 chữ thường"
        | PasswordMissingNumber -> "Phải có ít nhất 1 số"
        | PasswordMissingSpecial -> "Phải có ít nhất 1 ký tự đặc biệt")
```

#### **📞 Phone Number Validation**
```fsharp
let validateVietnamesePhone (phone: string) =
    let cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "")
    let phonePattern = @"^(\+84|84|0)(3|5|7|8|9)[0-9]{8}$"
    
    if String.IsNullOrWhiteSpace(phone) then Error "Số điện thoại không được rỗng"
    elif not (Regex.IsMatch(cleanPhone, phonePattern)) then 
        Error "Số điện thoại Việt Nam không hợp lệ"
    else Ok cleanPhone
```

#### **🎂 Age Validation**
```fsharp
open System

let validateAge (birthDate: DateTime) =
    let today = DateTime.Today
    let age = today.Year - birthDate.Year
    let adjustedAge = 
        if today < birthDate.AddYears(age) then age - 1 else age
    
    if birthDate > today then Error "Ngày sinh không thể ở tương lai"
    elif adjustedAge < 0 then Error "Tuổi không hợp lệ"
    elif adjustedAge > 150 then Error "Tuổi quá lớn"
    else Ok adjustedAge
```

---

## Composite Validation

### 🔗 **Combining Multiple Validations**

#### **🎯 Sequential Validation (Fail Fast)**
```fsharp
// Validate theo thứ tự, dừng khi gặp lỗi đầu tiên
let validateUserNameSequential userName =
    userName
    |> notEmpty
    >>= minLength 3
    >>= maxLength 20
    >>= (fun name -> 
        if Regex.IsMatch(name, @"^[a-zA-Z0-9_]+$") then Ok name
        else Error "Chỉ được chứa chữ, số và dấu gạch dưới")

// Usage
let result = validateUserNameSequential "abc123"  // Ok "abc123"
let error = validateUserNameSequential "ab"       // Error "Tối thiểu 3 ký tự"
```

#### **📋 Applicative Validation (Collect All Errors)**
```fsharp
// Collect tất cả errors, không dừng khi gặp lỗi đầu tiên
type ValidationError = string
type ValidationResult<'T> = Result<'T, ValidationError list>

let validateUserNameApplicative userName =
    let validations = [
        if String.IsNullOrWhiteSpace(userName) then yield "Không được rỗng"
        if userName.Length < 3 then yield "Tối thiểu 3 ký tự"
        if userName.Length > 20 then yield "Tối đa 20 ký tự"
        if not (Regex.IsMatch(userName, @"^[a-zA-Z0-9_]+$")) then 
            yield "Chỉ được chứa chữ, số và dấu gạch dưới"
    ]
    
    match validations with
    | [] -> Ok userName
    | errors -> Error errors

// Usage
let result1 = validateUserNameApplicative "abc123"  // Ok "abc123"
let result2 = validateUserNameApplicative "a@"      // Error ["Tối thiểu 3 ký tự"; "Chỉ được chứa..."]
```

#### **🏗️ Building Composite Validators**
```fsharp
// Helper functions cho applicative validation
let validateAll validations value =
    let errors = 
        validations 
        |> List.choose (fun validate -> 
            match validate value with
            | Ok _ -> None
            | Error err -> Some err)
    
    match errors with
    | [] -> Ok value
    | errs -> Error errs

// Define individual validation rules
let userNameValidations = [
    fun name -> if String.IsNullOrWhiteSpace(name) then Error "Không được rỗng" else Ok name
    fun name -> if name.Length >= 3 then Ok name else Error "Tối thiểu 3 ký tự"
    fun name -> if name.Length <= 20 then Ok name else Error "Tối đa 20 ký tự"
    fun name -> if Regex.IsMatch(name, @"^[a-zA-Z0-9_]+$") then Ok name 
                else Error "Ký tự không hợp lệ"
]

// Composite validator
let validateUserName = validateAll userNameValidations
```

---

## Applicative Validation

### 📋 **Collecting All Validation Errors**

#### **🎯 Applicative Functor Pattern**
```fsharp
// Advanced applicative validation with custom operators
module Validation =
    type Validation<'Success, 'Error> = 
        | Success of 'Success
        | Failure of 'Error list
    
    let succeed value = Success value
    let fail error = Failure [error]
    let failMany errors = Failure errors
    
    let map f validation =
        match validation with
        | Success value -> Success (f value)
        | Failure errors -> Failure errors
    
    let apply validationF validationX =
        match validationF, validationX with
        | Success f, Success x -> Success (f x)
        | Success f, Failure errors -> Failure errors
        | Failure errors, Success x -> Failure errors
        | Failure errors1, Failure errors2 -> Failure (errors1 @ errors2)
    
    // Operators
    let (<$>) = map
    let (<*>) = apply
    
    // Convert from Result
    let ofResult result =
        match result with
        | Ok value -> Success value
        | Error error -> Failure [error]

// Usage example
open Validation

type User = { Name: string; Email: string; Age: int }

let validateUser name email age =
    let createUser n e a = { Name = n; Email = e; Age = a }
    
    createUser
    <$> (validateUserName name |> ofResult)
    <*> (validateEmail email |> ofResult)  
    <*> (validateAge age |> ofResult)

// Test
let validUser = validateUser "john_doe" "john@example.com" 25
// Success { Name = "john_doe"; Email = "john@example.com"; Age = 25 }

let invalidUser = validateUser "ab" "invalid-email" -5
// Failure ["Tối thiểu 3 ký tự"; "Email không hợp lệ"; "Tuổi phải dương"]
```

#### **🔄 Applicative with Custom Types**
```fsharp
type UserRegistration = {
    UserName: string
    Email: string
    Password: string
    ConfirmPassword: string
    Terms: bool
}

type RegistrationError =
    | UserNameError of string list
    | EmailError of string list
    | PasswordError of string list
    | PasswordMismatch
    | TermsNotAccepted

let validateRegistration (reg: UserRegistration) =
    let validations = [
        // Username validation
        match validateUserName reg.UserName with
        | Ok _ -> None
        | Error errors -> Some (UserNameError errors)
        
        // Email validation  
        match validateEmail reg.Email with
        | Ok _ -> None
        | Error errors -> Some (EmailError [errors])
        
        // Password validation
        match validatePassword reg.Password with
        | Ok _ -> None
        | Error errors -> Some (PasswordError (passwordErrorMessages errors))
        
        // Password confirmation
        if reg.Password <> reg.ConfirmPassword then 
            Some PasswordMismatch else None
        
        // Terms acceptance
        if not reg.Terms then Some TermsNotAccepted else None
    ]
    
    let errors = validations |> List.choose id
    
    match errors with
    | [] -> Ok reg
    | errs -> Error errs
```

---

## Railway-Oriented Programming

### 🚂 **Error Pipeline Pattern**

#### **🎯 Basic Railway Pattern**
```fsharp
// Railway functions: Input -> Result<Output, Error>
let validateInput input = 
    if String.IsNullOrEmpty(input) then Error "Input rỗng"
    else Ok input

let trimInput input = Ok (input.Trim())

let convertToUpper input = Ok (input.ToUpper())

let validateLength input =
    if input.Length > 10 then Error "Quá dài"
    else Ok input

// Compose railway functions
let processInput input =
    input
    |> validateInput
    >>= trimInput
    >>= convertToUpper  
    >>= validateLength

// Usage
let result1 = processInput "  hello  "    // Ok "HELLO"
let result2 = processInput ""             // Error "Input rỗng"
let result3 = processInput "very long string"  // Error "Quá dài"
```

#### **🔄 Advanced Railway with Switch Functions**
```fsharp
// Switch function: normal function -> railway function
let switch f input = 
    try Ok (f input)
    with ex -> Error ex.Message

// Dead-end function: returns unit, might throw
let logSuccess input = 
    printfn "Processed successfully: %s" input
    input

// Convert to railway
let logSuccessR = switch logSuccess

// Railway pipeline with logging
let processWithLogging input =
    input
    |> validateInput
    >>= trimInput
    >>= convertToUpper
    >>= logSuccessR  // Log success step
    >>= validateLength
```

#### **🎯 Parallel Railway Tracks**
```fsharp
// Process multiple inputs, collect all errors
let processMultipleInputs inputs =
    inputs
    |> List.map processInput
    |> List.partition (function Ok _ -> true | Error _ -> false)
    |> fun (successes, failures) ->
        let values = successes |> List.choose (function Ok v -> Some v | _ -> None)
        let errors = failures |> List.choose (function Error e -> Some e | _ -> None)
        
        match errors with
        | [] -> Ok values
        | errs -> Error errs

// Usage
let inputs = ["hello"; ""; "world"; "very long string"]
let result = processMultipleInputs inputs
// Error ["Input rỗng"; "Quá dài"]
```

---

## Domain Modeling

### 🏗️ **Making Illegal States Unrepresentable**

#### **🎯 Smart Constructors Pattern**
```fsharp
// Private constructors enforce validation
type Email = private Email of string
    with 
    static member Create(email: string) =
        if String.IsNullOrWhiteSpace(email) then Error "Email rỗng"
        elif not (email.Contains("@")) then Error "Email không hợp lệ"
        else Ok (Email email)
    
    member this.Value = let (Email email) = this in email

type Age = private Age of int
    with
    static member Create(age: int) =
        if age < 0 then Error "Tuổi không thể âm"
        elif age > 150 then Error "Tuổi quá lớn"
        else Ok (Age age)
    
    member this.Value = let (Age age) = this in age

// Usage - bắt buộc phải validate
let createUser email age =
    match Email.Create(email), Age.Create(age) with
    | Ok validEmail, Ok validAge -> 
        Ok { Email = validEmail; Age = validAge }
    | Error emailErr, Ok _ -> Error emailErr
    | Ok _, Error ageErr -> Error ageErr  
    | Error emailErr, Error ageErr -> Error $"{emailErr}; {ageErr}"
```

#### **🔒 Constrained Types**
```fsharp
// Percentage: 0-100 only
type Percentage = private Percentage of decimal
    with
    static member Create(value: decimal) =
        if value < 0m then Error "Phần trăm không thể âm"
        elif value > 100m then Error "Phần trăm không thể lớn hơn 100"
        else Ok (Percentage value)
    
    member this.Value = let (Percentage p) = this in p
    
    // Safe operations
    static member (+) (Percentage a, Percentage b) =
        let sum = a + b
        if sum > 100m then Percentage 100m else Percentage sum

// Positive integer
type PositiveInt = private PositiveInt of int
    with
    static member Create(value: int) =
        if value <= 0 then Error "Phải là số nguyên dương"
        else Ok (PositiveInt value)
    
    member this.Value = let (PositiveInt i) = this in i

// Non-empty string
type NonEmptyString = private NonEmptyString of string
    with
    static member Create(value: string) =
        if String.IsNullOrWhiteSpace(value) then Error "Chuỗi không được rỗng"
        else Ok (NonEmptyString value.Trim())
    
    member this.Value = let (NonEmptyString s) = this in s
```

#### **🏪 Business Domain Example**
```fsharp
// E-commerce domain với validation
type ProductId = private ProductId of System.Guid
    with
    static member Create() = ProductId (System.Guid.NewGuid())
    static member Parse(str: string) =
        match System.Guid.TryParse(str) with
        | true, guid -> Ok (ProductId guid)
        | false, _ -> Error "Product ID không hợp lệ"

type Money = private Money of decimal
    with
    static member Create(amount: decimal) =
        if amount < 0m then Error "Số tiền không thể âm"
        else Ok (Money amount)
    
    member this.Value = let (Money m) = this in m
    
    static member (+) (Money a, Money b) = Money (a + b)
    static member (-) (Money a, Money b) = 
        if a >= b then Ok (Money (a - b))
        else Error "Không đủ tiền"

type Quantity = private Quantity of int
    with
    static member Create(qty: int) =
        if qty <= 0 then Error "Số lượng phải dương"
        else Ok (Quantity qty)
    
    member this.Value = let (Quantity q) = this in q

type Product = {
    Id: ProductId
    Name: NonEmptyString
    Price: Money
    Stock: Quantity
}

// Smart constructor cho Product
let createProduct name price stock =
    match NonEmptyString.Create(name), Money.Create(price), Quantity.Create(stock) with
    | Ok validName, Ok validPrice, Ok validStock ->
        Ok {
            Id = ProductId.Create()
            Name = validName
            Price = validPrice  
            Stock = validStock
        }
    | Error nameErr, _, _ -> Error nameErr
    | _, Error priceErr, _ -> Error priceErr
    | _, _, Error stockErr -> Error stockErr
```

---

## Async Validation

### ⚡ **Validating with External Services**

#### **🎯 Database Validation**
```fsharp
// Async validation functions
let checkEmailExistsAsync (email: string) = async {
    // Simulate database check
    do! Async.Sleep(100)
    let existingEmails = ["admin@test.com"; "user@test.com"]
    return List.contains email existingEmails
}

let checkUserNameExistsAsync (userName: string) = async {
    do! Async.Sleep(50)
    let existingUsers = ["admin"; "user"; "test"]
    return List.contains userName existingUsers
}

let validateUniqueEmailAsync email = async {
    match validateEmail email with
    | Error err -> return Error err
    | Ok validEmail ->
        let! exists = checkEmailExistsAsync validEmail
        return if exists then Error "Email đã tồn tại" 
               else Ok validEmail
}

let validateUniqueUserNameAsync userName = async {
    match validateUserName userName with
    | Error errs -> return Error errs
    | Ok validName ->
        let! exists = checkUserNameExistsAsync validName
        return if exists then Error ["Tên người dùng đã tồn tại"]
               else Ok validName
}
```

#### **🌐 External Service Validation**  
```fsharp
open System.Net.Http

let validateAddressAsync (address: string) = async {
    try
        use client = new HttpClient()
        let url = $"https://api.geocoding.com/validate?address={Uri.EscapeDataString(address)}"
        let! response = client.GetStringAsync(url) |> Async.AwaitTask
        
        // Parse response (simplified)
        let isValid = response.Contains("\"valid\":true")
        return if isValid then Ok address else Error "Địa chỉ không hợp lệ"
        
    with
    | ex -> return Error $"Lỗi kiểm tra địa chỉ: {ex.Message}"
}

let validateCreditCardAsync (cardNumber: string) = async {
    // Remove spaces and validate format
    let cleanCard = cardNumber.Replace(" ", "")
    
    if cleanCard.Length <> 16 then 
        return Error "Thẻ phải có 16 số"
    elif not (cleanCard |> String.forall Char.IsDigit) then
        return Error "Thẻ chỉ chứa số"
    else
        try
            // Simulate external validation service
            use client = new HttpClient()
            let! isValid = async {
                do! Async.Sleep(200) // Simulate network call
                return cleanCard.StartsWith("4") // Visa cards start with 4
            }
            
            return if isValid then Ok cleanCard else Error "Thẻ không hợp lệ"
        with
        | ex -> return Error $"Lỗi kiểm tra thẻ: {ex.Message}"
}
```

#### **🔄 Combining Sync and Async Validation**
```fsharp
type UserRegistrationAsync = {
    UserName: string
    Email: string  
    Password: string
    Address: string
}

let validateUserRegistrationAsync (reg: UserRegistrationAsync) = async {
    // Sync validations first (fast fail)
    let syncValidations = [
        validateUserName reg.UserName |> Result.mapError (fun errs -> "UserName", errs)
        validateEmail reg.Email |> Result.mapError (fun err -> "Email", [err])
        validatePassword reg.Password |> Result.mapError (fun errs -> "Password", passwordErrorMessages errs)
    ]
    
    let syncErrors = 
        syncValidations 
        |> List.choose (function Error e -> Some e | Ok _ -> None)
    
    match syncErrors with
    | [] -> 
        // All sync validations passed, do async validations
        try
            let! userNameCheck = validateUniqueUserNameAsync reg.UserName
            let! emailCheck = validateUniqueEmailAsync reg.Email  
            let! addressCheck = validateAddressAsync reg.Address
            
            match userNameCheck, emailCheck, addressCheck with
            | Ok _, Ok _, Ok _ -> return Ok reg
            | Error userErr, Ok _, Ok _ -> return Error [("UserName", [userErr])]
            | Ok _, Error emailErr, Ok _ -> return Error [("Email", [emailErr])]
            | Ok _, Ok _, Error addrErr -> return Error [("Address", [addrErr])]
            | Error userErr, Error emailErr, Ok _ -> 
                return Error [("UserName", [userErr]); ("Email", [emailErr])]
            | Error userErr, Ok _, Error addrErr ->
                return Error [("UserName", [userErr]); ("Address", [addrErr])]
            | Ok _, Error emailErr, Error addrErr ->
                return Error [("Email", [emailErr]); ("Address", [addrErr])]
            | Error userErr, Error emailErr, Error addrErr ->
                return Error [("UserName", [userErr]); ("Email", [emailErr]); ("Address", [addrErr])]
        with
        | ex -> return Error [("System", [ex.Message])]
    | errs -> 
        return Error errs
}
```

---

## Form Validation

### 📝 **Web Form Validation Patterns**

#### **🎯 Complete Form Validation Example**
```fsharp
type ContactForm = {
    FirstName: string
    LastName: string
    Email: string
    Phone: string
    Message: string
    Newsletter: bool
}

type FormFieldError = {
    Field: string
    Errors: string list
}

type FormValidationResult = Result<ContactForm, FormFieldError list>

let validateContactForm (form: ContactForm) : FormValidationResult =
    let validateField fieldName validator value =
        match validator value with
        | Ok _ -> None
        | Error errors -> 
            Some { Field = fieldName; Errors = if List.isEmpty errors then [errors] else errors }
    
    let errors = [
        validateField "FirstName" validateFirstName form.FirstName
        validateField "LastName" validateLastName form.LastName  
        validateField "Email" (validateEmail >> Result.mapError List.singleton) form.Email
        validateField "Phone" (validateVietnamesePhone >> Result.mapError List.singleton) form.Phone
        validateField "Message" validateMessage form.Message
    ] |> List.choose id
    
    match errors with
    | [] -> Ok form
    | errs -> Error errs

// Individual field validators
let validateFirstName name =
    let validations = [
        if String.IsNullOrWhiteSpace(name) then yield "Họ không được rỗng"
        if name.Length > 50 then yield "Họ không quá 50 ký tự"
        if not (Regex.IsMatch(name, @"^[a-zA-ZÀ-ỹ\s]+$")) then 
            yield "Họ chỉ chứa chữ cái và khoảng trắng"
    ]
    
    match validations with
    | [] -> Ok name.Trim()
    | errs -> Error errs

let validateLastName = validateFirstName // Same rules

let validateMessage message =
    let validations = [
        if String.IsNullOrWhiteSpace(message) then yield "Tin nhắn không được rỗng"
        if message.Length < 10 then yield "Tin nhắn tối thiểu 10 ký tự"
        if message.Length > 1000 then yield "Tin nhắn tối đa 1000 ký tự"
    ]
    
    match validations with
    | [] -> Ok message.Trim()
    | errs -> Error errs
```

#### **🔄 Real-time Validation Support**
```fsharp
// Validation state for real-time feedback
type FieldValidationState = 
    | NotValidated
    | Valid
    | Invalid of string list

type FormState = {
    FirstName: string * FieldValidationState
    LastName: string * FieldValidationState
    Email: string * FieldValidationState
    Phone: string * FieldValidationState
    Message: string * FieldValidationState
}

let validateSingleField fieldName value currentState =
    let newValidationState = 
        match fieldName with
        | "FirstName" -> 
            match validateFirstName value with
            | Ok _ -> Valid
            | Error errors -> Invalid errors
        | "LastName" -> 
            match validateLastName value with
            | Ok _ -> Valid  
            | Error errors -> Invalid errors
        | "Email" ->
            match validateEmail value with
            | Ok _ -> Valid
            | Error error -> Invalid [error]
        | "Phone" ->
            match validateVietnamesePhone value with
            | Ok _ -> Valid
            | Error error -> Invalid [error]
        | "Message" ->
            match validateMessage value with
            | Ok _ -> Valid
            | Error errors -> Invalid errors
        | _ -> NotValidated
    
    // Update form state
    match fieldName with
    | "FirstName" -> { currentState with FirstName = (value, newValidationState) }
    | "LastName" -> { currentState with LastName = (value, newValidationState) }
    | "Email" -> { currentState with Email = (value, newValidationState) }
    | "Phone" -> { currentState with Phone = (value, newValidationState) }
    | "Message" -> { currentState with Message = (value, newValidationState) }
    | _ -> currentState

let isFormValid formState =
    let states = [
        snd formState.FirstName
        snd formState.LastName
        snd formState.Email
        snd formState.Phone
        snd formState.Message
    ]
    
    states |> List.forall (function Valid -> true | _ -> false)
```

---

## API Input Validation

### 🌐 **REST API Validation Patterns**

#### **🎯 Request DTO Validation**
```fsharp
open System.ComponentModel.DataAnnotations

// Input DTOs with validation attributes (for ASP.NET integration)
[<CLIMutable>]
type CreateUserRequest = {
    [<Required>]
    [<StringLength(50, MinimumLength = 3)>]
    UserName: string
    
    [<Required>]
    [<EmailAddress>]
    Email: string
    
    [<Required>]
    [<StringLength(100, MinimumLength = 8)>]
    Password: string
    
    [<Required>]
    ConfirmPassword: string
    
    [<Range(18, 120)>]
    Age: int
}

// F# validation functions (more flexible than attributes)
let validateCreateUserRequest (req: CreateUserRequest) =
    let validations = [
        ("UserName", validateUserName req.UserName |> Result.mapError List.singleton)
        ("Email", validateEmail req.Email |> Result.mapError List.singleton) 
        ("Password", validatePassword req.Password |> Result.mapError passwordErrorMessages)
        ("Age", validateAge req.Age |> Result.mapError List.singleton)
    ]
    
    // Check password confirmation
    let passwordConfirmation = 
        if req.Password <> req.ConfirmPassword then 
            Some ("ConfirmPassword", ["Mật khẩu xác nhận không khớp"])
        else None
    
    let allValidations = 
        validations @ (passwordConfirmation |> Option.toList)
    
    let errors = 
        allValidations 
        |> List.choose (fun (field, result) ->
            match result with
            | Ok _ -> None
            | Error errs -> Some (field, errs))
    
    match errors with
    | [] -> Ok req
    | errs -> Error errs
```

#### **📊 Query Parameter Validation**
```fsharp
type SearchProductsQuery = {
    Query: string option
    Category: string option
    MinPrice: decimal option
    MaxPrice: decimal option
    Page: int
    PageSize: int
    SortBy: string option
}

let validateSearchQuery (query: SearchProductsQuery) =
    let validations = [
        // Query validation
        match query.Query with
        | Some q when String.IsNullOrWhiteSpace(q) -> 
            Some ("Query", ["Từ khóa tìm kiếm không được rỗng"])
        | Some q when q.Length > 100 -> 
            Some ("Query", ["Từ khóa tối đa 100 ký tự"])
        | _ -> None
        
        // Price range validation  
        match query.MinPrice, query.MaxPrice with
        | Some min, Some max when min > max -> 
            Some ("PriceRange", ["Giá tối thiểu không thể lớn hơn giá tối đa"])
        | Some min, _ when min < 0m -> 
            Some ("MinPrice", ["Giá tối thiểu phải >= 0"])
        | _, Some max when max < 0m -> 
            Some ("MaxPrice", ["Giá tối đa phải >= 0"])
        | _ -> None
        
        // Pagination validation
        if query.Page < 1 then Some ("Page", ["Trang phải >= 1"]) else None
        if query.PageSize < 1 || query.PageSize > 100 then 
            Some ("PageSize", ["Kích thước trang từ 1-100"]) else None
        
        // Sort validation
        match query.SortBy with
        | Some sort when not (["name"; "price"; "date"; "rating"] |> List.contains sort) ->
            Some ("SortBy", ["Sắp xếp không hợp lệ"])
        | _ -> None
    ] |> List.choose id
    
    match validations with
    | [] -> Ok query
    | errs -> Error errs
```

#### **🔒 Authorization & Business Rule Validation**
```fsharp
type User = { Id: int; Role: string; IsActive: bool }
type UpdateUserRequest = { UserId: int; NewRole: string; IsActive: bool }

let validateUpdateUserRequest (currentUser: User) (request: UpdateUserRequest) =
    let businessRules = [
        // Authentication check
        if currentUser.Id = 0 then Some "Người dùng chưa đăng nhập" else None
        
        // Authorization check
        if currentUser.Role <> "Admin" then 
            Some "Không có quyền cập nhật người dùng" else None
        
        // Self-modification check
        if currentUser.Id = request.UserId then 
            Some "Không thể tự cập nhật quyền của mình" else None
        
        // Role validation
        if not (["User"; "Moderator"; "Admin"] |> List.contains request.NewRole) then
            Some "Quyền không hợp lệ" else None
        
        // Prevent deactivating last admin
        if request.NewRole = "Admin" && not request.IsActive then
            // This would require database check in real scenario
            Some "Không thể vô hiệu hóa Admin" else None
    ] |> List.choose id
    
    match businessRules with
    | [] -> Ok request
    | errs -> Error errs
```

---

## Performance & Optimization

### ⚡ **Efficient Validation Strategies**

#### **🎯 Lazy Validation**
```fsharp
// Validate only when needed, cache results
type LazyValidator<'T, 'Error> = {
    Validate: 'T -> Result<'T, 'Error>
    mutable Cache: Map<'T, Result<'T, 'Error>>
}

let createLazyValidator validateFn = {
    Validate = validateFn
    Cache = Map.empty
}

let validateWithCache validator input =
    match validator.Cache.TryFind(input) with
    | Some cachedResult -> cachedResult
    | None ->
        let result = validator.Validate(input)
        validator.Cache <- validator.Cache.Add(input, result)
        result

// Usage
let emailValidator = createLazyValidator validateEmail
let result1 = validateWithCache emailValidator "user@test.com"  // Validates
let result2 = validateWithCache emailValidator "user@test.com"  // From cache
```

#### **⚡ Short-circuit Validation**
```fsharp
// Stop at first error for performance-critical scenarios
let validateFastFail validations value =
    let rec validate remaining =
        match remaining with
        | [] -> Ok value
        | validator :: rest ->
            match validator value with
            | Error err -> Error err
            | Ok _ -> validate rest
    
    validate validations

// Usage for expensive validations
let expensiveValidations = [
    cheapValidation1      // Fast checks first
    cheapValidation2
    expensiveValidation1  // Expensive checks last
    expensiveValidation2
]

let result = validateFastFail expensiveValidations input
```

#### **📊 Batch Validation Optimization**
```fsharp
// Validate multiple items efficiently
let validateBatch validator items =
    items
    |> Array.Parallel.map (fun item ->
        try Ok (validator item)
        with ex -> Error ex.Message)
    |> Array.partition (function Ok _ -> true | Error _ -> false)
    |> fun (successes, failures) ->
        let values = successes |> Array.choose (function Ok v -> Some v | _ -> None)  
        let errors = failures |> Array.choose (function Error e -> Some e | _ -> None)
        (values, errors)

// Usage
let items = [|"item1"; "item2"; "item3"|]
let (validItems, errors) = validateBatch someValidator items
```

---

## Validation Patterns Reference

### 📚 **Complete Patterns Summary**

#### **🎯 Basic Patterns**
```fsharp
// 1. Simple validation
let validate input = if condition then Ok input else Error "message"

// 2. Multi-step validation (Railway)
let pipeline = validate1 >=> validate2 >=> validate3

// 3. Applicative validation (Collect errors)
let validateAll = [validate1; validate2; validate3] |> collectErrors

// 4. Conditional validation
let conditionalValidate input = 
    if needsValidation input then validate input else Ok input

// 5. Dependent validation  
let validateDependent input1 input2 =
    match validate1 input1 with
    | Ok valid1 -> validate2WithContext valid1 input2
    | Error err -> Error err
```

#### **🔧 Advanced Patterns**
```fsharp
// 6. Async validation
let validateAsync input = async {
    let! externalCheck = checkExternalService input
    return if externalCheck then Ok input else Error "External validation failed"
}

// 7. Cached validation
let mutable cache = Map.empty
let cachedValidate input =
    match cache.TryFind input with
    | Some result -> result
    | None -> 
        let result = expensiveValidation input
        cache <- cache.Add(input, result)
        result

// 8. Contextual validation
let validateWithContext context input =
    match context.UserRole with
    | "Admin" -> validateAsAdmin input
    | "User" -> validateAsUser input
    | _ -> Error "Unknown role"
```

#### **📋 Error Handling Patterns**
```fsharp
// 1. Simple error strings
Result<'T, string>

// 2. Structured errors
type ValidationError = { Field: string; Message: string }
Result<'T, ValidationError list>

// 3. Domain-specific errors
type UserValidationError = | InvalidEmail | WeakPassword | UserExists
Result<'T, UserValidationError>

// 4. Hierarchical errors
type ValidationError = 
    | FieldError of string * string
    | BusinessRuleError of string
    | SystemError of exn
```

### **🎯 Decision Matrix**

| **Scenario** | **Pattern** | **Error Type** | **Performance** |
|--------------|-------------|----------------|-----------------|
| Single field | Simple validation | `Result<T, string>` | ⚡ Fast |
| Form validation | Applicative | `Result<T, Error list>` | ⚡ Fast |
| Business rules | Railway-oriented | Custom DU | ⚡ Fast |  
| External deps | Async validation | `Async<Result<T, E>>` | ⏳ Slow |
| Large datasets | Batch validation | Arrays with parallel | ⚡ Optimized |
| Real-time | Cached validation | Memoized results | ⚡ Very fast |

---

## 📚 Summary

**F# Validation Patterns** mang lại:

- 🎯 **Type Safety**: Compile-time guarantees thay vì runtime surprises
- 🛡️ **Composability**: Build complex validation từ simple building blocks  
- ⚡ **Performance**: Efficient validation với caching và lazy evaluation
- 🔧 **Maintainability**: Clear separation of concerns, easy to test
- 🌟 **Expressiveness**: Validation code reads like business requirements

### **🚀 Key Principles**
1. **Make Illegal States Unrepresentable**: Use types để enforce constraints
2. **Composable Functions**: Small, focused validators combine thành complex logic
3. **Railway-Oriented Programming**: Clean error handling flow
4. **Applicative Validation**: Collect all errors for better UX
5. **Smart Constructors**: Guarantee invariants at type level

### **💡 Best Practices**
- ✅ Use domain types với private constructors
- ✅ Validate early, validate often  
- ✅ Collect errors for user-facing validation
- ✅ Fail fast for system validation
- ✅ Cache expensive validations
- ✅ Test validation logic thoroughly

### **🛠️ Common Use Cases Covered**
- 📝 Form validation với real-time feedback
- 🌐 API input validation với business rules
- 🏗️ Domain modeling với smart constructors  
- ⚡ Async validation với external services
- 📊 Batch processing với performance optimization
- 🔒 Authorization và security validation

**Master these patterns để build robust, maintainable F# applications!** 🎉