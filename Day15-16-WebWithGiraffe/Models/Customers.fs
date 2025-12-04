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

      coordinates: float[] option // [longitude, latitude] for geospatial queries

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
    let countriesCoordinates = Map.ofList [
        "USA", [| -95.7129; 37.0902 |]
        "UK", [| -3.435973; 55.378051 |]
        "GERMANY", [| 10.451526; 51.165691 |]
        "FRANCE", [| 2.213749; 46.227638 |]
        "JAPAN", [| 138.252924; 36.204824 |]
        "AUSTRALIA", [| 133.775136; -25.274398 |]
        "CANADA", [| -106.346771; 56.130366 |]
    ]

    let cities = Map.ofList [
        "USA", [| "New York"; "Los Angeles"; "Chicago"; "Houston"; "Phoenix" |]
        "UK", [| "London"; "Manchester"; "Birmingham"; "Leeds"; "Glasgow" |]
        "GERMANY", [| "Berlin"; "Hamburg"; "Munich"; "Cologne"; "Frankfurt" |]
        "FRANCE", [| "Paris"; "Marseille"; "Lyon"; "Toulouse"; "Nice" |]
        "JAPAN", [| "Tokyo"; "Osaka"; "Nagoya"; "Sapporo"; "Fukuoka" |]
        "AUSTRALIA", [| "Sydney"; "Melbourne"; "Brisbane"; "Perth"; "Adelaide" |]
        "CANADA", [| "Toronto"; "Vancouver"; "Montreal"; "Calgary"; "Ottawa" |]
    ]
    let citiesCoordinates = Map.ofList [
        "New York", [| -74.0060; 40.7128 |]
        "Los Angeles", [| -118.2437; 34.0522 |]
        "Chicago", [| -87.6298; 41.8781 |]
        "Houston", [| -95.3698; 29.7604 |]
        "Phoenix", [| -112.0740; 33.4484 |]
        "London", [| -0.1276; 51.5074 |]
        "Manchester", [| -2.2426; 53.4808 |]
        "Birmingham", [| -1.8986; 52.4862 |]
        "Leeds", [| -1.5491; 53.8008 |]
        "Glasgow", [| -4.2518; 55.8642 |]
        "Berlin", [| 13.4050; 52.5200 |]
        "Hamburg", [| 9.9937; 53.5511 |]
        "Munich", [| 11.5820; 48.1351 |]
        "Cologne", [| 6.9603; 50.9375 |]
        "Frankfurt", [| 8.6821; 50.1109 |]
        "Paris", [| 2.3522; 48.8566 |]
        "Marseille", [| 5.3698; 43.2965 |]
        "Lyon", [| 4.8357; 45.7640 |]
        "Toulouse", [| 1.4442; 43.6047 |]
        "Nice", [| 7.2619; 43.7102 |]
        "Tokyo", [| 139.6917; 35.6895 |]
        "Osaka", [| 135.5023; 34.6937 |]
        "Nagoya", [| 136.9066; 35.1815 |]
        "Sapporo", [| 141.3545; 43.0618 |]
        "Fukuoka", [| 130.4017; 33.5904 |]
        "Sydney", [| 151.2093; -33.8688 |]
        "Melbourne", [| 144.9631; -37.8136 |]
        "Brisbane", [| 153.0251; -27.4698 |]
        "Perth", [| 115.8605; -31.9505 |]
        "Adelaide", [| 138.6007; -34.9285 |]
    ]

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
          city = let cityArray = cities.[country]
                 cityArray.[random.Next(cityArray.Length)]
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
          coordinates = citiesCoordinates.TryFind(
                        let cityArray = cities.[country]
                        cityArray.[random.Next(cityArray.Length)]
                      )
          partitionKey = PartitionKeys.customerPartition }
