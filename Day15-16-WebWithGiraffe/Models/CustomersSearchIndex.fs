module CustomersSearchIndex

open System
open Azure.Search.Documents.Indexes
open Azure.Search.Documents.Indexes.Models
open Microsoft.Spatial

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
