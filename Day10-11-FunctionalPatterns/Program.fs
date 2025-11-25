open System
open System.IO

printfn "============ Day 10-11: Functional Patterns Practice Tasks ============"

// ============ Common Types and Helper Functions ============

// Result type for Railway-Oriented Programming
type Result<'T, 'E> =
    | Ok of 'T
    | Error of 'E

module Result =
    let map f result =
        match result with
        | Ok value -> Ok (f value)
        | Error err -> Error err
    
    let bind f result =
        match result with
        | Ok value -> f value
        | Error err -> Error err

// Pipeline operators
let (|>) x f = f x
let (>>) f g x = g (f x)
let (>>=) result f = Result.bind f result

printfn "\n============ Task 1: Data Processing Pipeline ============"

// Task 1: Data Processing Pipeline
type Customer = {
    Id: int
    Name: string
    Email: string
    Age: int
    Country: string
}

// Data transformation pipeline using function composition
let normalizeCustomer customer =
    { customer with 
        Name = customer.Name.Trim()
        Email = customer.Email.Trim().ToLower()
        Country = customer.Country.Trim().ToUpper() }

let isValidCustomer customer =
    customer.Age >= 18 && customer.Email.Contains("@")

let processCustomers customers =
    customers
    |> List.map normalizeCustomer
    |> List.filter isValidCustomer
    |> List.groupBy (fun c -> c.Country)
    |> List.map (fun (country, customers) -> 
        (country, List.length customers, List.averageBy (fun c -> float c.Age) customers))

// Test Task 1
let testDataProcessingPipeline () =
    printfn "=== Data Processing Pipeline Test ==="
    
    let sampleCustomers = [
        { Id = 1; Name = " Alice Johnson "; Email = "ALICE@EXAMPLE.COM"; Age = 28; Country = "usa" }
        { Id = 2; Name = "bob smith"; Email = "bob@test.com"; Age = 35; Country = "uk" }
        { Id = 3; Name = "charlie brown"; Email = "invalid-email"; Age = 17; Country = "ca" }
        { Id = 4; Name = "diana prince"; Email = "diana@wonder.com"; Age = 30; Country = "usa" }
        { Id = 5; Name = "eve adams"; Email = "eve@tech.org"; Age = 25; Country = "uk" }
    ]
    
    let results = processCustomers sampleCustomers
    
    printfn "📊 Processed customers by country:"
    results |> List.iter (fun (country, count, avgAge) ->
        printfn "   %s: %d customers, avg age %.1f" country count avgAge)

printfn "\n============ Task 2: User Registration System ============"

// Task 2: User Registration with Railway-Oriented Programming
type RegisterUser = {
    Username: string
    Email: string
    Password: string
    Age: int
}

type UserAccount = {
    Id: Guid
    Username: string
    Email: string
    CreatedAt: DateTime
}

// Simple validation functions
let validateUsername username =
    if String.IsNullOrWhiteSpace(username) || username.Length < 3
    then Error "Username must be at least 3 characters"
    else Ok username

let validateEmail email =
    if String.IsNullOrWhiteSpace(email) || not (email.Contains("@"))
    then Error "Invalid email format"
    else Ok email

let validatePassword password =
    if String.IsNullOrWhiteSpace(password) || password.Length < 8
    then Error "Password must be at least 8 characters"
    else Ok password

let validateAge age =
    if age < 13
    then Error "Must be at least 13 years old"
    else Ok (string age)

// Registration pipeline using Railway-Oriented Programming
let registerUser (userData: RegisterUser) =
    // Validate all fields
    let validationResults = [
        validateUsername userData.Username
        validateEmail userData.Email
        validatePassword userData.Password
        validateAge userData.Age
    ]
    
    // Check if any validation failed
    let errors = 
        validationResults 
        |> List.choose (function | Error msg -> Some msg | Ok _ -> None)
    
    if List.isEmpty errors then
        // All validations passed, create user
        Ok {
            Id = Guid.NewGuid()
            Username = userData.Username
            Email = userData.Email
            CreatedAt = DateTime.Now
        }
    else
        Error errors

// Test Task 2
let testUserRegistrationSystem () =
    printfn "=== User Registration System Test ==="
    
    let testUsers = [
        ("Valid User", { Username = "john_doe"; Email = "john@example.com"; Password = "securepass123"; Age = 25 })
        ("Invalid User", { Username = "ab"; Email = "invalid"; Password = "123"; Age = 10 })
    ]
    
    testUsers |> List.iter (fun (name, userData) ->
        printfn "\n🔐 Testing: %s" name
        match registerUser userData with
        | Ok user ->
            printfn "✅ Registration successful!"
            printfn "   ID: %A" user.Id
            printfn "   Username: %s" user.Username
            printfn "   Email: %s" user.Email
        | Error errorList ->
            printfn "❌ Registration failed:"
            errorList |> List.iter (fun error -> printfn "   - %s" error)
    )

printfn "\n============ Task 3: Configuration Loader ============"

// Task 3: Configuration System with Function Composition
type AppConfig = {
    AppName: string
    Environment: string
    DatabaseHost: string
    DatabasePort: int
    LogLevel: string
}

