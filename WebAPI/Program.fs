open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open WebAPI.Controllers

// 🛣️ API Routes
let webApp =
    choose [
        GET >=>
            choose [
                route "/api/products" >=> ProductController.getAllProducts
                routef "/api/products/search/%s" ProductController.searchProducts
                routef "/api/products/%s" ProductController.getProductById
                route "/" >=> json {| 
                    message = "🛍️ F# Shop API with Giraffe"
                    version = "1.0"
                    endpoints = [
                        "GET /api/products - Get all products"
                        "GET /api/products/search/{query} - Search products"
                        "GET /api/products/stats - Get product statistics"
                        "GET /api/health - Health check"
                    ]
                |}
            ]
        setStatusCode 404 >=> text "Not Found"
    ]

// 🔧 Configure services
let configureServices (services: IServiceCollection) =
    services.AddGiraffe() |> ignore

// 🏗️ Configure app
let configureApp (app: IApplicationBuilder) =
    app
        .UseRouting()
        .UseGiraffe(webApp)

[<EntryPoint>]
let main args =
    // 📁 Ensure we can find the Data directory
    let contentRoot = Directory.GetCurrentDirectory()
    printfn "🚀 Starting F# Shop API"
    printfn "📂 Content Root: %s" contentRoot
    
    // Check if products.json exists
    let dataPath = Path.Combine(contentRoot, "Data", "products.json")
    if File.Exists dataPath then
        printfn "✅ Found products.json at: %s" dataPath
    else
        printfn "⚠️  products.json not found at: %s" dataPath
        printfn "📋 Available files:"
        if Directory.Exists(Path.Combine(contentRoot, "Data")) then
            Directory.GetFiles(Path.Combine(contentRoot, "Data"))
            |> Array.iter (fun f -> printfn "   - %s" (Path.GetFileName f))
        else
            printfn "   Data directory not found"
    
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .UseContentRoot(contentRoot)
                .Configure(configureApp)
                .ConfigureServices(configureServices)
                .UseUrls("http://localhost:5000", "https://localhost:5001")
                |> ignore)
        .Build()
        .Run()

    0