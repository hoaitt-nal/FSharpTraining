namespace WebAPI.Controllers

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Giraffe
open Shop.Models
open Shop.DataAccess

module ProductController =
    
    // 📋 Get all products
    let getAllProducts : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                try
                    let! result = ProductRepository.loadProductsAsync() |> Async.StartAsTask
                    match result with
                    | Ok products ->
                        let response = {|
                            success = true
                            message = "Products loaded successfully"
                            data = products
                            count = products.Length
                        |}
                        return! json response next ctx
                    | Error error ->
                        let errorResponse = {|
                            success = false
                            message = sprintf "Error loading products: %A" error
                            data = []
                            count = 0
                        |}
                        return! (setStatusCode 500 >=> json errorResponse) next ctx
                with ex ->
                    let errorResponse = {|
                        success = false
                        message = sprintf "Exception: %s" ex.Message
                        data = []
                        count = 0
                    |}
                    return! (setStatusCode 500 >=> json errorResponse) next ctx
            }
    
    // 🔍 Search products
    let searchProducts (query: string) : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                try
                    let! result = ProductRepository.loadProductsAsync() |> Async.StartAsTask
                    match result with
                    | Ok products ->
                        let matchedProducts = ProductRepository.searchProducts query products
                        let response = {|
                            success = true
                            message = sprintf "Found %d products matching '%s'" matchedProducts.Length query
                            data = matchedProducts
                            count = matchedProducts.Length
                        |}
                        return! json response next ctx
                    | Error error ->
                        let errorResponse = {|
                            success = false
                            message = sprintf "Error loading products: %A" error
                            data = []
                            count = 0
                        |}
                        return! (setStatusCode 500 >=> json errorResponse) next ctx
                with ex ->
                    let errorResponse = {|
                        success = false
                        message = sprintf "Exception: %s" ex.Message
                        data = []
                        count = 0
                    |}
                    return! (setStatusCode 500 >=> json errorResponse) next ctx
            }

    // 📊 Get product by ID
    let getProductById (productId: string) : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                try
                    let! result = ProductRepository.loadProductsAsync() |> Async.StartAsTask
                    match result with
                    | Ok products ->
                        let productOpt = 
                            products 
                            |> List.tryFind (fun p -> let (ProductId id) = p.Id in id = productId)
                        match productOpt with
                        | Some product ->
                            let response = {|
                                success = true
                                message = "Product found"
                                data = product
                            |}
                            return! json response next ctx
                        | None ->
                            let notFoundResponse = {|
                                success = false
                                message = sprintf "Product with ID %s not found" productId
                            |}
                            return! (setStatusCode 404 >=> json notFoundResponse) next ctx
                    | Error error ->
                        let errorResponse = {|
                            success = false
                            message = sprintf "Error loading products: %A" error
                        |}
                        return! (setStatusCode 500 >=> json errorResponse) next ctx
                with ex ->
                    let errorResponse = {|
                        success = false
                        message = sprintf "Exception: %s" ex.Message
                    |}
                    return! (setStatusCode 500 >=> json errorResponse) next ctx
            }