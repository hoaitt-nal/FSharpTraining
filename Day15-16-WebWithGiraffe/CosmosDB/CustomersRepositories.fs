module CustomersRepository

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
            if String.IsNullOrWhiteSpace(text) then
                Unchecked.defaultof<'T>
            else
                JsonSerializer.Deserialize<'T>(text, options)
    override _.ToStream<'T>(input: 'T) =
        let stream: MemoryStream = new System.IO.MemoryStream()
        use utf8Writer = new System.Text.Json.Utf8JsonWriter(stream)
        JsonSerializer.Serialize(utf8Writer, input, options)
        utf8Writer.Flush()
        stream.Position <- 0L
        stream :> System.IO.Stream

// ============ Simple Azure Cosmos DB Repository ============

// Custom DateTime converter to handle various formats
type FlexibleDateTimeConverter() =
    inherit JsonConverter<DateTime>()
    
    override _.Read(reader: byref<Utf8JsonReader>, typeToConvert: Type, options: JsonSerializerOptions) =
        match reader.TokenType with
        | JsonTokenType.String ->
            let dateStr = reader.GetString()
            match DateTime.TryParse(dateStr) with
            | true, dt -> dt
            | false, _ -> DateTime.MinValue
        | JsonTokenType.Number ->
            let timestamp = reader.GetInt64()
            DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime
        | _ -> DateTime.MinValue
    
    override _.Write(writer: Utf8JsonWriter, value: DateTime, options: JsonSerializerOptions) =
        writer.WriteStringValue(value.ToString("O"))

type CosmosConfig =
    { EndpointUrl: string
      PrimaryKey: string
      DatabaseId: string
      CustomerContainerId: string }

type CustomersRepository(config: CosmosConfig) =

    // Configure System.Text.Json for Cosmos DB
    // let jsonOptions = 
    //     let opts = JsonSerializerOptions()
    //     opts.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    //     opts.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull
    //     opts
    let jsonOptions =
        let opts = JsonSerializerOptions()
        opts.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
        opts.Converters.Add(FlexibleDateTimeConverter())  // Handle various DateTime formats
        let fsharpOptions = JsonFSharpOptions.Default().WithSkippableOptionFields()
        opts.Converters.Add(JsonFSharpConverter(fsharpOptions))   // hỗ trợ record/DU với option fields
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

    interface IDisposable with
        member this.Dispose() = cosmosClient.Dispose()
