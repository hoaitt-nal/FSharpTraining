# 🌐 Giraffe Web API với Azure Cosmos DB

## 📚 Các khái niệm chính trong Azure Cosmos DB

### 1. DATABASES: Logical container for collections/containers
- Tương đương với database trong SQL
- Chứa các containers (collections)
- Có thể tạo nhiều databases trong một Cosmos account

### 2. CONTAINERS (Collections): Logical container for items/documents
- Tương đương với table trong SQL hoặc collection trong MongoDB
- Chứa các items (documents)
- Được định nghĩa bởi partition key strategy
- Có throughput settings (RU/s - Request Units per second)

### 3. ITEMS (Documents): Individual records stored in containers
- Tương đương với row trong SQL hoặc document trong MongoDB
- Được lưu trữ dưới dạng JSON
- Mỗi item phải có unique id trong partition
- Có thể có schema linh hoạt

### 4. PARTITION KEY: Strategy để phân tán data across partitions
- Được chọn khi tạo container
- Không thể thay đổi sau khi container được tạo
- Ảnh hưởng đến performance và scalability
- Nên chọn property có high cardinality và even distribution

### 5. ID: Unique identifier cho mỗi item trong partition
- Kết hợp với partition key tạo thành composite primary key
- Phải unique trong cùng một partition
- Được sử dụng cho point reads (ReadItemAsync)

## 📚 Các khái niệm chính trong Azure Cognitive Search

### 1. SEARCH INDEX: Container for searchable content
- Tương đương với database table nhưng được tối ưu cho full-text search
- Chứa documents với defined schema (fields)
- Support nhiều data types: string, int, double, boolean, datetime, collection
- Có thể configure analyzers cho language-specific search

### 2. SEARCH DOCUMENT: Individual searchable record in index
- Tương đương với row trong SQL hoặc item trong Cosmos DB
- Phải có key field (unique identifier)
- Có thể có multiple searchable, filterable, sortable, facetable fields
- Maximum size: 16 MB per document

### 3. FIELDS: Properties/columns trong search document
**Field Attributes:**
- `IsKey`: Unique identifier (chỉ có 1 field)
- `IsSearchable`: Enable full-text search trên field này
- `IsFilterable`: Có thể dùng trong $filter queries
- `IsSortable`: Có thể sort results theo field này
- `IsFacetable`: Enable faceted navigation
- `IsRetrievable`: Field sẽ return trong search results (default: true)

### 4. ANALYZERS: Text processing for search
- **Standard Analyzer**: Default, works cho most languages
- **Language Analyzers**: Optimized cho specific languages (en.microsoft, vi.lucene, etc.)
- **Custom Analyzers**: Tự define tokenization và filtering rules
- Xử lý: tokenization, lowercasing, removing stop words, stemming

### 5. SEARCH QUERIES: Cách query data từ index
**Simple Query:**
```
search=coffee&$top=10
```

**Advanced Query với OData:**
```
search=coffee&
$filter=price le 20 and category eq 'Beverages'&
$orderby=rating desc&
$top=10&
$skip=0
```

### 6. SCORING & RANKING: Relevance calculation
- **TF-IDF**: Term frequency - Inverse document frequency
- **BM25**: Default ranking algorithm (Better than TF-IDF)
- **Scoring Profiles**: Custom boost rules cho specific fields
- **freshnesBoost**: Boost recent documents
- **magnitudeBoost**: Boost by numeric field values

### 7. INDEXERS (Optional): Automatic data sync
- Pull data từ data sources (Azure SQL, Cosmos DB, Blob Storage)
- Schedule automatic updates
- Change detection cho incremental indexing
- Không dùng trong project này (dùng manual push indexing)

### 8. SKILLSETS (AI Enrichment - Advanced):
- Extract text từ images (OCR)
- Entity recognition
- Key phrase extraction
- Language detection
- Sentiment analysis
- **Không implement trong project này**

