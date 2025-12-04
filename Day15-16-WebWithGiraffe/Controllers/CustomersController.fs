module CustomersController

open System
open Giraffe
open Models
open CustomersRepository
open SearchRepository
open Serilog

// ============ Web API Controllers/Handlers ============

// Health check endpoint
let healthHandler: HttpHandler =
    fun next ctx ->
        json
            {| status = "healthy"
               timestamp = DateTime.UtcNow |}
            next
            ctx

// Get all customers
let getAllCustomersHandler: HttpHandler =
    fun next ctx ->
        task {
            let repo = ctx.GetService<CustomersRepository>()
            let! result = repo.GetAllCustomersAsync()

            match result with
            | Ok customers -> return! json customers next ctx
            | Error error -> return! (setStatusCode 500 >=> json {| error = error |}) next ctx
        }

// Get customer by ID
let getCustomerHandler (customerId: string) : HttpHandler =
    fun next ctx ->
        task {
            let repo = ctx.GetService<CustomersRepository>()
            let! result = repo.GetCustomerAsync customerId

            match result with
            | Ok customer -> return! json customer next ctx
            | Error error -> return! (setStatusCode 404 >=> json {| error = error |}) next ctx
        }

// Create new customer
let createCustomerHandler: HttpHandler =
    fun next ctx ->
        task {
            let repo = ctx.GetService<CustomersRepository>()
            let! customer = ctx.BindJsonAsync<Customer>()

            // Set required fields if not provided
            let customerWithDefault =
                { customer with
                    id =
                        if String.IsNullOrEmpty(customer.id) then
                            IdGeneration.generateCustomerId ()
                        else
                            customer.id
                    registrationDate = DateTime.UtcNow }

            let! result = repo.CreateCustomerAsync customerWithDefault

            match result with
            | Ok createdCustomer -> return! (setStatusCode 201 >=> json createdCustomer) next ctx
            | Error error -> return! (setStatusCode 400 >=> json {| error = error |}) next ctx
        }

// Update customer
let updateCustomerHandler (customerId: string) : HttpHandler =
    fun next ctx ->
        task {
            let repo = ctx.GetService<CustomersRepository>()
            let searchRepo = ctx.GetService<SearchRepository>()
            let! customer = ctx.BindJsonAsync<Customer>()

            let customerToUpdate = { customer with id = customerId }

            let! result = repo.UpdateCustomerAsync customerToUpdate

            match result with
            | Ok updatedCustomer ->
                searchRepo.IndexCustomerAsync updatedCustomer |> ignore
                return! json updatedCustomer next ctx
            | Error error -> return! (setStatusCode 400 >=> json {| error = error |}) next ctx
        }

// Delete customer
let deleteCustomerHandler (customerId: string) : HttpHandler =
    fun next ctx ->
        task {
            let repo = ctx.GetService<CustomersRepository>()
            let! result = repo.DeleteCustomerAsync customerId

            match result with
            | Ok _ -> return! (setStatusCode 204) next ctx
            | Error error -> return! (setStatusCode 404 >=> json {| error = error |}) next ctx
        }

// Generate sample customer data
let generateSampleCustomerHandler: HttpHandler =
    fun next ctx ->
        task {
            let sampleCustomer: Customer = SampleData.generateSampleCustomer ()
            // createCustomerHandler
            // json sampleCustomer next ctx
            let repo: CustomersRepository = ctx.GetService<CustomersRepository>()
            let searchRepo: SearchRepository = ctx.GetService<SearchRepository>()
            let customer: Customer = sampleCustomer

            // Set required fields if not provided
            let customerWithDefault: Customer =
                { customer with
                    id =
                        if String.IsNullOrEmpty customer.id then
                            IdGeneration.generateCustomerId ()
                        else
                            customer.id
                    registrationDate = DateTime.UtcNow }

            let! result = repo.CreateCustomerAsync customerWithDefault

            match result with
            | Ok createdCustomer ->
                searchRepo.IndexCustomerAsync createdCustomer |> ignore
                return! (setStatusCode 201 >=> json {| result = createdCustomer |}) next ctx
            | Error error -> return! (setStatusCode 400 >=> json {| error = error |}) next ctx
        }

// ============ CSV Import Handler ============
// Helper: Validate customer data
let validateCustomer (customer: Customer) : Result<Customer, string> =
    let requiredFields =
        [ customer.email; customer.firstName; customer.lastName; customer.country; customer.city ] 
        |> List.forall (fun field -> not (String.IsNullOrWhiteSpace(field)))
    if not requiredFields then
        Error "Missing required fields"
    else if customer.email.Contains("@") |> not then
        Error "Invalid email format"
    else 
        Ok customer
// Helper: Parse CSV line to Customer
let parseCsvLineToCustomer (line: string) : Result<Customer, string> =
    let parts = line.Split(',')
    let parseData =
        try
            let email = parts.[0]
            let firstName = parts.[1]
            let lastName = parts.[2]
            let dateOfBirth = DateTime.Parse(parts.[3])
            let country = parts.[4]
            let city = parts.[5]
            let loyaltyTier = parts.[6]

            let customerId = IdGeneration.generateCustomerId ()

            Ok
                { id = customerId
                  customerId = customerId
                  email = email
                  firstName = firstName
                  lastName = lastName
                  dateOfBirth = dateOfBirth
                  country = country
                  city = city
                  registrationDate = DateTime.UtcNow
                  isActive = true
                  totalOrders = 0
                  totalSpent = 0.0M
                  loyaltyTier = loyaltyTier
                  lastLoginDate = None
                  lastPurchaseDate = None
                  coordinates = None
                  partitionKey = PartitionKeys.customerPartition }
        with ex ->
            Error(sprintf "Error parsing line: %s, Exception: %s" line ex.Message)

    parseData

