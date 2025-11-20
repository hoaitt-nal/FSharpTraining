namespace OrderProcessor

module DataLayer =
    // Electronics & Computer Products
    let product1 = { Id = 1; Name = "Laptop"; Price = 1000.00M }
    let product2 = { Id = 2; Name = "Mouse"; Price = 50.00M }
    let product3 = { Id = 3; Name = "Keyboard"; Price = 80.00M }
    let product4 = { Id = 4; Name = "Monitor"; Price = 200.00M }
    let product5 = { Id = 5; Name = "Printer"; Price = 150.00M }
    let product6 = { Id = 6; Name = "Webcam"; Price = 75.00M }
    let product7 = { Id = 7; Name = "Headphones"; Price = 120.00M }
    let product8 = { Id = 8; Name = "Tablet"; Price = 350.00M }
    let product9 = { Id = 9; Name = "Smartphone"; Price = 800.00M }
    let product10 = { Id = 10; Name = "USB Cable"; Price = 15.00M }
    
    // Office & Stationery Products  
    let product11 = { Id = 11; Name = "Office Chair"; Price = 250.00M }
    let product12 = { Id = 12; Name = "Desk Lamp"; Price = 45.00M }
    let product13 = { Id = 13; Name = "Notebook Set"; Price = 25.00M }
    let product14 = { Id = 14; Name = "Pen Set"; Price = 30.00M }
    let product15 = { Id = 15; Name = "Calculator"; Price = 35.00M }
    
    // Home & Lifestyle Products
    let product16 = { Id = 16; Name = "Coffee Maker"; Price = 180.00M }
    let product17 = { Id = 17; Name = "Water Bottle"; Price = 20.00M }
    let product18 = { Id = 18; Name = "Backpack"; Price = 65.00M }
    let product19 = { Id = 19; Name = "Wireless Charger"; Price = 40.00M }
    let product20 = { Id = 20; Name = "Bluetooth Speaker"; Price = 90.00M }
    
    let products = [
        // Electronics & Computer Products
        product1; product2; product3; product4; product5
        product6; product7; product8; product9; product10
        
        // Office & Stationery Products
        product11; product12; product13; product14; product15
        
        // Home & Lifestyle Products  
        product16; product17; product18; product19; product20
    ]
    
    // Product Categories for filtering
    let electronicsProducts = [product1; product2; product3; product4; product5; product6; product7; product8; product9; product10]
    let officeProducts = [product11; product12; product13; product14; product15]  
    let lifestyleProducts = [product16; product17; product18; product19; product20]
    
    // Helper functions to work with products
    let getProductById (id: int) : Product option =
        products |> List.tryFind (fun p -> p.Id = id)
    
    let getAllProducts () : Product list =
        products
        
    let getProductsByCategory (category: string) : Product list =
        match category.ToLower() with
        | "electronics" -> electronicsProducts
        | "office" -> officeProducts  
        | "lifestyle" -> lifestyleProducts
        | _ -> []
    
    let getProductsByPriceRange (minPrice: decimal) (maxPrice: decimal) : Product list =
        products 
        |> List.filter (fun p -> p.Price >= minPrice && p.Price <= maxPrice)
    
    let searchProductsByName (searchTerm: string) : Product list =
        products
        |> List.filter (fun p -> p.Name.ToLower().Contains(searchTerm.ToLower()))
    
    let getRandomProducts (count: int) : Product list =
        let random = System.Random()
        products 
        |> List.sortBy (fun _ -> random.Next())
        |> List.take (min count products.Length)
    
    // Statistics functions
    let getProductStats () =
        let totalProducts = products.Length
        let avgPrice = products |> List.averageBy (fun p -> p.Price)
        let minPrice = products |> List.minBy (fun p -> p.Price)
        let maxPrice = products |> List.maxBy (fun p -> p.Price)
        let totalValue = products |> List.sumBy (fun p -> p.Price)
        
        {| 
            TotalProducts = totalProducts
            AveragePrice = avgPrice
            MinPriceProduct = minPrice
            MaxPriceProduct = maxPrice  
            TotalInventoryValue = totalValue
            ElectronicsCount = electronicsProducts.Length
            OfficeCount = officeProducts.Length
            LifestyleCount = lifestyleProducts.Length
        |}