module SearchController

open System
open Giraffe
open Microsoft.Extensions.DependencyInjection
open SearchRepository
open CustomersRepository

// ============ Search API Handlers ============

// Initialize search index
let initializeSearchIndexHandler: HttpHandler =
    fun next ctx ->
        task {
            let searchRepo = ctx.GetService<SearchRepository>()
            let! result = searchRepo.CreateOrUpdateIndexAsync()

            match result with
            | Ok message -> return! json {| success = true; message = message |} next ctx
            | Error error -> return! (setStatusCode 500 >=> json {| success = false; error = error |}) next ctx
        }

// Index single customer
let indexCustomerHandler (customerId: string) : HttpHandler =
    fun next ctx ->
        task {
            let searchRepo = ctx.GetService<SearchRepository>()
            let cosmosRepo = ctx.GetService<CustomersRepository>()

            // Get customer from Cosmos DB
            let! customerResult = cosmosRepo.GetCustomerAsync(customerId)

            match customerResult with
            | Ok customer ->
                // Index to Azure Search
                let! indexResult = searchRepo.IndexCustomerAsync(customer)

                match indexResult with
                | Ok message -> return! json {| success = true; message = message |} next ctx
                | Error error -> return! (setStatusCode 500 >=> json {| success = false; error = error |}) next ctx
            | Error error -> return! (setStatusCode 404 >=> json {| success = false; error = error |}) next ctx
        }

// Bulk index all customers from Cosmos DB
let bulkIndexCustomersHandler: HttpHandler =
    fun next ctx ->
        task {
            let searchRepo = ctx.GetService<SearchRepository>()
            let cosmosRepo = ctx.GetService<CustomersRepository>()

            // Get all customers from Cosmos DB
            let! customersResult = cosmosRepo.GetAllCustomersAsync()
            printf "Retrieved %A customers from Cosmos DB for bulk indexing" customersResult

            match customersResult with
            | Ok customers ->
                // Bulk index to Azure Search
                let! indexResult = searchRepo.BulkIndexCustomersAsync(customers)

                match indexResult with
                | Ok message -> return! json {| success = true; message = message |} next ctx
                | Error error -> return! (setStatusCode 500 >=> json {| success = false; error = error |}) next ctx
            | Error error -> return! (setStatusCode 500 >=> json {| success = false; error = error |}) next ctx
        }

// Search customers
let searchCustomersHandler: HttpHandler =
    fun next ctx ->
        task {
            let searchRepo = ctx.GetService<SearchRepository>()
            let! req = ctx.BindJsonAsync<SearchRequest>()
            let searchText = req.searchText |> Option.defaultValue ""
            let filters = req.filters
            let sort = req.sort
            let top = req.top

            // Parse filter parameters from query string using helper function

            let! result = searchRepo.AdvancedSearchAsync(searchText, filters, sort, top)

            match result with
            | Ok documents ->
                return!
                    json
                        {| success = true
                           count = documents.Length
                           documents = documents |}
                        next
                        ctx
            | Error error -> return! (setStatusCode 500 >=> json {| success = false; error = error |}) next ctx
        }
