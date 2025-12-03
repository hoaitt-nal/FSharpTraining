module CustomersController

open System
open Giraffe
open Models
open CustomersRepository
open SearchRepository

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