### 9. SUGGESTER: Autocomplete và search suggestions
```fsharp
// Configure trong index definition
Suggester = new SearchSuggester("sg", ["firstName", "lastName", "city"])

// Usage:
let suggestions = searchClient.Suggest("joh", "sg")
// Returns: John, Johnson, Johnny...
```

### 10. FACETS: Aggregated counts for filtering
```fsharp
// Request:
facet=category&facet=loyaltyTier

// Response:
{
  "facets": {
    "category": [
      { "value": "Premium", "count": 45 },
      { "value": "Standard", "count": 120 }
    ],
    "loyaltyTier": [
      { "value": "Gold", "count": 30 },
      { "value": "Silver", "count": 50 },
      { "value": "Bronze", "count": 85 }
    ]
  }
}
```

### 🔄 Cosmos DB vs Azure Search Integration Pattern:
```
┌─────────────────┐         Push Index         ┌──────────────────────┐
│   Cosmos DB     │────────────────────────────>│  Azure Search Index  │
│  (Source of     │                             │  (Optimized for      │
│   Truth)        │<────────────────────────────│   Full-Text Search)  │
└─────────────────┘      Read if needed         └──────────────────────┘
       │                                                    │
       │ CRUD Operations                                   │ Search Queries
       ▼                                                    ▼
  Your Application ────────────────────────────────> Search Results
```

**Best Practices:**
- ✅ Cosmos DB: Source of truth, CRUD operations, point reads
- ✅ Azure Search: Full-text search, filters, sorting, facets
- ✅ Push to Search Index sau mỗi CREATE/UPDATE operation
- ✅ Delete from Search Index khi DELETE from Cosmos DB
- ✅ Periodic re-indexing để sync data

## ✅ **Completed Implementation**
- ✅ Setup Giraffe web framework
- ✅ Routing và middleware 
- ✅ JSON serialization cho APIs
- ✅ HTTP requests/responses handling
- ✅ REST API với full Customer CRUD operations
- ✅ Azure Cosmos DB integration
- ✅ Modular code organization với Controllers

### 🔍 **Chi tiết Learning Objectives:**

#### 1. **Setup Giraffe web framework**
```fsharp
// Basic Giraffe application setup
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection

let configureServices (services: IServiceCollection) =
    services.AddGiraffe() |> ignore

let configureApp (app: IApplicationBuilder) =
    app.UseGiraffe(webApp)

// Startup configuration
[<EntryPoint>]
let main args =
    WebHostBuilder()
        .UseKestrel()
        .ConfigureServices(configureServices)
        .Configure(configureApp)
        .Build()
        .Run()
    0
```
**Mục tiêu**: Setup modern F# web application với ASP.NET Core

#### 2. **Understand routing and middleware**
```fsharp
// Functional routing với pattern matching
let webApp =
    choose [
        GET >=> choose [
            route "/" >=> text "Home page"
            route "/api/users" >=> getUsersHandler
            routef "/api/users/%i" getUserByIdHandler
        ]
        POST >=> choose [
            route "/api/users" >=> createUserHandler
            route "/api/login" >=> loginHandler  
        ]
        PUT >=> routef "/api/users/%i" updateUserHandler
        DELETE >=> routef "/api/users/%i" deleteUserHandler
    ]

// Custom middleware
let loggingMiddleware: HttpHandler =
    fun next ctx -> async {
        printfn "Request: %s %s" ctx.Request.Method ctx.Request.Path
        return! next ctx
    }
```
**Mục tiêu**: Compose HTTP pipelines với functional approach

