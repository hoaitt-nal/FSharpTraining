#load "commonHelper.fsx"
#load "../Models/Customers.fs"
#load "../Models/CustomersSearchIndex.fs"
#load "../CosmosDB/CustomersRepositories.fs"
#load "../SearchIndex/SearchRepositories.fs"

open Models 
open CustomersRepository
open CustomersSearchIndex
open SearchRepository
open DotNetEnv

// Combined function to get valid country, city, and coordinates
let getCustomerLocationData (customer: Customer) : (string * string * float[] option) =
    let random = System.Random()
    
    // Step 1: Validate and get country
    let validCountry = 
        let countryExists = SampleData.countries |> Array.exists (fun c -> c = customer.country)
        if countryExists then 
            customer.country
        else 
            SampleData.countries.[random.Next(SampleData.countries.Length)]
    
    // Step 2: Get cities for the valid country
    let citiesForCountry = SampleData.cities.TryFind validCountry
    
    match citiesForCountry with
    | Some cityArray ->
        // Check if customer's city exists in the country's cities
        let cityExists = cityArray |> Array.exists (fun c -> c = customer.city)
        let validCity = 
            if cityExists then 
                customer.city
            else 
                cityArray.[random.Next(cityArray.Length)]
        
        // Step 3: Get coordinates for the city
        let coordinates = 
            match SampleData.citiesCoordinates.TryFind validCity with
            | Some coords -> Some coords
            | None -> 
                // Fallback to country coordinates
                SampleData.countriesCoordinates.TryFind validCountry
        
        validCountry, validCity, coordinates
    | None ->
        // If no cities found for country, use country coordinates
        let coordinates = SampleData.countriesCoordinates.TryFind validCountry
        validCountry, customer.city, coordinates
// Read existing customers from a CSV file and add coordinates
let DoMigrate () = 
    async {
        // Configure Cosmos DB connection
        let config = {
          EndpointUrl = Env.GetString("COSMOS_ENDPOINT_URL")
          PrimaryKey = Env.GetString("COSMOS_PRIMARY_KEY")
          DatabaseId = Env.GetString("COSMOS_DATABASE_ID")
          CustomerContainerId = Env.GetString("COSMOS_CUSTOMERS_CONTAINER_ID")
        }

        // Configure Azure Search
        let searchConfig : SearchConfig = {
            ServiceEndpoint = Env.GetString("AZURE_SEARCH_ENDPOINT")
            AdminApiKey = Env.GetString("AZURE_SEARCH_ADMIN_KEY")
            IndexName = Env.GetString("AZURE_SEARCH_INDEX_NAME")
        }

        printfn "Connecting to Cosmos DB at %A" config
        use repository = new CustomersRepository(config)
        let searchClient = new SearchRepository(searchConfig)
        // use searchRepository = new SearchRepository(searchConfig)
        let! customers = repository.GetAllCustomersAsync() |> Async.AwaitTask
        match customers with
        | Ok customerList ->
            printfn "Fetched %d customers" customerList.Length
            
            for customer in customerList do
                // Get valid country, city, and coordinates
                let (validCountry, validCity, coordinates) = getCustomerLocationData customer
                
                match coordinates with
                | Some coords ->
                    let updatedCustomer = 
                        { customer with 
                            country = validCountry
                            city = validCity
                            coordinates = Some coords }
                    let! updateResult = repository.UpdateCustomerAsync(updatedCustomer) |> Async.AwaitTask
                    searchClient.IndexCustomerAsync(updatedCustomer) |> Async.AwaitTask |> ignore
                    match updateResult with
                    | Ok _ -> 
                        if customer.country <> validCountry || customer.city <> validCity then
                            printfn "↻ Updated customer %s (ID: %s) - country: %s → %s, city: %s → %s, coordinates: %A" 
                                customer.firstName customer.id customer.country validCountry customer.city validCity coords
                        else
                            printfn "✓ Updated customer %s (ID: %s) with coordinates %A for %s, %s" 
                                customer.firstName customer.id coords validCity validCountry
                    | Error err -> printfn "✗ Failed to update customer %s: %s" customer.id err
                | None -> 
                    printfn "⚠ No coordinates found for country %s, city %s" validCountry validCity
        | Error err ->
            printfn "Failed to retrieve customers: %s" err
        return ()
    }
// Run the migration
DoMigrate () |> Async.RunSynchronously
