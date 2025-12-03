# Azure Cognitive Search Integration Guide

## 📚 Tổng quan

Azure Cognitive Search là dịch vụ search-as-a-service cho phép bạn thêm khả năng tìm kiếm full-text, faceted navigation, và advanced querying vào ứng dụng của bạn.

## 🔧 Cài đặt

### 1. Thêm package NuGet

```bash
cd Day15-16-WebWithGiraffe
dotnet add package Azure.Search.Documents
```

### 2. Tạo Azure Search Service

```bash
# Tạo resource group (nếu chưa có)
az group create --name myResourceGroup --location eastus

# Tạo Azure Search service
az search service create \
  --name my-search-service \
  --resource-group myResourceGroup \
  --sku basic

# Lấy admin key
az search admin-key show \
  --service-name my-search-service \
  --resource-group myResourceGroup
```

### 3. Cấu hình Environment Variables

Thêm vào file `.env`:

```bash
# Azure Search Configuration
AZURE_SEARCH_ENDPOINT=https://my-search-service.search.windows.net
AZURE_SEARCH_ADMIN_KEY=your-admin-key-here
AZURE_SEARCH_INDEX_NAME=customers-index
```

## 🏗️ Kiến trúc

### Luồng dữ liệu

```
Cosmos DB (Source) → Azure Search Index → Search Queries
     ↓                      ↓                    ↓
  CRUD Ops          Index/Update/Delete      Fast Search
```

### Các thành phần chính

1. **SearchRepository.fs** - Xử lý tất cả operations với Azure Search
2. **SearchControllers.fs** - HTTP handlers cho search endpoints
3. **CustomerSearchDocument** - Model cho search documents với attributes

## 📖 Sử dụng

### Khởi tạo Search Index

Tạo index trước khi sử dụng:

```bash
# Request
GET http://localhost:5002/api/search/init

# Response
{
  "success": true,
  "message": "Index 'customers-index' created/updated successfully"
}
```

### Bulk Indexing từ Cosmos DB

Đưa tất cả customers từ Cosmos DB vào Search index:

```bash
# Request
POST http://localhost:5002/api/search/bulk-index

# Response
{
  "success": true,
  "message": "Indexed 150/150 customers successfully"
}
```

### Tìm kiếm cơ bản

```bash
# Tìm kiếm full-text
GET http://localhost:5002/api/search?q=john&top=10

# Response
{
  "success": true,
  "count": 5,
  "customers": [...]
}
```

### Tìm kiếm với Filter

```bash
# Filter theo country và loyalty tier
GET http://localhost:5002/api/search/filter?q=*&filter=country eq 'USA' and loyaltyTier eq 'Gold'

# Response
{
  "success": true,
  "count": 12,
  "customers": [...]
}
```

### Tìm kiếm theo Country

```bash
GET http://localhost:5002/api/search/country/USA

# Response
{
  "success": true,
  "country": "USA",
  "count": 45,
  "customers": [...]
}
```

### Advanced Search

Tìm kiếm với multiple filters:

```bash
GET http://localhost:5002/api/search/advanced?q=john&country=USA&tier=Gold&minSpent=1000

# Response
{
  "success": true,
  "count": 3,
  "customers": [...]
}
```

## 🔍 Search Features

### 1. Full-Text Search

Tìm kiếm trong tất cả searchable fields:

```fsharp
let! result = searchRepo.SearchCustomersAsync("john smith", top = 10)
```

### 2. Filtering

Lọc chính xác với OData syntax:

```fsharp
// Filter examples
"country eq 'USA'"
"totalSpent ge 1000"
"isActive eq true"
"country eq 'USA' and loyaltyTier eq 'Gold'"
```

### 3. Faceted Search

Nhóm kết quả theo categories:

```fsharp
// Facets cho country và loyalty tier
options.Facets.Add("country")
options.Facets.Add("loyaltyTier")
```

### 4. Sorting

Sắp xếp kết quả:

```fsharp
options.OrderBy.Add("totalSpent desc")
options.OrderBy.Add("registrationDate asc")
```

### 5. Suggestions (Autocomplete)

Gợi ý trong khi người dùng typing:

```fsharp
let! suggestions = searchRepo.SuggestAsync("joh", "customer-suggester")
```

## 🎯 Use Cases

### 1. E-commerce Product Search

```fsharp
// Tìm customers theo spending và location
searchRepo.AdvancedSearchAsync(
    searchText = "*",
    country = Some "USA",
    loyaltyTier = Some "Gold",
    minSpent = Some 5000M
)
```

### 2. CRM Customer Lookup

```fsharp
// Tìm customer theo email hoặc name
searchRepo.SearchCustomersAsync("john@example.com")
```

### 3. Analytics Dashboard

```fsharp
// Faceted search cho dashboard filters
searchRepo.SearchByCountryAsync("USA")  // với facets
```