#### 3. **Implement JSON serialization for APIs**
```fsharp
// JSON responses
type User = { Id: int; Name: string; Email: string }

let getUsersHandler: HttpHandler =
    fun next ctx -> async {
        let users = [
            { Id = 1; Name = "Alice"; Email = "alice@test.com" }
            { Id = 2; Name = "Bob"; Email = "bob@test.com" }
        ]
        return! json users next ctx
    }

// JSON input parsing
let createUserHandler: HttpHandler =
    fun next ctx -> async {
        let! user = ctx.BindJsonAsync<User>()
        // Validate và save user
        let newUser = { user with Id = generateNewId() }
        return! json newUser next ctx
    }
```
**Mục tiêu**: Handle JSON input/output safely với type safety

#### 4. **Handle HTTP requests and responses**
```fsharp
// Query parameters
let searchUsersHandler: HttpHandler =
    fun next ctx ->
        let query = ctx.GetQueryStringValue "q"
        match query with
        | Ok searchTerm -> 
            let results = searchUsers searchTerm
            json results next ctx
        | Error _ ->
            setStatusCode 400 >=> text "Missing query parameter" next ctx

// Headers và status codes
let notFoundHandler: HttpHandler =
    setStatusCode 404 >=> json {| error = "User not found" |}

let corsHandler: HttpHandler =
    setHttpHeader "Access-Control-Allow-Origin" "*"
```
**Mục tiêu**: Handle HTTP semantics correctly (status codes, headers, etc.)

#### 5. **Build REST API with CRUD operations**
```fsharp
// Full CRUD operations
module UserApi =
    let getUsers: HttpHandler = (* list all users *)
    let getUserById id: HttpHandler = (* get single user *)
    let createUser: HttpHandler = (* create new user *)
    let updateUser id: HttpHandler = (* update existing user *)
    let deleteUser id: HttpHandler = (* delete user *)

// Route composition
let userRoutes =
    subRoute "/api/users" (
        choose [
            GET >=> choose [
                route "" >=> UserApi.getUsers
                routef "/%i" UserApi.getUserById
            ]
            POST >=> route "" >=> UserApi.createUser
            PUT >=> routef "/%i" UserApi.updateUser  
            DELETE >=> routef "/%i" UserApi.deleteUser
        ]
    )
```
**Mục tiêu**: Build RESTful APIs following HTTP conventions

#### 6. **Implement authentication and authorization**
```fsharp
// JWT authentication
let generateJwtToken userId =
    // JWT token generation logic
    
let authenticateHandler: HttpHandler =
    fun next ctx -> async {
        let! loginRequest = ctx.BindJsonAsync<LoginRequest>()
        match validateCredentials loginRequest with
        | Some userId ->
            let token = generateJwtToken userId
            return! json {| token = token |} next ctx
        | None ->
            return! setStatusCode 401 >=> text "Invalid credentials" next ctx
    }

// Authorization middleware
let requireAuth: HttpHandler =
    fun next ctx ->
        match ctx.User.Identity.IsAuthenticated with
        | true -> next ctx
        | false -> setStatusCode 401 >=> text "Unauthorized" next ctx
```
**Mục tiêu**: Secure APIs với proper authentication/authorization

#### 7. **Create a complete web application**
```fsharp
// Application structure
// Controllers/       // HTTP handlers
// Services/          // Business logic
// Models/            // Data models
// Database/          // Data access
// Middleware/        // Custom middleware
// Configuration/     // App settings

// Dependency injection setup
let configureServices (services: IServiceCollection) =
    services
        .AddSingleton<IUserService, UserService>()
        .AddSingleton<IRepository, DatabaseRepository>()
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(configureJwtOptions)
        .AddGiraffe() 
    |> ignore
```
**Mục tiêu**: Build production-ready web applications với proper architecture

### 🎯 **Tại sao những concept này quan trọng?**
- **Modern Web Development**: Giraffe brings functional programming to web
- **Type Safety**: F# type system prevents many web API bugs
- **Composability**: Functional approach makes complex routing simple
- **Performance**: F# + ASP.NET Core = high performance web apps
- **Maintainability**: Functional patterns make web code more maintainable
- **Integration**: Works seamlessly với .NET ecosystem
- **Productivity**: Less boilerplate code compared to C# MVC

