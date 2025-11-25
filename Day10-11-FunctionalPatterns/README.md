# 🔗 Day 10-11: Functional Patterns

## 📋 Learning Objectives
- [ ] Master pipeline operators and function composition
- [ ] Implement robust error handling with Result types
- [ ] Understand Railway-Oriented Programming
- [ ] Create validation pipelines
- [ ] Use monadic patterns effectively
- [ ] Build composable and testable code

### 🔍 **Chi tiết Learning Objectives:**

#### 1. **Master pipeline operators and function composition**
```fsharp
// Pipeline operators để chain operations
let processNumbers numbers =
    numbers
    |> List.filter (fun x -> x > 0)       // Keep positive
    |> List.map (fun x -> x * x)          // Square them  
    |> List.filter (fun x -> x < 100)     // Keep small squares
    |> List.sum                           // Sum everything

// Function composition với >>
let processData = 
    List.filter (fun x -> x > 0)
    >> List.map (fun x -> x * x)
    >> List.sum

// Reusable composed functions
let validateEmail = String.contains "@" >> fun hasAt -> hasAt && not (String.isEmpty)
```
**Mục tiêu**: Tạo clean, readable data transformation pipelines

#### 2. **Implement robust error handling with Result types**
```fsharp
// Result type cho error handling
type ValidationError = 
    | EmptyName
    | InvalidEmail  
    | TooYoung

// Functions returning Result
let validateName name =
    if String.IsNullOrWhiteSpace(name) then Error EmptyName
    else Ok name

let validateEmail email =
    if email.Contains("@") then Ok email
    else Error InvalidEmail

let validateAge age =
    if age >= 18 then Ok age
    else Error TooYoung
```
**Mục tiêu**: Type-safe error handling thay vì exceptions

#### 3. **Understand Railway-Oriented Programming**
```fsharp
// Railway-Oriented Programming pattern
let bind f result =
    match result with
    | Ok value -> f value
    | Error err -> Error err

// Chain validations
let validatePerson name email age =
    validateName name
    |> bind (fun validName ->
        validateEmail email
        |> bind (fun validEmail ->
            validateAge age
            |> bind (fun validAge ->
                Ok { Name = validName; Email = validEmail; Age = validAge })))
                
// Tee function để side effects
let tee f x = f x; x

let logAndProcess data =
    data
    |> tee (printfn "Processing: %A")
    |> processData
    |> tee (printfn "Result: %A")
```
**Mục tiêu**: Chain operations mà không break pipeline khi có error

#### 4. **Create validation pipelines**
```fsharp
// Applicative validation - collect all errors
type ValidationResult<'T> = 
    | Success of 'T
    | Failure of ValidationError list

let map2 f result1 result2 =
    match result1, result2 with
    | Success x, Success y -> Success (f x y)
    | Failure errs1, Success _ -> Failure errs1
    | Success _, Failure errs2 -> Failure errs2  
    | Failure errs1, Failure errs2 -> Failure (errs1 @ errs2)

// Validation pipeline collecting all errors
let createPerson name email age =
    let nameResult = validateName name
    let emailResult = validateEmail email  
    let ageResult = validateAge age
    
    (fun n e a -> { Name = n; Email = e; Age = a })
    <!> nameResult <*> emailResult <*> ageResult
```
**Mục tiêu**: Validate data và collect all validation errors

#### 5. **Use monadic patterns effectively**
```fsharp
// Maybe monad
type Maybe<'T> = 
    | Some of 'T
    | None

module Maybe =
    let map f maybe =
        match maybe with
        | Some value -> Some (f value)
        | None -> None
        
    let bind f maybe =
        match maybe with
        | Some value -> f value
        | None -> None

// Chain maybe operations
let safeDivide x y = if y = 0.0 then None else Some (x / y)
let safeSquareRoot x = if x < 0.0 then None else Some (sqrt x)

let calculate x y =
    safeDivide x y
    |> Maybe.bind safeSquareRoot
    |> Maybe.map (fun result -> result * 2.0)
```
**Mục tiêu**: Compose operations mà handle None/failure cases automatically

#### 6. **Build composable and testable code**
```fsharp
// Pure functions - dễ test
let calculateTax rate amount = amount * rate

// Dependency injection với function parameters
let processOrder calculateTax validatePayment order =
    order
    |> validatePayment
    |> Result.map (fun validOrder -> 
        { validOrder with Tax = calculateTax 0.1 validOrder.Amount })

// Higher-order functions for configuration
let createValidator rules value =
    rules
    |> List.fold (fun result rule ->
        match result with
        | Error _ -> result
        | Ok val -> rule val) (Ok value)
        
// Composable validation rules
let lengthRule min max value =
    if String.length value >= min && String.length value <= max 
    then Ok value 
    else Error "Invalid length"
```
**Mục tiêu**: Code dễ test, reuse và maintain