## 🔄 Sync Strategy

### Change Feed Pattern (Recommended)

Tự động sync Cosmos DB changes với Search index:

```fsharp
// Pseudo-code for Change Feed
cosmosContainer.GetChangeFeedProcessorBuilder()
    .WithInstanceName("search-indexer")
    .WithLeaseContainer(leaseContainer)
    .WithChangeFeedHandler(fun changes ->
        task {
            for change in changes do
                match change.Operation with
                | Create | Replace -> 
                    do! searchRepo.IndexCustomerAsync(change.Document)
                | Delete ->
                    do! searchRepo.DeleteFromIndexAsync(change.Document.id)
        })
```

### Manual Sync

Sync on-demand sau mỗi CRUD operation:

```fsharp
// After creating customer in Cosmos
let! createResult = cosmosRepo.CreateCustomerAsync(customer)
match createResult with
| Ok _ -> 
    // Index to Search
    do! searchRepo.IndexCustomerAsync(customer)
| Error _ -> ()
```

## 🎨 Search Document Attributes

```fsharp
[<CLIMutable>]
type CustomerSearchDocument =
    { [<SimpleField(IsKey = true, IsFilterable = true)>]
      id: string                              // Primary key
      
      [<SearchableField(IsFilterable = true)>]
      email: string                           // Full-text searchable + filterable
      
      [<SearchableField(IsSortable = true)>]
      firstName: string                       // Full-text searchable + sortable
      
      [<SimpleField(IsFilterable = true)>]
      totalOrders: int                        // Filterable but not searchable
      
      [<SearchableField(IsFacetable = true)>]
      country: string }                       // For faceted navigation
```

### Field Attributes Giải thích

- **SimpleField**: Không full-text search, dùng cho exact match
- **SearchableField**: Full-text search được
- **IsKey**: Primary key (phải unique)
- **IsFilterable**: Có thể dùng trong filter expression
- **IsSortable**: Có thể sort theo field này
- **IsFacetable**: Có thể dùng cho faceted navigation

## 📊 Performance Tips

### 1. Batch Indexing

```fsharp
// Good: Batch nhiều documents
searchRepo.BulkIndexCustomersAsync(customers)

// Bad: Index từng document riêng lẻ
for customer in customers do
    searchRepo.IndexCustomerAsync(customer)
```

### 2. Use Filters Before Full-Text Search

```fsharp
// Good: Filter trước để reduce search scope
filter = "country eq 'USA' and isActive eq true"

// Rồi mới search
searchText = "john"
```

### 3. Limit Result Size

```fsharp
// Luôn set top để avoid large result sets
searchRepo.SearchCustomersAsync(searchText, top = 20)
```

## 🐛 Troubleshooting

### Issue 1: Index not found

```bash
# Verify index exists
GET https://my-search-service.search.windows.net/indexes/customers-index?api-version=2021-04-30-Preview
```

### Issue 2: Field not searchable

Ensure field có `SearchableField` attribute:

```fsharp
[<SearchableField>]
firstName: string  // Now searchable
```

### Issue 3: Slow queries

- Enable query logging
- Check query complexity
- Consider adding replicas

## 🔐 Security Best Practices

1. **Use Query Keys for read-only operations**

```fsharp
// Production: Use query key instead of admin key
let credential = AzureKeyCredential(queryKey)
```

2. **Implement Row-Level Security**

```fsharp
// Filter by user's tenantId
filter = $"tenantId eq '{currentUser.TenantId}'"
```

3. **Rate Limiting**

```fsharp
// Implement rate limiting on search endpoints
app.UseRateLimiter()
```

## 📈 Monitoring

### Key Metrics

- **Search latency**: Monitor average query response time
- **Index size**: Track index growth
- **QPS (Queries Per Second)**: Monitor query load

### Azure Monitor Integration

```bash
# Enable diagnostic logging
az monitor diagnostic-settings create \
  --name search-diagnostics \
  --resource /subscriptions/.../searchServices/my-search-service \
  --logs '[{"category": "OperationLogs","enabled": true}]'
```

## 🎓 Learning Resources

- [Azure Search Documentation](https://docs.microsoft.com/azure/search/)
- [OData Filter Syntax](https://docs.microsoft.com/azure/search/search-query-odata-filter)
- [Lucene Query Syntax](https://docs.microsoft.com/azure/search/query-lucene-syntax)

## 📝 Next Steps

1. ✅ Install Azure.Search.Documents package
2. ✅ Create SearchRepository.fs
3. ✅ Create SearchControllers.fs
4. ⏳ Update Program.fs to register Search services
5. ⏳ Update project file with new files
6. ⏳ Add search endpoints to routing
7. ⏳ Test search functionality
8. ⏳ Implement Change Feed for auto-sync