## 🛠️ Setup Requirements
```bash
# Create new web project
dotnet new web -lang F# -n FSharpWebAPI
cd FSharpWebAPI

# Add required packages
dotnet add package Giraffe
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package Newtonsoft.Json
dotnet add package Serilog.AspNetCore
```

## 📝 Code Examples & Exercises

### Exercise 1: Basic Giraffe Setup
Create `Program.fs`:
```fsharp
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Giraffe

// Simple "Hello World" handler
let helloHandler: HttpHandler =
    fun next ctx ->
        text "Hello from F# Giraffe!" next ctx

// JSON response handler
let jsonHandler: HttpHandler =
    fun next ctx ->
        let response = {| message = "Hello from F# API!"; timestamp = System.DateTime.Now |}
        json response next ctx

// Route configuration
let webApp =
    choose [
        GET >=> route "/" >=> text "F# Giraffe Web API"
        GET >=> route "/hello" >=> helloHandler
        GET >=> route "/api/status" >=> jsonHandler
        setStatusCode 404 >=> text "Not Found"
    ]

// Configure services
let configureServices (services: IServiceCollection) =
    services.AddGiraffe() |> ignore

// Configure app
let configureApp (app: IApplicationBuilder) =
    app.UseGiraffe(webApp)

[<EntryPoint>]
let main args =
    WebApplication.CreateBuilder(args)
        .Services.AddGiraffe()
        .BuildServiceProvider()
        .GetService<IWebHostBuilder>()
        .Configure(configureApp)
        .ConfigureServices(configureServices)
        .Build()
        .Run()
    0
```

### Exercise 2: Product API with CRUD Operations
Create `Models/Product.fs`:
```fsharp
namespace FSharpWebAPI.Models

open System

type Product = {
    Id: Guid
    Name: string
    Description: string
    Price: decimal
    Category: string
    InStock: bool
    CreatedAt: DateTime
    UpdatedAt: DateTime
}

type CreateProductRequest = {
    Name: string
    Description: string
    Price: decimal
    Category: string
    InStock: bool
}

type UpdateProductRequest = {
    Name: string option
    Description: string option
    Price: decimal option
    Category: string option
    InStock: bool option
}

type ProductResponse = {
    Id: Guid
    Name: string
    Description: string
    Price: decimal
    Category: string
    InStock: bool
    CreatedAt: DateTime
    UpdatedAt: DateTime
}

type ApiResponse<'T> = {
    Success: bool
    Data: 'T option
    Message: string
    Timestamp: DateTime
}

module Product =
    let toResponse (product: Product) : ProductResponse = {
        Id = product.Id
        Name = product.Name
        Description = product.Description
        Price = product.Price
        Category = product.Category
        InStock = product.InStock
        CreatedAt = product.CreatedAt
        UpdatedAt = product.UpdatedAt
    }
    
    let fromCreateRequest (request: CreateProductRequest) : Product = {
        Id = Guid.NewGuid()
        Name = request.Name
        Description = request.Description
        Price = request.Price
        Category = request.Category
        InStock = request.InStock
        CreatedAt = DateTime.UtcNow
        UpdatedAt = DateTime.UtcNow
    }
    
    let updateFromRequest (product: Product) (request: UpdateProductRequest) : Product = {
        product with
            Name = request.Name |> Option.defaultValue product.Name
            Description = request.Description |> Option.defaultValue product.Description
            Price = request.Price |> Option.defaultValue product.Price
            Category = request.Category |> Option.defaultValue product.Category
            InStock = request.InStock |> Option.defaultValue product.InStock
            UpdatedAt = DateTime.UtcNow
    }
```