let defaultConfig = {
    AppName = "FSharpApp"
    Environment = "development"
    DatabaseHost = "localhost"
    DatabasePort = 5432
    LogLevel = "Info"
}

// Configuration loading functions
let loadFromFile fileName =
    try
        if File.Exists(fileName) then
            // Simplified: just return a sample config
            Ok { defaultConfig with AppName = "FileLoadedApp"; Environment = "production" }
        else
            Error "Configuration file not found"
    with ex ->
        Error $"Error reading config file: {ex.Message}"

let loadFromEnvironment () =
    // Simulate reading from environment variables
    let getEnvVar name defaultValue =
        match Environment.GetEnvironmentVariable(name) with
        | null | "" -> defaultValue
        | value -> value
    
    Ok { defaultConfig with 
           Environment = getEnvVar "APP_ENV" defaultConfig.Environment
           DatabaseHost = getEnvVar "DB_HOST" defaultConfig.DatabaseHost }

let mergeConfigs baseConfig overrideConfig =
    {
        AppName = if overrideConfig.AppName <> defaultConfig.AppName then overrideConfig.AppName else baseConfig.AppName
        Environment = if overrideConfig.Environment <> defaultConfig.Environment then overrideConfig.Environment else baseConfig.Environment
        DatabaseHost = if overrideConfig.DatabaseHost <> defaultConfig.DatabaseHost then overrideConfig.DatabaseHost else baseConfig.DatabaseHost
        DatabasePort = if overrideConfig.DatabasePort <> defaultConfig.DatabasePort then overrideConfig.DatabasePort else baseConfig.DatabasePort
        LogLevel = if overrideConfig.LogLevel <> defaultConfig.LogLevel then overrideConfig.LogLevel else baseConfig.LogLevel
    }

// Configuration loading pipeline
let loadConfiguration () =
    let configFile = "app.config.json"
    File.WriteAllText(configFile, """{"AppName": "TestApp"}""")
    
    loadFromFile configFile
    >>= fun fileConfig ->
        loadFromEnvironment ()
        >>= fun envConfig ->
            Ok (mergeConfigs defaultConfig (mergeConfigs fileConfig envConfig))

// Test Task 3
let testConfigurationLoader () =
    printfn "=== Configuration Loader Test ==="
    
    match loadConfiguration () with
    | Ok config ->
        printfn "✅ Configuration loaded successfully!"
        printfn "   App: %s" config.AppName
        printfn "   Environment: %s" config.Environment
        printfn "   Database: %s:%d" config.DatabaseHost config.DatabasePort
        printfn "   Log Level: %s" config.LogLevel
    | Error errorMsg ->
        printfn "❌ Configuration loading failed: %s" errorMsg
    
    // Cleanup
    let configFile = "app.config.json"
    if File.Exists(configFile) then File.Delete(configFile)

printfn "\n============ Task 4: API Client with Retry ============"

// Task 4: Retry Logic with Monadic Patterns
type ApiResult<'T> =
    | Success of 'T
    | Failure of string

type RetryConfig = {
    MaxAttempts: int
    DelayMs: int
}

// Simulated API operation
let simulateApiCall operationName attemptNumber =
    let random = Random()
    let successRate = random.Next(1, 101)
    
    if successRate <= 40 then  // 40% failure rate
        Failure $"{operationName} failed on attempt {attemptNumber}"
    else
        Success $"{operationName} succeeded on attempt {attemptNumber}"

// Retry logic using Railway-Oriented Programming
let rec retryOperation operation config currentAttempt =
    match operation currentAttempt with
    | Success result -> 
        Success result
    | Failure error ->
        if currentAttempt >= config.MaxAttempts then
            Failure $"Operation failed after {config.MaxAttempts} attempts. Last error: {error}"
        else
            printfn "   Attempt %d failed: %s. Retrying..." currentAttempt error
            Threading.Thread.Sleep(config.DelayMs)
            retryOperation operation config (currentAttempt + 1)

// API operations with function composition
let createApiOperation operationName =
    fun attempt -> simulateApiCall operationName attempt

let executeWithRetry operationName retryConfig =
    let operation = createApiOperation operationName
    retryOperation operation retryConfig 1

// Test Task 4
let testApiClientWithRetry () =
    printfn "=== API Client with Retry Test ==="
    
    let retryConfig = { MaxAttempts = 3; DelayMs = 100 }
    
    let operations = [
        "GET /users"
        "POST /users"
        "PUT /users/123"
        "DELETE /users/456"
    ]
    
    operations |> List.iter (fun operationName ->
        printfn "\n🌐 Testing: %s" operationName
        match executeWithRetry operationName retryConfig with
        | Success result ->
            printfn "✅ %s" result
        | Failure error ->
            printfn "❌ %s" error
    )

// ============ Main Program ============

[<EntryPoint>]
let main argv =
    try
        printfn "\n🚀 Running all Practice Tasks...\n"
        
        // Execute all tasks
        testDataProcessingPipeline ()
        testUserRegistrationSystem ()
        testConfigurationLoader ()
        testApiClientWithRetry ()
        
        printfn "\n🎉 All Practice Tasks completed successfully!"
        0
        
    with ex ->
        printfn "\n💥 Error running tasks: %s" ex.Message
        1