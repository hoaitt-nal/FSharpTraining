module SearchRepository

open System
open Azure
open Azure.Search.Documents
open Azure.Search.Documents.Indexes
open Azure.Search.Documents.Indexes.Models
open Azure.Search.Documents.Models
open Models

// ============ Azure Search Configuration ============

type SearchConfig =
    { ServiceEndpoint: string
      AdminApiKey: string
      IndexName: string }

type Filters = { 
    email: string option
    fullName: string option
    dateOfBirth: DateTime option
    country: string option
    city: string option
    isActive: bool option
    totalOrdersFrom: int option
    totalOrdersTo: int option
    totalSpentFrom: double option
    totalSpentTo: double option
    loyaltyTier: string option 
}
type SearchRequest = {
    searchText: string option
    filters: Filters option
    sort: string option
    top: int option
    skip: int option
}

// ============ Search Document Model ============
// Mapping từ Customer sang Search Document
[<CLIMutable>]
type CustomerSearchDocument =
    { [<SimpleField(IsKey = true)>]
      id: string

      [<SearchableField(IsFilterable = true, IsSortable = true)>]
      customerId: string

      [<SearchableField(IsFilterable = true, IsSortable = true)>]
      email: string

      [<SearchableField(IsFilterable = true, IsSortable = true)>]
      firstName: string

      [<SearchableField(IsFilterable = true, IsSortable = true)>]
      lastName: string

      [<SearchableField(IsFilterable = true, IsSortable = true)>]
      fullName: string

      [<SimpleField(IsFilterable = true, IsSortable = true)>]
      dateOfBirth: DateTime

      [<SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)>]
      country: string

      [<SearchableField(IsFilterable = true, IsSortable = true)>]
      city: string

      [<SimpleField(IsFilterable = true, IsSortable = true)>]
      registrationDate: DateTime

      [<SimpleField(IsFilterable = true, IsSortable = true)>]
      isActive: bool

      [<SimpleField(IsFilterable = true, IsSortable = true)>]
      totalOrders: int

      [<SimpleField(IsFilterable = true, IsSortable = true)>]
      totalSpent: double

      [<SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)>]
      loyaltyTier: string }

// ============ Helper Functions ============

module SearchHelpers =
    // Convert Customer to SearchDocument
    let customerToSearchDocument (customer: Customer) : CustomerSearchDocument =
        { id = customer.id
          customerId = customer.customerId
          email = customer.email
          firstName = customer.firstName
          lastName = customer.lastName
          fullName = $"{customer.firstName} {customer.lastName}"
          dateOfBirth = customer.dateOfBirth
          country = customer.country
          city = customer.city
          registrationDate = customer.registrationDate
          isActive = customer.isActive
          totalOrders = customer.totalOrders
          totalSpent = float customer.totalSpent
          loyaltyTier = customer.loyaltyTier }
          // Convert query string parameters to Filters record
    let parseFiltersToQueryString (filters: Filters option) : string = 
        let filterList: Collections.Generic.List<string> = System.Collections.Generic.List<string>()
        
        match filters with
        | Some f ->
            f.email 
                |> Option.iter (fun e -> filterList.Add($"email eq '{e}'"))
            f.fullName
                |> Option.iter (fun fn -> filterList.Add($"search.ismatch('{fn}', 'fullName')"))
            // f.dateOfBirth
            //     |> Option.iter (fun dob -> filterList.Add($"dateOfBirth eq {dob:yyyy-MM-dd}"))
            f.country
                |> Option.iter (fun c -> filterList.Add($"country eq '{c}'"))
            f.city
                |> Option.iter (fun c -> filterList.Add($"search.in('{c}', 'city')"))
            f.isActive
                |> Option.iter (fun active -> filterList.Add($"isActive eq {active.ToString().ToLower()}"))
            f.totalOrdersFrom
                |> Option.iter (fun min -> filterList.Add($"totalOrders ge {min}"))
            f.totalOrdersTo
                |> Option.iter (fun max -> filterList.Add($"totalOrders le {max}"))
            f.totalSpentFrom
                |> Option.iter (fun min -> filterList.Add($"totalSpent ge {min}"))
            f.totalSpentTo
                |> Option.iter (fun max -> filterList.Add($"totalSpent le {max}"))
            f.loyaltyTier
                |> Option.iter (fun tier -> filterList.Add $"loyaltyTier eq '{tier}'")
        | None -> ()

        String.concat " and " filterList