### 🎯 **Tại sao những concept này quan trọng?**
- **Code Quality**: Functional patterns tạo code cleaner và predictable
- **Error Handling**: Type-safe error handling prevents runtime crashes
- **Composability**: Small functions ghép thành complex logic
- **Testability**: Pure functions dễ unit test
- **Maintainability**: Clear data flow và separation of concerns
- **Robustness**: Handle edge cases và error scenarios elegantly

## 📝 Code Examples & Exercises

### Exercise 1: Advanced Pipelines
Create `Pipelines.fs`:
```fsharp
// Pipeline operators
let (|>) x f = f x
let (<|) f x = f x
let (>>) f g x = g (f x)
let (<<) g f x = g (f x)

// Data transformation pipeline
type Customer = { Id: int; Name: string; Email: string; Age: int }

let customers = [
    { Id = 1; Name = "Alice"; Email = "alice@test.com"; Age = 25 }
    { Id = 2; Name = "Bob"; Email = "bob@test.com"; Age = 35 }
    { Id = 3; Name = "Charlie"; Email = "charlie@invalid"; Age = 17 }
]

// Processing pipeline
let processCustomers =
    List.filter (fun c -> c.Age >= 18)          // Adults only
    >> List.filter (fun c -> c.Email.Contains("@"))  // Valid emails
    >> List.map (fun c -> c.Name.ToUpper())     // Uppercase names
    >> List.sort                                 // Sort alphabetically

let validCustomerNames = customers |> processCustomers

// Functional composition examples
let addOne x = x + 1
let multiplyByTwo x = x * 2
let square x = x * x

// Compose functions
let addOneThenDouble = addOne >> multiplyByTwo
let doubleAndAddOne = multiplyByTwo >> addOne
let complexTransform = addOne >> multiplyByTwo >> square

// Pipeline with multiple transformations
let processNumbers numbers =
    numbers
    |> List.filter (fun x -> x > 0)              // Positive only
    |> List.map (fun x -> x |> addOne |> multiplyByTwo)  // Transform
    |> List.filter (fun x -> x < 100)            // Under 100
    |> List.sum                                  // Sum result
```

### Exercise 2: Result Type & Error Handling
Create `ErrorHandling.fs`:
```fsharp
// Result type for error handling
type Result<'T, 'E> =
    | Ok of 'T
    | Error of 'E

// Result helper functions
module Result =
    let map f result =
        match result with
        | Ok value -> Ok (f value)
        | Error err -> Error err
    
    let bind f result =
        match result with
        | Ok value -> f value
        | Error err -> Error err
    
    let mapError f result =
        match result with
        | Ok value -> Ok value
        | Error err -> Error (f err)

// Safe operations
let safeDivide x y =
    if y <> 0.0 then Ok (x / y)
    else Error "Division by zero"

let safeSquareRoot x =
    if x >= 0.0 then Ok (sqrt x)
    else Error "Cannot take square root of negative number"

let parseFloat str =
    match System.Double.TryParse(str) with
    | (true, value) -> Ok value
    | (false, _) -> Error (sprintf "'%s' is not a valid number" str)

// Chaining operations with Result
let calculateComplexFormula input =
    input
    |> parseFloat
    |> Result.bind (safeDivide 100.0)
    |> Result.bind safeSquareRoot
    |> Result.map (fun x -> x * 2.0)

// Railway-Oriented Programming
let (>>=) result f = Result.bind f result
let (<!>) result f = Result.map f result

// Using railway operators
let processInput input =
    parseFloat input
    >>= (safeDivide 10.0)
    >>= safeSquareRoot
    <!> (fun x -> x * 100.0)
```

