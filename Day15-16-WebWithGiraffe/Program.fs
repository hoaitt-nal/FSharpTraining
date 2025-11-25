open System
open System.Threading.Tasks
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Giraffe
open Microsoft.Azure.Cosmos
open DotNetEnv
open Models
open CosmosRepository
open Controllers

// ============ Giraffe Web API with Azure Cosmos DB ============
let mutable jsonSerializerOption: JsonSerializerOptions = JsonSerializerOptions()
jsonSerializerOption.NumberHandling <- JsonNumberHandling.AllowReadingFromString
jsonSerializerOption.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull
jsonSerializerOption.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase  
type DateTimeOffsetConverterSTJ() =
    inherit JsonConverter<DateTime>()

    // override this.CanConvert (typeToConvert: Type) =
    //     typeof<string> = typeToConvert

    override this.Read (reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
        match reader.TokenType with
        | JsonTokenType.String -> 
            let str =reader.GetString()
            match str.Length with 
            | 19 -> 
                let dto = DateTime.SpecifyKind(str |> DateTime.Parse, DateTimeKind.Utc)
                dto
            | _ -> 
                let dt = str |> DateTime.Parse
                dt
        | _ -> 
            use document = JsonDocument.ParseValue(&reader)
            document.RootElement.Clone().ToString() |> DateTime.Parse

    override this.Write (writer: Utf8JsonWriter, value: DateTime, options: JsonSerializerOptions) =
        writer.WriteStringValue( value.ToString("yyyy-MM-ddTHH:mm:ssZ"));

// Create configuration from environment variables with fallbacks
let createCosmosConfig (configuration: IConfiguration) = {
    EndpointUrl = configuration.GetValue("COSMOS_ENDPOINT_URL", "COSMOS_ENDPOINT_URL")
    PrimaryKey = configuration.GetValue("COSMOS_PRIMARY_KEY", "COSMOS_PRIMARY_KEY")
    DatabaseId = configuration.GetValue("COSMOS_DATABASE_ID", "COSMOS_DATABASE_ID")
    CustomerContainerId = configuration.GetValue("COSMOS_CUSTOMERS_CONTAINER_ID", "COSMOS_CUSTOMERS_CONTAINER_ID")
}
// ============ API Routes ============

let webApp =
    choose [
        // Health check
        GET >=> route "/health" >=> healthHandler
        
        // Customer endpoints
        GET >=> route "/api/customers" >=> getAllCustomersHandler
        GET >=> routef "/api/customers/%s" getCustomerHandler
        POST >=> route "/api/customers" >=> createCustomerHandler
        PUT >=> routef "/api/customers/%s" updateCustomerHandler
        DELETE >=> routef "/api/customers/%s" deleteCustomerHandler
        
        // Utility endpoints
        GET >=> route "/api/sample-customer" >=> generateSampleCustomerHandler
        
        // 404 for unmatched routes
        setStatusCode 404 >=> json {| error = "Not Found" |}
    ]

// ============ Web Application Configuration ============

let configureApp (app: IApplicationBuilder) =
    app.UseGiraffe(webApp)

let configureServices (services: IServiceCollection) =
    // Add Giraffe services with System.Text.Json
    services.AddGiraffe() |> ignore
        
    services.ConfigureHttpJsonOptions(fun options ->
        options.SerializerOptions.PropertyNamingPolicy <- System.Text.Json.JsonNamingPolicy.CamelCase
        options.SerializerOptions.DefaultIgnoreCondition <- System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    ) |> ignore
    
    services.AddSingleton<Json.ISerializer>(SystemTextJson.Serializer(jsonSerializerOption) :> Json.ISerializer) |> ignore
    
    // Register CosmosRepository as singleton with configuration
    services.AddSingleton<CosmosRepository>(fun serviceProvider ->
        let configuration = serviceProvider.GetService<IConfiguration>()
        let cosmosConfig = createCosmosConfig configuration
        new CosmosRepository(cosmosConfig)) |> ignore

let configureLogging (builder: ILoggingBuilder) =
    builder.AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main args =
    // Load environment variables from .env file
    try
        Env.Load() |> ignore
        printfn "✅ Loaded environment variables from .env file"
    with
    | _ -> printfn "⚠️  No .env file found, using system environment variables"
    
    printfn "🚀 Starting Giraffe Web API with Azure Cosmos DB..."
    printfn "==============================================="
    printfn "📋 Available endpoints:"
    printfn "   GET  /health                    - Health check"
    printfn "   GET  /api/customers             - Get all customers" 
    printfn "   GET  /api/customers/{id}        - Get customer by ID"
    printfn "   POST /api/customers             - Create new customer"
    printfn "   PUT  /api/customers/{id}        - Update customer"
    printfn "   DELETE /api/customers/{id}      - Delete customer"
    printfn "   GET  /api/sample-customer       - Generate sample customer data"
    printfn "==============================================="
    printfn "🌍 Environment Variables (with fallbacks):"
    printfn "   COSMOS_ENDPOINT_URL             - Cosmos DB endpoint"
    printfn "   COSMOS_PRIMARY_KEY              - Cosmos DB primary key"
    printfn "   COSMOS_DATABASE_ID              - Database name"
    printfn "   COSMOS_CUSTOMERS_CONTAINER_ID   - Customers container name"
    printfn "   ASPNETCORE_URLS                 - Web server URLs"
    printfn "==============================================="
    
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .Configure(configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
                |> ignore)
        .Build()
        .Run()

    0