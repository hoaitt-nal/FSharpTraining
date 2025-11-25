open System
open Newtonsoft.Json

// ============ Azure Cosmos DB Core Concepts ============

(* 
Các khái niệm chính trong Azure Cosmos DB:

1. DATABASES: Logical container for collections/containers
   - Tương đương với database trong SQL
   - Chứa các containers (collections)
   - Có thể tạo nhiều databases trong một Cosmos account

2. CONTAINERS (Collections): Logical container for items/documents  
   - Tương đương với table trong SQL hoặc collection trong MongoDB
   - Chứa các items (documents)
   - Được định nghĩa bởi partition key strategy
   - Có throughput settings (RU/s - Request Units per second)

3. ITEMS (Documents): Individual records stored in containers
   - Tương đương với row trong SQL hoặc document trong MongoDB
   - Được lưu trữ dưới dạng JSON
   - Mỗi item phải có unique id trong partition
   - Có thể có schema linh hoạt

4. PARTITION KEY: Strategy để phân tán data across partitions
   - Được chọn khi tạo container
   - Không thể thay đổi sau khi container được tạo
   - Ảnh hưởng đến performance và scalability
   - Nên chọn property có high cardinality và even distribution

5. ID: Unique identifier cho mỗi item trong partition
   - Kết hợp với partition key tạo thành composite primary key
   - Phải unique trong cùng một partition
   - Được sử dụng cho point reads (ReadItemAsync)
*)

// ============ Domain Models ============

// Customer model with partition key strategy
[<CLIMutable>]
type Customer = {
    id: string                    // Unique identifier
    customerId: string           // Business identifier  
    email: string               // Contact information
    firstName: string           // Personal information
    lastName: string
    dateOfBirth: DateTime
    country: string             // Partition key - geographic distribution
    city: string
    registrationDate: DateTime
    isActive: bool
    totalOrders: int
    totalSpent: decimal
    loyaltyTier: string         // Bronze, Silver, Gold, Platinum
    lastLoginDate: DateTime option
    preferences: Map<string, obj>
    _etag: string option        // For optimistic concurrency control
}

// Order model with different partition strategy
[<CLIMutable>]
type Order = {
    id: string                  // Unique identifier
    orderId: string            // Business identifier
    customerId: string         // Reference to customer
    orderDate: DateTime
    status: string             // Pending, Processing, Shipped, Delivered, Cancelled
    totalAmount: decimal
    currency: string
    shippingAddress: Address
    items: OrderItem list
    paymentMethod: string
    trackingNumber: string option
    notes: string option
    _etag: string option
}

and [<CLIMutable>] Address = {
    street: string
    city: string  
    state: string
    postalCode: string
    country: string
}

and [<CLIMutable>] OrderItem = {
    productId: string
    productName: string
    quantity: int
    unitPrice: decimal
    totalPrice: decimal
}

// Product model with category-based partitioning
[<CLIMutable>]
type Product = {
    id: string                  // Unique identifier
    productId: string          // Business identifier (SKU)
    name: string
    description: string
    category: string           // Partition key - category-based distribution
    subcategory: string
    brand: string
    price: decimal
    currency: string
    inStock: bool
    stockQuantity: int
    tags: string list
    specifications: Map<string, obj>
    imageUrls: string list
    createdDate: DateTime
    lastUpdated: DateTime
    _etag: string option
}

// Analytics model with time-based partitioning  
[<CLIMutable>]
type CustomerAnalytics = {
    id: string                  // Unique identifier
    customerId: string         // Reference to customer
    yearMonth: string          // Partition key - time-based (YYYY-MM)
    totalOrders: int
    totalSpent: decimal
    averageOrderValue: decimal
    lastOrderDate: DateTime option
    favoriteCategories: string list
    activityScore: float
    riskScore: float
    predictedLifetimeValue: decimal
    segment: string            // High Value, Regular, At Risk, Lost
    lastAnalyzed: DateTime
    _etag: string option
}

// ============ Helper Functions ============

// Generate partition key values
module PartitionKeys =
    
    // Geographic partition for customers
    let customerPartition (country: string) = country.ToUpper()
    
    // Time-based partition for orders (YYYY-MM format)
    let orderPartition (orderDate: DateTime) = orderDate.ToString("yyyy-MM")
    
    // Category-based partition for products
    let productPartition (category: string) = category.ToLower().Replace(" ", "_")
    
    // Time-based partition for analytics (YYYY-MM format)
    let analyticsPartition (yearMonth: string) = yearMonth

