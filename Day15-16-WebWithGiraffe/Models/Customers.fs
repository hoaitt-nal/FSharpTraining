module Models

open System

// ============ Domain Models ============

// Customer model with partition key strategy and JSON settings
[<CLIMutable>]
type Customer =
    { id: string // Unique identifier

      customerId: string // Business identifier

      email: string // Contact information

      firstName: string // Personal information

      lastName: string

      dateOfBirth: DateTime

      country: string // Geographic data

      city: string

      registrationDate: DateTime

      isActive: bool

      totalOrders: int

      totalSpent: decimal

      loyaltyTier: string // Bronze, Silver, Gold, Platinum

      lastLoginDate: DateTime option

      lastPurchaseDate: DateTime option // Last purchase tracking

      partitionKey: string } // For optimistic concurrency control

// ============ Helper Functions ============

// Generate partition key values
module PartitionKeys =

    // Fixed partition for customers
    let customerPartition = "customers"

// ID generation helpers
module IdGeneration =

    let generateCustomerId () =
        let guid = Guid.NewGuid().ToString("N")
        $"customer_{guid.[..7]}"

// Sample data generation
module SampleData =

    let countries =
        [| "USA"; "UK"; "GERMANY"; "FRANCE"; "JAPAN"; "AUSTRALIA"; "CANADA" |]

    let loyaltyTiers = [| "Bronze"; "Silver"; "Gold"; "Platinum" |]

    let random = Random()

    let generateSampleCustomer () =
        let customerId = IdGeneration.generateCustomerId ()
        let country = countries.[random.Next(countries.Length)]

        { id = customerId
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
          lastLoginDate =
            if random.Next(0, 2) = 1 then
                Some(DateTime.Now.AddDays(-random.Next(1, 30)))
            else
                None
          lastPurchaseDate =
            if random.Next(0, 2) = 1 then
                Some(DateTime.Now.AddDays(-random.Next(1, 90)))
            else
                None
          partitionKey = PartitionKeys.customerPartition }