### Exercise 3: In-Memory Data Store
Create `Services/ProductService.fs`:
```fsharp
namespace FSharpWebAPI.Services

open System
open System.Collections.Concurrent
open FSharpWebAPI.Models

type IProductService =
    abstract member GetAllProducts: unit -> Product list
    abstract member GetProductById: Guid -> Product option
    abstract member CreateProduct: CreateProductRequest -> Product
    abstract member UpdateProduct: Guid -> UpdateProductRequest -> Product option
    abstract member DeleteProduct: Guid -> bool
    abstract member SearchProducts: string -> Product list

type InMemoryProductService() =
    let products = ConcurrentDictionary<Guid, Product>()
    
    // Initialize with sample data
    do
        let sampleProducts = [
            {
                Id = Guid.NewGuid()
                Name = "Gaming Laptop"
                Description = "High-performance gaming laptop"
                Price = 1299.99m
                Category = "Electronics"
                InStock = true
                CreatedAt = DateTime.UtcNow.AddDays(-10.0)
                UpdatedAt = DateTime.UtcNow.AddDays(-2.0)
            }
            {
                Id = Guid.NewGuid()
                Name = "Wireless Mouse"
                Description = "Ergonomic wireless mouse"
                Price = 29.99m
                Category = "Electronics"
                InStock = true
                CreatedAt = DateTime.UtcNow.AddDays(-5.0)
                UpdatedAt = DateTime.UtcNow.AddDays(-1.0)
            }
        ]
        
        sampleProducts |> List.iter (fun p -> products.TryAdd(p.Id, p) |> ignore)
    
    interface IProductService with
        member _.GetAllProducts() =
            products.Values |> Seq.toList |> List.sortBy (fun p -> p.CreatedAt)
        
        member _.GetProductById(id: Guid) =
            match products.TryGetValue(id) with
            | (true, product) -> Some product
            | (false, _) -> None
        
        member _.CreateProduct(request: CreateProductRequest) =
            let product = Product.fromCreateRequest request
            products.TryAdd(product.Id, product) |> ignore
            product
        
        member _.UpdateProduct(id: Guid) (request: UpdateProductRequest) =
            match products.TryGetValue(id) with
            | (true, existingProduct) ->
                let updatedProduct = Product.updateFromRequest existingProduct request
                products.TryUpdate(id, updatedProduct, existingProduct) |> ignore
                Some updatedProduct
            | (false, _) -> None
        
        member _.DeleteProduct(id: Guid) =
            products.TryRemove(id) |> fst
        
        member _.SearchProducts(query: string) =
            let searchTerm = query.ToLower()
            products.Values
            |> Seq.filter (fun p -> 
                p.Name.ToLower().Contains(searchTerm) || 
                p.Description.ToLower().Contains(searchTerm) ||
                p.Category.ToLower().Contains(searchTerm)
            )
            |> Seq.toList
```

