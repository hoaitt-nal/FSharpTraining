namespace Shop.DataAccess

open System
open System.Text.Json
open System.Text.Json.Serialization
open System.IO
open Shop.Models

// Custom JSON converter for CustomerId
type CustomerIdConverter() =
    inherit JsonConverter<CustomerId>()

    override _.Read(reader: byref<Utf8JsonReader>, typeToConvert: Type, options: JsonSerializerOptions) =
        CustomerId(reader.GetString())

    override _.Write(writer: Utf8JsonWriter, value: CustomerId, options: JsonSerializerOptions) =
        let (CustomerId id) = value
        writer.WriteStringValue(id)

module CustomerRepository =

    let private jsonOptions =
        let options =
            JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)

        options.Converters.Add(CustomerIdConverter())
        options

    // Load customers with default path (async)
    let loadCustomersAsync () : Async<Result<Customer list, ShopError>> =
        async {
            let defaultPath = Path.Combine("Data", "customers.json")
            let! result = JsonHandler.loadFromJsonFileAsync<Customer[]> defaultPath (Some jsonOptions)
            return result |> Result.map Array.toList
        }

    // Find customer by ID
    let findById (customerId: CustomerId) (customers: Customer list) : Customer option =
        customers |> List.tryFind (fun c -> c.Id = customerId)

    // Find customer by email
    let findByEmail (email: string) (customers: Customer list) : Customer option =
        customers |> List.tryFind (fun c -> c.Email.ToLower() = email.ToLower())

    // Search customers by name
    let searchByName (query: string) (customers: Customer list) : Customer list =
        let queryLower = query.ToLower()
        customers |> List.filter (fun c -> c.Name.ToLower().Contains(queryLower))