// ============ Azure Search Repository ============

type SearchRepository(config: SearchConfig) =

    let searchClient =
        let credential = AzureKeyCredential(config.AdminApiKey)
        new SearchClient(Uri(config.ServiceEndpoint), config.IndexName, credential)

    let indexClient =
        let credential = AzureKeyCredential(config.AdminApiKey)
        new SearchIndexClient(Uri(config.ServiceEndpoint), credential)

    // ============ INDEX MANAGEMENT ============

    // Tạo hoặc update search index
    member this.CreateOrUpdateIndexAsync() =
        task {
            try
                let indexDefinition = SearchIndex(config.IndexName)
                let fieldBuilder = FieldBuilder()
                indexDefinition.Fields <- fieldBuilder.Build(typeof<CustomerSearchDocument>)

                let! _ = indexClient.CreateOrUpdateIndexAsync(indexDefinition)
                return Ok $"Index '{config.IndexName}' created/updated successfully"
            with ex ->
                return Error $"Error creating index: {ex.Message}"
        }

    // ============ INDEXING OPERATIONS ============

    // Index một customer document
    member this.IndexCustomerAsync(customer: Customer) =
        task {
            try
                let searchDoc = SearchHelpers.customerToSearchDocument customer
                let batch = IndexDocumentsBatch.Upload([ searchDoc ])
                let! response = searchClient.IndexDocumentsAsync(batch)

                if response.Value.Results.Count > 0 && response.Value.Results.[0].Succeeded then
                    return Ok "Customer indexed successfully"
                else
                    return Error "Failed to index customer"
            with ex ->
                return Error $"Error indexing customer: {ex.Message}"
        }

    // Index nhiều customers cùng lúc (bulk indexing)
    member this.BulkIndexCustomersAsync(customers: Customer list) =
        task {
            try
                let searchDocs = customers |> List.map SearchHelpers.customerToSearchDocument
                let batch = IndexDocumentsBatch.Upload(searchDocs)
                let! response = searchClient.IndexDocumentsAsync(batch)

                let successCount =
                    response.Value.Results |> Seq.filter (fun r -> r.Succeeded) |> Seq.length

                return Ok $"Indexed {successCount}/{customers.Length} customers successfully"
            with ex ->
                return Error $"Error bulk indexing: {ex.Message}"
        }

    // Delete document khỏi index
    member this.DeleteFromIndexAsync(customerId: string) =
        task {
            try
                let batch = IndexDocumentsBatch.Delete("id", [ customerId ])
                let! response = searchClient.IndexDocumentsAsync(batch)
                return Ok "Document deleted from index"
            with ex ->
                return Error $"Error deleting from index: {ex.Message}"
        }

    // ============ SEARCH OPERATIONS ============
    // Advanced search với multiple filters và sorting
    member this.AdvancedSearchAsync
        (searchText: string, filters: Filters option, sort: string option, top : int option, skip: int option)
        =
        task {
            try
                let options = SearchOptions()
                printf "Filter string: %A" filters
                let filterString = SearchHelpers.parseFiltersToQueryString filters
                printf "Filter string: %s" filterString
                if not (String.IsNullOrWhiteSpace filterString) then
                    options.Filter <- filterString

                // Sorting
                options.OrderBy.Add(sort |> Option.defaultValue "customerId asc")
                options.Size <- defaultArg top  1
                options.Skip <- defaultArg skip 0
                options.IncludeTotalCount <- true

                let! response = searchClient.SearchAsync<CustomerSearchDocument>(searchText, options)

                let results =
                    response.Value.GetResults() |> Seq.map (fun r -> r.Document) |> List.ofSeq

                return Ok results
            with ex ->
                return Error $"Error in advanced search: {ex.Message}"
        }