// ID generation helpers
module IdGeneration =
    
    let generateCustomerId () = $"customer_{Guid.NewGuid().ToString("N")[..7]}"
    let generateOrderId () = $"order_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString("N")[..7]}"
    let generateProductId () = $"product_{Guid.NewGuid().ToString("N")[..7]}"
    let generateAnalyticsId customerId yearMonth = $"analytics_{customerId}_{yearMonth}"

// Sample data generation
module SampleData =
    
    let countries = [| "USA"; "UK"; "GERMANY"; "FRANCE"; "JAPAN"; "AUSTRALIA"; "CANADA" |]
    let categories = [| "electronics"; "clothing"; "books"; "home_garden"; "sports"; "toys" |]
    let loyaltyTiers = [| "Bronze"; "Silver"; "Gold"; "Platinum" |]
    let orderStatuses = [| "Pending"; "Processing"; "Shipped"; "Delivered"; "Cancelled" |]
    
    let random = Random()
    
    let generateSampleCustomer () =
        let customerId = IdGeneration.generateCustomerId()
        let country = countries.[random.Next(countries.Length)]
        {
            id = customerId
            customerId = customerId
            email = $"user{random.Next(1000, 9999)}@example.com"
            firstName = $"FirstName{random.Next(1, 100)}"
            lastName = $"LastName{random.Next(1, 100)}"
            dateOfBirth = DateTime.Now.AddYears(-random.Next(18, 65))
            country = country
            city = $"City{random.Next(1, 50)}"
            registrationDate = DateTime.Now.AddDays(-random.Next(1, 365))
            isActive = random.Next(0, 2) = 1
            totalOrders = random.Next(0, 50)
            totalSpent = decimal (random.NextDouble() * 5000.0)
            loyaltyTier = loyaltyTiers.[random.Next(loyaltyTiers.Length)]
            lastLoginDate = if random.Next(0, 2) = 1 then Some (DateTime.Now.AddDays(-random.Next(1, 30))) else None
            preferences = Map.empty
            _etag = None
        }
    
    let generateSampleProduct () =
        let productId = IdGeneration.generateProductId()
        let category = categories.[random.Next(categories.Length)]
        {
            id = productId
            productId = productId
            name = $"Product {random.Next(1000, 9999)}"
            description = $"Description for product {productId}"
            category = category
            subcategory = $"sub_{category}"
            brand = $"Brand{random.Next(1, 20)}"
            price = decimal (random.NextDouble() * 1000.0)
            currency = "USD"
            inStock = random.Next(0, 2) = 1
            stockQuantity = random.Next(0, 100)
            tags = [ $"tag1"; $"tag2"; category ]
            specifications = Map.empty
            imageUrls = [ $"https://example.com/image1.jpg"; $"https://example.com/image2.jpg" ]
            createdDate = DateTime.Now.AddDays(-random.Next(1, 365))
            lastUpdated = DateTime.Now
            _etag = None
        }
    
    let generateSampleOrder customerId =
        let orderId = IdGeneration.generateOrderId()
        let orderDate = DateTime.Now.AddDays(-random.Next(1, 90))
        {
            id = orderId
            orderId = orderId
            customerId = customerId
            orderDate = orderDate
            status = orderStatuses.[random.Next(orderStatuses.Length)]
            totalAmount = decimal (random.NextDouble() * 500.0)
            currency = "USD"
            shippingAddress = {
                street = $"{random.Next(100, 999)} Main St"
                city = $"City{random.Next(1, 50)}"
                state = $"State{random.Next(1, 10)}"
                postalCode = $"{random.Next(10000, 99999)}"
                country = countries.[random.Next(countries.Length)]
            }
            items = [
                {
                    productId = IdGeneration.generateProductId()
                    productName = $"Product {random.Next(1000, 9999)}"
                    quantity = random.Next(1, 5)
                    unitPrice = decimal (random.NextDouble() * 100.0)
                    totalPrice = decimal (random.NextDouble() * 200.0)
                }
            ]
            paymentMethod = "Credit Card"
            trackingNumber = if random.Next(0, 2) = 1 then Some $"TRK{random.Next(100000, 999999)}" else None
            notes = None
            _etag = None
        }