### Exercise 4: API Handlers
Create `Handlers/ProductHandlers.fs`:
```fsharp
namespace FSharpWebAPI.Handlers

open System
open Microsoft.AspNetCore.Http
open Giraffe
open FSharpWebAPI.Models
open FSharpWebAPI.Services

module ProductHandlers =
    
    let createSuccessResponse data message = {
        Success = true
        Data = Some data
        Message = message
        Timestamp = DateTime.UtcNow
    }
    
    let createErrorResponse message = {
        Success = false
        Data = None
        Message = message
        Timestamp = DateTime.UtcNow
    }
    
    let getAllProductsHandler: HttpHandler =
        fun next ctx ->
            let productService = ctx.GetService<IProductService>()
            let products = productService.GetAllProducts()
            let response = products |> List.map Product.toResponse
            let apiResponse = createSuccessResponse response "Products retrieved successfully"
            json apiResponse next ctx
    
    let getProductByIdHandler (id: string): HttpHandler =
        fun next ctx ->
            match Guid.TryParse(id) with
            | (true, productId) ->
                let productService = ctx.GetService<IProductService>()
                match productService.GetProductById(productId) with
                | Some product ->
                    let response = Product.toResponse product
                    let apiResponse = createSuccessResponse response "Product found"
                    json apiResponse next ctx
                | None ->
                    let apiResponse = createErrorResponse "Product not found"
                    setStatusCode 404 >=> json apiResponse $ next $ ctx
            | (false, _) ->
                let apiResponse = createErrorResponse "Invalid product ID format"
                setStatusCode 400 >=> json apiResponse $ next $ ctx
    
    let createProductHandler: HttpHandler =
        fun next ctx -> task {
            try
                let! request = ctx.BindJsonAsync<CreateProductRequest>()
                let productService = ctx.GetService<IProductService>()
                let product = productService.CreateProduct(request)
                let response = Product.toResponse product
                let apiResponse = createSuccessResponse response "Product created successfully"
                return! (setStatusCode 201 >=> json apiResponse) next ctx
            with
            | ex ->
                let apiResponse = createErrorResponse (sprintf "Failed to create product: %s" ex.Message)
                return! (setStatusCode 400 >=> json apiResponse) next ctx
        }
    
    let updateProductHandler (id: string): HttpHandler =
        fun next ctx -> task {
            match Guid.TryParse(id) with
            | (true, productId) ->
                try
                    let! request = ctx.BindJsonAsync<UpdateProductRequest>()
                    let productService = ctx.GetService<IProductService>()
                    match productService.UpdateProduct productId request with
                    | Some product ->
                        let response = Product.toResponse product
                        let apiResponse = createSuccessResponse response "Product updated successfully"
                        return! json apiResponse next ctx
                    | None ->
                        let apiResponse = createErrorResponse "Product not found"
                        return! (setStatusCode 404 >=> json apiResponse) next ctx
                with
                | ex ->
                    let apiResponse = createErrorResponse (sprintf "Failed to update product: %s" ex.Message)
                    return! (setStatusCode 400 >=> json apiResponse) next ctx
            | (false, _) ->
                let apiResponse = createErrorResponse "Invalid product ID format"
                return! (setStatusCode 400 >=> json apiResponse) next ctx
        }
    
    let deleteProductHandler (id: string): HttpHandler =
        fun next ctx ->
            match Guid.TryParse(id) with
            | (true, productId) ->
                let productService = ctx.GetService<IProductService>()
                if productService.DeleteProduct(productId) then
                    let apiResponse = createSuccessResponse () "Product deleted successfully"
                    json apiResponse next ctx
                else
                    let apiResponse = createErrorResponse "Product not found"
                    setStatusCode 404 >=> json apiResponse $ next $ ctx
            | (false, _) ->
                let apiResponse = createErrorResponse "Invalid product ID format"
                setStatusCode 400 >=> json apiResponse $ next $ ctx
    
    let searchProductsHandler: HttpHandler =
        fun next ctx ->
            match ctx.TryGetQueryStringValue("q") with
            | Some query ->
                let productService = ctx.GetService<IProductService>()
                let products = productService.SearchProducts(query)
                let response = products |> List.map Product.toResponse
                let apiResponse = createSuccessResponse response (sprintf "Found %d products" (List.length products))
                json apiResponse next ctx
            | None ->
                let apiResponse = createErrorResponse "Search query parameter 'q' is required"
                setStatusCode 400 >=> json apiResponse $ next $ ctx
```

