namespace Shop.DataAccess

open System
open System.Text.Json
open System.Text.Json.Serialization
open System.IO
open Shop.Models

// Custom JSON converter for ProductId
type ProductIdConverter() =
    inherit JsonConverter<ProductId>()
    
    override _.Read(reader: byref<Utf8JsonReader>, typeToConvert: Type, options: JsonSerializerOptions) =
        ProductId (reader.GetString())
    
    override _.Write(writer: Utf8JsonWriter, value: ProductId, options: JsonSerializerOptions) =
        let (ProductId id) = value
        writer.WriteStringValue(id)

module ProductRepository =
    
    let private jsonOptions = 
        let options = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
        options.Converters.Add(ProductIdConverter())
        options
    
    // Load products with default path (async)
    let loadProductsAsync () : Async<Result<Product list, ShopError>> =
        async {
            // Simple: Always use absolute path from current file location
            let productsPath = Path.Combine(__SOURCE_DIRECTORY__, "..", "Data", "products.json")
            let! result = JsonHandler.loadFromJsonFileAsync<Product[]> productsPath (Some jsonOptions)
            return result |> Result.map Array.toList
        }
    

    
    // Find product by ID
    let findById (productId: ProductId) (products: Product list) : Product option =
        products |> List.tryFind (fun p -> p.Id = productId)
    
    // Filter products by category
    let filterByCategory (category: string) (products: Product list) : Product list =
        products |> List.filter (fun p -> p.Category.ToLower() = category.ToLower())
    
    // Search products with fuzzy matching and relevance scoring using StringUtils.fuzzyMatch
    let searchProducts (query: string) (products: Product list) : Product list =
        products
        |> List.map (fun product ->
            let nameScore = StringUtils.fuzzyMatch query product.Name
            let descScore = StringUtils.fuzzyMatch query product.Description
            let tagScore = 
                if product.Tags.IsEmpty then 0.0
                else
                    product.Tags 
                    |> List.map (StringUtils.fuzzyMatch query)
                    |> List.max
            let totalScore = nameScore + descScore + tagScore
            (product, totalScore)
        )
        |> List.filter (fun (_, score) -> score > 0.0)
        |> List.sortByDescending snd
        |> List.map fst
    
    // Get products with low stock
    let getLowStockProducts (threshold: int) (products: Product list) : Product list =
        products |> List.filter (fun p -> p.Stock <= threshold)
    
    // Get all categories
    let getCategories (products: Product list) : string list =
        products 
        |> List.map (fun p -> p.Category)
        |> List.distinct
        |> List.sort
    
    // Update product stock
    let updateStock (productId: ProductId) (newStock: int) (products: Product list) : Product list =
        products
        |> List.map (fun p -> 
            if p.Id = productId then { p with Stock = newStock }
            else p)