### Exercise 3: Validation Patterns
Create `Validation.fs`:
```fsharp
// Validation result type
type ValidationResult<'T> =
    | Valid of 'T
    | Invalid of string list

// Validation functions
let validateEmail email =
    if email |> String.contains "@" && email |> String.contains "."
    then Valid email
    else Invalid ["Invalid email format"]

let validateAge age =
    if age >= 0 && age <= 150
    then Valid age
    else Invalid ["Age must be between 0 and 150"]

let validateName name =
    if not (String.IsNullOrWhiteSpace(name)) && name.Length >= 2
    then Valid name
    else Invalid ["Name must be at least 2 characters"]

// Applicative validation
module Validation =
    let map f validation =
        match validation with
        | Valid value -> Valid (f value)
        | Invalid errors -> Invalid errors
    
    let apply fValidation xValidation =
        match fValidation, xValidation with
        | Valid f, Valid x -> Valid (f x)
        | Valid _, Invalid errors -> Invalid errors
        | Invalid errors, Valid _ -> Invalid errors
        | Invalid errors1, Invalid errors2 -> Invalid (errors1 @ errors2)
    
    let combine validations =
        let rec loop acc remaining =
            match remaining with
            | [] -> 
                match acc with
                | Valid values -> Valid (List.rev values)
                | Invalid errors -> Invalid errors
            | validation :: rest ->
                match acc, validation with
                | Valid values, Valid value -> loop (Valid (value :: values)) rest
                | Valid _, Invalid errors -> loop (Invalid errors) rest
                | Invalid errors1, Valid _ -> loop (Invalid errors1) rest
                | Invalid errors1, Invalid errors2 -> loop (Invalid (errors1 @ errors2)) rest
        loop (Valid []) validations

// User validation example
type User = { Name: string; Email: string; Age: int }

let validateUser name email age =
    let nameValidation = validateName name
    let emailValidation = validateEmail email
    let ageValidation = validateAge age
    
    match nameValidation, emailValidation, ageValidation with
    | Valid n, Valid e, Valid a -> Valid { Name = n; Email = e; Age = a }
    | _ -> 
        let errors = 
            [nameValidation; emailValidation; ageValidation]
            |> List.collect (function Invalid errs -> errs | Valid _ -> [])
        Invalid errors
```

### Exercise 4: Monadic Patterns
Create `Monads.fs`:
```fsharp
// Maybe monad (Option alternative)
type Maybe<'T> =
    | Some of 'T
    | None

module Maybe =
    let bind f maybe =
        match maybe with
        | Some value -> f value
        | None -> None
    
    let map f maybe =
        match maybe with
        | Some value -> Some (f value)
        | None -> None
    
    let apply fMaybe xMaybe =
        match fMaybe, xMaybe with
        | Some f, Some x -> Some (f x)
        | _ -> None

// State monad for stateful computations
type State<'S, 'T> = State of ('S -> 'T * 'S)

module State =
    let run (State f) state = f state
    
    let returnState value = State (fun state -> (value, state))
    
    let bind f (State computation) =
        State (fun state ->
            let (value, newState) = computation state
            let (State nextComputation) = f value
            nextComputation newState
        )
    
    let map f state =
        bind (f >> returnState) state
    
    let getState = State (fun state -> (state, state))
    
    let setState newState = State (fun _ -> ((), newState))

// IO monad for managing side effects
type IO<'T> = IO of (unit -> 'T)

module IO =
    let run (IO action) = action ()
    
    let returnIO value = IO (fun () -> value)
    
    let bind f (IO action) =
        IO (fun () ->
            let value = action ()
            let (IO nextAction) = f value
            nextAction ()
        )
    
    let map f io = bind (f >> returnIO) io
    
    // Useful IO operations
    let printLine text = IO (fun () -> printfn "%s" text)
    let readLine = IO (fun () -> System.Console.ReadLine())
    let delay ms = IO (fun () -> System.Threading.Thread.Sleep(ms))
```

## 🏃‍♂️ Practice Tasks

### Task 1: Data Processing Pipeline
Build a system that:
1. Reads data from multiple sources
2. Validates and cleans data
3. Transforms using composition
4. Handles errors gracefully

### Task 2: User Registration System
Create a registration flow with:
1. Multi-step validation
2. Accumulating validation errors
3. Railway-oriented processing
4. Rollback on failure

### Task 3: Configuration Loader
Implement a config system that:
1. Loads from multiple sources (file, env, CLI)
2. Validates configuration
3. Provides defaults for missing values
4. Composes configuration sources

### Task 4: API Client with Retry
Build an HTTP client that:
1. Handles various error types
2. Implements retry logic
3. Uses monadic composition
4. Provides clean error reporting

## ✅ Completion Checklist
- [ ] Master pipeline operators and composition
- [ ] Understand Result type for error handling
- [ ] Can implement Railway-Oriented Programming
- [ ] Comfortable with validation patterns
- [ ] Understand monadic patterns
- [ ] Completed all 4 exercises
- [ ] Finished all 4 practice tasks

## 🎯 Next Steps
Ready for **Day 12-13: Modules & Testing** to learn code organization and testing strategies!