// Import customers from CSV file
let importCustomersFromCsvHandler: HttpHandler =
    fun next ctx ->
        task {
            try
                Log.Information("Starting CSV import process")
                
                // Check if request is multipart/form-data or raw CSV
                let! csvContent =
                    if ctx.Request.HasFormContentType then
                        task {
                            // Handle multipart/form-data
                            let! form = ctx.Request.ReadFormAsync()
                            let file = form.Files.GetFile("file")
                            Log.Information("Processing uploaded file: {FileName}, Size: {FileSize} bytes", file.FileName, file.Length)
                            use reader = new System.IO.StreamReader(file.OpenReadStream())
                            return! reader.ReadToEndAsync()
                        }
                    else
                        task {
                            // Handle raw CSV (text/csv)
                            Log.Information("Processing raw CSV data from request body")
                            use reader = new System.IO.StreamReader(ctx.Request.Body)
                            return! reader.ReadToEndAsync()
                        }

                let lines =
                    csvContent.Split('\n')
                    |> Array.filter (fun l -> not (String.IsNullOrWhiteSpace(l)))

                Log.Information("Parsed {TotalLines} lines from CSV (including header)", lines.Length)
                
                let dataLines = lines |> Array.skip 1
                Log.Information("Processing {DataLineCount} customer records", dataLines.Length)

                let parseErrors = ResizeArray<string>()
                let validationErrors = ResizeArray<string>()

                let customers =
                    dataLines
                    |> Array.mapi (fun idx line ->
                        let lineNum = idx + 2 // +2 because: +1 for 1-based, +1 for header
                        match parseCsvLineToCustomer line with
                        | Ok c -> Ok (lineNum, c)
                        | Error e ->
                            let errMsg = sprintf "Line %d: Parse error - %s" lineNum e
                            parseErrors.Add(errMsg)
                            Log.Warning(errMsg)
                            Error e)
                    |> Array.choose (function
                        | Ok (lineNum, c) -> Some (lineNum, c)
                        | Error _ -> None)
                    |> Array.choose (fun (lineNum, c) ->
                        match validateCustomer c with
                        | Ok validCustomer -> Some validCustomer
                        | Error e ->
                            let errMsg = sprintf "Line %d: Validation error - %s" lineNum e
                            validationErrors.Add(errMsg)
                            Log.Warning(errMsg)
                            None)
                
                Log.Information("Successfully parsed and validated {ValidCount} customers", customers.Length)
                if parseErrors.Count > 0 then
                    Log.Warning("Parse errors: {ParseErrorCount}", parseErrors.Count)
                if validationErrors.Count > 0 then
                    Log.Warning("Validation errors: {ValidationErrorCount}", validationErrors.Count)


                let repo = ctx.GetService<CustomersRepository>()
                let searchRepo = ctx.GetService<SearchRepository>()

                Log.Information("Starting database insert for {CustomerCount} customers", customers.Length)
                
                let mutable successCount = 0
                let mutable failedCount = 0
                let errors = ResizeArray<string>()

                for customer in customers do
                    try
                        let! result = repo.CreateCustomerAsync(customer)

                        match result with
                        | Ok _ ->
                            successCount <- successCount + 1
                            Log.Debug("Successfully inserted customer: {Email}", customer.email)
                            
                            // Optional: Index to search
                            try
                                searchRepo.IndexCustomerAsync(customer) |> ignore
                                Log.Debug("Successfully indexed customer to search: {Email}", customer.email)
                            with searchEx ->
                                Log.Warning("Failed to index customer {Email} to search: {Error}", customer.email, searchEx.Message)
                        | Error e ->
                            failedCount <- failedCount + 1
                            errors.Add(e)
                            Log.Error("Failed to insert customer {Email}: {Error}", customer.email, e)
                    with insertEx ->
                        failedCount <- failedCount + 1
                        let errMsg = sprintf "Exception inserting %s: %s" customer.email insertEx.Message
                        errors.Add(errMsg)
                        Log.Error(insertEx, "Exception inserting customer {Email}", customer.email)

                let totalProcessed = successCount + failedCount
                let allErrors = 
                    (parseErrors |> Seq.toList) @ 
                    (validationErrors |> Seq.toList) @ 
                    (errors |> Seq.toList)

                Log.Information(
                    "CSV Import completed - Total: {Total}, Success: {Success}, Failed: {Failed}, Parse Errors: {ParseErrors}, Validation Errors: {ValidationErrors}",
                    totalProcessed, successCount, failedCount, parseErrors.Count, validationErrors.Count)

                return!
                    json
                        {| success = true
                           message = "Import completed"
                           totalProcessed = totalProcessed
                           successCount = successCount
                           failedCount = failedCount
                           parseErrors = parseErrors.Count
                           validationErrors = validationErrors.Count
                           errors = allErrors |}
                        next
                        ctx

            with ex ->
                Log.Error(ex, "CSV Import failed with exception")
                return!
                    (setStatusCode 500
                     >=> json
                             {| success = false
                                error = ex.Message |})
                        next
                        ctx
        }