### Exercise 5: Routing and Middleware
Create `Routes.fs`:
```fsharp
namespace FSharpWebAPI

open Giraffe
open FSharpWebAPI.Handlers.ProductHandlers
open Microsoft.AspNetCore.Http

module Routes =
    
    // Logging middleware
    let loggingHandler: HttpHandler =
        fun next ctx -> task {
            let method = ctx.Request.Method
            let path = ctx.Request.Path.ToString()
            let timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            printfn "[%s] %s %s" timestamp method path
            return! next ctx
        }
    
    // CORS middleware
    let corsHandler: HttpHandler =
        fun next ctx ->
            ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*")
            ctx.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS")
            ctx.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization")
            next ctx
    
    // API routes
    let apiRoutes =
        subRoute "/api" (
            choose [
                subRoute "/products" (
                    choose [
                        GET >=> route "" >=> getAllProductsHandler
                        GET >=> route "/search" >=> searchProductsHandler
                        GET >=> routef "/%s" getProductByIdHandler
                        POST >=> route "" >=> createProductHandler
                        PUT >=> routef "/%s" updateProductHandler
                        DELETE >=> routef "/%s" deleteProductHandler
                    ]
                )
                GET >=> route "/health" >=> json {| status = "healthy"; timestamp = System.DateTime.UtcNow |}
            ]
        )
    
    // Main web application
    let webApp =
        loggingHandler >=> corsHandler >=> choose [
            GET >=> route "/" >=> htmlFile "wwwroot/index.html"
            apiRoutes
            setStatusCode 404 >=> json {| error = "Not Found" |}
        ]
```

### Exercise 6: Complete Program Setup
Update `Program.fs`:
```fsharp
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Giraffe
open FSharpWebAPI
open FSharpWebAPI.Services

let configureServices (services: IServiceCollection) =
    services
        .AddGiraffe()
        .AddSingleton<IProductService, InMemoryProductService>()
        .AddLogging()
        .AddCors() |> ignore

let configureApp (app: IApplicationBuilder) =
    app
        .UseCors(fun policy ->
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader() |> ignore
        )
        .UseStaticFiles()
        .UseGiraffe(Routes.webApp) |> ignore

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    
    configureServices builder.Services
    
    let app = builder.Build()
    
    configureApp app
    
    app.Run()
    
    0
```

## 🏃‍♂️ Practice Tasks

### Task 1: Authentication & Authorization
Implement:
1. JWT token authentication
2. User registration and login
3. Role-based authorization
4. Protected routes

### Task 2: Database Integration
Add support for:
1. Entity Framework Core
2. Database migrations
3. Connection string configuration
4. Repository pattern

### Task 3: Advanced Features
Implement:
1. File upload handling
2. Pagination for large datasets
3. Caching with Redis
4. Rate limiting

### Task 4: Frontend Integration
Create:
1. HTML pages with forms
2. JavaScript client for API consumption
3. Real-time updates with SignalR
4. Single-page application

## ✅ Completion Checklist
- [ ] Set up Giraffe web framework
- [ ] Implemented REST API with CRUD operations
- [ ] Added proper routing and middleware
- [ ] Created JSON serialization
- [ ] Implemented error handling
- [ ] Added logging and CORS support
- [ ] Built complete product management API
- [ ] Tested all endpoints

## 🌍 **Environment Configuration** ⭐ **(MỚI)**

### **🔐 Security & Configuration:**
- ✅ **Environment Variables**: Credentials không còn hardcoded
- ✅ **.env File Support**: Easy local development setup  
- ✅ **.gitignore**: Bảo vệ secrets khỏi version control
- ✅ **Fallback Values**: Default values cho development

### **Setup Environment:**
```bash
# 1. Copy template
cp .env.example .env

# 2. Update với credentials của bạn (optional - có defaults)
nano .env

# 3. Run application
dotnet run
```

### **Environment Variables:**
```bash
# Azure Cosmos DB Configuration
COSMOS_ENDPOINT_URL=https://your-account.documents.azure.com:443/
COSMOS_PRIMARY_KEY=your-cosmos-primary-key
COSMOS_DATABASE_ID=SampleDB
COSMOS_CUSTOMERS_CONTAINER_ID=Customers

# Web Server Configuration  
ASPNETCORE_URLS=http://localhost:5002
```

## 🏆 Final Achievement
Congratulations! You've completed the 16-day F# training program and built:
- Console applications
- Data processing tools  
- Web APIs with Azure Cosmos DB integration
- Environment-based configuration 
- Complete understanding of functional programming

**You're now ready to build production F# applications! 🎉**