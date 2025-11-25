module CosmosRepository

open System
open System.IO
open System.Text
open System.Text.Json
open System.Text.Json.Serialization
open FSharp.Control
open FSharp.SystemTextJson
open Microsoft.Azure.Cosmos
open Models

// ============ System.Text.Json Serializer for Cosmos DB ============

type SystemTextJsonCosmosSerializer(options: JsonSerializerOptions) =
    inherit CosmosSerializer()
    
    override _.FromStream<'T>(stream: System.IO.Stream) =
        if stream = null || stream.Length = 0L then
            Unchecked.defaultof<'T>
        else
            use sr = new System.IO.StreamReader(stream)
            let text = sr.ReadToEnd()
            JsonSerializer.Deserialize<'T>(text, options)
    override _.ToStream<'T>(input: 'T) =
        let stream = new System.IO.MemoryStream()
        use utf8Writer = new System.Text.Json.Utf8JsonWriter(stream)
        JsonSerializer.Serialize(utf8Writer, input, options)
        utf8Writer.Flush()
        stream.Position <- 0L
        stream :> System.IO.Stream

// ============ Simple Azure Cosmos DB Repository ============

type CosmosConfig =
    { EndpointUrl: string
      PrimaryKey: string
      DatabaseId: string
      CustomerContainerId: string }

type CosmosRepository(config: CosmosConfig) =

    // Configure System.Text.Json for Cosmos DB
    // let jsonOptions = 
    //     let opts = JsonSerializerOptions()
    //     opts.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    //     opts.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull
    //     opts
    let jsonOptions =
        let opts = JsonSerializerOptions()
        opts.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
        opts.Converters.Add(JsonFSharpConverter())   // hỗ trợ record/DU
        opts

    let clientOptions =
        let options = CosmosClientOptions()
        options.Serializer <- SystemTextJsonCosmosSerializer(jsonOptions)
        options

    let cosmosClient =
        new CosmosClient(
            config.EndpointUrl,
            config.PrimaryKey,
            clientOptions
        )

    let database = cosmosClient.GetDatabase(config.DatabaseId)
    let customerContainer = database.GetContainer(config.CustomerContainerId)



    // ============ CREATE ============
    member this.CreateCustomerAsync(customer: Customer) =
        task {
            try
                let partitionKey = PartitionKey(PartitionKeys.customerPartition)
                let! response = customerContainer.CreateItemAsync<Customer>(customer, partitionKey)
                return Ok customer
            with
            | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.Conflict ->
                return Error $"Customer {customer.id} already exists"
            | ex -> return Error $"Error creating customer: {ex.StackTrace}"
        }

    // ============ READ ============
    member this.GetCustomerAsync(customerId: string) =
        task {
            try
                let partitionKey = PartitionKey(PartitionKeys.customerPartition)
                let! response = customerContainer.ReadItemAsync<Customer>(customerId, partitionKey)
                return Ok response.Resource
            with
            | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.NotFound ->
                return Error $"Customer {customerId} not found"
            | ex -> return Error $"Error reading customer: {ex.Message}"
        }

    member this.GetAllCustomersAsync() =
        task {
            try
                let query = "SELECT * FROM c"
                let queryDefinition = QueryDefinition(query)
                let requestOptions = QueryRequestOptions()
                requestOptions.PartitionKey <- PartitionKey(PartitionKeys.customerPartition)
                
                let queryIterator = customerContainer.GetItemQueryIterator<Customer>(queryDefinition, requestOptions = requestOptions)
                let results = ResizeArray<Customer>()
                
                while queryIterator.HasMoreResults do
                    printfn "HasMoreResults: %A" queryIterator.HasMoreResults
                    let! response = queryIterator.ReadNextAsync()
                    printfn "Retrieved %d items from Cosmos DB" response.Count
                    results.AddRange(response.Resource)
                
                return Ok (List.ofSeq results)
            with ex ->
                return Error $"Error getting customers: {ex.Message}"
        }

    // ============ UPDATE ============
    member this.UpdateCustomerAsync(customer: Customer) =
        task {
            try
                let partitionKey = PartitionKey(PartitionKeys.customerPartition)
                let! response = customerContainer.UpsertItemAsync(customer, partitionKey)
                return Ok customer
            with ex ->
                return Error $"Error updating customer: {ex.Message}"
        }

    // ============ DELETE ============
    member this.DeleteCustomerAsync(customerId: string) =
        task {
            try
                let partitionKey = PartitionKey(PartitionKeys.customerPartition)
                let! response = customerContainer.DeleteItemAsync<Customer>(customerId, partitionKey)
                return Ok true
            with
            | :? CosmosException as ex when ex.StatusCode = System.Net.HttpStatusCode.NotFound ->
                return Error $"Customer {customerId} not found"
            | ex -> return Error $"Error deleting customer: {ex.Message}"
        }

    // ============ QUERY ============
    // member this.GetCustomersByCountryAsync(country: string) =
    //     task {
    //         try
    //             let query = $"SELECT * FROM c WHERE c.country = '{country}'"
    //             let queryDefinition = QueryDefinition(query)
    //             let requestOptions = QueryRequestOptions()
    //             requestOptions.PartitionKey <- PartitionKey(PartitionKeys.customerPartition)

    //             let queryIterator =
    //                 customerContainer.GetItemQueryIterator<JObject>(queryDefinition, requestOptions = requestOptions)

    //             let results = ResizeArray<Customer>()

    //             while queryIterator.HasMoreResults do
    //                 let! response = queryIterator.ReadNextAsync()

    //                 // Convert each JObject to Customer, filtering out system properties
    //                 for jObj in response do
    //                     match CosmosHelpers.jObjectToCustomer jObj with
    //                     | Some customer -> results.Add(customer)
    //                     | None -> printfn "Warning: Skipping invalid customer object"

    //             return Ok(List.ofSeq results)
    //         with ex ->
    //             return Error $"Error querying customers: {ex.Message}"
    //     }

    interface IDisposable with
        member this.Dispose() = cosmosClient.Dispose()
